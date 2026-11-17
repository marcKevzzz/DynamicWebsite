import '@fontsource-variable/archivo/wght.css'
import '@fontsource/ibm-plex-mono/latin-400.css'
import './site.css'
import gsap from 'gsap'
import { attachTheme } from './theme.js'
import { attachCommands } from './commands.js'
import { createMagnet } from './cursor.js'
import { attachAdminAnalytics, attachAdminDatePicker, formatAdminDateRange } from './admin-analytics.js'
import { attachAdminForms } from './admin-forms.js'

attachTheme(document.querySelector('[data-admin-theme]'))

const shell = document.querySelector('.admin-shell')
const sidebar = document.querySelector('.admin-sidebar')
const navigation = sidebar.querySelector('.admin-nav')
const workspace = document.querySelector('.admin-workspace')
const toggle = document.querySelector('[data-admin-toggle]')
const collapse = document.querySelector('[data-admin-collapse]')
const close = document.querySelector('[data-admin-close]')
const narrow = matchMedia('(max-width: 760px)')
const root = document.documentElement
let collapsed = root.dataset.adminSidebarCollapsed === 'true'

function updateCollapse() {
  const label = collapsed ? 'Expand sidebar' : 'Collapse sidebar'
  collapse.setAttribute('aria-expanded', String(!collapsed))
  collapse.setAttribute('aria-label', label)
  collapse.title = label
}

function showCurrentSection() {
  const link = navigation.querySelector('[aria-current="page"]')
  const bounds = navigation.getBoundingClientRect()
  const current = link.getBoundingClientRect()
  if (current.bottom > bounds.bottom) navigation.scrollTop += current.bottom - bounds.bottom
  else if (current.top < bounds.top) navigation.scrollTop -= bounds.top - current.top
}

function setDrawer(open, restoreFocus = true) {
  shell.dataset.drawer = open ? 'open' : 'closed'
  sidebar.inert = narrow.matches && !open
  workspace.inert = open
  toggle.setAttribute('aria-expanded', String(open))
  if (open) {
    showCurrentSection()
    close.focus()
  }
  else if (restoreFocus) (narrow.matches ? toggle : collapse).focus()
}

function updateLayout() {
  const sidebarHadFocus = sidebar.contains(document.activeElement)
  const mobileToggleHadFocus = document.activeElement === toggle
  toggle.hidden = !narrow.matches
  collapse.hidden = narrow.matches
  close.hidden = !narrow.matches
  setDrawer(false, sidebarHadFocus || mobileToggleHadFocus)
  if (!narrow.matches) showCurrentSection()
}

shell.dataset.ready = 'true'
updateCollapse()
updateLayout()
narrow.addEventListener('change', updateLayout)
toggle.addEventListener('click', () => setDrawer(true))
collapse.addEventListener('click', () => {
  collapsed = !collapsed
  shell.dataset.animateSidebar = 'true'
  root.dataset.adminSidebarCollapsed = String(collapsed)
  updateCollapse()
  try { sessionStorage.setItem('admin-sidebar-collapsed', String(collapsed)) } catch {}
})
close.addEventListener('click', () => setDrawer(false))
document.querySelector('[data-admin-backdrop]').addEventListener('click', () => setDrawer(false))
document.addEventListener('keydown', event => {
  if (event.key === 'Escape' && shell.dataset.drawer === 'open') setDrawer(false)
})

attachCommands(document.querySelector('.admin-search-dialog'), {
  trigger: document.querySelector('[data-admin-search]'),
  canOpen: () => shell.dataset.drawer !== 'open' && !document.querySelector('.admin-record-dialog[open]'),
  run: option => location.assign(option.dataset.url),
})

document.addEventListener('click', event => {
  const opener = event.target.closest('[data-admin-open]')
  if (opener) {
    document.getElementById(opener.dataset.adminOpen)?.showModal()
    return
  }
  if (event.target.closest('[data-admin-dismiss]')) {
    event.target.closest('dialog').close()
  } else if (event.target.matches('.admin-record-dialog')) {
    event.target.close()
  }
})

attachAdminForms(gsap)
attachAdminAnalytics()

const activity = document.querySelector('[data-activity-list]')
if (activity) {
  const picker = document.querySelector('[data-admin-date-picker]')
  const groups = [...activity.querySelectorAll('[data-activity-date]')]
  const empty = activity.querySelector('[data-activity-empty]')
  const columns = activity.querySelector('.admin-timeline-columns')
  const firstDate = groups.at(-1).dataset.activityDate
  const lastDate = groups[0].dataset.activityDate
  const setCaption = attachAdminDatePicker(picker, (start, end) => {
    for (const group of groups) {
      const date = group.dataset.activityDate
      group.hidden = date < start || date > end
    }
    const hasActivity = groups.some(group => !group.hidden)
    columns.hidden = !hasActivity
    empty.hidden = hasActivity
    setCaption(start === firstDate && end === lastDate ? 'All dates' : formatAdminDateRange(start, end))
  })
}

gsap.matchMedia().add('(pointer: fine) and (prefers-reduced-motion: no-preference)', () => {
  const magnet = createMagnet(gsap)
  const events = new AbortController()
  const { signal } = events
  document.addEventListener('pointermove', event => {
    const target = event.pointerType === 'mouse' && event.target.closest('.button, .admin-icon-button, .admin-search-trigger')
    magnet.move(target, event.clientX, event.clientY, .34)
  }, { signal, passive: true })
  root.addEventListener('pointerleave', magnet.release, { signal })
  window.addEventListener('blur', magnet.release, { signal })
  return () => {
    events.abort()
    magnet.release()
  }
})
