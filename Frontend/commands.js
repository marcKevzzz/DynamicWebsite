export function attachCommands(dialog, { run, canOpen = () => true, onOpen, onClose, trigger }) {
  const search = dialog.querySelector('input')
  const options = [...dialog.querySelectorAll('[role="option"]')]
  const empty = dialog.querySelector('.empty-results')
  let visible = options
  let selected = 0
  let previousFocus

  function select(index) {
    selected = index
    for (const option of options) {
      const active = option === visible[index]
      option.classList.toggle('selected', active)
      option.setAttribute('aria-selected', String(active))
    }
    if (visible[index]) search.setAttribute('aria-activedescendant', visible[index].id)
    else search.removeAttribute('aria-activedescendant')
  }

  function filter() {
    const query = search.value.trim().toLowerCase()
    visible = options.filter(option => {
      option.hidden = !option.textContent.toLowerCase().includes(query)
      return !option.hidden
    })
    empty.hidden = visible.length !== 0
    select(0)
  }

  function open() {
    if (dialog.open || !canOpen()) return
    previousFocus = document.activeElement
    search.value = ''
    filter()
    dialog.showModal()
    onOpen?.()
    search.focus()
  }

  function activate(option) {
    dialog.close()
    run(option)
  }

  if (trigger) {
    trigger.hidden = false
    trigger.addEventListener('click', open)
  }
  dialog.querySelector('[data-close-menu]').addEventListener('click', () => dialog.close())
  dialog.addEventListener('click', event => { if (event.target === dialog) dialog.close() })
  dialog.addEventListener('close', () => {
    onClose?.()
    if (document.activeElement === document.body || dialog.contains(document.activeElement)) previousFocus?.focus()
  })
  document.addEventListener('keydown', event => {
    if (!(event.ctrlKey || event.metaKey) || event.key.toLowerCase() !== 'k') return
    event.preventDefault()
    if (dialog.open) dialog.close()
    else open()
  })
  search.addEventListener('input', filter)
  search.addEventListener('keydown', event => {
    if (!visible.length) return
    if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
      event.preventDefault()
      const step = event.key === 'ArrowDown' ? 1 : -1
      select((selected + step + visible.length) % visible.length)
      visible[selected].scrollIntoView({ block: 'nearest' })
    }
    if (event.key === 'Enter') {
      event.preventDefault()
      activate(visible[selected])
    }
  })
  for (const option of options) {
    option.addEventListener('pointerenter', () => select(visible.indexOf(option)))
    option.addEventListener('click', () => activate(option))
  }
}
