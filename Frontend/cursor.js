export function createMagnet(gsap) {
  let current

  function release() {
    if (!current) return
    gsap.to(current, { x: 0, y: 0, duration: 0.45, ease: 'elastic.out(1, 0.4)', overwrite: 'auto' })
    current = undefined
  }

  function move(target, x, y, strength = 1) {
    if (target !== current) release()
    current = target
    if (!current) return
    const rect = current.getBoundingClientRect()
    gsap.to(current, {
      x: (x - rect.left - rect.width / 2 + Number(gsap.getProperty(current, 'x'))) * 0.22 * strength,
      y: (y - rect.top - rect.height / 2 + Number(gsap.getProperty(current, 'y'))) * 0.28 * strength,
      duration: 0.3, ease: 'power3.out', overwrite: 'auto',
    })
  }

  return { move, release }
}

export function attachCursor(gsap) {
  const root = document.documentElement
  const cursor = document.querySelector('.cursor')
  const cursorHome = cursor.parentElement
  const point = cursor.querySelector('.cursor__pt')
  const label = cursor.querySelector('.cursor__label')
  const hero = document.querySelector('.hero')
  const followOptions = { duration: 0.6, ease: 'power3.out' }
  const layers = [...(hero?.querySelectorAll('[data-depth]') ?? [])].flatMap(card =>
    [...card.querySelectorAll('img, .hero-caption')].map(layer => ({
      depth: Number(card.dataset.depth) * (layer.matches('.hero-caption') ? 0.35 : 1),
      x: gsap.quickTo(layer, 'x', followOptions),
      y: gsap.quickTo(layer, 'y', followOptions),
    })),
  )
  const setters = [
    gsap.quickTo(cursor.querySelector('.cursor__h'), 'y', { duration: 0.08 }),
    gsap.quickTo(cursor.querySelector('.cursor__v'), 'x', { duration: 0.08 }),
    gsap.quickTo(point, 'x', { duration: 0.16 }),
    gsap.quickTo(point, 'y', { duration: 0.16 }),
  ]
  const events = new AbortController()
  const { signal } = events
  const magnet = createMagnet(gsap)
  let layersMoving = false

  function resetLayers() {
    if (!layersMoving) return
    for (const layer of layers) {
      layer.x(0)
      layer.y(0)
    }
    layersMoving = false
  }

  function hideCrosshair() {
    cursor.classList.remove('is-visible', 'is-down')
    root.classList.remove('has-cursor')
  }

  function reset() {
    hideCrosshair()
    resetLayers()
    magnet.release()
  }

  window.addEventListener('pointermove', event => {
    if (event.pointerType !== 'mouse' || document.querySelector('.shortcut-dialog[open]') ||
        root.classList.contains('opening')) {
      reset()
      return
    }

    const cursorLayer = document.querySelector(':is(.team-dialog, .project-dialog)[open]') ?? cursorHome
    if (cursor.parentElement !== cursorLayer) cursorLayer.append(cursor)

    const { clientX: x, clientY: y } = event
    const insideHero = event.target.closest('.hero')
    if (insideHero) {
      const bounds = hero.getBoundingClientRect()
      const horizontal = (x - bounds.left) / bounds.width * 2 - 1
      const vertical = (y - bounds.top) / bounds.height * 2 - 1
      for (const layer of layers) {
        layer.x(-horizontal * layer.depth)
        layer.y(-vertical * layer.depth)
      }
      layersMoving = true
    } else {
      resetLayers()
    }

    const serviceIcon = event.target.closest('.service__tag')?.querySelector('.service__tile')
    const next = serviceIcon ?? event.target.closest('.button, .circle-button, .team-close, .work-card')
    magnet.move(next, x, y, next?.matches('.work-card') ? 0.22 : 1)

    root.classList.add('has-cursor')
    cursor.classList.add('is-visible')
    setters[0](y)
    setters[1](x)
    setters[2](x)
    setters[3](y)
    const interactive = event.target.closest('a, button, summary, [data-cursor]')
    cursor.classList.toggle('is-hover', Boolean(interactive))
    label.textContent = interactive
      ? interactive.dataset.cursor ?? (interactive.target === '_blank' ? 'visit ↗' : interactive.tagName === 'A' ? 'open →' : 'click')
      : `x ${String(x).padStart(4, '0')} · y ${String(y).padStart(4, '0')}`
  }, { signal, passive: true })
  for (const dialog of document.querySelectorAll('.team-dialog, .project-dialog')) {
    dialog.addEventListener('close', () => {
      reset()
      cursorHome.append(cursor)
    }, { signal })
  }
  root.addEventListener('pointerleave', reset, { signal })
  window.addEventListener('blur', reset, { signal })
  window.addEventListener('pointerdown', () => cursor.classList.add('is-down'), { signal })
  window.addEventListener('pointerup', () => cursor.classList.remove('is-down'), { signal })

  return () => {
    events.abort()
    cursorHome.append(cursor)
    reset()
  }
}
