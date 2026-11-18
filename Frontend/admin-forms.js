import { attachToasts } from './toasts.js'
import { confirmAdminAction } from './admin-confirm.js'

const imageType = /^image\/(png|jpeg|webp|gif)$/i
const imageExtension = /\.(png|jpe?g|webp|gif)$/i
const imageTypeError = 'Choose a PNG, JPG, WebP, or GIF image.'

function imagePreview(file, removable) {
  const row = document.createElement('li')
  row.className = 'admin-image-tile'
  row.upload = file
  const preview = document.createElement('img')
  preview.alt = ''
  preview.draggable = false
  preview.src = URL.createObjectURL(file)
  const open = document.createElement('button')
  open.type = 'button'
  open.className = 'admin-image-open'
  open.dataset.adminImageOpen = ''
  open.setAttribute('aria-label', `Preview ${file.name}`)
  open.append(preview)
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
  row.append(open, name)
  if (removable) row.append(remove)
  return row
}

function removeImage(item) {
  const source = item.querySelector('img').src
  if (source.startsWith('blob:')) URL.revokeObjectURL(source)
  item.remove()
}

function attachImageInput(input, refresh) {
  const field = input.closest('.admin-image-field')
  const list = field.querySelector('[data-admin-images]')
  for (const control of list.querySelectorAll('[data-native-retain]')) control.hidden = true
  input.form.addEventListener('formdata', ({ formData }) => {
    formData.delete('Images')
    formData.delete('Uploads')
    let index = 0
    for (const tile of list.children) {
      if (tile.upload) {
        formData.append('Images', 'new:' + index++)
        formData.append('Uploads', tile.upload)
      } else {
        formData.append('Images', tile.dataset.imagePath)
      }
    }
  })
  const isLogo = field.dataset.imageStyle === 'logo'
  const isStack = field.dataset.imageStyle === 'stack'
  const usesPrimary = field.closest('.admin-fields').dataset.layout !== 'home'

  function arrangeStack() {
    if (!isStack) return
    const cards = [...list.children]
    const center = (cards.length - 1) / 2
    const spacing = cards.length < 4 ? 104 : 76
    list.style.setProperty('--fan-reach', `${Math.max(0, center) * spacing + 62}px`)
    cards.forEach((card, index) => {
      const position = (index - center) / Math.max(center, 2)
      const angle = position * Math.PI / 2
      card.draggable = true
      const name = card.querySelector('.admin-image-name').textContent
      const opener = card.querySelector('[data-admin-image-open]')
      card.dataset.imageOrder = String(index + 1)
      opener.setAttribute('aria-keyshortcuts', 'Alt+ArrowUp Alt+ArrowDown')
      opener.setAttribute('aria-label',
        `Preview ${name}, image ${index + 1} of ${cards.length}${usesPrimary && index === 0 ? ', primary' : ''}`)
      card.style.setProperty('--fan-offset-x', (110 + Math.cos(angle) * 68) + 'px')
      card.style.setProperty('--fan-offset-y', (index - center) * spacing + 'px')
      card.style.setProperty('--fan-angle', position * 14 + 'deg')
      card.style.setProperty('--fan-delay', index * 35 + 'ms')
    })
  }

  arrangeStack()
  if (isStack) {
    attachImageOrdering(list, arrangeStack)
    const resumeFan = () => delete field.dataset.lightboxDismissed
    field.addEventListener('pointerleave', resumeFan)
    field.addEventListener('pointermove', resumeFan)
    field.addEventListener('keydown', resumeFan)
  }
  input.addEventListener('change', () => {
    const files = [...input.files]
    if (!files.length) return
    const valid = files.filter(file => file.type ? imageType.test(file.type) : imageExtension.test(file.name))
    input.setCustomValidity(valid.length === files.length ? '' : imageTypeError)
    if (!input.multiple && valid.length) [...list.children].forEach(removeImage)
    list.append(...valid.map(file => imagePreview(file, !isLogo)))
    arrangeStack()
    input.value = ''
    refresh(true)
  })

  list.addEventListener('click', async event => {
    const button = event.target.closest('[data-admin-remove-image]')
    if (!button) return
    const item = button.closest('li')
    if (!await confirmAdminAction('Remove this image?', 'Remove image', 'This image will be removed from the current selection.', true)) return
    const next = item.nextElementSibling ?? item.previousElementSibling
    removeImage(item)
    arrangeStack()
    input.setCustomValidity('')
    refresh()
    const focusTarget = next?.querySelector('button') ?? input
    focusTarget.focus()
  })

  field.querySelector('[data-admin-remove-logo]')?.addEventListener('click', async () => {
    if (!await confirmAdminAction('Remove this image?', 'Remove image', 'This field will return to its empty state.', true)) return
    [...list.children].forEach(removeImage)
    input.setCustomValidity('')
    refresh()
    input.focus()
  })
}

function attachImageOrdering(list, arrangeStack) {
  let dragged
  function move(card, target) {
    if (!target || card === target) return
    const movingDown = Boolean(card.compareDocumentPosition(target) & Node.DOCUMENT_POSITION_FOLLOWING)
    if (movingDown) target.after(card)
    else target.before(card)
    arrangeStack()
    card.querySelector('[data-admin-image-open]').focus({ preventScroll: true })
  }

  list.addEventListener('dragstart', event => {
    const card = event.target.closest('.admin-image-tile')
    if (!card) return
    dragged = card
    event.dataTransfer.setData('text/plain', card.querySelector('.admin-image-name').textContent)
    event.dataTransfer.effectAllowed = 'move'
  })
  list.addEventListener('dragover', event => {
    if (!dragged || !event.target.closest('.admin-image-tile')) return
    event.preventDefault()
    event.dataTransfer.dropEffect = 'move'
  })
  list.addEventListener('drop', event => {
    if (!dragged) return
    event.preventDefault()
    move(dragged, event.target.closest('.admin-image-tile'))
    dragged = null
  })
  list.addEventListener('dragend', () => { dragged = null })
  list.addEventListener('keydown', event => {
    if (!event.altKey || !['ArrowUp', 'ArrowDown'].includes(event.key)) return
    const card = event.target.closest('.admin-image-tile')
    if (!card) return
    event.preventDefault()
    move(card, event.key === 'ArrowUp' ? card.previousElementSibling : card.nextElementSibling)
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
  const value = container.previousElementSibling
  container.hidden = false
  value.hidden = true
  input.id = value.id
  value.removeAttribute('id')
  value.form.addEventListener('formdata', ({ formData }) => {
    formData.set(value.name, [...container.querySelectorAll('.admin-chip > span')].map(chip => chip.textContent).join(', '))
  })

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
  container.addEventListener('click', async event => {
    const remove = event.target.closest('[data-remove-chip]')
    if (!remove || remove.disabled) return
    if (!await confirmAdminAction(remove.getAttribute('aria-label') + '?', 'Remove', 'This item will be removed from the list.', true)) return
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
  if (document.body.dataset.signedIn === 'true') notify('Signed in', 'Welcome to the admin dashboard.', 'success')
  const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)')
  for (const form of document.querySelectorAll('[data-confirm-content]')) {
    form.addEventListener('submit', async event => {
      event.preventDefault()
      if (await confirmAdminAction(form.dataset.confirmContent, event.submitter.textContent.trim(), 'The published portfolio will update immediately.')) form.submit()
    })
  }

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

    form.addEventListener('submit', async event => {
      event.preventDefault()
      for (const commit of commitChips) commit()
      const invalid = inputs.filter(input => !validate(input, true))
      if (invalid.length) {
        invalid[0].focus()
        notify('Check your changes', 'Correct the highlighted fields and try again.', 'error')
        return
      }
      const action = event.submitter?.textContent.trim() || 'Save changes'
      if (await confirmAdminAction(action + '?', action, 'Your changes will be visible on the portfolio.')) form.submit()
    })
  }
}
