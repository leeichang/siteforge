import { mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs'
import { spawnSync } from 'node:child_process'
import { createRequire } from 'node:module'
import path from 'node:path'

const require = createRequire('/Users/leeichang/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/')
const { chromium } = require('playwright')

const rootDir = '/Users/leeichang/Documents/siteforge'
const imageDir = path.join(rootDir, 'docs/manual/images')
const mediaDir = path.join(rootDir, 'docs/manual/media')
const tmpDir = path.join(rootDir, 'artifacts/editor-tutorial-media')
const ffmpeg = process.env.FFMPEG_BIN || '/opt/homebrew/bin/ffmpeg'

const slides = [
  {
    image: 'manual-10-editor-project-tree.png',
    title: '1. 進入 Editor',
    body: '從 Workspace 點頁面，進入完整畫布、頁面樹與右側樣式面板。'
  },
  {
    image: 'manual-11-editor-blocks.png',
    title: '2. 用 Blocks 新增內容',
    body: '打開左側 Blocks，把 Section、Text、Button、Image 或 Form 拖到畫布。'
  },
  {
    image: 'manual-12-editor-project-tree-layers.png',
    title: '3. 用 Layers 選取結構',
    body: 'Project Tree 切換頁面，Layers 用來選深層元素與確認頁面層級。'
  },
  {
    image: 'manual-13-editor-global-styles.png',
    title: '4. 調整全域樣式',
    body: '在 Global Styles 修改品牌色、字體與行高，再回到畫布檢查效果。'
  },
  {
    image: 'manual-14-editor-assets.png',
    title: '5. 管理圖片素材',
    body: '在 Assets 貼上圖片 URL 或選既有素材，之後可替換畫布中的圖片。'
  },
  {
    image: 'manual-16-editor-ai-assistant.png',
    title: '6. 用 AI Assistant 產生區塊',
    body: '寫清楚要新增或替換、放在哪裡、需要幾欄與品牌語氣。'
  },
  {
    image: 'manual-17-editor-properties.png',
    title: '7. 檢查 Properties',
    body: '選取圖片或元素後，在右側 Properties 調整 URL、alt text 與頁面資訊。'
  },
  {
    image: 'manual-18-editor-code-modal.png',
    title: '8. 用 Code 檢查輸出',
    body: 'Code 視窗可快速查看目前頁面的 HTML 與 CSS，方便除錯。'
  },
  {
    image: 'manual-19-editor-tablet.png',
    title: '9. 檢查 Tablet',
    body: '發佈前切到 Tablet，確認版面沒有擠壓、重疊或文字溢出。'
  },
  {
    image: 'manual-20-editor-mobile.png',
    title: '10. 檢查 Mobile 並儲存',
    body: '最後切到 Mobile 檢查，完成後按 Save，再按 Publish 發佈網站。'
  }
]

function run(args) {
  const result = spawnSync(ffmpeg, args, { stdio: 'inherit' })
  if (result.status !== 0) {
    throw new Error(`ffmpeg failed: ${args.join(' ')}`)
  }
}

function escapeHtml(value) {
  return value
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
}

mkdirSync(mediaDir, { recursive: true })
rmSync(tmpDir, { recursive: true, force: true })
mkdirSync(tmpDir, { recursive: true })

const browser = await chromium.launch({ headless: true })
const page = await browser.newPage({ viewport: { width: 1280, height: 720 }, deviceScaleFactor: 1 })

const framePaths = []
try {
  for (const [index, slide] of slides.entries()) {
    const imagePath = path.join(imageDir, slide.image)
    const framePath = path.join(tmpDir, `frame-${String(index + 1).padStart(2, '0')}.png`)
    const imageUrl = `data:image/png;base64,${readFileSync(imagePath).toString('base64')}`
    await page.setContent(`<!doctype html>
      <html lang="zh-Hant">
        <head>
          <meta charset="utf-8" />
          <style>
            * { box-sizing: border-box; }
            body {
              margin: 0;
              width: 1280px;
              height: 720px;
              overflow: hidden;
              background: #111116;
              font-family: -apple-system, BlinkMacSystemFont, "PingFang TC", "Hiragino Sans GB", "Noto Sans CJK TC", sans-serif;
              color: white;
            }
            .stage {
              position: relative;
              width: 1280px;
              height: 720px;
              display: grid;
              place-items: center;
              background:
                linear-gradient(180deg, rgba(17,17,22,0.08), rgba(17,17,22,0.3)),
                #111116;
            }
            img {
              width: 1280px;
              height: 720px;
              object-fit: contain;
              display: block;
            }
            .top {
              position: absolute;
              top: 28px;
              left: 42px;
              right: 42px;
              display: flex;
              align-items: center;
              justify-content: space-between;
              color: #d8c3ff;
              font-size: 22px;
              font-weight: 700;
              text-shadow: 0 2px 10px rgba(0,0,0,.72);
            }
            .caption {
              position: absolute;
              left: 0;
              right: 0;
              bottom: 0;
              min-height: 172px;
              padding: 24px 54px 28px;
              background: linear-gradient(180deg, rgba(0,0,0,.34), rgba(0,0,0,.78));
              border-top: 1px solid rgba(255,255,255,.1);
            }
            h1 {
              margin: 0 0 14px;
              font-size: 34px;
              line-height: 1.18;
              letter-spacing: 0;
            }
            p {
              margin: 0;
              max-width: 1110px;
              font-size: 23px;
              line-height: 1.45;
              color: #e5e7eb;
            }
          </style>
        </head>
        <body>
          <main class="stage">
            <img src="${imageUrl}" alt="" />
            <div class="top">
              <span>SiteForge Editor 教學</span>
              <span>${index + 1}/${slides.length}</span>
            </div>
            <section class="caption">
              <h1>${escapeHtml(slide.title)}</h1>
              <p>${escapeHtml(slide.body)}</p>
            </section>
          </main>
        </body>
      </html>`, { waitUntil: 'load' })
    await page.screenshot({ path: framePath, fullPage: false })
    framePaths.push(framePath)
  }
} finally {
  await browser.close()
}

const segmentPaths = framePaths.map((framePath, index) => {
  const segmentPath = path.join(tmpDir, `segment-${String(index + 1).padStart(2, '0')}.mp4`)

  run([
    '-y',
    '-loop', '1',
    '-t', '3.4',
    '-i', framePath,
    '-r', '30',
    '-c:v', 'libx264',
    '-preset', 'veryfast',
    '-crf', '18',
    '-pix_fmt', 'yuv420p',
    segmentPath
  ])

  return segmentPath
})

const concatList = path.join(tmpDir, 'concat.txt')
writeFileSync(concatList, segmentPaths.map((segment) => `file '${segment}'`).join('\n'))

const mp4Path = path.join(mediaDir, 'siteforge-editor-tutorial.mp4')
run([
  '-y',
  '-f', 'concat',
  '-safe', '0',
  '-i', concatList,
  '-c', 'copy',
  mp4Path
])

const gifPath = path.join(mediaDir, 'siteforge-editor-tutorial.gif')
run([
  '-y',
  '-i', mp4Path,
  '-vf', 'fps=8,scale=960:-1:flags=lanczos,split[s0][s1];[s0]palettegen=max_colors=96[p];[s1][p]paletteuse=dither=bayer',
  '-loop', '0',
  gifPath
])

console.log(JSON.stringify({
  ok: true,
  mp4Path,
  gifPath,
  slides: slides.length
}, null, 2))
