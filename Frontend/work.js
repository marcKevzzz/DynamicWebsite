export function animateWorkGallery(gsap, ScrollTrigger) {
  const work = document.querySelector('#work')
  const counter = work.querySelector('.work-current')
  const items = [...work.querySelectorAll('.work-item')]

  function updateCount() {
    let current = 0
    let closest = Infinity
    for (const [index, item] of items.entries()) {
      const bounds = item.getBoundingClientRect()
      const distance = Math.abs(bounds.top + bounds.height / 2 - innerHeight / 2)
      if (distance < closest) {
        current = index
        closest = distance
      }
    }
    counter.textContent = String(current + 1).padStart(2, '0')
  }

  ScrollTrigger.create({ trigger: work, start: 'top bottom', end: 'bottom top', onUpdate: updateCount, onRefresh: updateCount })
  gsap.matchMedia().add('(prefers-reduced-motion: no-preference)', () => {
    for (const item of items) {
      gsap.fromTo(item, { opacity: 0 }, {
        opacity: 1, duration: 0.8, ease: 'power2.out', clearProps: 'opacity',
        scrollTrigger: { trigger: item, start: 'top 90%', once: true },
      })
      const preview = item.querySelector('.work-preview')
      gsap.fromTo(preview, { yPercent: -8 }, {
        yPercent: 8, ease: 'none',
        scrollTrigger: { trigger: item, start: 'top bottom', end: 'bottom top', scrub: 1.1 },
      })
    }
  })
}
