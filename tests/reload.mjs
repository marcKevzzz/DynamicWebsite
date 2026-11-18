// Run on the signed-in portfolio with a Playwright page.
export async function checkReload(page) {
  for (const fraction of [0.5, 1]) {
    await page.waitForFunction(() => !document.documentElement.classList.contains('opening'))
    await page.evaluate(fraction => window.scrollTo(0, document.documentElement.scrollHeight * fraction), fraction)
    await page.waitForFunction(() => scrollY > innerHeight)
    await page.reload()
    await page.waitForFunction(() => !document.documentElement.classList.contains('opening'))
    await page.evaluate(() => new Promise(resolve => requestAnimationFrame(() => requestAnimationFrame(resolve))))
    if (await page.evaluate(() => scrollY !== 0)) throw Error('Reload did not return to the hero')
  }
}
