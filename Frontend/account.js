import '@fontsource-variable/archivo/wght.css'
import '@fontsource/ibm-plex-mono/latin-400.css'
import './site.css'
import gsap from 'gsap'
import { attachTheme } from './theme.js'
import { attachCursor } from './cursor.js'

attachTheme()
const form = document.querySelector('[data-account-form]')
const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)')

const notifications = document.querySelector('.account-toasts')
const announcer = document.querySelector('.account-toast-announcer')
const stackToggle = document.createElement('button')
stackToggle.type = 'button'
stackToggle.className = 'account-toast-toggle'
stackToggle.setAttribute('aria-expanded', 'false')
notifications.append(stackToggle)

function arrangeToasts() {
  const toasts = [...notifications.querySelectorAll('.account-toast')]
  notifications.hidden = !toasts.length
  let height = 0
  toasts.forEach((toast, index) => {
    toast.style.setProperty('--stack-top', `${Math.min(index, 2) * 8}px`)
    toast.style.setProperty('--stack-scale', 1 - Math.min(index, 2) * .04)
    toast.style.setProperty('--expanded-top', `${height}px`)
    toast.style.zIndex = toasts.length - index
    height += toast.offsetHeight + 8
  })
  const multiple = toasts.length > 1
  notifications.style.setProperty('--stack-height', `${(toasts[0]?.offsetHeight || 0) + (multiple ? 52 : 0)}px`)
  notifications.style.setProperty('--expanded-height', `${height + (multiple ? 28 : 0)}px`)
  stackToggle.hidden = !multiple
  stackToggle.textContent = notifications.dataset.expanded === 'true' ? 'Collapse ↑' : `+${toasts.length - 1} ↓`
}
stackToggle.addEventListener('click', () => {
  const expanded = notifications.dataset.expanded !== 'true'
  notifications.dataset.expanded = String(expanded)
  stackToggle.setAttribute('aria-expanded', String(expanded))
  arrangeToasts()
})
const toastSizes = new ResizeObserver(arrangeToasts)
function notify(title, message, kind = 'info') {
  const toast = document.createElement('button')
  toast.type = 'button'
  toast.className = 'account-toast'
  toast.dataset.kind = kind
  toast.innerHTML = `<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><circle cx="12" cy="12" r="9"/>${kind === 'error' ? '<path d="M12 7v6m0 3v1"/>' : kind === 'success' ? '<path d="m7 12 3 3 7-7"/>' : '<path d="M12 11v6m0-10v1"/>'}</svg><span><strong></strong><small></small></span>`
  toast.querySelector('strong').textContent = title
  toast.querySelector('small').textContent = message
  toast.setAttribute('aria-label', `${title}. ${message} Dismiss notification`)
  function dismiss() {
    clearTimeout(expiration)
    const focused = document.activeElement === toast
    toastSizes.unobserve(toast)
    toast.remove()
    arrangeToasts()
    if (focused) (notifications.querySelector('.account-toast') || form?.querySelector('input:not([type="hidden"])') || close).focus({ preventScroll: true })
  }
  const expiration = setTimeout(dismiss, 5000)
  toast.addEventListener('click', dismiss)
  notifications.prepend(toast)
  arrangeToasts()
  toastSizes.observe(toast)
  announcer.textContent = `${title}. ${message}`
}

const visual = document.querySelector('.account-visual')
const imageShape = document.querySelector('#account-image-shape')
const close = document.querySelector('.account-close')
new ResizeObserver(([entry]) => {
  const { width, height } = entry.contentRect
  if (!width || !height) return
  const size = close.getBoundingClientRect().width
  const notch = size + 24
  const radius = size / 2
  const edge = width - notch
  imageShape.setAttribute('clipPathUnits', 'userSpaceOnUse')
  imageShape.querySelector('path').setAttribute('d',
    `M28 0H${edge - 28}Q${edge} 0 ${edge} 28V${notch - radius}Q${edge} ${notch} ${edge + radius} ${notch}H${width - 28}Q${width} ${notch} ${width} ${notch + 28}V${height - 28}Q${width} ${height} ${width - 28} ${height}H28Q0 ${height} 0 ${height - 28}V28Q0 0 28 0Z`)
}).observe(visual)

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

  const summary = form.querySelector('.validation-summary-errors')
  if (summary) {
    for (const error of summary.querySelectorAll('li')) notify('Unable to continue', error.textContent, 'error')
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
media.add('(prefers-reduced-motion: no-preference)', () => {
  gsap.from('.account-shell', { y: 20, opacity: 0, duration: .65, ease: 'power3.out' })
  gsap.from('.account-title, .account-description, .account-field, .account-recovery-link, .account-submit, .account-links', {
    y: 14, opacity: 0, duration: .55, stagger: .045, delay: .12, ease: 'power3.out',
  })
})
media.add('(pointer: fine) and (prefers-reduced-motion: no-preference)', () => attachCursor(gsap))
