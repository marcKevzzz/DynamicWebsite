export function confirmAdminAction(title, action, description, destructive = false) {
  const dialog = document.getElementById('admin-confirm')
  if (dialog.open) return Promise.resolve(false)
  dialog.querySelector('h2').textContent = title
  dialog.querySelector('.admin-dialog-description').textContent = description
  dialog.querySelector('[value="confirm"]').textContent = action
  dialog.toggleAttribute('data-destructive', destructive)
  dialog.returnValue = ''
  return new Promise(resolve => {
    function dismiss(event) {
      if (event.target === dialog || event.target.closest('[data-confirm-dismiss]')) dialog.close('cancel')
    }
    dialog.addEventListener('click', dismiss)
    dialog.addEventListener('close', () => {
      dialog.removeEventListener('click', dismiss)
      resolve(dialog.returnValue === 'confirm')
    }, { once: true })
    dialog.showModal()
  })
}
