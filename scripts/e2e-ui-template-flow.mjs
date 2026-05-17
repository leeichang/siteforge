import { createRequire } from 'node:module'
import { mkdirSync } from 'node:fs'
import path from 'node:path'

const require = createRequire('/Users/leeichang/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/')
const { chromium } = require('playwright')

const baseUrl = process.env.SITEFORGE_BASE_URL || 'http://localhost:5010'
const artifactDir = path.resolve('artifacts/e2e-ui')
mkdirSync(artifactDir, { recursive: true })

const stamp = Date.now()
const email = `ui-${stamp}@siteforge.test`
const password = 'E2ePassw0rd!'
const siteName = `UI Template ${stamp}`

function log(step, detail = '') {
  console.log(`UI-E2E ${step}${detail ? ` - ${detail}` : ''}`)
}

async function screenshot(page, name) {
  await page.screenshot({ path: path.join(artifactDir, `${name}.png`), fullPage: true })
}

async function waitForAny(page, selectors, timeout = 30000) {
  const start = Date.now()
  while (Date.now() - start < timeout) {
    for (const selector of selectors) {
      const locator = typeof selector === 'string' ? page.locator(selector).first() : selector.first()
      if (await locator.isVisible().catch(() => false)) return locator
    }
    await page.waitForTimeout(250)
  }
  throw new Error(`timed out waiting for any selector: ${selectors.join(', ')}`)
}

async function clickFirstVisible(page, selectors) {
  for (const selector of selectors) {
    const locator = typeof selector === 'string' ? page.locator(selector) : selector
    const count = await locator.count().catch(() => 0)
    for (let index = 0; index < count; index += 1) {
      const item = locator.nth(index)
      if (await item.isVisible().catch(() => false)) {
        await item.click()
        return item
      }
    }
  }
  throw new Error(`no visible target found for: ${selectors.map(String).join(', ')}`)
}

async function waitForLoadedImages(page, selector, minCount) {
  await page.waitForFunction(
    ({ selector, minCount }) => Array.from(document.querySelectorAll(selector))
      .filter((image) => image.complete && image.naturalWidth > 0 && image.naturalHeight > 0)
      .length >= minCount,
    { selector, minCount },
    { timeout: 30000 }
  )
}

async function resizePane(page, handleSelector, deltaX, measuredSelector) {
  const before = await page.locator(measuredSelector).boundingBox()
  const handle = await page.locator(handleSelector).boundingBox()
  if (!before || !handle) throw new Error(`unable to measure ${handleSelector} / ${measuredSelector}`)
  const y = handle.y + Math.min(220, Math.max(40, handle.height / 2))
  const startX = handle.x + (handle.width / 2)
  await page.mouse.move(startX, y)
  await page.mouse.down()
  await page.mouse.move(startX + deltaX, y, { steps: 10 })
  await page.mouse.up()
  await page.waitForTimeout(250)
  const after = await page.locator(measuredSelector).boundingBox()
  if (!after) throw new Error(`unable to measure ${measuredSelector} after resize`)
  return { before, after }
}

async function main() {
  const browser = await chromium.launch({ headless: true })
  const page = await browser.newPage({ viewport: { width: 2048, height: 980 } })
  const errors = []

  page.on('console', (message) => {
    if (message.type() === 'error') errors.push(`console: ${message.text()}`)
  })
  page.on('pageerror', (error) => errors.push(`pageerror: ${error.message}`))

  try {
    log('open login')
    await page.goto(`${baseUrl}/login`, { waitUntil: 'networkidle' })
    await screenshot(page, '01-login')

    log('register via UI', email)
    await page.getByRole('button', { name: /註冊|Register/ }).click()
    await page.locator('input[type="email"]').fill(email)
    await page.locator('input[type="password"]').fill(password)
    await page.locator('input[type="text"]').fill('UI E2E Tester')
    await Promise.all([
      page.waitForURL(/\/$/, { timeout: 30000 }).catch(() => null),
      page.getByRole('button', { name: /註冊|Register/ }).last().click()
    ])
    await waitForAny(page, ['.sf-dashboard', '.sf-dash-empty', 'text=My Projects', 'text=我的專案'], 30000)
    await screenshot(page, '02-dashboard')

    log('open create modal')
    await clickFirstVisible(page, [
      page.getByRole('button', { name: /Create your first project|建立第一個專案|New Project|Create New Site|建立新網站/ }),
      '.sf-dash-card-new',
      '.sf-dash-empty button'
    ])
    await waitForAny(page, ['.sf-modal', '[role="dialog"]'], 10000)
    const modalInputs = page.locator('.sf-modal input, [role="dialog"] input')
    await modalInputs.nth(0).fill(siteName)
    await modalInputs.nth(1).fill('UI flow test from the current SiteForge template picker.')

    const siteTemplatePreviews = await page.locator('.site-template-picker .template-preview').count()
    if (siteTemplatePreviews < 2) throw new Error(`expected site template previews, got ${siteTemplatePreviews}`)
    const sitePreviewImages = await page.locator('.site-template-picker img.template-preview-image').count()
    if (sitePreviewImages > 0) {
      await waitForLoadedImages(page, '.site-template-picker img.template-preview-image', Math.min(sitePreviewImages, 4))
    }
    await clickFirstVisible(page, [
      page.locator('.site-template-picker .template-card:not(.blank)'),
      page.locator('.site-template-picker button:not(.blank)')
    ])
    await screenshot(page, '03-create-template-site')

    log('create template website')
    await Promise.all([
      page.waitForURL(/\/sites\//, { timeout: 60000 }),
      clickFirstVisible(page, [
        page.getByRole('button', { name: /套用樣板並進入工作區|Apply template and open workspace|建立並進入工作區|Create and open workspace/ }),
        '.sf-modal .sf-btn-primary'
      ])
    ])
    await waitForAny(page, ['.sf-workspace', '.sf-ws-card'], 30000)
    await screenshot(page, '04-workspace')

    if (await page.locator('.sf-sidebar').count()) throw new Error('global dashboard sidebar is visible in workspace')
    if (await page.locator('.workspace-nav').count()) throw new Error('workspace still renders a left navigation sidebar')
    const pageCards = await page.locator('.sf-ws-card').count()
    if (pageCards < 5) throw new Error(`expected template website pages, got ${pageCards}`)

    log('verify workspace tabs and page menu')
    await clickFirstVisible(page, [page.getByRole('button', { name: /^Templates$/ }), 'button:has-text("Templates")'])
    await waitForAny(page, ['.sf-ws-templates-grid', 'text=Available Templates'], 10000)
    await clickFirstVisible(page, [page.getByRole('button', { name: /^Theme$/ }), 'button:has-text("Theme")'])
    await waitForAny(page, ['.sf-ws-theme-placeholder', 'text=Theme settings coming soon'], 10000)
    await clickFirstVisible(page, [page.getByRole('button', { name: /^Pages$/ }), 'button:has-text("Pages")'])
    await page.waitForTimeout(250)
    const firstCard = page.locator('.sf-ws-card').first()
    let openedEditorFromMenu = false
    await firstCard.hover()
    const menuButton = firstCard.locator('.sf-ws-card-menu')
    if (await menuButton.isVisible().catch(() => false)) {
      await menuButton.click()
      await waitForAny(page, ['.sf-ws-menu', 'text=Edit'], 10000)
      await screenshot(page, '05-workspace-page-menu')
      await clickFirstVisible(page, [page.locator('.sf-ws-menu button').filter({ hasText: /Edit/ })])
      openedEditorFromMenu = true
    }

    log('open editor')
    if (!openedEditorFromMenu) {
      await page.locator('.sf-ws-card').first().click()
    }
    await page.waitForURL(/\/editor\//, { timeout: 60000 })
    await waitForAny(page, ['.studio-editor', '#gjs'], 30000)
    await page.waitForTimeout(3000)
    await screenshot(page, '06-editor')

    if (await page.locator('.sf-sidebar').count()) throw new Error('global dashboard sidebar is visible in editor')
    const editorWidth = await page.locator('.studio-editor').evaluate((element) => Math.round(element.getBoundingClientRect().width))
    const viewportWidth = page.viewportSize()?.width || 0
    if (viewportWidth && Math.abs(editorWidth - viewportWidth) > 2) {
      throw new Error(`editor is not full screen: editor=${editorWidth}, viewport=${viewportWidth}`)
    }
    const treeNodes = await page.locator('.project-tree .tree-node').count()
    if (treeNodes < 5) throw new Error(`expected editor project tree nodes, got ${treeNodes}`)
    const canvasVisible = await page.locator('#gjs').isVisible()
    if (!canvasVisible) throw new Error('editor canvas is not visible')

    log('verify editor panels')
    const railButtons = page.locator('.editor-rail button')
    if (await railButtons.count() < 6) throw new Error('expected editor rail buttons')
    await railButtons.nth(0).click()
    await waitForAny(page, ['#blocks-panel', 'text=Blocks'], 10000)
    await railButtons.nth(2).click()
    await waitForAny(page, ['text=Global Styles', 'text=Colors'], 10000)
    await railButtons.nth(3).click()
    await waitForAny(page, ['text=Register URL', '.asset-grid'], 10000)
    await railButtons.nth(5).click()
    await waitForAny(page, ['.ai-panel', 'text=AI Assistant'], 10000)

    log('resize editor panes')
    await page.locator('.left-resizer').waitFor({ timeout: 10000 })
    const leftResize = await resizePane(page, '.left-resizer', 90, '.studio-left-panel')
    if (leftResize.after.width < leftResize.before.width + 60) {
      throw new Error(`left editor pane did not resize: before=${leftResize.before.width}, after=${leftResize.after.width}`)
    }
    const rightResize = await resizePane(page, '.right-resizer', -80, '.studio-right-panel')
    if (rightResize.after.width < rightResize.before.width + 50) {
      throw new Error(`right editor pane did not resize: before=${rightResize.before.width}, after=${rightResize.after.width}`)
    }
    await screenshot(page, '07-editor-resized')

    if (errors.length) {
      throw new Error(`browser errors:\n${errors.join('\n')}`)
    }

    log('passed', artifactDir)
  } finally {
    await browser.close()
  }
}

main().catch((error) => {
  console.error(error)
  process.exit(1)
})
