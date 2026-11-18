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
document.querySelector('[data-admin-logout]').addEventListener('submit', async event => {
  event.preventDefault()
  const form = event.currentTarget
  if (await confirmAdminAction('Log out?', 'Log out', 'You’ll leave the admin workspace and return to sign in.', true)) {
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
    if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return
    event.preventDefault()
    document.getElementById(opener.dataset.adminOpen)?.showModal()
    return
  }
  if (event.target.closest('[data-admin-dismiss]')) {
    event.target.closest('dialog').close()
  } else if (event.target.matches('.admin-record-dialog, .admin-image-lightbox')) {
    event.target.close()
  }
})

attachAdminForms(gsap)
const profileDialog = document.getElementById('admin-profile')
const profileForm = profileDialog.querySelector('form')
const profileStatus = profileDialog.querySelector('[data-profile-status]')
document.querySelector('[data-admin-open="admin-profile"]').setAttribute('aria-haspopup', 'dialog')
profileDialog.addEventListener('close', () => {
  profileForm.reset()
  profileStatus.textContent = ''
})
profileForm.addEventListener('submit', async event => {
  event.preventDefault()
  const save = profileForm.querySelector('[type="submit"]')
  if (save.disabled) return
  save.disabled = true
  try {
    const response = await fetch(profileForm.action, {
      method: 'POST', body: new FormData(profileForm), headers: { Accept: 'application/json' },
    })
    const result = await response.json()
    if (!response.ok) throw new Error(result.errors?.join(' ') || 'Unable to save your profile.')
    for (const [field, value] of [['FirstName', result.firstName], ['LastName', result.lastName]]) {
      profileForm.elements[field].defaultValue = value
      profileForm.elements[field].value = value
    }
    profileStatus.textContent = 'Your profile has been saved.'
  } catch (error) {
    profileStatus.textContent = error instanceof SyntaxError ? 'Reload the page and try again.' : error.message
  } finally {
    save.disabled = false
  }
})
attachAdminAnalytics(gsap)

for (const button of document.querySelectorAll('.admin-account-toggle')) {
  button.form.addEventListener('submit', async event => {
    event.preventDefault()
    const active = button.dataset.active !== 'true'
    const verb = active ? 'Activate' : 'Deactivate'
    if (await confirmAdminAction(verb + ' ' + button.dataset.userName + '?', verb,
      active ? 'This person will be able to sign in.' : 'This person will lose access to the site.', !active)) button.form.submit()
  })
}

const activity = document.querySelector('[data-activity-list]')
if (activity?.querySelector('[data-activity-date]')) {
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
