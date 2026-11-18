import '@fontsource-variable/archivo/wght.css'
import '@fontsource/ibm-plex-mono/latin-400.css'
import './site.css'
import gsap from 'gsap'
import { attachTheme } from './theme.js'
import { attachCursor } from './cursor.js'
import { attachToasts } from './toasts.js'

attachTheme()
const form = document.querySelector('[data-account-form]')
const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)')

const close = document.querySelector('.account-close')
const notify = attachToasts(
  document.querySelector('.account-toasts'),
  document.querySelector('.account-toast-announcer'),
  () => form?.querySelector('input:not([type="hidden"])') || close,
)
if (form?.dataset.loggedOut === 'true') notify('Logged out', 'Your admin session has ended.', 'success')

if (form) {
  form.noValidate = true
  const inputs = [...form.querySelectorAll('input:not([type="hidden"])')]

  function shake(input) {
    const target = input.closest('.account-code-entry') || input
    if (!reducedMotion.matches) gsap.fromTo(target, { x: -4 }, { x: 0, duration: .4, ease: 'elastic.out(3, .25)', overwrite: 'auto' })
  }

  function validate(input, feedback = false) {
    let message = ''
    if (!input.value.trim()) message = input.dataset.valRequired
    else if (input.validity.typeMismatch) message = input.dataset.valEmail
    else if (input.dataset.valRegex && !new RegExp(input.dataset.valRegexPattern).test(input.value)) message = input.dataset.valRegex
    else if (input.minLength > 0 && input.value.length < input.minLength) message = input.dataset.valMinlength
    else if (input.value.length > input.maxLength) message = input.dataset.valLength
    else if (input.dataset.valEqualto && input.value !== form.elements.namedItem('Password').value) message = input.dataset.valEqualto
    const output = document.getElementById(input.getAttribute('aria-describedby'))
    output.textContent = message || ''
    input.setAttribute('aria-invalid', String(Boolean(message)))
    input.classList.toggle('input-validation-error', Boolean(message))
    if (input === code) form.querySelectorAll('[data-code-digit]').forEach(digit => digit.setAttribute('aria-invalid', String(Boolean(message))))
    if (message && feedback) shake(input)
    return !message
  }

  const code = form.querySelector('[data-code-input]')
  if (code) {
    const slots = document.createElement('div')
    slots.className = 'account-code-slots'
    slots.setAttribute('role', 'group')
    slots.setAttribute('aria-label', 'Six-digit verification code')
    const digits = Array.from({ length: 6 }, (_, index) => {
      const digit = document.createElement('input')
      digit.id = `Code-digit-${index + 1}`
      digit.type = 'text'
      digit.inputMode = 'numeric'
      digit.autocomplete = index === 0 ? 'one-time-code' : 'off'
      digit.maxLength = 6
      digit.dataset.codeDigit = ''
      digit.setAttribute('aria-label', `Digit ${index + 1} of 6`)
      digit.setAttribute('aria-describedby', 'Code-error')
      digit.setAttribute('aria-invalid', String(code.classList.contains('input-validation-error')))
      digit.value = code.value[index] || ''
      slots.append(digit)
      return digit
    })
    const syncCode = () => {
      code.value = digits.map(digit => digit.value).join('')
      if (code.getAttribute('aria-invalid') === 'true') validate(code)
    }
    const fillDigits = (value, index) => {
      const numbers = value.replace(/[^0-9]/g, '').slice(0, 6)
      if (numbers.length === 6) index = 0
      for (let offset = 0; offset < numbers.length && index + offset < 6; offset++) digits[index + offset].value = numbers[offset]
      digits[Math.min(index + numbers.length, 5)].focus()
      syncCode()
    }
    digits.forEach((digit, index) => {
      digit.addEventListener('focus', () => digit.select())
      digit.addEventListener('input', () => {
        const value = digit.value
        digit.value = ''
        if (value) fillDigits(value, index)
        else syncCode()
      })
      digit.addEventListener('paste', event => {
        event.preventDefault()
        fillDigits(event.clipboardData.getData('text'), index)
      })
      digit.addEventListener('keydown', event => {
        if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
          event.preventDefault()
          digits[Math.max(0, Math.min(5, index + (event.key === 'ArrowLeft' ? -1 : 1)))].focus()
        } else if (event.key === 'Backspace' && !digit.value && index > 0) {
          event.preventDefault()
          digits[index - 1].value = ''
          digits[index - 1].focus()
          syncCode()
        }
      })
    })
    slots.addEventListener('focusout', event => {
      if (!slots.contains(event.relatedTarget)) validate(code, true)
    })
    code.type = 'hidden'
    code.parentElement.append(slots)
    form.querySelector('label[for="Code"]').htmlFor = digits[0].id
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

  form.addEventListener('submit', event => {
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
      const target = invalid === code ? form.querySelector('[data-code-digit]') : invalid
      target.focus()
      notify('Check your details', 'Please correct the highlighted fields and try again.', 'error')
      return
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
