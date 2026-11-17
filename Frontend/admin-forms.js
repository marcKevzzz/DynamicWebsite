import { attachToasts } from './toasts.js'

const imageType = /^image\/(png|jpeg|webp|svg\+xml|gif|avif)$/i
const imageExtension = /\.(png|jpe?g|webp|svg|gif|avif)$/i
const imageTypeError = 'Choose a PNG, JPG, WebP, SVG, GIF, or AVIF image.'

function imagePreview(file, removable) {
  const row = document.createElement('li')
  row.className = 'admin-image-tile'
  const preview = document.createElement('img')
  const url = URL.createObjectURL(file)
  const release = () => URL.revokeObjectURL(url)
  preview.addEventListener('load', release, { once: true })
  preview.addEventListener('error', release, { once: true })
  preview.alt = ''
  preview.src = url
  const name = document.createElement('span')
  name.className = 'admin-image-name'
  name.textContent = file.name
  name.title = file.name
  const remove = document.createElement('button')
  remove.className = 'admin-image-remove circle-button'
  remove.type = 'button'
  remove.dataset.adminRemoveImage = ''
  remove.setAttribute('aria-label', `Remove ${file.name}`)
  remove.innerHTML = '<svg aria-hidden="true" width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round"><path d="m6 6 12 12M18 6 6 18"/></svg>'
  row.append(preview, name)
  if (removable) row.append(remove)
  return row
}

function attachImageInput(input, refresh) {
  const field = input.closest('.admin-image-field')
  const list = field.querySelector('[data-admin-images]')
  const isLogo = field.dataset.imageStyle === 'logo'

  function arrangeStack() {
    if (field.dataset.imageStyle !== 'stack') return
    const cards = [...list.children]
    const center = (cards.length - 1) / 2
    cards.forEach((card, index) => {
      const position = (index - center) / Math.max(center, .5)
      const angle = position * Math.PI / 2
      card.style.setProperty('--fan-offset-x', (90 + Math.cos(angle) * 48) + 'px')
      card.style.setProperty('--fan-offset-y', (index - center) * 76 + 'px')
      card.style.setProperty('--fan-angle', position * 8 + 'deg')
      card.style.setProperty('--fan-delay', index * 35 + 'ms')
    })
  }

  arrangeStack()
  input.addEventListener('change', () => {
    const files = [...input.files]
    if (!files.length) return
    const valid = files.filter(file => file.type ? imageType.test(file.type) : imageExtension.test(file.name))
    input.setCustomValidity(valid.length === files.length ? '' : imageTypeError)
    if (!input.multiple && valid.length) list.replaceChildren()
    list.append(...valid.map(file => imagePreview(file, !isLogo)))
    arrangeStack()
    input.value = ''
    refresh(true)
  })

  list.addEventListener('click', event => {
    const button = event.target.closest('[data-admin-remove-image]')
    if (!button) {
      if (field.dataset.imageStyle === 'stack') input.click()
      return
    }
    const item = button.closest('li')
    const next = item.nextElementSibling ?? item.previousElementSibling
    item.remove()
    arrangeStack()
    input.setCustomValidity('')
    refresh()
    const focusTarget = next?.querySelector('button') ?? input
    focusTarget.focus()
  })

  field.querySelector('[data-admin-remove-logo]')?.addEventListener('click', () => {
    list.replaceChildren()
    input.setCustomValidity('')
    refresh()
    input.focus()
  })
}

function validationMessage(input) {
  if (input.type === 'file') {
    if (input.validity.customError) return input.validationMessage
    if (input.getAttribute('aria-required') === 'true' && !input.closest('.admin-image-field').querySelector('li')) {
      return input.multiple ? 'Add at least one image.' : 'Choose an image.'
    }
    return ''
  }
  const value = input.value.trim()
  if (!value) return input.required ? `Enter ${input.dataset.label.toLowerCase()}.` : ''
  if (input.type === 'email' && input.validity.typeMismatch) return 'Enter a valid email address.'
  if (input.type === 'url' && (input.validity.typeMismatch || !/^https?:\/\//i.test(value))) return 'Enter a URL starting with https:// or http://.'
  return ''
}

function attachChips(container, reducedMotion) {
  const input = container.querySelector('[data-chip-input]')

  function commit() {
    const chips = [...container.querySelectorAll('.admin-chip > span')]
    const existing = new Set(chips.map(chip => chip.textContent.toLowerCase()))
    for (const value of input.value.split(/[\n,]+/).map(part => part.trim()).filter(Boolean)) {
      const key = value.toLowerCase()
      if (existing.has(key)) continue
      existing.add(key)
      const chip = document.createElement('span')
      chip.className = 'admin-chip'
      const text = document.createElement('span')
      text.textContent = value
      const remove = document.createElement('button')
      remove.type = 'button'
      remove.dataset.removeChip = ''
      remove.setAttribute('aria-label', 'Remove ' + value)
      remove.textContent = '×'
      chip.append(text, remove)
      input.before(chip)
      if (!reducedMotion.matches) chip.animate([{ opacity: 0, transform: 'scale(.9)' }, { opacity: 1, transform: 'scale(1)' }], 180)
    }
    input.value = ''
  }

  input.addEventListener('keydown', event => {
    if ((event.key === 'Enter' || event.key === ',') && !event.isComposing) {
      event.preventDefault()
      commit()
    } else if (event.key === 'Backspace' && !input.value) {
      container.querySelector('.admin-chip:last-of-type button')?.focus()
    }
  })
  input.addEventListener('input', event => {
    if (!event.isComposing && input.value.includes(',')) commit()
  })
  input.addEventListener('blur', commit)
  container.addEventListener('click', event => {
    const remove = event.target.closest('[data-remove-chip]')
    if (!remove || remove.disabled) return
    input.focus()
    const chip = remove.closest('.admin-chip')
    if (reducedMotion.matches) {
      chip.remove()
      return
    }
    remove.disabled = true
    chip.animate([{ opacity: 1 }, { opacity: 0, transform: 'scale(.9)' }], { duration: 140, fill: 'forwards' }).finished.then(() => chip.remove())
  })
  return commit
}

export function attachAdminForms(gsap) {
  const forms = [...document.querySelectorAll('[data-admin-form]:has([data-admin-input]:not([readonly]))')]
  if (!forms.length) return

  const notifications = document.createElement('section')
  notifications.className = 'account-toasts'
  notifications.hidden = true
  notifications.setAttribute('aria-label', 'Notifications')
  const announcer = document.createElement('div')
  announcer.className = 'account-toast-announcer sr-only'
  announcer.setAttribute('role', 'status')
  announcer.setAttribute('aria-atomic', 'true')
  document.body.append(announcer, notifications)
  const notify = attachToasts(notifications, announcer, origin => origin?.isConnected ? origin : null)
  const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)')

  for (const form of forms) {
    const inputs = [...form.querySelectorAll('[data-admin-input]:not([readonly])')]
    const commitChips = [...form.querySelectorAll('.admin-chips')].map(container => attachChips(container, reducedMotion))
    form.noValidate = true

    function validate(input, shake = false) {
      const message = validationMessage(input)
      const output = document.getElementById(input.getAttribute('aria-describedby'))
      output.textContent = message
      input.setAttribute('aria-invalid', String(Boolean(message)))
      if (message && shake && !reducedMotion.matches) {
        const target = input.closest('.admin-image-field')?.querySelector('.admin-image-picker') ?? input
        gsap.fromTo(target, { x: -4 }, { x: 0, duration: .4, ease: 'elastic.out(3, .25)', overwrite: 'auto' })
      }
      return !message
    }

    for (const input of inputs) {
      if (input.type === 'file') {
        attachImageInput(input, shake => validate(input, shake))
      } else {
        input.addEventListener('blur', () => validate(input, true))
        input.addEventListener('input', () => {
          if (input.getAttribute('aria-invalid') === 'true') validate(input)
        })
      }
    }

    form.addEventListener('submit', event => {
      event.preventDefault()
      for (const commit of commitChips) commit()
      const invalid = inputs.filter(input => !validate(input, true))
      if (invalid.length) {
        invalid[0].focus()
        notify('Check your changes', 'Correct the highlighted fields and try again.', 'error')
        return
      }
      notify('Preview complete', 'Changes stay on this page.')
    })
  }
}
