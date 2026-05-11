<template>
  <div class="workspace-shell" :class="`theme-${theme}`">
    <header class="workspace-header">
      <div class="header-left">
        <button class="header-back" @click="router.push('/')" type="button">←</button>
        <div class="site-title">
          <p class="sf-kicker">{{ t('workspace.kicker') }}</p>
          <h1>{{ site?.name || t('workspace.loadingSite') }}</h1>
        </div>
      </div>
      <div class="header-actions">
        <button class="theme-toggle language-toggle" @click="localeStore.toggleLocale()" type="button" :title="t('common.language')">
          {{ locale === 'en' ? '繁' : 'EN' }}
        </button>
        <button class="theme-toggle" @click="themeStore.toggle()" type="button" :title="theme === 'dark' ? t('common.switchToLight') : t('common.switchToDark')">
          {{ theme === 'dark' ? '☀️' : '🌙' }}
        </button>
        <a v-if="site?.publishedUrl" :href="site.publishedUrl" target="_blank" class="sf-button">{{ t('workspace.viewPublic') }}</a>
        <button class="sf-button primary" @click="publishSite" :disabled="publishing">
          {{ publishing ? t('workspace.publishing') : t('workspace.publishSite') }}
        </button>
      </div>
    </header>

    <main class="workspace-main">
      <section class="workspace-content">
        <section class="workspace-hero">
          <div>
            <p class="sf-kicker">{{ t('workspace.flowKicker') }}</p>
            <h2>{{ t('workspace.heroTitle') }}</h2>
            <p>{{ t('workspace.heroBody') }}</p>
            <div class="site-meta-row">
              <span class="sf-pill" :class="site?.status === 'published' ? 'success' : 'warning'">
                {{ site?.status === 'published' ? t('common.published') : t('common.draft') }}
              </span>
              <strong>{{ site?.slug ? `/${site.slug}` : t('common.notLoaded') }}</strong>
              <small>{{ site?.description || t('workspace.noSiteDescription') }}</small>
            </div>
          </div>
          <div class="health-grid">
            <div>
              <span>{{ t('workspace.pages') }}</span>
              <strong>{{ pages.length }}</strong>
            </div>
            <div>
              <span>{{ t('workspace.publishedPages') }}</span>
              <strong>{{ publishedPages }}</strong>
            </div>
            <div>
              <span>{{ t('workspace.homePage') }}</span>
              <strong>{{ homePageTitle }}</strong>
            </div>
          </div>
        </section>

        <section class="pages-panel">
          <div class="content-header">
            <div>
              <h2>{{ t('workspace.pages') }}</h2>
              <p>{{ t('workspace.pagesDescription') }}</p>
            </div>
            <div class="content-actions">
              <button class="sf-button" @click="showAiPage = true">{{ t('workspace.aiGeneratePage') }}</button>
              <button class="sf-button primary" @click="showCreatePage = true">{{ t('workspace.addPage') }}</button>
            </div>
          </div>

          <div v-if="loading" class="muted-panel">{{ t('workspace.loadingPages') }}</div>

          <div v-else class="page-table">
            <div class="page-row page-row-head">
              <span>{{ t('workspace.title') }}</span>
              <span>{{ t('workspace.path') }}</span>
              <span>{{ t('workspace.status') }}</span>
              <span></span>
            </div>
            <div v-for="page in pages" :key="page.id" class="page-row">
              <div>
                <strong>{{ page.title }}</strong>
                <small v-if="page.isHome">{{ t('common.homePage') }}</small>
              </div>
              <span class="page-slug">/{{ page.slug }}</span>
              <span class="sf-pill" :class="page.isPublished ? 'success' : 'warning'">
                {{ page.isPublished ? t('common.published') : t('common.draft') }}
              </span>
              <div class="row-actions">
                <button class="sf-button primary" @click="openEditor(page.id)">{{ t('common.edit') }}</button>
                <button class="sf-button danger" @click="deletePage(page.id)" :disabled="page.isHome">{{ t('common.delete') }}</button>
              </div>
            </div>
          </div>
        </section>
      </section>
    </main>

    <div v-if="showCreatePage" class="sf-modal-overlay" @click.self="showCreatePage = false">
      <div class="sf-modal page-modal">
        <p class="sf-kicker">{{ t('workspace.newPage') }}</p>
        <h3>{{ t('workspace.addPage') }}</h3>
        <form @submit.prevent="createPage">
          <label>
            {{ t('workspace.pageTitle') }}
            <input v-model="newPage.title" class="sf-input" required :placeholder="t('workspace.pageTitlePlaceholder')" />
          </label>
          <label>
            URL Slug
            <input v-model="newPage.slug" class="sf-input" :placeholder="t('workspace.slugPlaceholder')" />
          </label>
          <div class="modal-actions">
            <button type="button" class="sf-button" @click="showCreatePage = false">{{ t('common.cancel') }}</button>
            <button class="sf-button primary" :disabled="creatingPage">{{ creatingPage ? t('workspace.creating') : t('workspace.createPage') }}</button>
          </div>
        </form>
      </div>
    </div>

    <div v-if="showAiPage" class="sf-modal-overlay" @click.self="showAiPage = false">
      <div class="sf-modal ai-page-modal">
        <p class="sf-kicker">{{ t('workspace.aiPageGenerator') }}</p>
        <h3>{{ t('workspace.generateNewPage') }}</h3>
        <form @submit.prevent="generatePage">
          <div class="ai-page-grid">
            <label>
              {{ t('workspace.pageName') }}
              <input v-model="aiPage.pageName" class="sf-input" required :placeholder="t('workspace.pageNamePlaceholder')" />
            </label>
            <label>
              {{ t('workspace.pageType') }}
              <select v-model="aiPage.pageType" class="sf-input">
                <option value="home">{{ t('pageType.home') }}</option>
                <option value="about">{{ t('pageType.about') }}</option>
                <option value="services">{{ t('pageType.services') }}</option>
                <option value="product">{{ t('pageType.product') }}</option>
                <option value="portfolio">{{ t('pageType.portfolio') }}</option>
                <option value="blog">{{ t('pageType.blog') }}</option>
                <option value="contact">{{ t('pageType.contact') }}</option>
                <option value="anti-counterfeit">{{ t('pageType.antiCounterfeit') }}</option>
                <option value="scan-result">{{ t('pageType.scanResult') }}</option>
                <option value="lottery">{{ t('pageType.lottery') }}</option>
                <option value="points-redemption">{{ t('pageType.pointsRedemption') }}</option>
                <option value="traceability">{{ t('pageType.traceability') }}</option>
                <option value="dpp">{{ t('pageType.dpp') }}</option>
              </select>
            </label>
          </div>
          <div class="template-field">
            <span class="field-label">{{ t('workspace.pageTemplate') }}</span>
            <div class="template-picker page-template-picker">
              <button
                type="button"
                class="template-card blank"
                :class="{ selected: aiPage.templateKey === '' }"
                @click="selectPageTemplate('')"
              >
                <div class="template-preview blank-preview">
                  <span>AI</span>
                </div>
                <strong>{{ t('workspace.customAiPage') }}</strong>
                <small>{{ t('workspace.customAiPageHint') }}</small>
              </button>
              <button
                v-for="template in pageTemplates"
                :key="template.key"
                type="button"
                class="template-card"
                :class="{ selected: aiPage.templateKey === template.key }"
                @click="selectPageTemplate(template.key)"
              >
                <div
                  class="template-preview"
                  :class="[templatePreviewClass(template.key), { 'has-image': template.thumbnailUrl }]"
                >
                  <img
                    v-if="template.thumbnailUrl"
                    class="template-preview-image"
                    :src="template.thumbnailUrl"
                    :alt="`${template.label} preview`"
                    loading="lazy"
                  />
                  <template v-else>
                    <span></span>
                    <span></span>
                    <span></span>
                  </template>
                </div>
                <strong>{{ template.label }}</strong>
                <small>{{ template.category }} / {{ template.pageTypes?.[0] || 'page' }}</small>
              </button>
            </div>
          </div>
          <label>
            {{ t('workspace.generationPrompt') }}
            <textarea
              v-model="aiPage.prompt"
              class="sf-input"
              :required="!aiPage.templateKey"
              :placeholder="t('workspace.generationPlaceholder')"
            ></textarea>
          </label>
          <div class="ai-page-grid">
            <label>
              {{ t('workspace.style') }}
              <select v-model="aiPage.style" class="sf-input">
                <option value="studio">Studio</option>
                <option value="tech">Tech</option>
                <option value="premium">Premium</option>
                <option value="eco">Eco</option>
                <option value="fashion">Fashion</option>
              </select>
            </label>
            <label>
              {{ t('workspace.contentLength') }}
              <select v-model="aiPage.contentLength" class="sf-input">
                <option value="concise">{{ t('workspace.concise') }}</option>
                <option value="medium">{{ t('workspace.medium') }}</option>
                <option value="long">{{ t('workspace.long') }}</option>
              </select>
            </label>
          </div>
          <div class="modal-actions">
            <button type="button" class="sf-button" @click="showAiPage = false">{{ t('common.cancel') }}</button>
            <button class="sf-button primary" :disabled="generatingPage">
              {{ generatingPage ? t('workspace.generating') : t('workspace.generateAndOpen') }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useThemeStore } from '../stores/theme'
import { useLocaleStore } from '../stores/locale'
import api, { errorMessage, unwrap } from '../api/client'

const route = useRoute()
const router = useRouter()
const themeStore = useThemeStore()
const localeStore = useLocaleStore()
const theme = computed(() => themeStore.theme)
const locale = computed(() => localeStore.locale)
const t = localeStore.t
const siteId = route.params.siteId

const site = ref(null)
const pages = ref([])
const templates = ref([])
const loading = ref(false)
const publishing = ref(false)
const creatingPage = ref(false)
const generatingPage = ref(false)
const showCreatePage = ref(false)
const showAiPage = ref(false)
const newPage = ref({ title: '', slug: '' })
const aiPage = ref({
  pageName: '',
  pageType: 'services',
  prompt: '',
  templateKey: '',
  style: 'studio',
  contentLength: 'medium'
})

const publishedPages = computed(() => pages.value.filter((page) => page.isPublished).length)
const homePageTitle = computed(() => pages.value.find((page) => page.isHome)?.title || t('common.notLoaded'))
const pageTemplates = computed(() => templates.value.filter((template) => template.kind === 'page'))

onMounted(loadWorkspace)

async function loadWorkspace() {
  loading.value = true
  try {
    const [siteResponse, pagesResponse, templatesResponse] = await Promise.all([
      api.get(`/Sites/${siteId}`),
      api.get(`/Pages/site/${siteId}`),
      api.get('/AiConversations/templates?kind=page')
    ])
    site.value = unwrap(siteResponse)
    pages.value = (unwrap(pagesResponse) || []).sort((a, b) => a.displayOrder - b.displayOrder)
    templates.value = unwrap(templatesResponse) || []
  } catch (e) {
    alert(errorMessage(e, t('common.operationFailed')))
  } finally {
    loading.value = false
  }
}

async function createPage() {
  creatingPage.value = true
  try {
    const response = await api.post(`/Pages/site/${siteId}`, {
      title: newPage.value.title,
      slug: newPage.value.slug,
      pageType: 'custom',
      isHome: false
    })
    const page = unwrap(response)
    showCreatePage.value = false
    newPage.value = { title: '', slug: '' }
    await loadWorkspace()
    if (page?.id) openEditor(page.id)
  } catch (e) {
    alert(errorMessage(e, t('common.operationFailed')))
  } finally {
    creatingPage.value = false
  }
}

async function generatePage() {
  generatingPage.value = true
  try {
    const response = await api.post('/AiConversations/generate-page', {
      siteId,
      ...aiPage.value
    })
    const generated = unwrap(response)
    showAiPage.value = false
    aiPage.value = {
      pageName: '',
      pageType: 'services',
      prompt: '',
      templateKey: '',
      style: 'studio',
      contentLength: 'medium'
    }
    await loadWorkspace()
    if (generated?.pageId) openEditor(generated.pageId)
  } catch (e) {
    alert(errorMessage(e, t('common.operationFailed')))
  } finally {
    generatingPage.value = false
  }
}

function applySelectedPageTemplate() {
  const template = pageTemplates.value.find((item) => item.key === aiPage.value.templateKey)
  if (!template) return

  aiPage.value.pageName = template.label
  aiPage.value.pageType = template.pageTypes?.[0] || aiPage.value.pageType
  if (!aiPage.value.prompt) {
    aiPage.value.prompt = template.description
  }
}

function selectPageTemplate(templateKey) {
  aiPage.value.templateKey = templateKey
  if (templateKey) {
    applySelectedPageTemplate()
  }
}

function templatePreviewClass(key) {
  return `preview-${key.replace(/[^a-z0-9]+/gi, '-')}`
}

async function deletePage(pageId) {
  if (!confirm(`${t('common.delete')}?`)) return
  try {
    await api.delete(`/Pages/${pageId}`)
    await loadWorkspace()
  } catch (e) {
    alert(errorMessage(e, t('common.operationFailed')))
  }
}

async function publishSite() {
  publishing.value = true
  try {
    await api.post(`/Sites/${siteId}/publish`, { taskType: 'full_publish', targetUrl: '' })
    await loadWorkspace()
    alert(t('editor.publishSuccess'))
  } catch (e) {
    alert(errorMessage(e, t('common.operationFailed')))
  } finally {
    publishing.value = false
  }
}

function openEditor(pageId) {
  router.push(`/editor/${siteId}/${pageId}`)
}
</script>

<style scoped>
.workspace-shell {
  --workspace-page-bg: var(--sf-page, var(--sf-bg));
  --workspace-line: var(--sf-line, rgba(120, 124, 140, 0.28));
  --workspace-line-soft: color-mix(in srgb, var(--workspace-line) 72%, transparent);
  --workspace-line-strong: var(--sf-line-strong, rgba(120, 124, 140, 0.42));
  --workspace-header-line: var(--sf-header-line, var(--workspace-line-strong));
  --workspace-header-bg: var(--sf-header-bg, color-mix(in srgb, var(--workspace-page-bg) 86%, var(--sf-surface) 14%));
  --workspace-card-border: var(--sf-card-border, var(--workspace-line));
  --workspace-card-bg: var(--sf-card-bg, var(--sf-surface));
  --workspace-primary-soft: var(--sf-primary-soft, color-mix(in srgb, var(--sf-primary) 10%, transparent));
  --workspace-accent-soft: var(--sf-accent-soft, color-mix(in srgb, var(--sf-accent, var(--sf-primary)) 8%, transparent));
  --workspace-shadow: var(--sf-shadow-soft, 0 18px 46px rgba(20, 24, 32, 0.08));
  min-height: 100vh;
  background: var(--workspace-page-bg);
  color: var(--sf-ink);
  transition: background 280ms ease, color 280ms ease;
}

/* ── Header ── */
.workspace-header {
  min-height: 78px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 16px 28px;
  border-bottom: 1px solid var(--workspace-header-line);
  background: var(--workspace-header-bg);
  box-shadow: inset 0 -1px 0 color-mix(in srgb, var(--workspace-header-line) 52%, transparent);
  backdrop-filter: blur(14px);
  -webkit-backdrop-filter: blur(14px);
  transition: border-color 280ms ease, background 280ms ease;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 14px;
}

.header-back {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  border: 1px solid var(--sf-line);
  border-radius: var(--sf-radius-sm);
  background: var(--sf-surface);
  color: var(--sf-ink);
  cursor: pointer;
  font-size: 18px;
  transition: border-color 160ms ease, background 160ms ease;
}

.header-back:hover {
  border-color: var(--sf-line-strong);
  background: var(--sf-surface-hover);
}

.site-title h1 {
  margin-top: 3px;
  font-size: 25px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 10px;
}

.sf-button,
.theme-toggle {
  min-height: 38px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  border: 1px solid var(--sf-line);
  border-radius: 8px;
  padding: 0 14px;
  background: var(--sf-surface);
  color: var(--sf-ink);
  cursor: pointer;
  font-size: 14px;
  font-weight: 850;
  line-height: 1;
  text-decoration: none;
  white-space: nowrap;
  transition: transform 160ms ease, border-color 160ms ease, background 160ms ease, color 160ms ease;
}

.sf-button:hover,
.theme-toggle:hover {
  border-color: var(--sf-primary);
  background: var(--sf-surface-hover);
}

.sf-button:active,
.theme-toggle:active {
  transform: translateY(1px);
}

.sf-button.primary {
  border-color: var(--sf-primary);
  background: var(--sf-primary);
  color: var(--sf-on-primary);
}

.sf-button.danger {
  border-color: color-mix(in srgb, var(--sf-error) 45%, var(--sf-line));
  color: var(--sf-error);
}

.sf-button:disabled {
  cursor: not-allowed;
  opacity: 0.48;
  transform: none;
}

/* ── Main layout ── */
.workspace-main {
  min-height: calc(100vh - 78px);
}

/* ── Content area ── */
.workspace-content {
  display: grid;
  gap: 22px;
  padding: 28px;
  max-width: 1280px;
  width: 100%;
  margin: 0 auto;
}

.workspace-hero,
.pages-panel {
  border: 1px solid var(--workspace-card-border);
  border-radius: 10px;
  background: var(--workspace-card-bg);
  box-shadow: var(--workspace-shadow);
  transition: border-color 280ms ease, background 280ms ease, box-shadow 280ms ease;
}

.workspace-hero {
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(360px, 0.72fr);
  gap: 24px;
  padding: 24px;
  background: var(--sf-surface);
  background-image: linear-gradient(135deg, var(--workspace-primary-soft), var(--workspace-accent-soft));
}

.workspace-hero h2 {
  max-width: 760px;
  margin: 7px 0 10px;
  font-size: clamp(24px, 3vw, 36px);
  line-height: 1.12;
  color: var(--sf-ink);
}

.workspace-hero p:not(.sf-kicker) {
  max-width: 680px;
  color: var(--sf-muted);
}

.site-meta-row {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
  margin-top: 18px;
  color: var(--sf-muted);
}

.site-meta-row strong {
  color: var(--sf-ink);
  overflow-wrap: anywhere;
}

.site-meta-row small {
  max-width: 560px;
  line-height: 1.4;
}

.health-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 0;
  overflow: hidden;
  border: 1px solid var(--workspace-line);
  border-radius: 8px;
  background: var(--sf-surface);
}

.health-grid div {
  min-width: 0;
  border-left: 1px solid var(--workspace-line-soft);
  padding: 15px;
  transition: border-color 280ms ease, background 280ms ease;
}

.health-grid div:first-child {
  border-left: 0;
}

.health-grid span {
  display: block;
  color: var(--sf-muted);
  font-size: 13px;
  font-weight: 750;
}

.health-grid strong {
  display: block;
  margin-top: 5px;
  overflow-wrap: anywhere;
  font-size: 22px;
  line-height: 1.1;
  color: var(--sf-ink);
}

/* ── Pages table ── */
.pages-panel {
  padding: 22px;
}

.content-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: 18px;
  margin-bottom: 0;
  padding-bottom: 18px;
  border-bottom: 1px solid var(--workspace-line);
}

.content-header h2 {
  font-size: 24px;
  color: var(--sf-ink);
}

.content-header p,
.muted-panel {
  color: var(--sf-muted);
}

.content-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
}

.page-table {
  border: 1px solid var(--workspace-line);
  border-radius: 8px;
  margin-top: 18px;
  overflow: hidden;
  background: var(--sf-surface);
  transition: border-color 280ms ease, background 280ms ease;
}

.page-row {
  display: grid;
  grid-template-columns: minmax(180px, 1.3fr) minmax(140px, 1fr) 120px 190px;
  gap: 16px;
  align-items: center;
  padding: 15px 18px;
  border-top: 1px solid var(--workspace-line);
  transition: border-color 280ms ease;
}

.page-row:first-child {
  border-top: 0;
}

.page-row-head {
  background: var(--sf-surface-2);
  color: var(--sf-muted);
  font-size: 13px;
  font-weight: 850;
}

.page-row strong {
  display: block;
  color: var(--sf-ink);
}

.page-row small {
  display: inline-block;
  margin-top: 4px;
  color: var(--sf-primary);
  font-weight: 800;
}

.page-slug {
  color: var(--sf-muted);
  overflow-wrap: anywhere;
}

.row-actions {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
}

@media (min-width: 841px) {
  .workspace-hero > .health-grid {
    align-self: stretch;
  }

  .page-row > * {
    min-width: 0;
  }

  .page-row > * + * {
    border-left: 1px solid var(--workspace-line-soft);
    padding-left: 16px;
  }
}

/* ── Modal ── */
.page-modal h3,
.ai-page-modal h3 {
  margin: 6px 0 18px;
  font-size: 26px;
  color: var(--sf-ink);
}

.page-modal form,
.ai-page-modal form {
  display: grid;
  gap: 15px;
}

.page-modal label,
.ai-page-modal label {
  font-weight: 750;
  color: var(--sf-ink);
}

.page-modal input,
.ai-page-modal input,
.ai-page-modal select,
.ai-page-modal textarea {
  margin-top: 7px;
}

.ai-page-modal {
  width: min(920px, 100%);
  max-height: min(86vh, 860px);
  overflow: auto;
}

.ai-page-modal textarea {
  min-height: 140px;
  resize: vertical;
}

.ai-page-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 12px;
}

.template-field {
  display: grid;
  gap: 8px;
}

.field-label {
  font-weight: 750;
  color: var(--sf-ink);
}

.template-picker {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: 12px;
}

.template-card {
  min-height: 186px;
  border: 1px solid var(--sf-line);
  border-radius: 10px;
  background: var(--sf-surface);
  color: var(--sf-ink);
  cursor: pointer;
  padding: 10px;
  text-align: left;
  transition: border-color 160ms ease, background 160ms ease, transform 160ms ease;
}

.template-card:hover,
.template-card.selected {
  border-color: var(--sf-primary);
  background: var(--sf-surface-hover);
}

.template-card.selected {
  box-shadow: inset 0 0 0 1px var(--sf-primary);
}

.template-card strong {
  display: block;
  margin-top: 10px;
  font-size: 14px;
}

.template-card small {
  display: block;
  margin-top: 4px;
  color: var(--sf-muted);
  font-size: 12px;
  line-height: 1.35;
}

.template-preview {
  position: relative;
  height: 94px;
  display: grid;
  grid-template-rows: 24px 1fr 18px;
  gap: 7px;
  overflow: hidden;
  border: 1px solid var(--sf-line);
  border-radius: 8px;
  padding: 8px;
  background: linear-gradient(135deg, #2a2440, #15161b);
}

.template-preview.has-image {
  display: block;
  padding: 0;
  background: #101116;
}

.template-preview-image {
  width: 100%;
  height: 100%;
  display: block;
  object-fit: cover;
}

.template-preview span {
  display: block;
  border-radius: 5px;
  background: rgba(255,255,255,.72);
}

.template-preview span:nth-child(2) {
  width: 70%;
}

.template-preview span:nth-child(3) {
  width: 48%;
}

.blank-preview {
  grid-template-rows: 1fr;
  place-items: center;
  background: var(--sf-surface-2);
  color: var(--sf-muted);
  font-weight: 900;
}

.preview-page-anti-counterfeit {
  background: linear-gradient(135deg, #101828, #3a7bd5);
}

.preview-page-scan-result {
  background: linear-gradient(135deg, #f8fbff, #2374ab);
}

.preview-page-lottery {
  background: linear-gradient(135deg, #fff2cc, #ef476f);
}

.preview-page-points-redemption {
  background: linear-gradient(135deg, #ecfdf3, #12b76a);
}

.preview-page-traceability {
  background: linear-gradient(135deg, #172554, #84cc16);
}

.preview-page-dpp {
  background: linear-gradient(135deg, #f9fafb, #111827);
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  margin-top: 4px;
}

/* ── Responsive ── */
@media (max-width: 1100px) {
  .workspace-hero {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 840px) {
  .workspace-header {
    flex-direction: column;
    align-items: stretch;
  }
  .workspace-content {
    padding: 18px;
  }
  .health-grid {
    grid-template-columns: 1fr;
  }
  .health-grid div {
    border-left: 0;
    border-top: 1px solid var(--workspace-line-soft);
  }
  .health-grid div:first-child {
    border-top: 0;
  }
  .page-row {
    grid-template-columns: 1fr;
  }
  .row-actions,
  .content-actions,
  .content-header,
  .header-actions,
  .modal-actions {
    align-items: stretch;
    flex-direction: column;
  }
  .ai-page-grid {
    grid-template-columns: 1fr;
  }
}
</style>
