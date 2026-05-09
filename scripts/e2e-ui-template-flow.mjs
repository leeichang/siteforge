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
    await page.getByRole('button', { name: '註冊' }).click()
    await page.locator('input[type="email"]').fill(email)
    await page.locator('input[type="password"]').fill(password)
    await page.locator('input[type="text"]').fill('UI E2E Tester')
    await Promise.all([
      page.waitForURL(`${baseUrl}/`, { timeout: 20000 }),
      page.getByRole('button', { name: '註冊' }).last().click()
    ])
    await page.waitForLoadState('networkidle')
    await page.getByText(/No projects yet|Create your first project|Create New Site/).first().waitFor({ timeout: 20000 })
    await screenshot(page, '02-dashboard')

    log('open create modal')
    await page.getByText(/Create New Site|Create your first project/).last().click()
    await page.locator('input[placeholder*="Pebisnis"]').fill('')
    await page.locator('input[placeholder*="描述"]').fill('UI flow test from Stitch retail template.')
    const siteTemplatePreviews = await page.locator('.site-template-picker .template-preview').count()
    if (siteTemplatePreviews < 5) throw new Error(`expected site template previews, got ${siteTemplatePreviews}`)
    const sitePreviewImages = await page.locator('.site-template-picker img.template-preview-image').count()
    if (sitePreviewImages < 4) throw new Error(`expected real site template preview images, got ${sitePreviewImages}`)
    await waitForLoadedImages(page, '.site-template-picker img.template-preview-image', 4)
    await page.getByRole('button', { name: /零售業品牌網站/ }).click()
    await screenshot(page, '03-create-template-site')

    log('create template website')
    await Promise.all([
      page.waitForURL(/\/sites\//, { timeout: 30000 }),
      page.getByRole('button', { name: /套用樣板並進入工作區/ }).click()
    ])
    await page.waitForLoadState('networkidle')
    await page.waitForFunction(
      () => document.querySelectorAll('.page-row:not(.page-row-head)').length >= 5,
      null,
      { timeout: 30000 }
    )
    await screenshot(page, '04-workspace')

    if (await page.locator('.sf-sidebar').count()) throw new Error('global dashboard sidebar is visible in workspace')
    if (await page.locator('.workspace-nav').count()) throw new Error('workspace still renders a left navigation sidebar')
    const pageRows = await page.locator('.page-row:not(.page-row-head)').count()
    if (pageRows < 5) throw new Error(`expected template website pages, got ${pageRows}`)
    const buttonChrome = await page.locator('.content-actions .sf-button').first().evaluate((button) => {
      const style = getComputedStyle(button)
      return {
        radius: style.borderRadius,
        height: style.minHeight,
        background: style.backgroundColor
      }
    })
    if (buttonChrome.radius === '0px' || buttonChrome.height === '0px') {
      throw new Error(`workspace action button is not styled: ${JSON.stringify(buttonChrome)}`)
    }

    log('open page template modal')
    await page.getByRole('button', { name: 'AI 產生頁面' }).click()
    const pageTemplatePreviews = await page.locator('.page-template-picker .template-preview').count()
    if (pageTemplatePreviews < 7) throw new Error(`expected page template previews, got ${pageTemplatePreviews}`)
    const pagePreviewImages = await page.locator('.page-template-picker img.template-preview-image').count()
    if (pagePreviewImages < 6) throw new Error(`expected real page template preview images, got ${pagePreviewImages}`)
    await waitForLoadedImages(page, '.page-template-picker img.template-preview-image', 6)
    await page.getByRole('button', { name: /DPP 顯示網頁/ }).click()
    await screenshot(page, '05-create-template-page')

    log('create DPP template page')
    await Promise.all([
      page.waitForURL(/\/editor\//, { timeout: 30000 }),
      page.getByRole('button', { name: /生成並打開編輯器/ }).click()
    ])
    await page.waitForLoadState('networkidle')

    await page.frameLocator('.gjs-frame').locator('body').getByText(/DPP|Digital Product Passport|Product Identity/).first().waitFor({ timeout: 30000 })
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
