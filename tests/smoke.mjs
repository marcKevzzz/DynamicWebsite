// Run against the MVC home page with the Codex browser tab API.
export async function smoke(tab) {
  const assert = (condition, message) => { if (!condition) throw new Error(message) }
  const ui = tab.playwright
  const read = fn => ui.evaluate(fn)

  assert(await read(() => document.querySelectorAll('h1').length === 1 &&
    ['about', 'work', 'services', 'team', 'contact'].every(id => document.getElementById(id))), 'MVC renders all portfolio sections')
  await ui.locator('.opening-cover').waitFor({ state: 'hidden', timeoutMs: 10000 })

  await ui.locator('.header').getByRole('link', { name: 'dcd. home' }).click()
  await ui.locator('.header:not(.is-scrolled)').waitFor({ state: 'visible', timeoutMs: 10000 })
  const workAction = ui.locator('.header-cta')
  await workAction.waitFor({ state: 'hidden' })
  assert(await read(() => document.querySelector('.header nav .nav-link:nth-child(2)').getAttribute('href') === '#about' &&
    document.querySelector('#about .about-note p') && document.title === 'dcd.studio'), 'About is second and the studio title is present')

  const hero = ui.locator('.hero')
  assert(await hero.getByRole('link', { name: 'Meet the team' }).isVisible(), 'Hero has the team link')
  assert(await read(() => document.querySelectorAll('.hero-actions a').length === 1 &&
    !document.querySelector('.header [data-menu], .hero-grid, .work-intro, .project-details, .team-intro p')), 'Requested removals')
  assert(await read(() => getComputedStyle(document.querySelector('.hero')).getPropertyValue('--bg') ===
    getComputedStyle(document.querySelector('.portfolio')).getPropertyValue('--bg')), 'Hero uses the site palette')
  await ui.getByRole('navigation', { name: 'Main navigation' }).getByRole('link', { name: 'About' }).click()
  await workAction.waitFor({ state: 'visible', timeoutMs: 10000 })
  const menu = ui.locator('.header').getByRole('link', { name: 'dcd. home' })
  const search = ui.getByRole('combobox')
  await menu.press('Control+k')
  await search.fill('zzzz')
  assert(await ui.getByText('No matches.', { exact: false }).isVisible(), 'Empty search feedback')
  await search.press('Escape')
  assert(await read(() => !document.querySelector('.shortcut-dialog').open && document.activeElement?.classList.contains('brand')), 'Escape restores focus')
  await menu.press('Control+k')
  await search.fill('Toggle light')
  await search.press('Enter')
  assert(await read(() => !document.querySelector('.shortcut-dialog').open), 'Enter closes the palette')
  assert(await read(() => document.querySelector('.header').classList.contains('is-scrolled')), 'Header expands on scroll')
  const theme = ui.getByRole('button', { name: /Switch to (dark|light) theme/ })
  const switchedLabel = await theme.getAttribute('aria-label')
  const previousBackground = await hero.evaluate(el => getComputedStyle(el).getPropertyValue('--bg'))
  await theme.click()
  await ui.getByRole('button', { name: switchedLabel === 'Switch to dark theme' ? 'Switch to light theme' : 'Switch to dark theme' }).waitFor({ state: 'visible' })

  assert(await hero.evaluate((el, previous) => getComputedStyle(el).getPropertyValue('--bg') !== previous, previousBackground), 'Hero changes with the theme')
  assert(await read(() => document.documentElement.scrollWidth <= document.documentElement.clientWidth), 'No horizontal overflow')
  assert(await read(() => getComputedStyle(document.querySelector('.header')).position === 'sticky'), 'Sticky header')
  assert(await read(() => document.querySelectorAll('.work-card').length === 8), 'All eight projects are rendered')
  const project = ui.getByRole('link', { name: 'SchedSnap', exact: true })
  await project.click()
  const projectPage = ui.getByRole('dialog', { name: 'SchedSnap', exact: true })
  await projectPage.getByRole('heading', { name: 'SchedSnap' }).waitFor({ state: 'visible' })
  await projectPage.getByRole('button', { name: 'Tools of the trade' }).press('Enter')
  await projectPage.getByRole('link', { name: 'Visit project' }).waitFor({ state: 'visible' })
  assert(await projectPage.getByRole('link', { name: 'Visit project' }).getAttribute('href') === 'https://schedsnap.chrbuilds.dev/', 'Project details link to the supplied destination')
  await projectPage.getByRole('button', { name: 'Close project' }).press('Escape')
  await projectPage.waitFor({ state: 'hidden' })
  assert(await project.evaluate(el => el === document.activeElement), 'Closing the project restores focus to its card')
  assert(await read(() => document.querySelectorAll('.service-row').length === 3 && document.querySelectorAll('#team .team-card').length === 4), 'Services and roster are rendered')
  const member = ui.getByRole('link', { name: 'Pixyz', exact: true })
  await member.click()
  const profile = ui.getByRole('dialog', { name: 'Pixyz', exact: true })
  assert(await profile.getByRole('heading', { name: 'Pixyz' }).isVisible(), 'Selected teammate opens')
  assert(await profile.evaluate(el => {
    const track = el.querySelector('.team-track')
    return track.querySelector('.team-profile').clientHeight === track.clientHeight
  }), 'The portrait cannot make its profile taller than the viewport')
  await profile.getByRole('button', { name: 'Tools of the trade', exact: true }).click()
  await profile.getByRole('heading', { name: 'Tools of the trade', exact: true }).waitFor({ state: 'visible' })
  await profile.getByRole('button', { name: 'Other team members' }).click()
  await profile.getByRole('heading', { name: 'Other team members' }).waitFor({ state: 'visible' })
  assert(await profile.getByRole('link', { name: 'Pixyz', exact: true }).count() === 0, 'The final chapter excludes the active teammate')
  await profile.getByRole('link', { name: 'Leo', exact: true }).click()
  assert(await profile.getByRole('button', { name: 'Close profile' }).evaluate(el => el.style.getPropertyValue('--team-progress')) === '0turn', 'The close indicator resets for each teammate')
  const nextProfile = ui.getByRole('dialog', { name: 'Leo', exact: true })
  assert(await nextProfile.getByRole('heading', { name: 'Leo' }).isVisible(), 'Next teammate resets to the overview')
  await nextProfile.getByRole('button', { name: 'Close profile' }).press('Escape')
  await nextProfile.waitFor({ state: 'hidden' })
  assert(await member.evaluate(el => el === document.activeElement), 'Closing a profile restores focus to the roster')
  return 'Portfolio smoke checks passed'
}
