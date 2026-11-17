export function attachToasts(notifications, announcer, fallbackFocus) {
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

  return function notify(title, message, kind = 'info') {
    const toast = document.createElement('button')
    const previousFocus = document.activeElement
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
      if (focused) (notifications.querySelector('.account-toast') || fallbackFocus(previousFocus))?.focus({ preventScroll: true })
    }
    const expiration = setTimeout(dismiss, 5000)
    toast.addEventListener('click', dismiss)
    notifications.prepend(toast)
    arrangeToasts()
    toastSizes.observe(toast)
    announcer.textContent = `${title}. ${message}`
  }
}
