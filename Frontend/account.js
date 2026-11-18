import '@fontsource-variable/archivo/wght.css'
import '@fontsource/ibm-plex-mono/latin-400.css'
import './site.css'
import gsap from 'gsap'
import { attachTheme } from './theme.js'
import { attachCursor } from './cursor.js'
import { attachToasts } from './toasts.js'
import { confirmAdminAction } from './admin-confirm.js'

attachTheme()
const form = document.querySelector('[data-account-form]')
const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)')

const close = document.querySelector('.account-close')
const notify = attachToasts(
  document.querySelector('.account-toasts'),
  document.querySelector('.account-toast-announcer'),
  () => form?.querySelector('input:not([type="hidden"])') || close,
)
if (form?.dataset.loggedOut === 'true') notify('Logged out', 'Your session has ended.', 'success')
if (form?.dataset.notice) notify('Profile saved', form.dataset.notice, 'success')

if (form) {
  form.noValidate = true
  const inputs = [...form.querySelectorAll('input:not([type="hidden"]):not([readonly])')]

  function shake(input) {
    if (!reducedMotion.matches) gsap.fromTo(input, { x: -4 }, { x: 0, duration: .4, ease: 'elastic.out(3, .25)', overwrite: 'auto' })
  }

  function validate(input, feedback = false) {
    let message = ''
    if (!input.value.trim()) message = input.dataset.valRequired
    else if (input.validity.typeMismatch) message = input.dataset.valEmail
    else if (input.minLength > 0 && input.value.length < input.minLength) message = input.dataset.valMinlength
    else if (input.value.length > input.maxLength) message = input.dataset.valLength
    else if (input.dataset.valEqualto && input.value !== form.elements.namedItem('Password').value) message = input.dataset.valEqualto
    const output = document.getElementById(input.getAttribute('aria-describedby'))
    output.textContent = message || ''
    input.setAttribute('aria-invalid', String(Boolean(message)))
    input.classList.toggle('input-validation-error', Boolean(message))
    if (message && feedback) shake(input)
    return !message
  }

  for (const input of inputs) {
    if (input.classList.contains('input-validation-error')) input.setAttribute('aria-invalid', 'true')
    input.addEventListener('blur', () => validate(input, true))
    input.addEventListener('input', () => {
      if (input.getAttribute('aria-invalid') === 'true') validate(input)
      const confirmation = form.elements.namedItem('ConfirmPassword')
      if (input.name === 'Password' && confirmation?.value) validate(confirmation)
    })
  }

  form.addEventListener('submit', async event => {
    if (form.getAttribute('aria-busy') === 'true') {
      event.preventDefault()
      return
    }
    for (const input of inputs) {
      if (input.type !== 'password' && input.autocomplete !== 'new-password' && input.autocomplete !== 'current-password') input.value = input.value.trim()
    }
    const results = inputs.map(input => validate(input, true))
    if (results.includes(false)) {
      event.preventDefault()
      const invalid = inputs[results.indexOf(false)]
      invalid.focus()
      notify('Check your details', 'Please correct the highlighted fields and try again.', 'error')
      return
    }
    if (form.hasAttribute('data-profile-save')) {
      event.preventDefault()
      if (!await confirmAdminAction('Save profile?', 'Save profile', 'Your account will use these names.')) return
      form.submit()
    }
    form.setAttribute('aria-busy', 'true')
    form.querySelector('[type="submit"]').disabled = true
  })
  window.addEventListener('pageshow', () => {
    form.removeAttribute('aria-busy')
    form.querySelector('[type="submit"]').disabled = false
  })

  const summary = form.querySelector('[data-sign-in-errors]')?.content.querySelector('.validation-summary-errors')
    ?? form.querySelector('.validation-summary-errors')
  if (summary) {
    for (const error of summary.querySelectorAll('li')) notify(form.hasAttribute('data-sign-in') ? 'Unable to sign in' : 'Unable to continue', error.textContent, 'error')
    summary.hidden = true
  }

  for (const toggle of form.querySelectorAll('[data-password-toggle]')) {
    toggle.addEventListener('click', () => {
      const password = toggle.closest('.account-password').querySelector('input')
      const visible = toggle.getAttribute('aria-pressed') !== 'true'
      const label = password.name === 'ConfirmPassword' ? 'confirm password' : 'password'
      password.type = visible ? 'text' : 'password'
      toggle.setAttribute('aria-pressed', String(visible))
      toggle.setAttribute('aria-label', `${visible ? 'Hide' : 'Show'} ${label}`)
    })
  }
}

const media = gsap.matchMedia()
media.add('(pointer: fine) and (prefers-reduced-motion: no-preference)', () => attachCursor(gsap))
