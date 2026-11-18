export function confirmAdminAction(title, action, description, destructive = false) {
  const dialog = document.getElementById('admin-confirm')
  if (dialog.open) return Promise.resolve(false)
  dialog.querySelector('h2').textContent = title
  dialog.querySelector('.admin-dialog-description').textContent = description
  dialog.querySelector('[value="confirm"]').textContent = action
  dialog.toggleAttribute('data-destructive', destructive)
  dialog.returnValue = ''
  return new Promise(resolve => {
    dialog.addEventListener('close', () => resolve(dialog.returnValue === 'confirm'), { once: true })
    dialog.showModal()
  })
}
