import '@fontsource-variable/archivo/wght.css'
import '@fontsource/ibm-plex-mono/latin-400.css'
import 'lenis/dist/lenis.css'
import './site.css'
import gsap from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'
import Lenis from 'lenis'
import { playOpening } from './opening.js'
import { animateWorkGallery } from './work.js'
import { attachCursor } from './cursor.js'
import { attachProfiles } from './team.js'

gsap.registerPlugin(ScrollTrigger)

const root = document.documentElement
const header = document.querySelector('.header')
const hero = document.querySelector('.hero')
const ruler = header.querySelector('.ruler-marker')
const theme = header.querySelector('[data-theme]')
const metaTheme = document.querySelector('meta[name="theme-color"]')
const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)')
const systemTheme = matchMedia('(prefers-color-scheme: light)')
let hasThemeOverride = false
let themeTransition
let lenis

try { hasThemeOverride = ['light', 'dark'].includes(localStorage.getItem('theme')) } catch {}

function applyTheme(mode) {
  root.dataset.theme = mode
  theme.setAttribute('aria-label', mode === 'light' ? 'Switch to dark theme' : 'Switch to light theme')
  metaTheme.content = getComputedStyle(root).getPropertyValue('--bg').trim()
}

function toggleTheme() {
  const mode = root.dataset.theme === 'light' ? 'dark' : 'light'
  const apply = () => {
    hasThemeOverride = true
    try { localStorage.setItem('theme', mode) } catch {}
    applyTheme(mode)
  }
  if (!document.startViewTransition || reducedMotion.matches || themeTransition) {
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
theme.hidden = false
theme.addEventListener('click', toggleTheme)
systemTheme.addEventListener('change', event => {
  if (!hasThemeOverride) applyTheme(event.matches ? 'light' : 'dark')
})

const mobileNav = header.querySelector('nav')
const menuToggle = header.querySelector('[data-mobile-nav]')
const mobileHeader = matchMedia('(max-width: 700px)')

function setMobileNav(open, restoreFocus = false) {
  header.classList.toggle('is-menu-open', open)
  menuToggle.setAttribute('aria-expanded', String(open))
  menuToggle.setAttribute('aria-label', open ? 'Close navigation' : 'Open navigation')
  if (restoreFocus) menuToggle.focus({ preventScroll: true })
  mobileNav.inert = mobileHeader.matches && !open
}

header.classList.add('mobile-nav-ready')
menuToggle.hidden = false
setMobileNav(false)
mobileHeader.addEventListener('change', () => setMobileNav(false))
menuToggle.addEventListener('click', () => setMobileNav(menuToggle.getAttribute('aria-expanded') === 'false'))
header.addEventListener('click', event => {
  if (mobileHeader.matches && event.target.closest('a')) setMobileNav(false, mobileNav.contains(event.target))
})
document.addEventListener('pointerdown', event => {
  if (header.classList.contains('is-menu-open') && !header.contains(event.target)) {
    setMobileNav(false, mobileNav.contains(document.activeElement))
  }
})
header.addEventListener('keydown', event => {
  if (event.key !== 'Escape' || !header.classList.contains('is-menu-open')) return
  event.preventDefault()
  setMobileNav(false, true)
})

function updateRuler() {
  const distance = root.scrollHeight - innerHeight
  const progress = distance > 0 ? Math.min(100, Math.max(0, scrollY / distance * 100)) : 0
  header.style.setProperty('--scroll-progress', `${progress}%`)
  ruler.textContent = `${String(Math.round(progress)).padStart(2, '0')}%`
  const threshold = header.classList.contains('is-scrolled') ? 36 : 56
  header.classList.toggle('is-scrolled', scrollY > threshold)
}
window.addEventListener('scroll', updateRuler, { passive: true })
window.addEventListener('resize', updateRuler)
updateRuler()

const dialog = document.querySelector('.shortcut-dialog')
const search = dialog.querySelector('input')
const options = [...dialog.querySelectorAll('[role="option"]')]
const empty = dialog.querySelector('.empty-results')
let visibleOptions = options
let selected = 0
let previousFocus

function select(index) {
  selected = index
  for (const option of options) {
    const active = option === visibleOptions[index]
    option.classList.toggle('selected', active)
    option.setAttribute('aria-selected', String(active))
  }
  if (visibleOptions[index]) search.setAttribute('aria-activedescendant', visibleOptions[index].id)
  else search.removeAttribute('aria-activedescendant')
}

function filter() {
  const query = search.value.trim().toLowerCase()
  visibleOptions = options.filter(option => {
    option.hidden = !option.textContent.toLowerCase().includes(query)
    return !option.hidden
  })
  empty.hidden = visibleOptions.length !== 0
  select(0)
}

function openMenu() {
  if (dialog.open || document.querySelector(':is(.team-dialog, .project-dialog)[open]')) return
  previousFocus = document.activeElement
  search.value = ''
  filter()
  dialog.showModal()
  lenis?.stop()
  search.focus()
}

function runOption(option) {
  dialog.close()
  lenis?.start()
  if (option.dataset.target) {
    const target = document.getElementById(option.dataset.target)
    target.tabIndex = -1
    target.focus({ preventScroll: true })
    if (lenis) lenis.scrollTo(target)
    else target.scrollIntoView()
  } else {
    toggleTheme()
  }
}

dialog.querySelector('[data-close-menu]').addEventListener('click', () => dialog.close())
dialog.addEventListener('click', event => { if (event.target === dialog) dialog.close() })
dialog.addEventListener('close', () => {
  lenis?.start()
  if (document.activeElement === document.body || dialog.contains(document.activeElement)) previousFocus?.focus()
})
document.addEventListener('keydown', event => {
  if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 'k') {
    event.preventDefault()
    if (dialog.open) dialog.close()
    else openMenu()
  }
})
search.addEventListener('input', filter)
search.addEventListener('keydown', event => {
  if (!visibleOptions.length) return
  if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
    event.preventDefault()
    const step = event.key === 'ArrowDown' ? 1 : -1
    select((selected + step + visibleOptions.length) % visibleOptions.length)
    visibleOptions[selected].scrollIntoView({ block: 'nearest' })
  }
  if (event.key === 'Enter') {
    event.preventDefault()
    runOption(visibleOptions[selected])
  }
})
for (const option of options) {
  option.addEventListener('pointerenter', () => select(visibleOptions.indexOf(option)))
  option.addEventListener('click', () => runOption(option))
}

for (const row of document.querySelectorAll('.service__link')) {
  const setOrigin = event => {
    const rect = row.getBoundingClientRect()
    row.style.setProperty('--origin', event.clientY - rect.top < rect.height / 2 ? 'top' : 'bottom')
  }
  row.addEventListener('pointerenter', setOrigin)
  row.addEventListener('pointerleave', setOrigin)
}

function lockPage(locked) {
  if (locked) lenis?.stop()
  else lenis?.start()
}

for (const section of document.querySelectorAll('#team, #work')) {
  attachProfiles(section, gsap, ScrollTrigger, reducedMotion, lockPage)
}
animateWorkGallery(gsap, ScrollTrigger)
playOpening(gsap, reducedMotion)

const media = gsap.matchMedia()
media.add('(min-width: 768px) and (prefers-reduced-motion: no-preference)', () => {
  lenis = new Lenis({
    duration: 1.05,
    anchors: true,
    prevent: element => Boolean(element.closest('dialog')),
    virtualScroll: () => !root.classList.contains('reload-locked'),
  })
  lenis.on('scroll', ScrollTrigger.update)
  const tick = time => lenis.raf(time * 1000)
  gsap.ticker.add(tick)
  return () => {
    gsap.ticker.remove(tick)
    lenis.destroy()
    lenis = undefined
  }
})

media.add('(prefers-reduced-motion: no-preference)', () => {
  gsap.to('.hero-collage', {
    y: -54, ease: 'none',
    scrollTrigger: { trigger: hero, start: 'top top', end: 'bottom top', scrub: 0.8 },
  })

  for (const row of document.querySelectorAll('.service__link')) {
    gsap.fromTo(row.querySelectorAll('.service__tag'),
      { y: index => index % 2 ? 7 : 11 },
      { y: index => index % 2 ? -6 : -10, ease: 'none',
        scrollTrigger: { trigger: row, start: 'top bottom', end: 'bottom top', scrub: 0.6 } })
  }

  gsap.timeline({ scrollTrigger: { trigger: '.about-note', start: 'top 88%', once: true } })
    .from('.about-note p span', { y: 22, opacity: 0, stagger: 0.12, duration: 0.64, ease: 'power3.out' })
    .from('.about-mark', { x: 48, opacity: 0, duration: 0.75, ease: 'power3.out' }, 0.24)

  const manifesto = gsap.timeline({
    scrollTrigger: { trigger: '.manifesto', start: 'top 74%', end: 'bottom 62%', scrub: true },
  })
  manifesto.fromTo('.wf-solid', { clipPath: 'inset(-0.35em 100% -0.45em 0%)' },
    { clipPath: 'inset(-0.35em -2% -0.45em -1%)', stagger: 0.48, ease: 'none' })
})

media.add('(pointer: fine) and (prefers-reduced-motion: no-preference)', () => attachCursor(gsap))
document.fonts.ready.then(() => {
  ScrollTrigger.refresh()
  updateRuler()
})
