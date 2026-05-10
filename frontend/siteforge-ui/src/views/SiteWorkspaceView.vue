<template>
  <div class="sf-workspace" :class="`theme-${theme}`">
    <!-- Breadcrumb -->
    <nav class="sf-ws-breadcrumb">
      <span class="sf-ws-breadcrumb-item">Sites</span>
      <span class="material-symbols-outlined sf-ws-breadcrumb-sep">chevron_right</span>
      <span class="sf-ws-breadcrumb-current">{{ site?.name || 'Loading...' }}</span>
    </nav>

    <!-- Header -->
    <header class="sf-ws-header">
      <h1 class="sf-ws-title">Page Management</h1>
      <button class="sf-ws-new-btn" @click="showCreatePage = true" type="button">
        <span class="material-symbols-outlined">add</span>
        New Page
      </button>
    </header>

    <!-- Tabs -->
    <div class="sf-ws-tabs">
      <button
        :class="{ active: activeTab === 'pages' }"
        @click="activeTab = 'pages'"
        type="button"
      >
        Pages
      </button>
      <button
        :class="{ active: activeTab === 'templates' }"
        @click="activeTab = 'templates'"
        type="button"
      >
        Templates
      </button>
      <button
        :class="{ active: activeTab === 'theme' }"
        @click="activeTab = 'theme'"
        type="button"
      >
        Theme
      </button>
    </div>

    <!-- Pages Tab -->
    <div v-show="activeTab === 'pages'" class="sf-ws-tab-content">
      <div v-if="loading" class="sf-ws-loading">Loading pages...</div>

      <div v-else-if="pages.length > 0" class="sf-ws-grid">
        <div
          v-for="page in pages"
          :key="page.id"
          class="sf-ws-card"
          @click="openEditor(page.id)"
        >
          <div class="sf-ws-card-header">
            <div class="sf-ws-card-icon" :class="page.isHome ? 'sf-ws-card-icon-home' : ''">
              <span class="material-symbols-outlined">{{ page.isHome ? 'home' : 'description' }}</span>
            </div>
            <div class="sf-ws-card-info">
              <h3 class="sf-ws-card-title">{{ page.title }}</h3>
              <p class="sf-ws-card-type">{{ page.isHome ? 'Home Page' : 'Standard Page' }}</p>
            </div>
            <button
              class="sf-ws-card-menu"
              @click.stop="showPageMenu(page)"
              type="button"
            >
              <span class="material-symbols-outlined">more_vert</span>
            </button>
          </div>

          <div class="sf-ws-card-footer">
            <span class="sf-ws-card-time">
              <span class="material-symbols-outlined">schedule</span>
              {{ formatDate(page.updatedAt) }}
            </span>
            <div class="sf-ws-card-status">
              <span :class="page.isPublished ? 'sf-ws-status-published' : 'sf-ws-status-draft'">
                {{ page.isPublished ? 'Published' : 'Draft' }}
              </span>
              <label class="sf-ws-toggle">
                <input
                  type="checkbox"
                  :checked="page.isPublished"
                  @click.stop="togglePagePublish(page)"
                />
                <span class="sf-ws-toggle-track"></span>
              </label>
            </div>
          </div>
        </div>
      </div>

      <div v-else class="sf-ws-empty">
        <span class="material-symbols-outlined">folder_open</span>
        <h3>No pages yet</h3>
        <p>Create your first page to get started</p>
        <button class="sf-ws-new-btn" @click="showCreatePage = true" type="button">
          <span class="material-symbols-outlined">add</span>
          New Page
        </button>
      </div>
    </div>

    <!-- Templates Tab -->
    <div v-show="activeTab === 'templates'" class="sf-ws-tab-content">
      <div class="sf-ws-section-header">
        <h2>Available Templates</h2>
        <p>Pre-built sections to rapidly assemble your pages</p>
      </div>
      <div class="sf-ws-templates-grid">
        <div class="sf-ws-template-card" v-for="i in 6" :key="i">
          <div class="sf-ws-template-thumb">
            <div class="sf-ws-template-placeholder">
              <span class="material-symbols-outlined">view_quilt</span>
            </div>
          </div>
          <div class="sf-ws-template-info">
            <h4>Template {{ i }}</h4>
            <p>Section layout</p>
          </div>
          <button class="sf-ws-template-add" type="button">
            <span class="material-symbols-outlined">add</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Theme Tab -->
    <div v-show="activeTab === 'theme'" class="sf-ws-tab-content">
      <div class="sf-ws-section-header">
        <h2>Site Theme</h2>
        <p>Customize colors, typography, and global styles</p>
      </div>
      <div class="sf-ws-theme-placeholder">
        <span class="material-symbols-outlined">palette</span>
        <p>Theme settings coming soon</p>
      </div>
    </div>

    <!-- Create Page Modal -->
    <div v-if="showCreatePage" class="sf-ws-overlay" @click.self="showCreatePage = false">
      <div class="sf-ws-modal">
        <h2>Create New Page</h2>
        <form @submit.prevent="createPage">
          <label>
            Page Title
            <input v-model="newPage.title" class="sf-ws-input" required placeholder="e.g. About Us" />
          </label>
          <label>
            URL Slug
            <input v-model="newPage.slug" class="sf-ws-input" placeholder="about-us" />
          </label>
          <div class="sf-ws-modal-actions">
            <button type="button" class="sf-ws-btn" @click="showCreatePage = false">Cancel</button>
            <button class="sf-ws-btn sf-ws-btn-primary" :disabled="creatingPage">
              {{ creatingPage ? 'Creating...' : 'Create Page' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Page Menu Dropdown -->
    <div v-if="pageMenuOpen" class="sf-ws-menu-overlay" @click.self="pageMenuOpen = false">
      <div class="sf-ws-menu" :style="menuPosition">
        <button @click="editPage(selectedPage)" type="button">
          <span class="material-symbols-outlined">edit</span>
          Edit
        </button>
        <button @click="duplicatePage(selectedPage)" type="button">
          <span class="material-symbols-outlined">content_copy</span>
          Duplicate
        </button>
        <button
          @click="deletePage(selectedPage.id)"
          type="button"
          :disabled="selectedPage?.isHome"
          class="sf-ws-menu-danger"
        >
          <span class="material-symbols-outlined">delete</span>
          Delete
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import api, { unwrap } from '../api/client.js'

const route = useRoute()
const router = useRouter()

const siteId = computed(() => route.params.siteId)

// Theme
const theme = ref(localStorage.getItem('sf-theme') || 'dark')

// Data
const site = ref(null)
const pages = ref([])
const loading = ref(true)

// UI State
const activeTab = ref('pages')
const showCreatePage = ref(false)
const creatingPage = ref(false)
const pageMenuOpen = ref(false)
const selectedPage = ref(null)
const menuPosition = ref({ top: '0px', left: '0px' })

const newPage = ref({
  title: '',
  slug: ''
})

// Computed
const publishedPages = computed(() => pages.value.filter(p => p.isPublished).length)

// Fetch
async function fetchSite() {
  try {
    const response = await api.get(`/Sites/${siteId.value}`)
    site.value = unwrap(response)
  } catch (err) {
    console.error('Failed to load site:', err)
  }
}

async function fetchPages() {
  loading.value = true
  try {
    const response = await api.get(`/Pages/site/${siteId.value}`)
    pages.value = unwrap(response) || []
  } catch (err) {
    console.error('Failed to load pages:', err)
  } finally {
    loading.value = false
  }
}

// Actions
async function createPage() {
  if (!newPage.value.title) return

  creatingPage.value = true
  try {
    const response = await api.post(`/Pages/site/${siteId.value}`, {
      title: newPage.value.title,
      slug: newPage.value.slug || '',
      isHome: pages.value.length === 0
    })
    const created = unwrap(response)
    pages.value.push(created)
    newPage.value = { title: '', slug: '' }
    showCreatePage.value = false

    // Open editor for new page
    router.push(`/editor/${siteId.value}/${created.id}`)
  } catch (err) {
    console.error('Failed to create page:', err)
    alert('Failed to create page')
  } finally {
    creatingPage.value = false
  }
}

async function deletePage(pageId) {
  if (!confirm('Are you sure you want to delete this page?')) return

  try {
    await api.delete(`/Pages/${pageId}`)
    pages.value = pages.value.filter(p => p.id !== pageId)
    pageMenuOpen.value = false
  } catch (err) {
    console.error('Failed to delete page:', err)
    alert('Failed to delete page')
  }
}

async function togglePagePublish(page) {
  try {
    const updated = { ...page, isPublished: !page.isPublished }
    await api.put(`/Pages/${page.id}`, updated)
    page.isPublished = !page.isPublished
  } catch (err) {
    console.error('Failed to update page:', err)
    alert('Failed to update page status')
  }
}

function openEditor(pageId) {
  router.push(`/editor/${siteId.value}/${pageId}`)
}

function editPage(page) {
  pageMenuOpen.value = false
  openEditor(page.id)
}

function duplicatePage(page) {
  pageMenuOpen.value = false
  // TODO: Implement duplicate
  alert('Duplicate feature coming soon')
}

function showPageMenu(page) {
  selectedPage.value = page
  pageMenuOpen.value = true
  // Position would be calculated based on click event in real implementation
}

function formatDate(dateString) {
  if (!dateString) return 'Never'
  const date = new Date(dateString)
  const now = new Date()
  const diff = now - date
  const hours = Math.floor(diff / (1000 * 60 * 60))
  const days = Math.floor(hours / 24)

  if (hours < 1) return 'Just now'
  if (hours < 24) return `${hours} hour${hours > 1 ? 's' : ''} ago`
  if (days < 7) return `${days} day${days > 1 ? 's' : ''} ago`
  return date.toLocaleDateString()
}

// Lifecycle
onMounted(() => {
  fetchSite()
  fetchPages()
})
</script>

<style scoped>
.sf-workspace {
  padding: 24px 32px;
  max-width: 1200px;
  margin: 0 auto;
  min-height: calc(100vh - 56px);
  background: var(--sf-bg);
}

/* Breadcrumb */
.sf-ws-breadcrumb {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 16px;
  font-size: 13px;
  color: var(--sf-on-surface-variant);
}

.sf-ws-breadcrumb-item {
  cursor: pointer;
}

.sf-ws-breadcrumb-item:hover {
  color: var(--sf-primary);
}

.sf-ws-breadcrumb-sep {
  font-size: 16px;
  color: var(--sf-outline);
}

.sf-ws-breadcrumb-current {
  color: var(--sf-primary);
  font-weight: 500;
}

/* Header */
.sf-ws-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 24px;
}

.sf-ws-title {
  font-size: 28px;
  font-weight: 600;
  color: var(--sf-on-surface);
  margin: 0;
}

.sf-ws-new-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 20px;
  background: var(--sf-primary);
  color: var(--sf-on-primary);
  border: none;
  border-radius: 10px;
  font-size: 14px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-ws-new-btn:hover {
  filter: brightness(1.1);
}

/* Tabs */
.sf-ws-tabs {
  display: flex;
  gap: 8px;
  margin-bottom: 24px;
  padding: 4px;
  background: var(--sf-surface-variant);
  border-radius: 12px;
  border: 1px solid var(--sf-outline-variant);
  width: fit-content;
}

.sf-ws-tabs button {
  padding: 10px 20px;
  border: none;
  background: transparent;
  color: var(--sf-on-surface-variant);
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  border-radius: 10px;
  transition: all 0.15s;
}

.sf-ws-tabs button.active {
  color: var(--sf-on-primary);
  background: var(--sf-primary);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.sf-ws-tabs button:hover:not(.active) {
  color: var(--sf-on-surface);
  background: var(--sf-surface-container-high);
}

/* Tab Content */
.sf-ws-tab-content {
  animation: fadeIn 0.2s ease;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(4px); }
  to { opacity: 1; transform: translateY(0); }
}

/* Grid */
.sf-ws-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 16px;
}

/* Card */
.sf-ws-card {
  background: var(--sf-surface-container-lowest);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 16px;
  padding: 20px;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.sf-ws-card:hover {
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  border-color: var(--sf-outline);
  transform: translateY(-2px);
}

.sf-ws-card-header {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  margin-bottom: 16px;
}

.sf-ws-card-icon {
  width: 44px;
  height: 44px;
  border-radius: 12px;
  background: var(--sf-surface-variant);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--sf-on-surface-variant);
  flex-shrink: 0;
  border: 1px solid var(--sf-outline-variant);
}

.sf-ws-card-icon-home {
  background: var(--sf-primary-container);
  color: var(--sf-primary);
  border-color: var(--sf-primary-fixed-dim);
}

.sf-ws-card-icon .material-symbols-outlined {
  font-size: 22px;
}

.sf-ws-card-info {
  flex: 1;
  min-width: 0;
}

.sf-ws-card-title {
  font-size: 16px;
  font-weight: 600;
  color: var(--sf-on-surface);
  margin: 0 0 4px;
}

.sf-ws-card-type {
  font-size: 12px;
  color: var(--sf-on-surface-variant);
  margin: 0;
}

.sf-ws-card-menu {
  width: 32px;
  height: 32px;
  border: none;
  background: transparent;
  color: var(--sf-on-surface-variant);
  border-radius: 6px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  opacity: 0;
  transition: all 0.15s;
}

.sf-ws-card:hover .sf-ws-card-menu {
  opacity: 1;
}

.sf-ws-card-menu:hover {
  background: var(--sf-surface-variant);
  color: var(--sf-on-surface);
}

/* Card Footer */
.sf-ws-card-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-top: 16px;
  margin-top: 16px;
  border-top: 1px solid var(--sf-outline-variant);
}

.sf-ws-card-time {
  display: flex;
  align-items: center;
  gap: 6px;
  font-size: 12px;
  color: var(--sf-on-surface-variant);
}

.sf-ws-card-time .material-symbols-outlined {
  font-size: 14px;
}

.sf-ws-card-status {
  display: flex;
  align-items: center;
  gap: 10px;
}

.sf-ws-status-published {
  font-size: 12px;
  color: var(--sf-primary);
  font-weight: 600;
  padding: 2px 8px;
  background: var(--sf-primary-container);
  border-radius: 4px;
}

.sf-ws-status-draft {
  font-size: 12px;
  color: var(--sf-on-surface-variant);
  font-weight: 500;
  padding: 2px 8px;
  background: var(--sf-surface-variant);
  border-radius: 4px;
}

/* Toggle */
.sf-ws-toggle {
  position: relative;
  display: inline-block;
  width: 36px;
  height: 20px;
  cursor: pointer;
}

.sf-ws-toggle input {
  opacity: 0;
  width: 0;
  height: 0;
}

.sf-ws-toggle-track {
  position: absolute;
  inset: 0;
  background: var(--sf-surface-variant);
  border-radius: 10px;
  transition: all 0.2s;
}

.sf-ws-toggle-track::before {
  content: '';
  position: absolute;
  width: 16px;
  height: 16px;
  left: 2px;
  top: 2px;
  background: white;
  border-radius: 50%;
  transition: all 0.2s;
  box-shadow: 0 1px 3px rgba(0,0,0,0.2);
}

.sf-ws-toggle input:checked + .sf-ws-toggle-track {
  background: var(--sf-primary);
}

.sf-ws-toggle input:checked + .sf-ws-toggle-track::before {
  transform: translateX(16px);
}

/* Empty State */
.sf-ws-empty {
  text-align: center;
  padding: 64px 24px;
  color: var(--sf-on-surface-variant);
  background: var(--sf-surface-container-lowest);
  border: 2px dashed var(--sf-outline-variant);
  border-radius: 16px;
  margin: 24px 0;
}

.sf-ws-empty .material-symbols-outlined {
  font-size: 48px;
  margin-bottom: 16px;
  color: var(--sf-outline);
}

.sf-ws-empty h3 {
  font-size: 18px;
  font-weight: 600;
  color: var(--sf-on-surface);
  margin: 0 0 8px;
}

.sf-ws-empty p {
  font-size: 14px;
  margin: 0 0 24px;
}

/* Section Header */
.sf-ws-section-header {
  margin-bottom: 24px;
}

.sf-ws-section-header h2 {
  font-size: 22px;
  font-weight: 600;
  color: var(--sf-on-surface);
  margin: 0 0 4px;
}

.sf-ws-section-header p {
  font-size: 14px;
  color: var(--sf-on-surface-variant);
  margin: 0;
}

/* Templates */
.sf-ws-templates-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 16px;
}

.sf-ws-template-card {
  background: var(--sf-surface-container-lowest);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 16px;
  overflow: hidden;
  transition: all 0.2s;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.sf-ws-template-card:hover {
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
  border-color: var(--sf-outline);
  transform: translateY(-2px);
}

.sf-ws-template-thumb {
  height: 120px;
  background: var(--sf-surface-variant);
  display: flex;
  align-items: center;
  justify-content: center;
  border-bottom: 1px solid var(--sf-outline-variant);
}

.sf-ws-template-placeholder {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  background: var(--sf-surface-container-high);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--sf-outline);
}

.sf-ws-template-placeholder .material-symbols-outlined {
  font-size: 24px;
}

.sf-ws-template-info {
  padding: 12px;
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.sf-ws-template-info h4 {
  font-size: 14px;
  font-weight: 600;
  color: var(--sf-on-surface);
  margin: 0;
}

.sf-ws-template-info p {
  font-size: 12px;
  color: var(--sf-on-surface-variant);
  margin: 4px 0 0;
}

.sf-ws-template-add {
  width: 32px;
  height: 32px;
  border: none;
  background: var(--sf-surface-variant);
  color: var(--sf-on-surface-variant);
  border-radius: 8px;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.15s;
}

.sf-ws-template-add:hover {
  background: var(--sf-primary-container);
  color: var(--sf-primary);
}

/* Theme Placeholder */
.sf-ws-theme-placeholder {
  text-align: center;
  padding: 64px;
  color: var(--sf-on-surface-variant);
  background: var(--sf-surface-container-lowest);
  border: 2px dashed var(--sf-outline-variant);
  border-radius: 16px;
}

.sf-ws-theme-placeholder .material-symbols-outlined {
  font-size: 48px;
  margin-bottom: 16px;
  color: var(--sf-outline);
}

/* Modal */
.sf-ws-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
}

.sf-ws-modal {
  background: var(--sf-surface-container);
  border-radius: 20px;
  padding: 28px;
  width: 100%;
  max-width: 440px;
  border: 1px solid var(--sf-outline-variant);
  box-shadow: 0 24px 48px rgba(0, 0, 0, 0.25);
}

.sf-ws-modal h2 {
  font-size: 20px;
  font-weight: 600;
  color: var(--sf-on-surface);
  margin: 0 0 20px;
}

.sf-ws-modal label {
  display: block;
  font-size: 13px;
  font-weight: 500;
  color: var(--sf-on-surface-variant);
  margin-bottom: 16px;
}

.sf-ws-input {
  width: 100%;
  padding: 10px 14px;
  margin-top: 6px;
  background: var(--sf-surface-variant);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 10px;
  color: var(--sf-on-surface);
  font-size: 14px;
  outline: none;
  transition: all 0.15s;
}

.sf-ws-input:focus {
  border-color: var(--sf-primary);
}

.sf-ws-modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 24px;
}

.sf-ws-btn {
  padding: 10px 18px;
  border: 1px solid var(--sf-outline-variant);
  background: transparent;
  color: var(--sf-on-surface);
  border-radius: 10px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-ws-btn:hover {
  background: var(--sf-surface-variant);
}

.sf-ws-btn-primary {
  background: var(--sf-primary);
  color: var(--sf-on-primary);
  border-color: var(--sf-primary);
}

.sf-ws-btn-primary:hover {
  filter: brightness(1.1);
}

/* Menu */
.sf-ws-menu-overlay {
  position: fixed;
  inset: 0;
  z-index: 2000;
}

.sf-ws-menu {
  position: absolute;
  background: var(--sf-surface-container);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 12px;
  padding: 6px;
  min-width: 180px;
  box-shadow: 0 12px 32px rgba(0, 0, 0, 0.2);
}

.sf-ws-menu button {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 10px 12px;
  border: none;
  background: transparent;
  color: var(--sf-on-surface);
  font-size: 13px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-ws-menu button:hover {
  background: var(--sf-surface-variant);
}

.sf-ws-menu button:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.sf-ws-menu-danger {
  color: var(--sf-error) !important;
}

.sf-ws-menu-danger:hover {
  background: var(--sf-error-container) !important;
}

/* Loading */
.sf-ws-loading {
  text-align: center;
  padding: 64px;
  color: var(--sf-on-surface-variant);
  font-size: 14px;
  background: var(--sf-surface-container-lowest);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 16px;
}
</style>