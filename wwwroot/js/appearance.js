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
    window.scrollTo(0, 0)
    root.classList.add('reload-locked')
    window.setTimeout(() => root.classList.remove('reload-locked'), 1000)
  }

  if (!root.dataset.page && !location.hash && matchMedia('(prefers-reduced-motion: no-preference)').matches) {
    root.classList.add('opening', 'opening-pending')
    window.setTimeout(() => {
      if (root.classList.contains('opening-pending')) root.classList.remove('opening', 'opening-pending')
    }, 4500)
  }
})()
