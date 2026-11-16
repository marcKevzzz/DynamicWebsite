export function playOpening(gsap, reducedMotion) {
  const root = document.documentElement
  if (!root.classList.contains('opening') || reducedMotion.matches) return

  const hero = document.querySelector('.hero')
  const cover = document.querySelector('.opening-cover')
  const cards = [...hero.querySelectorAll('[data-enter]')]
  const words = hero.querySelectorAll('[data-hero-word]')
  const details = hero.querySelectorAll('[data-hero-detail]')
  const captions = hero.querySelectorAll('.hero-caption')
  const header = document.querySelector('.header')

  const poses = cards.map((card, index) => {
    const rect = card.getBoundingClientRect()
    return {
      x: (innerWidth - rect.width) / 2 - rect.left,
      y: (hero.clientHeight - rect.height) / 2 - (rect.top - hero.getBoundingClientRect().top),
      rotation: Number(card.dataset.introRotation ?? (index - (cards.length - 1) / 2) * 3),
    }
  })

  const intro = gsap.timeline({
    onComplete: () => {
      root.classList.remove('opening', 'opening-nav-expanded')
      gsap.set([cover, ...cards, ...words, ...details, ...captions], { clearProps: 'opacity,visibility,transform' })
    },
  })
  const skip = () => intro.progress(1)
  hero.addEventListener('focusin', skip, { once: true })
  header.addEventListener('focusin', skip, { once: true })

  intro.to(cover, { opacity: 0, duration: 0.38, ease: 'power1.inOut' }, 0.05)
  cards.forEach((card, index) => {
    const pose = poses[index]
    intro.fromTo(card, { ...pose, y: pose.y + 70, opacity: 0, scale: 0.89 },
      { ...pose, opacity: 1, scale: 1, duration: 0.48, ease: 'power3.out' }, 0.21 + index * 0.1)
    intro.to(card, { x: 0, y: 0, rotation: 0, duration: 0.87, ease: 'power4.inOut' }, 0.84 + index * 0.045)
  })
  intro.fromTo(captions, { y: 12, opacity: 0 }, { y: 0, opacity: 1, stagger: 0.07, duration: 0.42 }, 1.47)
  intro.addLabel('copy')
    .fromTo(words, { yPercent: 105, opacity: 0 }, { yPercent: 0, opacity: 1, stagger: 0.09, duration: 0.46, ease: 'power3.out' }, 'copy')
    .fromTo(details, { y: 13, opacity: 0 }, { y: 0, opacity: 1, stagger: 0.12, duration: 0.42, ease: 'power2.out' }, 'copy+=0.24')
    .call(() => root.classList.add('opening-nav-expanded'), [], 'copy')
    .to({}, { duration: 0.25 })
  root.classList.remove('opening-pending')
  reducedMotion.addEventListener('change', event => { if (event.matches) skip() }, { once: true })
}
