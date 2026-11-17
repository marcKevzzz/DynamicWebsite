export function attachTheme(theme) {
  const root = document.documentElement
  const metaTheme = document.querySelector('meta[name="theme-color"]')
  const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)')
  const systemTheme = matchMedia('(prefers-color-scheme: light)')
  let hasThemeOverride = false
  let themeTransition

  try { hasThemeOverride = ['light', 'dark'].includes(localStorage.getItem('theme')) } catch {}

  function applyTheme(mode) {
    root.dataset.theme = mode
    theme?.setAttribute('aria-label', mode === 'light' ? 'Switch to dark theme' : 'Switch to light theme')
    metaTheme.content = getComputedStyle(root).getPropertyValue('--bg').trim()
  }

  function toggleTheme() {
    const mode = root.dataset.theme === 'light' ? 'dark' : 'light'
    const apply = () => {
      hasThemeOverride = true
      try { localStorage.setItem('theme', mode) } catch {}
      applyTheme(mode)
    }
    if (!theme || !document.startViewTransition || reducedMotion.matches || themeTransition) {
      themeTransition?.skipTransition()
      apply()
      return
    }

    const rect = theme.getBoundingClientRect()
    const x = rect.left + rect.width / 2
    const y = rect.top + rect.height / 2
    const radius = Math.hypot(Math.max(x, innerWidth - x), Math.max(y, innerHeight - y))
    const transition = document.startViewTransition(apply)
    themeTransition = transition
    transition.ready.then(() => root.animate(
      { clipPath: [`circle(0px at ${x}px ${y}px)`, `circle(${radius}px at ${x}px ${y}px)`] },
      { duration: 780, easing: 'cubic-bezier(0.3, 0.7, 0.25, 1)', pseudoElement: '::view-transition-new(root)' },
    )).catch(() => {})
    transition.finished.finally(() => { if (themeTransition === transition) themeTransition = undefined })
  }

  applyTheme(root.dataset.theme)
  if (theme) {
    theme.hidden = false
    theme.addEventListener('click', toggleTheme)
  }
  systemTheme.addEventListener('change', event => {
    if (!hasThemeOverride) applyTheme(event.matches ? 'light' : 'dark')
  })
  return toggleTheme
}
