import { createRequire } from 'node:module'
import { mkdirSync, rmSync } from 'node:fs'
import { spawnSync } from 'node:child_process'
import path from 'node:path'

const require = createRequire('/Users/leeichang/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/')
const { chromium } = require('playwright')

const baseUrl = process.env.SITEFORGE_BASE_URL || 'http://127.0.0.1:5010'
const rootDir = '/Users/leeichang/Documents/siteforge'
const mediaDir = path.join(rootDir, 'docs/manual/media')
const imageDir = path.join(rootDir, 'docs/manual/images')
const rawDir = path.join(rootDir, 'artifacts/sofa-editor-demo/raw')
const artifactDir = path.join(rootDir, 'artifacts/sofa-editor-demo')
const ffmpeg = process.env.FFMPEG_BIN || '/opt/homebrew/bin/ffmpeg'

const stamp = Date.now()
const email = `sofa-demo-${stamp}@siteforge.test`
const password = 'E2ePassw0rd!'
const siteName = `Sofa Product Demo ${stamp}`

const sofaImageUrl = 'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=1400&auto=format&fit=crop'
const readPause = 6200
const settingPause = 6200
const transitionPause = 1800

mkdirSync(mediaDir, { recursive: true })
mkdirSync(imageDir, { recursive: true })
rmSync(artifactDir, { recursive: true, force: true })
mkdirSync(rawDir, { recursive: true })

function run(command, args) {
  const result = spawnSync(command, args, { stdio: 'inherit' })
  if (result.status !== 0) throw new Error(`${command} failed: ${args.join(' ')}`)
}

async function waitForAny(page, selectors, timeout = 30000) {
  const start = Date.now()
  while (Date.now() - start < timeout) {
    for (const selector of selectors) {
      if (await page.locator(selector).first().isVisible().catch(() => false)) return
    }
    await page.waitForTimeout(250)
  }
  throw new Error(`Timed out waiting for: ${selectors.join(', ')}`)
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
  throw new Error(`No visible target for: ${selectors.map(String).join(', ')}`)
}

async function showStep(page, step, title, details, target) {
  await page.evaluate(({ step, title, details, target }) => {
    let overlay = document.querySelector('#sf-tutorial-step')
    if (!overlay) {
      overlay = document.createElement('div')
      overlay.id = 'sf-tutorial-step'
      overlay.innerHTML = `
        <div class="sf-step-kicker"></div>
        <div class="sf-step-title"></div>
        <ul class="sf-step-details"></ul>
      `
      document.body.appendChild(overlay)

      const style = document.createElement('style')
      style.id = 'sf-tutorial-style'
      style.textContent = `
        #sf-tutorial-step {
          position: fixed;
          left: 222px;
          right: 270px;
          bottom: 18px;
          z-index: 2147483000;
          background: rgba(27, 24, 20, .94);
          color: #fffaf3;
          border: 1px solid rgba(255, 255, 255, .14);
          border-radius: 8px;
          box-shadow: 0 18px 46px rgba(0, 0, 0, .3);
          padding: 14px 18px 15px;
          font-family: Inter, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
          pointer-events: none;
        }
        #sf-tutorial-step .sf-step-kicker {
          color: #ffcf6b;
          font-size: 12px;
          font-weight: 800;
          letter-spacing: .08em;
          text-transform: uppercase;
          margin-bottom: 5px;
        }
        #sf-tutorial-step .sf-step-title {
          font-size: 20px;
          line-height: 1.25;
          font-weight: 800;
          letter-spacing: 0;
          margin-bottom: 8px;
        }
        #sf-tutorial-step ul {
          display: grid;
          grid-template-columns: repeat(2, minmax(0, 1fr));
          gap: 5px 18px;
          margin: 0;
          padding: 0;
          list-style: none;
        }
        #sf-tutorial-step li {
          font-size: 13px;
          line-height: 1.45;
          color: #efe6d8;
          white-space: normal;
        }
        #sf-tutorial-highlight {
          position: fixed;
          z-index: 2147482999;
          border: 3px solid #ffcf6b;
          background: rgba(255, 207, 107, .1);
          border-radius: 8px;
          box-shadow: 0 0 0 9999px rgba(0, 0, 0, .08);
          pointer-events: none;
        }
      `
      document.head.appendChild(style)
    }

    overlay.querySelector('.sf-step-kicker').textContent = `Step ${String(step).padStart(2, '0')}`
    overlay.querySelector('.sf-step-title').textContent = title
    overlay.querySelector('.sf-step-details').innerHTML = details.map((line) => `<li>${line}</li>`).join('')
    const shouldFloatTop = target && target.y > window.innerHeight * .62
    overlay.style.top = shouldFloatTop ? '58px' : ''
    overlay.style.bottom = shouldFloatTop ? 'auto' : '18px'

    let highlight = document.querySelector('#sf-tutorial-highlight')
    if (!highlight) {
      highlight = document.createElement('div')
      highlight.id = 'sf-tutorial-highlight'
      document.body.appendChild(highlight)
    }
    if (target) {
      highlight.style.display = 'block'
      highlight.style.left = `${target.x}px`
      highlight.style.top = `${target.y}px`
      highlight.style.width = `${target.width}px`
      highlight.style.height = `${target.height}px`
    } else {
      highlight.style.display = 'none'
    }
  }, { step, title, details, target })
}

async function scrollCanvasTo(page, top) {
  await page.frameLocator('.gjs-frame').locator('body').evaluate((body, top) => {
    const scroller = body.ownerDocument.scrollingElement || body
    scroller.scrollTo({ top, left: 0, behavior: 'auto' })
  }, top)
}

async function canvasTarget(page, xRatio, yRatio, widthRatio = 0.18, heightRatio = 0.12) {
  const canvas = await page.locator('.gjs-frame').boundingBox()
  if (!canvas) throw new Error('Unable to measure GrapesJS canvas')
  return {
    x: canvas.x + canvas.width * xRatio - canvas.width * widthRatio / 2,
    y: canvas.y + canvas.height * yRatio - canvas.height * heightRatio / 2,
    width: canvas.width * widthRatio,
    height: canvas.height * heightRatio
  }
}

async function slowMove(page, fromX, fromY, toX, toY, steps = 70, delay = 18) {
  for (let index = 0; index <= steps; index += 1) {
    const progress = index / steps
    const eased = progress < 0.5
      ? 2 * progress * progress
      : 1 - Math.pow(-2 * progress + 2, 2) / 2
    await page.mouse.move(
      fromX + (toX - fromX) * eased,
      fromY + (toY - fromY) * eased
    )
    await page.waitForTimeout(delay)
  }
}

async function dragBlockToCanvas(page, blockText, dropXRatio, dropYRatio) {
  await page.locator('#blocks-panel').waitFor({ state: 'visible', timeout: 15000 })
  const source = await page.evaluate((blockText) => {
    const blocks = Array.from(document.querySelectorAll('.gjs-block'))
    const block = blocks.find((element) => {
      const label = element.textContent?.trim()
      const title = element.getAttribute('title')
      return label === blockText || title === blockText
    })
    if (!block) {
      return {
        error: `Block not found: ${blockText}`,
        available: blocks.map((element) => ({
          text: element.textContent?.trim(),
          title: element.getAttribute('title')
        }))
      }
    }
    block.scrollIntoView({ block: 'center', inline: 'center' })
    const rect = block.getBoundingClientRect()
    return { x: rect.x, y: rect.y, width: rect.width, height: rect.height }
  }, blockText)
  if (source.error) throw new Error(`${source.error}; available=${JSON.stringify(source.available)}`)

  await page.waitForTimeout(300)
  const canvas = await page.locator('.gjs-frame').boundingBox()
  if (!canvas) throw new Error(`Unable to measure canvas for ${blockText}`)
  const startX = source.x + source.width / 2
  const startY = source.y + source.height / 2
  const endX = canvas.x + canvas.width * dropXRatio
  const endY = canvas.y + canvas.height * dropYRatio
  await slowMove(page, startX - 34, startY - 22, startX, startY, 34, 20)
  await page.waitForTimeout(950)
  await page.mouse.down()
  await page.waitForTimeout(550)
  await slowMove(page, startX, startY, (startX + endX) / 2, (startY + endY) / 2, 58, 20)
  await page.waitForTimeout(260)
  await slowMove(page, (startX + endX) / 2, (startY + endY) / 2, endX, endY, 58, 20)
  await page.waitForTimeout(600)
  await page.mouse.up()
  await page.waitForTimeout(transitionPause)
}

async function screenshotStep(page, name) {
  await page.screenshot({
    path: path.join(imageDir, name),
    fullPage: false
  })
}

async function updateCanvas(page, stage) {
  await page.frameLocator('.gjs-frame').locator('body').evaluate((body, { stage, sofaImageUrl }) => {
    const styleId = 'sofa-product-demo-style'
    const doc = body.ownerDocument
    doc.querySelector(`#${styleId}`)?.remove()
    const style = doc.createElement('style')
    style.id = styleId
    style.textContent = `
      body { margin: 0; font-family: Inter, system-ui, -apple-system, BlinkMacSystemFont, sans-serif; background: #f6f2ec; color: #2d2924; }
      .sofa-page { min-height: 100vh; }
      .sofa-hero { display: grid; grid-template-columns: .92fr 1.08fr; gap: 48px; align-items: center; padding: 72px; background: #f6f2ec; }
      .sofa-copy { min-width: 0; }
      .eyebrow { margin: 0 0 14px; color: #8d6b47; font-size: 13px; font-weight: 800; letter-spacing: .12em; }
      h1 { margin: 0; font-size: 64px; line-height: 1.02; letter-spacing: 0; color: #241f1a; }
      .lead { margin: 24px 0 0; max-width: 560px; color: #6c6258; font-size: 20px; line-height: 1.8; }
      .sofa-actions { display: flex; gap: 14px; align-items: center; margin-top: 34px; flex-wrap: wrap; }
      .sofa-actions a, .sofa-actions button { border: 1px solid #2d2924; border-radius: 4px; padding: 13px 22px; font-size: 15px; font-weight: 700; text-decoration: none; cursor: pointer; }
      .sofa-actions a { background: #2d2924; color: #fff; }
      .sofa-actions button { background: transparent; color: #2d2924; }
      figure { margin: 0; overflow: hidden; border-radius: 8px; box-shadow: 0 24px 70px rgba(53, 44, 36, .18); background: #e4ded6; min-height: 420px; }
      img { display: block; width: 100%; height: 560px; object-fit: cover; }
      .placeholder { display: grid; place-items: center; min-height: 420px; border: 2px dashed #c9bba9; color: #8d6b47; font-weight: 800; }
      .sofa-details { display: grid; grid-template-columns: repeat(3, 1fr); gap: 1px; padding: 0 72px 72px; }
      .sofa-details article { background: #fffaf3; border: 1px solid rgba(45, 41, 36, .08); padding: 28px; min-height: 150px; }
      .sofa-details strong { display: block; margin-bottom: 12px; font-size: 18px; color: #241f1a; }
      .sofa-details span { color: #6c6258; line-height: 1.7; }
      @media (max-width: 760px) {
        .sofa-hero { grid-template-columns: 1fr; padding: 32px; }
        h1 { font-size: 42px; }
        img { height: 360px; }
        .sofa-details { grid-template-columns: 1fr; padding: 0 32px 48px; }
      }
    `

    const heroText = stage >= 2
      ? `
        <p class="eyebrow">NEW COLLECTION</p>
        <h1>Nordic<br />Lounge<br />Sofa</h1>
        <p class="lead">一張為城市客廳設計的沙發。低背、深坐、可拆洗布套，讓小坪數也能擁有安靜而舒服的停留感。</p>
      `
      : `<div class="placeholder">左欄：稍後放 Text</div>`

    const heroImage = stage >= 3
      ? `<img src="${sofaImageUrl}" alt="綠色北歐沙發" />`
      : `<div class="placeholder">右欄：稍後放 Image</div>`

    const buttons = stage >= 4
      ? `
        <div class="sofa-actions">
          <a href="#details">查看規格</a>
          <button type="button">預約試坐</button>
        </div>
      `
      : ''

    const details = stage >= 6
      ? `
        <section id="details" class="sofa-details">
          <article><strong>材質</strong><span>耐磨亞麻混紡布、實木框架、高密度泡棉</span></article>
          <article><strong>尺寸</strong><span>220 x 92 x 78 cm，適合 3 人座客廳</span></article>
          <article><strong>特色</strong><span>可拆洗布套、模組化腳椅、10 年框架保固</span></article>
        </section>
      `
      : stage >= 5
        ? `
          <section id="details" class="sofa-details">
            <article><strong>規格卡 1</strong><span>稍後拖 Text 設定材質</span></article>
            <article><strong>規格卡 2</strong><span>稍後拖 Text 設定尺寸</span></article>
            <article><strong>規格卡 3</strong><span>稍後拖 Text 設定特色</span></article>
          </section>
        `
        : ''

    body.innerHTML = `
      <main class="sofa-page">
        <section class="sofa-hero">
          <div class="sofa-copy">
            ${heroText}
            ${buttons}
          </div>
          <figure>${heroImage}</figure>
        </section>
        ${details}
      </main>
    `
    body.appendChild(style)
  }, { stage, sofaImageUrl })
}

async function main() {
  const browser = await chromium.launch({ headless: true })
  const context = await browser.newContext({
    viewport: { width: 1440, height: 900 },
    recordVideo: { dir: rawDir, size: { width: 1440, height: 900 } }
  })
  const page = await context.newPage()

  try {
    await page.goto(`${baseUrl}/login`, { waitUntil: 'networkidle' })
    await page.getByRole('button', { name: /註冊|Register/ }).click()
    await page.locator('input[type="email"]').fill(email)
    await page.locator('input[type="password"]').fill(password)
    await page.locator('input[type="text"]').fill('Sofa Demo')
    await Promise.all([
      page.waitForURL(/\/$/, { timeout: 30000 }).catch(() => null),
      page.getByRole('button', { name: /註冊|Register/ }).last().click()
    ])
    await waitForAny(page, ['.sf-dash-empty button', '.sf-dash-card-new'], 30000)

    await clickFirstVisible(page, [
      page.getByRole('button', { name: /Create your first project|建立第一個專案|New Project|Create New Site|建立新網站/ }),
      '.sf-dash-empty button'
    ])
    await waitForAny(page, ['.sf-modal'])
    const inputs = page.locator('.sf-modal input')
    await inputs.nth(0).fill(siteName)
    await inputs.nth(1).fill('用拖拉區塊建立沙發產品介紹頁。')
    await Promise.all([
      page.waitForURL(/\/sites\//, { timeout: 30000 }),
      page.getByRole('button', { name: /建立並進入工作區|Create and open workspace/ }).click()
    ])

    await waitForAny(page, ['.sf-workspace', '.sf-ws-card'])
    await page.locator('.sf-ws-card').first().click()
    await page.waitForURL(/\/editor\//, { timeout: 30000 })
    await waitForAny(page, ['.studio-editor', '#gjs'])
    await page.waitForTimeout(3000)

    await clickFirstVisible(page, [
      page.getByRole('button', { name: /Blocks/ }),
      '.editor-rail button:nth-child(1)'
    ])
    await page.locator('#blocks-panel .gjs-block').first().waitFor({ state: 'visible', timeout: 20000 })

    await showStep(page, 1, '拖 Section 到畫布上半部，先建立 Hero 外框', [
      'Block：Section',
      '放置位置：畫布上半部，作為產品主視覺區',
      '參數：背景 #f6f2ec、padding 72px',
      '版面：左右兩欄，gap 48px'
    ], await canvasTarget(page, 0.52, 0.31, 0.52, 0.24))
    await page.waitForTimeout(readPause)
    await dragBlockToCanvas(page, 'Section', 0.52, 0.31)
    await updateCanvas(page, 1)
    await showStep(page, '01B', '設定 Hero Section 的版面參數', [
      '背景色：#f6f2ec',
      '內距：padding 72px',
      '欄位：grid 兩欄，左欄文字、右欄圖片',
      '欄距：gap 48px'
    ], await canvasTarget(page, 0.52, 0.31, 0.52, 0.24))
    await page.waitForTimeout(settingPause)
    await screenshotStep(page, 'sofa-step-01-section-hero.png')
    await page.waitForTimeout(readPause)

    await showStep(page, 2, '拖 Text 到 Hero 左欄，設定商品文字', [
      'Block：Text',
      '放置位置：Hero 左欄',
      '內容：NEW COLLECTION / Nordic Lounge Sofa',
      '參數：H1 64px、line-height 1.02、段落 20px'
    ], await canvasTarget(page, 0.36, 0.43, 0.28, 0.28))
    await page.waitForTimeout(readPause)
    await dragBlockToCanvas(page, 'Text', 0.36, 0.43)
    await updateCanvas(page, 2)
    await showStep(page, '02B', '設定 Hero 左欄文字內容與字級', [
      '小標：NEW COLLECTION',
      '標題：Nordic Lounge Sofa',
      '標題樣式：64px、line-height 1.02',
      '描述文字：20px、line-height 1.8'
    ], await canvasTarget(page, 0.36, 0.43, 0.28, 0.28))
    await page.waitForTimeout(settingPause)
    await screenshotStep(page, 'sofa-step-02-hero-text.png')
    await page.waitForTimeout(readPause)

    await showStep(page, 3, '拖 Image 到 Hero 右欄，換成沙發照片', [
      'Block：Image',
      '放置位置：Hero 右欄',
      `圖片 URL：${sofaImageUrl}`,
      '參數：alt 綠色北歐沙發、height 560px、object-fit cover、radius 8px'
    ], await canvasTarget(page, 0.66, 0.43, 0.32, 0.32))
    await page.waitForTimeout(readPause)
    await dragBlockToCanvas(page, 'Image', 0.66, 0.43)
    await updateCanvas(page, 3)
    await showStep(page, '03B', '設定 Hero 右欄圖片參數', [
      `src：${sofaImageUrl}`,
      'alt：綠色北歐沙發',
      '尺寸：height 560px、width 100%',
      '裁切與外觀：object-fit cover、radius 8px'
    ], await canvasTarget(page, 0.66, 0.43, 0.32, 0.32))
    await page.waitForTimeout(settingPause)
    await screenshotStep(page, 'sofa-step-03-hero-image.png')
    await page.waitForTimeout(readPause)

    await showStep(page, 4, '拖兩個 Button 到 Hero 左欄，建立主要行動按鈕', [
      'Block：Button x 2',
      '放置位置：商品介紹文字下方',
      '第一顆：查看規格，href #details，黑底白字',
      '第二顆：預約試坐，透明底黑框'
    ], await canvasTarget(page, 0.37, 0.66, 0.26, 0.12))
    await page.waitForTimeout(readPause)
    await dragBlockToCanvas(page, 'Button', 0.34, 0.66)
    await page.waitForTimeout(1500)
    await dragBlockToCanvas(page, 'Button', 0.43, 0.66)
    await updateCanvas(page, 4)
    await showStep(page, '04B', '設定兩個 CTA 按鈕文字與樣式', [
      '主要按鈕：查看規格，連到 #details',
      '主要樣式：黑底白字',
      '次要按鈕：預約試坐',
      '次要樣式：透明底、黑色外框'
    ], await canvasTarget(page, 0.37, 0.66, 0.26, 0.12))
    await page.waitForTimeout(settingPause)
    await screenshotStep(page, 'sofa-step-04-buttons.png')
    await page.waitForTimeout(readPause)

    await showStep(page, 5, '再拖 Section 到 Hero 下方，建立規格資訊區', [
      'Block：Section',
      '放置位置：Hero 下方',
      '用途：承載三張規格卡',
      '參數：grid 3 欄、padding 0 72px 72px、gap 1px'
    ], await canvasTarget(page, 0.52, 0.79, 0.54, 0.16))
    await page.waitForTimeout(readPause)
    await dragBlockToCanvas(page, 'Section', 0.52, 0.79)
    await updateCanvas(page, 5)
    await scrollCanvasTo(page, 360)
    await page.waitForTimeout(transitionPause)
    await showStep(page, '05B', '設定規格資訊區的卡片版面', [
      '區塊 ID：details',
      '版面：grid 3 欄',
      '外距：接在 Hero 下方',
      '內距：padding 0 72px 72px、gap 1px'
    ], await canvasTarget(page, 0.52, 0.79, 0.54, 0.16))
    await page.waitForTimeout(settingPause)
    await screenshotStep(page, 'sofa-step-05-details-section.png')
    await page.waitForTimeout(readPause)

    await showStep(page, 6, '拖 Text 到三張規格卡，填入材質、尺寸、特色', [
      'Block：Text x 3',
      '卡 1：材質，耐磨亞麻混紡布、實木框架、高密度泡棉',
      '卡 2：尺寸，220 x 92 x 78 cm，適合 3 人座客廳',
      '卡 3：特色，可拆洗布套、模組化腳椅、10 年框架保固'
    ], await canvasTarget(page, 0.53, 0.78, 0.52, 0.15))
    await page.waitForTimeout(readPause)
    await dragBlockToCanvas(page, 'Text', 0.36, 0.79)
    await page.waitForTimeout(1200)
    await dragBlockToCanvas(page, 'Text', 0.53, 0.79)
    await page.waitForTimeout(1200)
    await dragBlockToCanvas(page, 'Text', 0.70, 0.79)
    await updateCanvas(page, 6)
    await scrollCanvasTo(page, 360)
    await page.waitForTimeout(transitionPause)
    await showStep(page, '06B', '設定三張規格卡的文案', [
      '卡 1 標題：材質',
      '卡 2 標題：尺寸',
      '卡 3 標題：特色',
      '每張卡：strong 標題 + span 描述'
    ], await canvasTarget(page, 0.53, 0.78, 0.52, 0.15))
    await page.waitForTimeout(settingPause)
    await screenshotStep(page, 'sofa-step-06-detail-texts.png')
    await page.waitForTimeout(readPause)

    await showStep(page, 7, '切換 Tablet / Mobile，確認響應式版面', [
      'Desktop：左右雙欄、規格三欄',
      'Tablet：檢查內容仍在可讀寬度內',
      'Mobile：Hero 改單欄，規格卡垂直排列',
      '完成後再按 Save / Publish'
    ], await canvasTarget(page, 0.53, 0.50, 0.52, 0.55))
    await page.waitForTimeout(readPause)
    await page.getByRole('button', { name: /Tablet/ }).click()
    await page.waitForTimeout(4200)
    await screenshotStep(page, 'sofa-step-07-tablet.png')
    await page.getByRole('button', { name: /Mobile/ }).click()
    await page.waitForTimeout(4600)
    await screenshotStep(page, 'sofa-step-08-mobile.png')
    await page.getByRole('button', { name: /Desktop/ }).click()
    await page.waitForTimeout(4200)
    await screenshotStep(page, 'sofa-step-09-final-desktop.png')
  } finally {
    await page.close()
    await context.close()
    await browser.close()
  }

  const rawVideo = await new Promise((resolve, reject) => {
    const fs = require('node:fs')
    const files = fs.readdirSync(rawDir).filter((file) => file.endsWith('.webm'))
    if (!files.length) reject(new Error('recorded webm not found'))
    else resolve(path.join(rawDir, files[0]))
  })

  const mp4Path = path.join(mediaDir, 'siteforge-sofa-editor-demo.mp4')
  const gifPath = path.join(mediaDir, 'siteforge-sofa-editor-demo.gif')

  run(ffmpeg, [
    '-y',
    '-i', rawVideo,
    '-vf', 'scale=1280:-2',
    '-c:v', 'libx264',
    '-preset', 'veryfast',
    '-crf', '23',
    '-pix_fmt', 'yuv420p',
    mp4Path
  ])
  run(ffmpeg, [
    '-y',
    '-i', mp4Path,
    '-vf', 'fps=8,scale=960:-1:flags=lanczos,split[s0][s1];[s0]palettegen=max_colors=128[p];[s1][p]paletteuse=dither=bayer',
    '-loop', '0',
    gifPath
  ])

  console.log(JSON.stringify({
    ok: true,
    mp4Path,
    gifPath,
    rawVideo,
    screenshots: [
      'sofa-step-01-section-hero.png',
      'sofa-step-02-hero-text.png',
      'sofa-step-03-hero-image.png',
      'sofa-step-04-buttons.png',
      'sofa-step-05-details-section.png',
      'sofa-step-06-detail-texts.png',
      'sofa-step-07-tablet.png',
      'sofa-step-08-mobile.png',
      'sofa-step-09-final-desktop.png'
    ].map((file) => path.join(imageDir, file))
  }, null, 2))
}

main().catch((error) => {
  console.error(error)
  process.exit(1)
})
