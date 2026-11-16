export function attachProfiles(section, gsap, ScrollTrigger, reducedMotion, lockPage) {
  const dialog = section.querySelector('.team-dialog')
  if (!dialog?.showModal) return

  const roster = [...section.querySelectorAll('.team-person, .work-card')]
  const profiles = section.querySelector('.team-profiles')
  const cards = [...profiles.children]
  const scroller = dialog.querySelector('.team-scroll')
  const track = dialog.querySelector('.team-track')
  const close = dialog.querySelector('[data-team-close]')
  const title = document.getElementById(dialog.getAttribute('aria-labelledby'))
  const others = dialog.querySelector('.team-others')
  const related = [...others.querySelectorAll('a')]
  let opener
  let card
  let chapter = -1
  let motion
  let scrub
  let entrance
  let exit

  const verticalDistance = () => scroller.scrollHeight - scroller.clientHeight
  const horizontalDistance = () => track.scrollWidth - track.clientWidth
  const mobileDetails = matchMedia('(max-width: 767px)')
  const last = 2

  function renderProgress(progress) {
    close.style.setProperty('--team-progress', `${gsap.utils.clamp(0, 1, progress)}turn`)
    if (!card) return
    chapter = mobileDetails.matches ? 0 : Math.round(progress * last)
    const viewport = track.getBoundingClientRect()
    for (const [i, panel] of [...card.children].entries()) {
      const bounds = panel.getBoundingClientRect()
      if (mobileDetails.matches) {
        panel.inert = bounds.bottom <= viewport.top || bounds.top >= viewport.bottom
        if (bounds.top <= viewport.top + 1) chapter = i
      } else {
        panel.inert = bounds.right <= viewport.left || bounds.left >= viewport.right
      }
    }
  }

  function goTo(index, animate = true) {
    const behavior = animate && !reducedMotion.matches ? 'smooth' : 'instant'
    const progress = gsap.utils.clamp(0, last, index) / last
    if (mobileDetails.matches) {
      const panel = card.children[Math.round(progress * last)]
      const top = track.scrollTop + panel.getBoundingClientRect().top - track.getBoundingClientRect().top
      track.scrollTo({ left: 0, top, behavior })
    } else if (scrub) {
      scroller.scrollTo({ top: progress * verticalDistance(), behavior })
      if (behavior === 'instant') {
        ScrollTrigger.update()
        scrub.scrollTrigger.getTween()?.pause()
        scrub.progress(progress)
        track.scrollLeft = progress * horizontalDistance()
      }
    } else {
      track.scrollTo({ left: progress * horizontalDistance(), top: 0, behavior })
    }
    if (behavior === 'instant') renderProgress(mobileDetails.matches ? track.scrollTop / (track.scrollHeight - track.clientHeight || 1) : progress)
  }

  function showProfile(index, enteringDialog = false) {
    if (exit) return
    entrance?.progress(1)
    if (card) {
      for (const panel of card.children) panel.inert = false
      profiles.append(card)
    }
    card = cards[index]
    card.append(others)
    track.append(card)
    for (const panel of card.children) panel.scrollTop = 0
    related.forEach((link, i) => { link.hidden = i === index })
    title.textContent = roster[index].getAttribute('aria-label')
    close.focus({ preventScroll: true })
    chapter = -1
    goTo(0, false)
    if (!reducedMotion.matches) {
      const surface = enteringDialog ? dialog : scroller
      entrance = gsap.fromTo(surface, { yPercent: 100 }, {
        yPercent: 0, duration: 0.7, ease: 'power3.inOut', clearProps: 'transform',
      })
    }
  }

  for (const [index, link] of roster.entries()) {
    link.setAttribute('aria-haspopup', 'dialog')
    link.addEventListener('click', event => {
      if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return
      event.preventDefault()
      opener = link
      title.textContent = link.getAttribute('aria-label')
      lockPage(true)
      dialog.showModal()
      showProfile(index, true)
      motion = gsap.matchMedia()
      motion.add('(min-width: 768px) and (min-height: 580px) and (prefers-reduced-motion: no-preference)', () => {
        const current = chapter
        const playhead = { progress: 0 }
        dialog.classList.add('is-scrubbed')
        scrub = gsap.to(playhead, {
          progress: 1,
          ease: 'none',
          scrollTrigger: { scroller, start: 0, end: verticalDistance, scrub: 0.65 },
          onUpdate: () => {
            track.scrollLeft = playhead.progress * horizontalDistance()
            renderProgress(playhead.progress)
          },
        })
        goTo(current, false)
        return () => {
          scrub = undefined
          dialog.classList.remove('is-scrubbed')
          if (dialog.open) goTo(chapter, false)
        }
      })
    })
  }

  profiles.hidden = !cards.some(profile => location.hash === '#' + profile.id)
  track.addEventListener('wheel', event => {
    if (!scrub || event.ctrlKey) return
    const mode = event.deltaMode
    const sideways = Math.abs(event.deltaX) > Math.abs(event.deltaY)
    const delta = sideways ? event.deltaX : event.deltaY
    const panel = event.target.closest('.team-chapter')
    if (!sideways && panel && (delta < 0 ? panel.scrollTop > 0 : panel.scrollTop + panel.clientHeight < panel.scrollHeight - 1)) return
    event.preventDefault()
    const units = mode === 1 ? 16 : mode === 2 ? scroller.clientHeight : 1
    scroller.scrollTop += delta * units
  }, { passive: false })
  track.addEventListener('scroll', () => {
    if (scrub) return
    const distance = mobileDetails.matches ? track.scrollHeight - track.clientHeight : horizontalDistance()
    const position = mobileDetails.matches ? track.scrollTop : track.scrollLeft
    if (distance > 0) renderProgress(position / distance)
  }, { passive: true })
  track.addEventListener('click', event => {
    const target = event.target.closest('[data-team-chapter]')
    if (!target) return
    close.focus({ preventScroll: true })
    goTo(Number(target.dataset.teamChapter))
  })
  related.forEach((link, index) => link.addEventListener('click', event => {
    if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return
    event.preventDefault()
    showProfile(index)
  }))
  function requestClose() {
    if (exit) return
    if (reducedMotion.matches) {
      dialog.close()
      return
    }
    entrance?.kill()
    exit = gsap.to(dialog, {
      yPercent: -100,
      duration: 0.6,
      ease: 'power3.inOut',
      onComplete: () => dialog.close(),
    })
  }

  close.addEventListener('click', requestClose)
  dialog.addEventListener('cancel', event => {
    event.preventDefault()
    requestClose()
  })
  dialog.addEventListener('keydown', event => {
    if (exit || event.altKey || event.ctrlKey || event.metaKey || event.shiftKey) return
    if (event.key !== 'ArrowLeft' && event.key !== 'ArrowRight') return
    event.preventDefault()
    close.focus({ preventScroll: true })
    goTo(chapter + (event.key === 'ArrowLeft' ? -1 : 1))
  })
  dialog.addEventListener('close', () => {
    motion?.revert()
    entrance?.kill()
    exit?.kill()
    entrance = exit = undefined
    gsap.set([dialog, scroller], { clearProps: 'transform' })
    dialog.append(others)
    for (const panel of card.children) panel.inert = false
    profiles.append(card)
    card = undefined
    lockPage(false)
    opener?.focus({ preventScroll: true })
  })
}
