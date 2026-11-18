(() => {
  const root = document.documentElement
  let saved
  try { saved = localStorage.getItem('theme') } catch {}
  root.dataset.theme = saved === 'light' || saved === 'dark'
    ? saved
    : matchMedia('(prefers-color-scheme: light)').matches ? 'light' : 'dark'

  if (root.dataset.page === 'admin') {
    try { root.dataset.adminSidebarCollapsed = sessionStorage.getItem('admin-sidebar-collapsed') === 'true' } catch {}
  }

  if (!root.dataset.page && performance.getEntriesByType('navigation')[0]?.type === 'reload') {
    history.scrollRestoration = 'manual'
    if (location.hash) history.replaceState(history.state, '', location.pathname + location.search)
    // The browser can restore the outgoing document's position after this head script runs.
    window.addEventListener('pageshow', () => requestAnimationFrame(() => {
      window.scrollTo({ top: 0, left: 0, behavior: 'instant' })
      history.scrollRestoration = 'auto'
    }), { once: true })
  }

  if (!root.dataset.page && !location.hash && matchMedia('(prefers-reduced-motion: no-preference)').matches) {
    root.classList.add('opening', 'opening-pending')
    window.setTimeout(() => {
      if (root.classList.contains('opening-pending')) root.classList.remove('opening', 'opening-pending')
    }, 4500)
  }
})()
