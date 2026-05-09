
<template>
  <div class="sf-dashboard">
    <!-- Top Header -->
    <header class="sf-header">
      <div class="sf-header-left">
        <h2 class="sf-title-lg" style="color: var(--sf-on-surface);">{{ t('dashboard.myProjects') }}</h2>
        <span class="sf-count-badge">{{ sites.length }}</span>
      </div>
      <div class="sf-header-right">
        <div class="sf-search-wrap">
          <span class="material-symbols-outlined sf-search-icon">search</span>
          <input 
            v-model="search" 
            type="text" 
            class="sf-input sf-search-input" 
            :placeholder="t('dashboard.searchPlaceholder')"
          />
        </div>
        <button class="sf-icon-btn">
          <span class="material-symbols-outlined">filter_list</span>
        </button>
      </div>
    </header>

    <!-- Projects Grid -->
    <div class="sf-content">
      <div v-if="loading" class="sf-loading">{{ t('dashboard.loadingProjects') }}</div>
      
      <div v-else-if="filteredSites.length > 0" class="sf-grid">
        <!-- Featured card (first project, spans 2 columns on large) -->
        <div 
          v-if="filteredSites[0]" 
          class="sf-card sf-card-featured"
          @click="openSite(filteredSites[0].id)"
        >
          <div class="sf-card-media">
            <div class="sf-card-thumb" :class="{ published: filteredSites[0].status === 'published' }">
              <span class="material-symbols-outlined" style="font-size: 48px; opacity: 0.3;">web</span>
            </div>
            <div class="sf-card-gradient"></div>
            <div class="sf-card-status">
              <span class="sf-badge" :class="filteredSites[0].status === 'published' ? 'sf-badge-published' : 'sf-badge-draft'">
                {{ filteredSites[0].status === 'published' ? t('common.published') : t('common.draft') }}
              </span>
            </div>
          </div>
          <div class="sf-card-body">
            <div class="sf-card-header">
              <h3 class="sf-title-md sf-card-title">{{ filteredSites[0].name }}</h3>
              <button class="sf-icon-btn sf-card-menu" @click.stop="showSiteMenu(filteredSites[0])">
                <span class="material-symbols-outlined" style="font-size: 20px;">more_vert</span>
              </button>
            </div>
            <p class="sf-body-md sf-card-desc">{{ filteredSites[0].description || t('common.noDescription') }}</p>
            <div class="sf-card-meta">
              <span class="sf-label-sm">
                <span class="material-symbols-outlined" style="font-size: 14px;">edit_calendar</span>
                {{ formatDate(filteredSites[0].updatedAt) }}
              </span>
            </div>
          </div>
        </div>

        <!-- Regular cards -->
        <div 
          v-for="site in filteredSites.slice(1)" 
          :key="site.id" 
          class="sf-card"
          @click="openSite(site.id)"
        >
          <div class="sf-card-media">
            <div class="sf-card-thumb" :class="{ published: site.status === 'published' }">
              <span class="material-symbols-outlined" style="font-size: 36px; opacity: 0.3;">web</span>
            </div>
            <div class="sf-card-gradient"></div>
            <div class="sf-card-status">
              <span class="sf-badge" :class="site.status === 'published' ? 'sf-badge-published' : 'sf-badge-draft'">
                {{ site.status === 'published' ? t('common.published') : t('common.draft') }}
              </span>
            </div>
          </div>
          <div class="sf-card-body">
            <div class="sf-card-header">
              <h3 class="sf-title-md sf-card-title">{{ site.name }}</h3>
              <button class="sf-icon-btn sf-card-menu" @click.stop="showSiteMenu(site)">
                <span class="material-symbols-outlined" style="font-size: 20px;">more_vert</span>
              </button>
            </div>
            <div class="sf-card-meta">
              <span class="sf-label-sm">
                <span class="material-symbols-outlined" style="font-size: 14px;">edit_calendar</span>
                {{ formatDate(site.updatedAt) }}
              </span>
            </div>
          </div>
        </div>

        <!-- Create new card -->
        <div class="sf-card sf-card-new" @click="showCreate = true">
          <div class="sf-card-new-inner">
            <div class="sf-new-icon">
              <span class="material-symbols-outlined" style="font-size: 28px;">add</span>
            </div>
            <h3 class="sf-title-md" style="color: var(--sf-on-surface);">{{ t('dashboard.createNewSite') }}</h3>
            <p class="sf-label-sm" style="color: var(--sf-on-surface-variant);">{{ t('dashboard.startFromScratch') }}</p>
          </div>
        </div>
      </div>

      <div v-else class="sf-empty">
        <span class="material-symbols-outlined" style="font-size: 48px; color: var(--sf-on-surface-variant); opacity: 0.5;">folder_open</span>
        <h3 class="sf-title-md" style="color: var(--sf-on-surface-variant); margin-top: 16px;">{{ t('dashboard.noProjects') }}</h3>
        <button class="sf-btn-primary" @click="showCreate = true" style="margin-top: 16px;">
          <span class="material-symbols-outlined material-symbols-filled">add</span>
          {{ t('dashboard.createFirstProject') }}
        </button>
      </div>
    </div>

    <!-- Create Modal -->
    <div v-if="showCreate" class="sf-modal-overlay" @click.self="showCreate = false">
      <div class="sf-modal">
        <h2 class="sf-headline-sm" style="margin-bottom: 24px; color: var(--sf-on-surface);">{{ t('dashboard.createNewProject') }}</h2>
        <p v-if="createError" class="sf-create-error">{{ createError }}</p>
        <div class="sf-form-field">
          <label class="sf-label-lg" style="color: var(--sf-on-surface-variant); display: block; margin-bottom: 8px;">{{ t('dashboard.siteName') }}</label>
          <input 
            v-model="newSite.name" 
            type="text" 
            class="sf-input" 
            :placeholder="t('dashboard.siteNamePlaceholder')"
            style="padding-left: 16px; border-radius: 12px;"
          />
        </div>
        <div class="sf-form-field" style="margin-top: 16px;">
          <label class="sf-label-lg" style="color: var(--sf-on-surface-variant); display: block; margin-bottom: 8px;">{{ t('dashboard.description') }}</label>
          <input 
            v-model="newSite.description" 
            type="text" 
            class="sf-input" 
            :placeholder="t('dashboard.descriptionPlaceholder')"
            style="padding-left: 16px; border-radius: 12px;"
          />
        </div>
        <div class="sf-form-field" style="margin-top: 16px;">
          <label class="sf-label-lg" style="color: var(--sf-on-surface-variant); display: block; margin-bottom: 8px;">{{ t('dashboard.siteTemplate') }}</label>
          <div class="template-picker site-template-picker">
            <button
              type="button"
              class="template-card blank"
              :class="{ selected: selectedTemplate === '' }"
              @click="selectedTemplate = ''"
            >
              <div class="template-preview blank-preview">
                <span class="material-symbols-outlined">add</span>
              </div>
              <strong>{{ t('dashboard.blankSite') }}</strong>
              <small>{{ t('dashboard.blankSiteHint') }}</small>
            </button>
            <button
              v-for="template in siteTemplates"
              :key="template.key"
              type="button"
              class="template-card"
              :class="{ selected: selectedTemplate === template.key }"
              @click="selectedTemplate = template.key"
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
              <small>{{ template.category }} / {{ t('dashboard.pagesCount', { count: template.pageCount }) }}</small>
            </button>
          </div>
          <p class="template-hint">
            {{ selectedTemplateInfo?.description || t('dashboard.templateHint') }}
          </p>
        </div>
        <div class="sf-modal-actions">
          <button class="sf-btn-text" @click="showCreate = false" :disabled="creatingSite">{{ t('common.cancel') }}</button>
          <button class="sf-btn-primary" @click="createSite" :disabled="creatingSite">
            {{ creatingSite ? t('dashboard.creating') : selectedTemplate ? t('dashboard.applyTemplate') : t('dashboard.createAndOpen') }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useLocaleStore } from '../stores/locale'
import api from '../api/client'

const router = useRouter()
const auth = useAuthStore()
const localeStore = useLocaleStore()
const t = localeStore.t

const sites = ref([])
const templates = ref([])
const loading = ref(true)
const creatingSite = ref(false)
const createError = ref('')
const search = ref('')
const showCreate = ref(false)
const newSite = ref({ name: '', description: '' })
const selectedTemplate = ref('')

const filteredSites = computed(() => {
  if (!search.value) return sites.value
  const q = search.value.toLowerCase()
  return sites.value.filter(s => s.name?.toLowerCase().includes(q))
})

const siteTemplates = computed(() => templates.value.filter((template) => template.kind === 'site'))
const selectedTemplateInfo = computed(() => siteTemplates.value.find((template) => template.key === selectedTemplate.value))

const formatDate = (d) => {
  if (!d) return t('common.recently')
  const date = new Date(d)
  const now = new Date()
  const diff = (now - date) / 1000 / 60 / 60 // hours
  if (diff < 1) return t('common.justNow')
  if (diff < 24) return t('common.hoursAgo', { count: Math.floor(diff) })
  if (diff < 48) return t('common.yesterday')
  return date.toLocaleDateString(localeStore.locale === 'en' ? 'en-US' : 'zh-TW', { month: 'short', day: 'numeric' })
}

const loadSites = async () => {
  loading.value = true
  try {
    const res = await api.get('/Sites')
    const data = res.data?.data || res.data || []
    sites.value = Array.isArray(data) ? data : []
  } catch (e) {
    console.error('Failed to load sites:', e)
    sites.value = []
  } finally {
    loading.value = false
  }
}

const loadTemplates = async () => {
  try {
    const res = await api.get('/AiConversations/templates?kind=site')
    templates.value = res.data?.data || res.data || []
  } catch (e) {
    console.error('Failed to load templates:', e)
    templates.value = []
  }
}

const createSite = async () => {
  if (creatingSite.value) return
  createError.value = ''
  creatingSite.value = true
  try {
    const fallbackName = selectedTemplateInfo.value?.label || 'SiteForge Project'
    const siteName = newSite.value.name.trim() || `${fallbackName} ${Date.now()}`
    const description = newSite.value.description.trim()

    if (selectedTemplate.value) {
      const res = await api.post('/AiConversations/generate-site', {
        siteName,
        description,
        prompt: description || `Create a complete business website for ${siteName}.`,
        templateKey: selectedTemplate.value
      })
      const generated = res.data?.data || res.data
      if (generated?.siteId) {
        router.push(`/sites/${generated.siteId}`)
      }
      return
    }

    const res = await api.post('/Sites', { name: siteName, description })
    const site = res.data?.data || res.data
    if (site?.id) {
      router.push(`/sites/${site.id}`)
    }
  } catch (e) {
    console.error('Failed to create site:', e)
    createError.value = `${t('common.operationFailed')}: ${e.response?.data?.message || e.message}`
  } finally {
    creatingSite.value = false
  }
}

const openSite = (id) => {
  router.push(`/sites/${id}`)
}

const showSiteMenu = (site) => {
  // TODO: implement site context menu
  console.log('Site menu:', site.name)
}

const templatePreviewClass = (key) => `preview-${key.replace(/[^a-z0-9]+/gi, '-')}`

onMounted(() => {
  loadSites()
  loadTemplates()
})
</script>

<style scoped>
.sf-dashboard {
  display: flex;
  flex-direction: column;
  height: 100vh;
  overflow: hidden;
}

.sf-header {
  height: 64px;
  padding: 0 24px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: var(--sf-bg);
  border-bottom: 1px solid var(--sf-outline-variant);
  flex-shrink: 0;
  position: sticky;
  top: 0;
  z-index: 40;
}

.sf-header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.sf-count-badge {
  background: var(--sf-surface-container-high);
  color: var(--sf-on-surface-variant);
  padding: 2px 10px;
  border-radius: 9999px;
  font-size: 12px;
  font-weight: 500;
}

.sf-header-right {
  display: flex;
  align-items: center;
  gap: 16px;
}

.sf-search-wrap {
  position: relative;
  width: 256px;
}

.sf-search-icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--sf-on-surface-variant);
  font-size: 18px;
}

.sf-search-input {
  padding-left: 40px;
}

.sf-icon-btn {
  background: none;
  border: none;
  color: var(--sf-on-surface-variant);
  cursor: pointer;
  padding: 8px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s;
}

.sf-icon-btn:hover {
  background: var(--sf-surface-variant);
  color: var(--sf-on-surface);
}

.sf-content {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
}

.sf-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 24px;
  max-width: 1600px;
}

@media (min-width: 1024px) {
  .sf-grid {
    grid-template-columns: repeat(3, 1fr);
  }
  .sf-card-featured {
    grid-column: span 2;
  }
}

.sf-card {
  background: var(--sf-surface-container-low);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 16px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.3s ease;
}

.sf-card:hover {
  border-color: rgba(208, 188, 255, 0.3);
  box-shadow: 0 8px 30px rgba(0,0,0,0.4);
}

[data-theme="light"] .sf-card:hover {
  border-color: rgba(79, 55, 138, 0.3);
  box-shadow: 0 8px 24px rgba(0,0,0,0.1);
}

.sf-card-featured {
  display: flex;
  flex-direction: column;
}

@media (min-width: 1024px) {
  .sf-card-featured {
    flex-direction: row;
  }
}

.sf-card-media {
  position: relative;
  height: 160px;
  overflow: hidden;
}

.sf-card-featured .sf-card-media {
  height: 200px;
}

@media (min-width: 1024px) {
  .sf-card-featured .sf-card-media {
    width: 60%;
    height: auto;
  }
}

.sf-card-thumb {
  width: 100%;
  height: 100%;
  background: var(--sf-surface-variant);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--sf-on-surface-variant);
  transition: transform 0.7s ease;
}

.sf-card:hover .sf-card-thumb {
  transform: scale(1.05);
}

.sf-card-thumb.published {
  background: linear-gradient(135deg, var(--sf-primary-container), var(--sf-secondary-container));
}

.sf-card-gradient {
  position: absolute;
  inset: 0;
  background: linear-gradient(to top, var(--sf-surface-container-low) 20%, transparent 100%);
}

.sf-card-status {
  position: absolute;
  top: 12px;
  left: 12px;
}

.sf-badge {
  padding: 4px 12px;
  border-radius: 9999px;
  font-size: 12px;
  font-weight: 500;
  backdrop-filter: blur(8px);
}

.sf-card-body {
  padding: 16px;
  flex: 1;
  display: flex;
  flex-direction: column;
}

.sf-card-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  margin-bottom: 8px;
}

.sf-card-title {
  color: var(--sf-on-surface);
  transition: color 0.2s;
}

.sf-card:hover .sf-card-title {
  color: var(--sf-primary);
}

.sf-card-menu {
  opacity: 0;
  transition: opacity 0.2s;
}

.sf-card:hover .sf-card-menu {
  opacity: 1;
}

.sf-card-desc {
  color: var(--sf-on-surface-variant);
  margin-bottom: 16px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.sf-card-meta {
  margin-top: auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  color: var(--sf-on-surface-variant);
  opacity: 0.7;
}

.sf-card-meta span {
  display: flex;
  align-items: center;
  gap: 4px;
}

.sf-card-new {
  border-style: dashed;
  border-color: var(--sf-outline-variant);
  background: var(--sf-surface-container-lowest);
  min-height: 240px;
}

.sf-card-new:hover {
  border-color: var(--sf-primary);
  background: var(--sf-surface-container-low);
}

.sf-card-new-inner {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 100%;
  padding: 24px;
}

.sf-new-icon {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: var(--sf-surface-variant);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 12px;
  color: var(--sf-on-surface-variant);
}

.sf-loading, .sf-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 400px;
  color: var(--sf-on-surface-variant);
}

/* Modal */
.sf-modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.6);
  backdrop-filter: blur(4px);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 100;
}

.sf-modal {
  background: var(--sf-surface-container);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 24px;
  padding: 32px;
  max-width: 860px;
  width: 90%;
  max-height: min(86vh, 820px);
  overflow: auto;
}

.sf-form-field {
  margin-bottom: 16px;
}

.sf-create-error {
  margin: 0 0 16px;
  border: 1px solid color-mix(in srgb, var(--sf-error) 45%, var(--sf-outline-variant));
  border-radius: 12px;
  padding: 12px 14px;
  background: var(--sf-error-container);
  color: var(--sf-on-error-container);
  font-size: 14px;
  line-height: 1.45;
}

.template-hint {
  margin: 8px 0 0;
  color: var(--sf-on-surface-variant);
  font-size: 12px;
  line-height: 1.5;
}

.template-picker {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(160px, 1fr));
  gap: 12px;
}

.template-card {
  min-height: 190px;
  border: 1px solid var(--sf-outline-variant);
  border-radius: 12px;
  background: var(--sf-surface-container-low);
  color: var(--sf-on-surface);
  cursor: pointer;
  padding: 10px;
  text-align: left;
  transition: border-color 160ms ease, background 160ms ease, transform 160ms ease;
}

.template-card:hover,
.template-card.selected {
  border-color: var(--sf-primary);
  background: var(--sf-surface-container-high);
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
  color: var(--sf-on-surface-variant);
  font-size: 12px;
  line-height: 1.35;
}

.template-preview {
  position: relative;
  height: 96px;
  display: grid;
  grid-template-rows: 24px 1fr 18px;
  gap: 7px;
  overflow: hidden;
  border: 1px solid var(--sf-outline-variant);
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
  background: rgba(255,255,255,.68);
}

.template-preview span:nth-child(2) {
  width: 70%;
}

.template-preview span:nth-child(3) {
  width: 48%;
}

.blank-preview {
  place-items: center;
  grid-template-rows: 1fr;
  color: var(--sf-on-surface-variant);
  background: var(--sf-surface-container);
}

.blank-preview .material-symbols-outlined {
  font-size: 34px;
}

.preview-site-retail {
  background: linear-gradient(135deg, #f7f2ea, #173154);
}

.preview-site-beauty {
  background: linear-gradient(135deg, #f5dfcf, #6b5c4c);
}

.preview-site-beverage {
  background: linear-gradient(135deg, #381c18, #d8a85d);
}

.preview-site-3c {
  background: linear-gradient(135deg, #071827, #2f80ed);
}

.sf-modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}

.sf-btn-text {
  background: none;
  border: none;
  color: var(--sf-on-surface-variant);
  padding: 10px 20px;
  border-radius: 9999px;
  cursor: pointer;
  font-size: 14px;
  font-weight: 500;
  transition: all 0.2s;
}

.sf-btn-text:hover {
  background: var(--sf-surface-variant);
  color: var(--sf-on-surface);
}
</style>
