import { createRequire } from 'node:module'
import { mkdirSync } from 'node:fs'
import path from 'node:path'

const require = createRequire('/Users/leeichang/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/')
const { chromium } = require('playwright')

const baseUrl = process.env.SITEFORGE_BASE_URL || 'http://localhost:5010'
const artifactDir = path.resolve('artifacts/e2e-preview-regression')
mkdirSync(artifactDir, { recursive: true })

const stamp = Date.now()
const email = `preview-${stamp}@siteforge.test`
const password = 'E2ePassw0rd!'
const siteName = `Preview Beauty ${stamp}`

function log(step, detail = '') {
  console.log(`PREVIEW-E2E ${step}${detail ? ` - ${detail}` : ''}`)
}

async function api(pathname, options = {}) {
  const response = await fetch(`${baseUrl}${pathname}`, {
    ...options,
    headers: {
      'content-type': 'application/json',
      ...(options.token ? { authorization: `Bearer ${options.token}` } : {}),
      ...(options.headers || {})
    },
    body: options.body && typeof options.body !== 'string'
      ? JSON.stringify(options.body)
      : options.body
  })
  const text = await response.text()
  const json = text ? JSON.parse(text) : null
  if (!response.ok) {
    throw new Error(`${options.method || 'GET'} ${pathname} failed: ${response.status} ${text}`)
  }
  return json?.data || json
}

async function screenshot(page, name, fullPage = true) {
  await page.screenshot({ path: path.join(artifactDir, `${name}.png`), fullPage })
}

async function main() {
  log('register')
  const auth = await api('/api/Auth/register', {
    method: 'POST',
    body: { email, password, displayName: 'Preview E2E' }
  })
  const token = auth.token
  if (!token) throw new Error('register did not return token')

  log('generate beauty site')
  const generated = await api('/api/AiConversations/generate-site', {
    method: 'POST',
    token,
    body: {
      siteName,
      prompt: 'Use the curated Stitch beauty template exactly as designed.',
      templateKey: 'site-beauty'
    }
  })
  if (!generated.siteId || !generated.pages?.length) {
    throw new Error(`invalid generated site response: ${JSON.stringify(generated)}`)
  }

  log('publish generated site')
  const publish = await api(`/api/Sites/${generated.siteId}/publish`, {
    method: 'POST',
    token,
    body: { taskType: 'full_publish', targetUrl: '' }
  })
  if (publish.status !== 'done' || !publish.targetUrl) {
    throw new Error(`publish did not finish: ${JSON.stringify(publish)}`)
  }

  const browser = await chromium.launch({ headless: true })
  const context = await browser.newContext({ viewport: { width: 2048, height: 980 } })
  await context.route('**/*', (route) => {
    const url = route.request().url()
    if (
      url.includes('cdn.tailwindcss.com') ||
      url.includes('fonts.googleapis.com') ||
      url.includes('fonts.gstatic.com')
    ) {
      return route.abort()
    }
    return route.continue()
  })

  const page = await context.newPage()
  const browserErrors = []
  page.on('console', (message) => {
    if (message.type() !== 'error') return
    const text = message.text()
    if (text.includes('Failed to load resource: net::ERR_FAILED')) return
    browserErrors.push(text)
  })
  page.on('pageerror', (error) => browserErrors.push(error.message))

  try {
    log('verify page management width')
    await page.addInitScript(({ token, user }) => {
      localStorage.setItem('token', token)
      localStorage.setItem('refreshToken', 'e2e-refresh')
      localStorage.setItem('user', JSON.stringify(user))
      localStorage.setItem('sf-locale', 'zh-Hant')
      localStorage.setItem('sf-theme', 'light')
    }, { token, user: auth.user })
    await page.goto(`${baseUrl}/sites/${generated.siteId}`, { waitUntil: 'networkidle' })
    await page.locator('.sf-workspace').waitFor({ timeout: 20000 })
    const workspaceMetrics = await page.evaluate(() => {
      const workspace = document.querySelector('.sf-workspace')
      const grid = document.querySelector('.sf-ws-grid')
      const card = document.querySelector('.sf-ws-card')
      const workspaceRect = workspace?.getBoundingClientRect()
      const gridColumns = grid ? getComputedStyle(grid).gridTemplateColumns.split(' ').length : 0
      return {
        workspaceWidth: Math.round(workspaceRect?.width || 0),
        viewportWidth: window.innerWidth,
        gridColumns,
        cardWidth: Math.round(card?.getBoundingClientRect().width || 0)
      }
    })
    await screenshot(page, '01-page-management')
    if (workspaceMetrics.workspaceWidth < 1200 || workspaceMetrics.gridColumns < 2) {
      throw new Error(`Page Management still looks mobile: ${JSON.stringify(workspaceMetrics)}`)
    }

    log('verify published preview with CDN blocked')
    const previewUrl = `${baseUrl}${publish.targetUrl}products/`
    const previewResponse = await page.goto(previewUrl, { waitUntil: 'domcontentloaded' })
    const previewStatus = previewResponse?.status()
    if (!previewResponse?.ok()) {
      await screenshot(page, '02-published-preview-load-failed')
      throw new Error(`published preview failed to load ${previewUrl}: status=${previewStatus}, finalUrl=${page.url()}`)
    }
    try {
      await page.locator('.siteforge-stitch-template').waitFor({ state: 'attached', timeout: 20000 })
    } catch (error) {
      await screenshot(page, '02-published-preview-missing-template')
      const bodySample = await page.locator('body').innerText({ timeout: 2000 }).catch(() => '')
      throw new Error(`published preview missing Stitch root: status=${previewStatus}, finalUrl=${page.url()}, body=${bodySample.slice(0, 500)}`)
    }
    await page.waitForTimeout(1000)
    const publishedMetrics = await page.evaluate(() => {
      const fallback = !!document.querySelector('style[data-siteforge-stitch-fallback]')
      const template = document.querySelector('.siteforge-stitch-template')
      const brand = document.body.innerText.includes('AETHERIS 瑰麗美學')
      const leakedSiteName = document.body.innerText.includes('Preview Beauty')
      const nav = template?.querySelector('nav')
      const storyGrid = Array.from(template?.querySelectorAll('section') || [])
        .find((section) => section.className.includes('md:grid-cols-2'))
      const hero = template?.querySelector('section')
      const image = template?.querySelector('img')
      const icons = Array.from(template?.querySelectorAll('.material-symbols-outlined') || [])
        .map((icon) => ({
          text: icon.textContent.trim(),
          dataIcon: icon.getAttribute('data-icon'),
          before: getComputedStyle(icon, '::before').content,
          fontSize: getComputedStyle(icon).fontSize
        }))
      const visibleRawIcons = icons.filter((icon) =>
        icon.text &&
        /^(menu|search|shopping_bag|format_quote|check_circle)$/.test(icon.text) &&
        icon.fontSize !== '0px'
      )
      const missingFallbackGlyphs = icons.filter((icon) => !icon.before || icon.before === 'none' || icon.before === 'normal')
      return {
        fallback,
        brand,
        leakedSiteName,
        visibleRawIcons: visibleRawIcons.length,
        missingFallbackGlyphs: missingFallbackGlyphs.length,
        navDisplay: nav ? getComputedStyle(nav).display : null,
        gridDisplay: storyGrid ? getComputedStyle(storyGrid).display : null,
        gridColumns: storyGrid ? getComputedStyle(storyGrid).gridTemplateColumns : null,
        heroHeight: Math.round(hero?.getBoundingClientRect().height || 0),
        firstImageWidth: Math.round(image?.getBoundingClientRect().width || 0),
        iconCount: icons.length,
        missingDataIcons: icons.filter((icon) => !icon.dataIcon).length,
        sampleIcons: icons.slice(0, 4)
      }
    })
    await screenshot(page, '02-published-preview-cdn-blocked')

    if (!publishedMetrics.fallback) throw new Error('published preview did not inject fallback CSS')
    if (!publishedMetrics.brand) throw new Error('published preview did not preserve original AETHERIS brand')
    if (publishedMetrics.leakedSiteName) throw new Error('published preview leaked generated site name into Stitch design')
    if (publishedMetrics.visibleRawIcons > 0 || publishedMetrics.missingFallbackGlyphs > 0) {
      throw new Error(`material icon fallback is visually leaking: ${JSON.stringify(publishedMetrics.sampleIcons)}`)
    }
    if (publishedMetrics.navDisplay !== 'flex') throw new Error(`desktop nav is not flex: ${JSON.stringify(publishedMetrics)}`)
    if (publishedMetrics.gridDisplay !== 'grid' || !publishedMetrics.gridColumns?.includes('px')) {
      throw new Error(`story grid is not desktop grid: ${JSON.stringify(publishedMetrics)}`)
    }
    if (publishedMetrics.heroHeight < 500 || publishedMetrics.firstImageWidth < 700) {
      throw new Error(`published preview visual scale is wrong: ${JSON.stringify(publishedMetrics)}`)
    }
    if (publishedMetrics.iconCount < 4 || publishedMetrics.missingDataIcons > 0) {
      throw new Error(`material icons are not normalized: ${JSON.stringify(publishedMetrics)}`)
    }

    if (browserErrors.length) {
      throw new Error(`browser errors:\n${browserErrors.join('\n')}`)
    }

    log('passed', artifactDir)
    console.log(JSON.stringify({ workspaceMetrics, publishedMetrics, targetUrl: publish.targetUrl }, null, 2))
  } finally {
    await browser.close()
  }
}

main().catch((error) => {
  console.error(error)
  process.exit(1)
})
