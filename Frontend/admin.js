import '@fontsource-variable/archivo/wght.css'
import '@fontsource/ibm-plex-mono/latin-400.css'
import './site.css'
import gsap from 'gsap'
import { attachTheme } from './theme.js'
import { attachCommands } from './commands.js'
import { createMagnet } from './cursor.js'
import { attachAdminAnalytics, attachAdminDatePicker, formatAdminDateRange } from './admin-analytics.js'
import { attachAdminForms } from './admin-forms.js'
import { confirmAdminAction } from './admin-confirm.js'

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
  if (event.key === 'Escape' && shell.dataset.drawer === 'open' && !document.querySelector('dialog[open]')) setDrawer(false)
})

attachCommands(document.querySelector('.admin-search-dialog'), {
  trigger: document.querySelector('[data-admin-search]'),
  canOpen: () => shell.dataset.drawer !== 'open' && !document.querySelector('.admin-record-dialog[open], .admin-image-lightbox[open]'),
  run: option => location.assign(option.dataset.url),
})

const lightbox = document.querySelector('[data-admin-lightbox]')
const profile = document.querySelector('#admin-profile')
const profileForm = profile.querySelector('form')
let names = {}
try { names = JSON.parse(localStorage.getItem('admin-preview-profile')) || {} } catch {}
function saveProfile() {
  const firstName = profileForm.elements.firstName.value.trim()
  const lastName = profileForm.elements.lastName.value.trim()
  names = { firstName, lastName }
  try { localStorage.setItem('admin-preview-profile', JSON.stringify(names)) } catch {}
  profile.close()
}
document.querySelector('[data-admin-logout]').addEventListener('submit', async event => {
  event.preventDefault()
  const form = event.currentTarget
  if (await confirmAdminAction('Log out?', 'Log out', 'You’ll leave the admin preview and return to sign in.', true)) {
    form.submit()
  }
})
const lightboxImage = lightbox.querySelector('img')
let lightboxStack
lightbox.addEventListener('close', () => {
  if (lightboxStack) lightboxStack.dataset.lightboxDismissed = ''
})

document.addEventListener('click', event => {
  const imageOpener = event.target.closest('[data-admin-image-open]')
  if (imageOpener) {
    const tile = imageOpener.closest('.admin-image-tile')
    lightboxStack = tile.closest('[data-image-style="stack"]')
    lightbox.toggleAttribute('data-logo-preview', Boolean(tile.closest('[data-layout="about"] [data-image-style="logo"]')))
    lightboxImage.src = tile.querySelector('img').src
    lightboxImage.alt = tile.querySelector('.admin-image-name').textContent
    lightbox.showModal()
    return
  }
  const opener = event.target.closest('[data-admin-open]')
  if (opener) {
    if (opener.dataset.adminOpen === profile.id) {
      profileForm.elements.firstName.value = names.firstName || ''
      profileForm.elements.lastName.value = names.lastName || ''
      profileForm.querySelectorAll('[aria-invalid]').forEach(input => input.removeAttribute('aria-invalid'))
      profileForm.querySelectorAll('.admin-field-message').forEach(message => { message.textContent = '' })
    }
    document.getElementById(opener.dataset.adminOpen)?.showModal()
    return
  }
  if (event.target.closest('[data-admin-dismiss]')) {
    event.target.closest('dialog').close()
  } else if (event.target.matches('.admin-record-dialog, .admin-image-lightbox')) {
    event.target.close()
  }
})

attachAdminForms(gsap, saveProfile)
attachAdminAnalytics(gsap)

for (const button of document.querySelectorAll('.admin-account-toggle')) {
  button.hidden = false
  button.addEventListener('click', async () => {
    const status = button.closest('li').querySelector('.admin-status')
    const active = status.dataset.active !== 'true'
    const verb = active ? 'Activate' : 'Deactivate'
    if (!await confirmAdminAction(verb + ' ' + button.dataset.userName + '?', verb, 'Their status in this preview will change to ' + (active ? 'active.' : 'inactive.'), !active)) return
    const action = active ? 'Deactivate' : 'Activate'
    button.dataset.active = status.dataset.active = String(active)
    status.textContent = active ? 'Active' : 'Inactive'
    button.querySelector('[data-action-label]').textContent = action
    button.setAttribute('aria-label', action + ' ' + button.dataset.userName)
  })
}

const activity = document.querySelector('[data-activity-list]')
if (activity) {
  const picker = document.querySelector('[data-admin-date-picker]')
  const groups = [...activity.querySelectorAll('[data-activity-date]')]
  const empty = activity.querySelector('[data-activity-empty]')
  const firstDate = groups.at(-1).dataset.activityDate
  const lastDate = groups[0].dataset.activityDate
  const setCaption = attachAdminDatePicker(picker, (start, end) => {
    for (const group of groups) {
      const date = group.dataset.activityDate
      group.hidden = date < start || date > end
    }
    const hasActivity = groups.some(group => !group.hidden)
    empty.hidden = hasActivity
    setCaption(start === firstDate && end === lastDate ? 'All dates' : formatAdminDateRange(start, end))
  })
}

gsap.matchMedia().add('(pointer: fine) and (prefers-reduced-motion: no-preference)', () => {
  const magnet = createMagnet(gsap)
  const events = new AbortController()
  const { signal } = events
  document.addEventListener('pointermove', event => {
    const target = event.pointerType === 'mouse' && event.target.closest('.button, .admin-icon-button, .admin-search-trigger, .admin-stat-arrow')
    magnet.move(target, event.clientX, event.clientY, .34)
  }, { signal, passive: true })
  root.addEventListener('pointerleave', magnet.release, { signal })
  window.addEventListener('blur', magnet.release, { signal })
  return () => {
    events.abort()
    magnet.release()
  }
})
