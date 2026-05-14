import { createRequire } from 'node:module'
import { mkdirSync } from 'node:fs'
import path from 'node:path'

const require = createRequire('/Users/leeichang/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/')
const { chromium } = require('playwright')

const baseUrl = process.env.SITEFORGE_BASE_URL || 'http://localhost:5010'
const artifactDir = path.resolve('artifacts/e2e-dashboard-delete')
mkdirSync(artifactDir, { recursive: true })

const stamp = Date.now()
const email = `dashboard-delete-${stamp}@siteforge.test`
const password = 'E2ePassw0rd!'
const siteName = `Delete E2E ${stamp}`

async function api(pathname, options = {}) {
  const response = await fetch(`${baseUrl}/api${pathname}`, {
    headers: {
      'Content-Type': 'application/json',
      ...(options.token ? { Authorization: `Bearer ${options.token}` } : {})
    },
    ...options,
    body: options.body ? JSON.stringify(options.body) : undefined
  })

  if (!response.ok) {
    throw new Error(`${options.method || 'GET'} ${pathname} failed: ${response.status} ${await response.text()}`)
  }

  return response.json()
}

async function main() {
  const auth = await api('/Auth/register', {
    method: 'POST',
    body: {
      email,
      password,
      displayName: 'Dashboard Delete E2E'
    }
  })
  const authData = auth.data || auth
  if (!authData.token || !authData.user) throw new Error('registration did not return auth data')

  const browser = await chromium.launch({ headless: true })
  const page = await browser.newPage({ viewport: { width: 2048, height: 1152 } })
  const errors = []

  page.on('console', (message) => {
    if (message.type() === 'error') errors.push(`console: ${message.text()}`)
  })
  page.on('pageerror', (error) => errors.push(`pageerror: ${error.message}`))

  try {
    await page.addInitScript(({ token, user }) => {
      localStorage.setItem('token', token)
      localStorage.setItem('refreshToken', 'e2e-refresh-token')
      localStorage.setItem('user', JSON.stringify(user))
    }, { token: authData.token, user: authData.user })

    await page.goto(baseUrl, { waitUntil: 'networkidle' })
    await page.screenshot({ path: path.join(artifactDir, '01-dashboard-empty.png'), fullPage: true })

    await page.locator('.sf-dash-card-new, .sf-dash-empty button').first().click()
    await page.locator('.sf-modal input').nth(0).fill(siteName)
    await page.locator('.sf-modal input').nth(1).fill('Created by dashboard delete E2E.')
    await Promise.all([
      page.waitForURL(/\/sites\//, { timeout: 20000 }),
      page.getByRole('button', { name: /建立並進入工作區|Create and open workspace/ }).click()
    ])

    const createdSites = (await api('/Sites', { token: authData.token })).data
    const created = createdSites.find((site) => site.name === siteName)
    if (!created?.id) throw new Error('created site was not returned by API')

    await page.goto(baseUrl, { waitUntil: 'networkidle' })
    const card = page.locator('.sf-dash-card').filter({ hasText: siteName }).first()
    await card.waitFor({ timeout: 20000 })
    await page.screenshot({ path: path.join(artifactDir, '02-dashboard-created.png'), fullPage: true })

    page.once('dialog', async (dialog) => {
      if (!dialog.message().includes(siteName)) throw new Error(`delete confirmation did not name the project: ${dialog.message()}`)
      await dialog.accept()
    })
    await card.locator('.sf-dash-card-menu').click()
    await card.getByRole('button', { name: /刪除專案|Delete project/ }).click()

    await page.waitForFunction((name) => !document.body.innerText.includes(name), siteName, { timeout: 20000 })
    await page.screenshot({ path: path.join(artifactDir, '03-dashboard-deleted.png'), fullPage: true })

    const remainingSites = (await api('/Sites', { token: authData.token })).data
    if (remainingSites.some((site) => site.id === created.id || site.name === siteName)) {
      throw new Error('deleted site is still returned by API')
    }

    if (errors.length) {
      throw new Error(`browser errors:\n${errors.join('\n')}`)
    }

    console.log(JSON.stringify({
      ok: true,
      siteName,
      createdId: created.id,
      remainingSites: remainingSites.length,
      artifacts: artifactDir
    }, null, 2))
  } finally {
    await browser.close()
  }
}

main().catch((error) => {
  console.error(error)
  process.exit(1)
})
