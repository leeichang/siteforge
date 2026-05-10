<template>
  <div class="sf-editor" :class="`theme-${theme}`">
    <!-- Top Toolbar -->
    <header class="sf-editor-topbar">
      <div class="sf-editor-topbar-left">
        <span class="sf-editor-brand">SiteForge AI</span>
      </div>

      <div class="sf-editor-topbar-center">
        <div class="sf-editor-mode-switch">
          <button
            :class="{ active: editorMode === 'blocks' }"
            @click="editorMode = 'blocks'"
            type="button"
          >
            <span class="material-symbols-outlined">view_quilt</span>
            Blocks
          </button>
          <button
            :class="{ active: editorMode === 'code' }"
            @click="showCode = true"
            type="button"
          >
            <span class="material-symbols-outlined">code</span>
            Code
          </button>
        </div>

        <div class="sf-editor-device-dropdown">
          <button class="sf-editor-device-btn" @click="deviceMenuOpen = !deviceMenuOpen" type="button">
            <span class="material-symbols-outlined">{{ deviceIcon }}</span>
            {{ deviceLabel }}
            <span class="material-symbols-outlined sf-editor-chevron">expand_more</span>
          </button>
          <div v-if="deviceMenuOpen" class="sf-editor-device-menu">
            <button
              v-for="d in devices"
              :key="d.value"
              :class="{ active: device === d.value }"
              @click="setDevice(d.value); deviceMenuOpen = false"
              type="button"
            >
              <span class="material-symbols-outlined">{{ d.icon }}</span>
              {{ d.label }}
            </button>
          </div>
        </div>

        <div class="sf-editor-toolbar-divider"></div>

        <button class="sf-editor-toolbtn" @click="undo" :disabled="!editorReady" type="button" title="Undo">
          <span class="material-symbols-outlined">undo</span>
        </button>
        <button class="sf-editor-toolbtn" @click="redo" :disabled="!editorReady" type="button" title="Redo">
          <span class="material-symbols-outlined">redo</span>
        </button>
      </div>

      <div class="sf-editor-topbar-right">
        <span v-if="hasUnsavedChanges" class="sf-editor-unsaved">Unsaved changes</span>
        <button class="sf-editor-publish-btn" @click="publishSite" :disabled="publishing" type="button">
          <span v-if="publishing">Publishing...</span>
          <span v-else>Publish</span>
        </button>
        <button class="sf-editor-upgrade-btn" type="button">Upgrade</button>
      </div>
    </header>

    <!-- Main Workspace -->
    <div class="sf-editor-workspace">
      <!-- Left Panel: Blocks (220px) -->
      <aside class="sf-editor-left">
        <div class="sf-editor-left-tabs">
          <button
            :class="{ active: leftTab === 'blocks' }"
            @click="leftTab = 'blocks'"
            type="button"
          >
            Regular
          </button>
          <button
            :class="{ active: leftTab === 'symbols' }"
            @click="leftTab = 'symbols'"
            type="button"
          >
            Symbols
          </button>
        </div>

        <div class="sf-editor-left-search">
          <span class="material-symbols-outlined">search</span>
          <input
            v-model="blockSearch"
            type="search"
            placeholder="Search blocks..."
          />
        </div>

        <div class="sf-editor-block-categories">
          <div class="sf-editor-block-category">
            <button
              class="sf-editor-category-header"
              @click="toggleCategory('basic')"
              type="button"
            >
              <span>Basic</span>
              <span class="material-symbols-outlined" :class="{ rotated: !categoriesOpen.basic }">expand_more</span>
            </button>
            <div v-show="categoriesOpen.basic" class="sf-editor-category-content">
              <div id="blocks-panel" class="gjs-panel-host"></div>
            </div>
          </div>
        </div>

        <button class="sf-editor-add-blocks" type="button">
          <span class="material-symbols-outlined">add</span>
          Add more blocks
        </button>
      </aside>

      <!-- Center: Canvas -->
      <main class="sf-editor-canvas-area">
        <div v-if="loading || editorError" class="sf-editor-state">
          <strong>{{ editorError || 'Loading editor...' }}</strong>
        </div>
        <div id="gjs" class="sf-editor-canvas"></div>
      </main>

      <!-- Right Panel: Properties (250px) -->
      <aside class="sf-editor-right">
        <div class="sf-editor-right-tabs">
          <button
            :class="{ active: rightTab === 'styles' }"
            @click="rightTab = 'styles'"
            type="button"
          >
            Styles
          </button>
          <button
            :class="{ active: rightTab === 'properties' }"
            @click="rightTab = 'properties'"
            type="button"
          >
            Properties
          </button>
        </div>

        <div v-show="rightTab === 'styles'" class="sf-editor-right-content">
          <div class="sf-editor-selection-card">
            <div class="sf-editor-selection-header">
              <span>Selection</span>
            </div>
            <strong>{{ selectedComponentName || 'None' }}</strong>
            <p v-if="!selectedComponentName">Click on an element to edit its styles</p>
          </div>
          <div id="selectors-panel" class="gjs-panel-host compact"></div>
          <div id="styles-panel" class="gjs-panel-host"></div>
        </div>

        <div v-show="rightTab === 'properties'" class="sf-editor-right-content">
          <div class="sf-editor-selection-card">
            <div class="sf-editor-selection-header">
              <span>Page</span>
            </div>
            <strong>{{ page?.slug ? `/${page.slug}` : 'Draft' }}</strong>
            <p>{{ page?.metaDescription || 'Select an element to edit its properties' }}</p>
          </div>
          <div id="traits-panel" class="gjs-panel-host"></div>
        </div>
      </aside>
    </div>

    <!-- Code Dialog -->
    <div v-if="showCode" class="sf-editor-code-overlay" @click.self="showCode = false">
      <section class="sf-editor-code-dialog">
        <header>
          <div>
            <p class="sf-editor-kicker">Export</p>
            <h2>Current Page Code</h2>
          </div>
          <button @click="showCode = false" type="button">
            <span class="material-symbols-outlined">close</span>
          </button>
        </header>
        <div class="sf-editor-code-grid">
          <label>
            HTML
            <textarea readonly :value="currentHtml"></textarea>
          </label>
          <label>
            CSS
            <textarea readonly :value="currentCss"></textarea>
          </label>
        </div>
      </section>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onBeforeUnmount, watch, nextTick } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import grapesjs from 'grapesjs'
import 'grapesjs/dist/css/grapes.min.css'
import api, { unwrap } from '../api/client.js'

const route = useRoute()
const router = useRouter()

const siteId = computed(() => route.params.siteId)
const pageId = computed(() => route.params.pageId)

// Theme
const theme = ref(localStorage.getItem('sf-theme') || 'dark')

// Editor state
const editor = ref(null)
const editorReady = ref(false)
const loading = ref(true)
const editorError = ref('')
const hasUnsavedChanges = ref(false)
const saving = ref(false)
const publishing = ref(false)

// Page data
const site = ref(null)
const page = ref(null)
const treePages = ref([])

// UI state
const leftTab = ref('blocks')
const rightTab = ref('styles')
const editorMode = ref('blocks')
const showCode = ref(false)
const blockSearch = ref('')
const deviceMenuOpen = ref(false)

const categoriesOpen = ref({
  basic: true
})

const device = ref('Desktop')
const devices = [
  { value: 'Desktop', label: 'Desktop', icon: 'desktop_windows' },
  { value: 'Tablet', label: 'Tablet', icon: 'tablet_mac' },
  { value: 'Mobile portrait', label: 'Mobile', icon: 'phone_iphone' }
]

const deviceLabel = computed(() => {
  const d = devices.find(x => x.value === device.value)
  return d?.label || 'Desktop'
})

const deviceIcon = computed(() => {
  const d = devices.find(x => x.value === device.value)
  return d?.icon || 'desktop_windows'
})

const selectedComponentName = ref('')

const currentHtml = computed(() => {
  if (!editor.value) return ''
  return editor.value.getHtml()
})

const currentCss = computed(() => {
  if (!editor.value) return ''
  return editor.value.getCss()
})

// Fetch data
async function fetchSite() {
  if (!siteId.value) return
  try {
    const response = await api.get(`/Sites/${siteId.value}`)
    site.value = unwrap(response)
  } catch (err) {
    console.error('Failed to load site:', err)
  }
}

async function fetchPages() {
  if (!siteId.value) return
  try {
    const response = await api.get(`/Pages/site/${siteId.value}`)
    const pages = unwrap(response) || []
    treePages.value = pages.map(p => ({
      ...p,
      isHome: p.slug === '' || p.slug === 'home'
    }))
  } catch (err) {
    console.error('Failed to load pages:', err)
  }
}

async function fetchPage() {
  if (!pageId.value) return
  try {
    const response = await api.get(`/Pages/${pageId.value}`)
    page.value = unwrap(response)
  } catch (err) {
    console.error('Failed to load page:', err)
    editorError.value = 'Failed to load page'
  }
}

// GrapesJS init
async function initEditor() {
  if (!pageId.value) return

  loading.value = true
  await fetchPage()
  await fetchSite()

  if (!page.value) {
    loading.value = false
    return
  }

  await nextTick()

  const container = document.getElementById('gjs')
  if (!container) {
    editorError.value = 'Editor container not found'
    loading.value = false
    return
  }

  try {
    editor.value = grapesjs.init({
      container: '#gjs',
      height: '100%',
      width: '100%',
      fromElement: false,
      storageManager: false,
      panels: { defaults: [] },
      canvas: {
        styles: [
          'https://cdn.jsdelivr.net/npm/tailwindcss@2.2.19/dist/tailwind.min.css'
        ]
      },
      styleManager: {
        sectors: [
          { name: 'Layout', open: false, properties: ['display', 'position', 'width', 'height', 'margin', 'padding'] },
          { name: 'Typography', open: false, properties: ['font-family', 'font-size', 'font-weight', 'color', 'line-height', 'text-align'] },
          { name: 'Decorations', open: false, properties: ['background-color', 'border-radius', 'border', 'box-shadow'] },
        ]
      },
      blockManager: {
        appendTo: '#blocks-panel',
        blocks: []
      },
      layerManager: {
        appendTo: '#layers-panel'
      },
      traitManager: {
        appendTo: '#traits-panel'
      },
      selectorManager: {
        appendTo: '#selectors-panel'
      },
      styleManagerConfig: {
        appendTo: '#styles-panel'
      }
    })

    // Load content
    if (page.value.components) {
      editor.value.setComponents(JSON.parse(page.value.components))
    } else if (page.value.htmlContent) {
      editor.value.setComponents(page.value.htmlContent)
    }

    if (page.value.styles) {
      editor.value.setStyle(JSON.parse(page.value.styles))
    } else if (page.value.cssContent) {
      editor.value.setStyle(page.value.cssContent)
    }

    // Load widget templates as blocks
    await loadWidgetTemplates()

    // Event listeners
    editor.value.on('component:selected', (component) => {
      selectedComponentName.value = component.getName() || component.get('tagName') || 'Element'
    })

    editor.value.on('component:deselected', () => {
      selectedComponentName.value = ''
    })

    editor.value.on('change', () => {
      hasUnsavedChanges.value = true
    })

    editorReady.value = true
    loading.value = false
  } catch (err) {
    console.error('Editor init failed:', err)
    editorError.value = 'Failed to initialize editor'
    loading.value = false
  }
}

async function loadWidgetTemplates() {
  try {
    const response = await api.get('/WidgetTemplates')
    const templates = unwrap(response) || []

    templates.forEach((template) => {
      if (editor.value && template.defaultContent) {
        editor.value.BlockManager.add(template.name, {
          label: template.name,
          category: template.category || 'Basic',
          content: template.defaultContent,
          attributes: { class: 'gjs-block-section' }
        })
      }
    })
  } catch (err) {
    console.error('Failed to load widget templates:', err)
  }
}

// Actions
function undo() {
  if (editor.value) editor.value.UndoManager.undo()
}

function redo() {
  if (editor.value) editor.value.UndoManager.redo()
}

function setDevice(deviceName) {
  device.value = deviceName
  if (!editor.value) return

  const deviceManager = editor.value.DeviceManager
  deviceManager.select(deviceName)
}

async function savePage() {
  if (!editor.value || !pageId.value) return

  saving.value = true
  try {
    const html = editor.value.getHtml()
    const css = editor.value.getCss()
    const components = JSON.stringify(editor.value.getComponents())
    const styles = JSON.stringify(editor.value.getStyle())

    await api.put(`/Pages/${pageId.value}`, {
      htmlContent: html,
      cssContent: css,
      components,
      styles
    })

    hasUnsavedChanges.value = false
  } catch (err) {
    console.error('Save failed:', err)
    alert('Failed to save page')
  } finally {
    saving.value = false
  }
}

async function publishSite() {
  if (!siteId.value) return
  publishing.value = true
  try {
    await api.post(`/Sites/${siteId.value}/publish`)
    alert('Site published successfully!')
  } catch (err) {
    console.error('Publish failed:', err)
    alert('Failed to publish site')
  } finally {
    publishing.value = false
  }
}

function backToWorkspace() {
  router.push(`/sites/${siteId.value}`)
}

function openPage(pid) {
  router.push(`/editor/${siteId.value}/${pid}`)
}

function toggleCategory(cat) {
  categoriesOpen.value[cat] = !categoriesOpen.value[cat]
}

// Lifecycle
onMounted(() => {
  fetchPages()
  initEditor()
})

onBeforeUnmount(() => {
  if (editor.value) {
    editor.value.destroy()
    editor.value = null
  }
})

// Watch pageId changes
watch(pageId, () => {
  if (editor.value) {
    editor.value.destroy()
    editor.value = null
    editorReady.value = false
  }
  initEditor()
})

// Keyboard shortcuts
function handleKeydown(e) {
  if ((e.ctrlKey || e.metaKey) && e.key === 's') {
    e.preventDefault()
    savePage()
  }
}

onMounted(() => {
  document.addEventListener('keydown', handleKeydown)
})

onBeforeUnmount(() => {
  document.removeEventListener('keydown', handleKeydown)
})
</script>

<style scoped>
.sf-editor {
  display: flex;
  flex-direction: column;
  height: 100vh;
  width: 100vw;
  overflow: hidden;
  background: var(--sf-bg);
  color: var(--sf-on-surface);
  font-family: 'Inter', system-ui, sans-serif;
}

/* ===== Top Toolbar ===== */
.sf-editor-topbar {
  height: 56px;
  background: var(--sf-surface);
  border-bottom: 1px solid var(--sf-outline-variant);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 16px;
  flex-shrink: 0;
  z-index: 100;
}

.sf-editor-brand {
  font-size: 18px;
  font-weight: 700;
  color: var(--sf-primary);
  letter-spacing: -0.5px;
}

.sf-editor-topbar-center {
  display: flex;
  align-items: center;
  gap: 12px;
  position: absolute;
  left: 50%;
  transform: translateX(-50%);
}

.sf-editor-mode-switch {
  display: flex;
  background: var(--sf-surface-variant);
  border-radius: 8px;
  padding: 3px;
  gap: 2px;
}

.sf-editor-mode-switch button {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 6px 14px;
  border: none;
  background: transparent;
  color: var(--sf-on-surface-variant);
  font-size: 13px;
  font-weight: 500;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-editor-mode-switch button.active {
  background: var(--sf-surface-container-high);
  color: var(--sf-on-surface);
  box-shadow: 0 1px 3px rgba(0,0,0,0.1);
}

.sf-editor-mode-switch button:hover:not(.active) {
  color: var(--sf-on-surface);
}

.sf-editor-device-dropdown {
  position: relative;
}

.sf-editor-device-btn {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 6px 12px;
  background: var(--sf-surface-variant);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 8px;
  color: var(--sf-on-surface);
  font-size: 13px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-editor-device-btn:hover {
  background: var(--sf-surface-container-high);
}

.sf-editor-chevron {
  font-size: 16px;
  color: var(--sf-on-surface-variant);
}

.sf-editor-device-menu {
  position: absolute;
  top: calc(100% + 4px);
  left: 0;
  background: var(--sf-surface-container);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 8px;
  padding: 4px;
  min-width: 160px;
  box-shadow: 0 8px 24px rgba(0,0,0,0.2);
  z-index: 200;
}

.sf-editor-device-menu button {
  display: flex;
  align-items: center;
  gap: 10px;
  width: 100%;
  padding: 8px 12px;
  border: none;
  background: transparent;
  color: var(--sf-on-surface-variant);
  font-size: 13px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-editor-device-menu button:hover,
.sf-editor-device-menu button.active {
  background: var(--sf-surface-variant);
  color: var(--sf-on-surface);
}

.sf-editor-toolbar-divider {
  width: 1px;
  height: 24px;
  background: var(--sf-outline-variant);
}

.sf-editor-toolbtn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: none;
  background: transparent;
  color: var(--sf-on-surface-variant);
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-editor-toolbtn:hover {
  background: var(--sf-surface-variant);
  color: var(--sf-on-surface);
}

.sf-editor-toolbtn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.sf-editor-topbar-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.sf-editor-unsaved {
  font-size: 12px;
  color: var(--sf-tertiary);
  font-weight: 500;
}

.sf-editor-publish-btn,
.sf-editor-upgrade-btn {
  padding: 8px 18px;
  border: none;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-editor-publish-btn {
  background: linear-gradient(135deg, #7c3aed, #a855f7);
  color: white;
}

.sf-editor-publish-btn:hover {
  filter: brightness(1.1);
}

.sf-editor-publish-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.sf-editor-upgrade-btn {
  background: linear-gradient(135deg, #f59e0b, #fbbf24);
  color: #1a1a1a;
}

.sf-editor-upgrade-btn:hover {
  filter: brightness(1.1);
}

/* ===== Workspace ===== */
.sf-editor-workspace {
  display: flex;
  flex: 1;
  overflow: hidden;
}

/* ===== Left Panel (220px) ===== */
.sf-editor-left {
  width: 220px;
  background: var(--sf-surface);
  border-right: 1px solid var(--sf-outline-variant);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  flex-shrink: 0;
}

.sf-editor-left-tabs {
  display: flex;
  padding: 12px 12px 0;
  gap: 8px;
}

.sf-editor-left-tabs button {
  flex: 1;
  padding: 8px 12px;
  border: none;
  background: transparent;
  color: var(--sf-on-surface-variant);
  font-size: 13px;
  font-weight: 500;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-editor-left-tabs button.active {
  background: var(--sf-surface-variant);
  color: var(--sf-on-surface);
}

.sf-editor-left-tabs button:hover:not(.active) {
  color: var(--sf-on-surface);
}

.sf-editor-left-search {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  margin: 8px 12px;
  background: var(--sf-surface-variant);
  border-radius: 8px;
  border: 1px solid var(--sf-outline-variant);
}

.sf-editor-left-search span {
  color: var(--sf-on-surface-variant);
  font-size: 18px;
}

.sf-editor-left-search input {
  flex: 1;
  border: none;
  background: transparent;
  color: var(--sf-on-surface);
  font-size: 13px;
  outline: none;
}

.sf-editor-left-search input::placeholder {
  color: var(--sf-on-surface-variant);
}

.sf-editor-block-categories {
  flex: 1;
  overflow-y: auto;
  padding: 0 8px;
}

.sf-editor-block-category {
  margin-bottom: 4px;
}

.sf-editor-category-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  width: 100%;
  padding: 10px 12px;
  border: none;
  background: transparent;
  color: var(--sf-on-surface);
  font-size: 13px;
  font-weight: 600;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-editor-category-header:hover {
  background: var(--sf-surface-variant);
}

.sf-editor-category-header .material-symbols-outlined {
  font-size: 18px;
  color: var(--sf-on-surface-variant);
  transition: transform 0.2s;
}

.sf-editor-category-header .material-symbols-outlined.rotated {
  transform: rotate(-90deg);
}

.sf-editor-category-content {
  padding: 4px 0;
}

.sf-editor-add-blocks {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  margin: 8px 12px 12px;
  padding: 10px;
  border: 1px dashed var(--sf-outline-variant);
  background: transparent;
  color: var(--sf-on-surface-variant);
  font-size: 13px;
  font-weight: 500;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-editor-add-blocks:hover {
  border-color: var(--sf-primary);
  color: var(--sf-primary);
  background: var(--sf-primary-container);
}

/* ===== Canvas Area ===== */
.sf-editor-canvas-area {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: var(--sf-surface-container-lowest);
  position: relative;
}

.sf-editor-canvas {
  flex: 1;
  overflow: hidden;
}

.sf-editor-state {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  background: var(--sf-surface-container-lowest);
  z-index: 50;
}

.sf-editor-state strong {
  color: var(--sf-on-surface-variant);
  font-size: 14px;
}

/* ===== Right Panel (250px) ===== */
.sf-editor-right {
  width: 250px;
  background: var(--sf-surface);
  border-left: 1px solid var(--sf-outline-variant);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  flex-shrink: 0;
}

.sf-editor-right-tabs {
  display: flex;
  padding: 12px 12px 0;
  gap: 8px;
}

.sf-editor-right-tabs button {
  flex: 1;
  padding: 8px 12px;
  border: none;
  background: transparent;
  color: var(--sf-on-surface-variant);
  font-size: 13px;
  font-weight: 500;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.15s;
}

.sf-editor-right-tabs button.active {
  background: var(--sf-surface-variant);
  color: var(--sf-on-surface);
}

.sf-editor-right-tabs button:hover:not(.active) {
  color: var(--sf-on-surface);
}

.sf-editor-right-content {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.sf-editor-selection-card {
  padding: 12px;
  margin-bottom: 8px;
  background: var(--sf-surface-variant);
  border-radius: 10px;
  border: 1px solid var(--sf-outline-variant);
}

.sf-editor-selection-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 4px;
}

.sf-editor-selection-header span {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--sf-on-surface-variant);
}

.sf-editor-selection-card strong {
  display: block;
  font-size: 14px;
  color: var(--sf-on-surface);
  margin-bottom: 6px;
}

.sf-editor-selection-card p {
  font-size: 12px;
  color: var(--sf-on-surface-variant);
  line-height: 1.4;
  margin: 0;
}

/* ===== Code Dialog ===== */
.sf-editor-code-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  backdrop-filter: blur(4px);
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
}

.sf-editor-code-dialog {
  background: var(--sf-surface-container);
  border-radius: 16px;
  width: 100%;
  max-width: 900px;
  max-height: 80vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  border: 1px solid var(--sf-outline-variant);
  box-shadow: 0 24px 48px rgba(0, 0, 0, 0.3);
}

.sf-editor-code-dialog header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid var(--sf-outline-variant);
}

.sf-editor-code-dialog header h2 {
  font-size: 18px;
  font-weight: 600;
  margin: 0;
  color: var(--sf-on-surface);
}

.sf-editor-kicker {
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 1px;
  color: var(--sf-primary);
  margin: 0 0 4px;
}

.sf-editor-code-dialog header button {
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

.sf-editor-code-dialog header button:hover {
  background: var(--sf-surface-container-high);
  color: var(--sf-on-surface);
}

.sf-editor-code-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  padding: 20px 24px;
  overflow: hidden;
}

.sf-editor-code-grid label {
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  color: var(--sf-on-surface-variant);
}

.sf-editor-code-grid textarea {
  flex: 1;
  min-height: 400px;
  background: var(--sf-surface-variant);
  border: 1px solid var(--sf-outline-variant);
  border-radius: 10px;
  padding: 12px;
  color: var(--sf-on-surface);
  font-family: 'SF Mono', 'Fira Code', monospace;
  font-size: 12px;
  line-height: 1.5;
  resize: none;
  outline: none;
}

.sf-editor-code-grid textarea:focus {
  border-color: var(--sf-primary);
}

/* ===== GrapesJS Panel Overrides ===== */
:deep(.gjs-pn-panel) {
  position: relative !important;
  box-shadow: none !important;
  border: none !important;
}

:deep(.gjs-pn-commands),
:deep(.gjs-pn-options) {
  display: none !important;
}

:deep(.gjs-block) {
  background: var(--sf-surface-variant) !important;
  border: 1px solid var(--sf-outline-variant) !important;
  border-radius: 8px !important;
  color: var(--sf-on-surface) !important;
  transition: all 0.15s !important;
  margin: 4px 0 !important;
}

:deep(.gjs-block:hover) {
  background: var(--sf-surface-container-high) !important;
  border-color: var(--sf-primary) !important;
}

:deep(.gjs-block-label) {
  font-size: 11px !important;
  font-weight: 500 !important;
}

:deep(.gjs-title) {
  font-size: 12px !important;
  font-weight: 600 !important;
  color: var(--sf-on-surface) !important;
  background: transparent !important;
  border: none !important;
  padding: 8px 12px !important;
}

:deep(.gjs-sm-sector) {
  background: transparent !important;
  border: none !important;
  border-bottom: 1px solid var(--sf-outline-variant) !important;
}

:deep(.gjs-sm-sector-title) {
  font-size: 12px !important;
  font-weight: 600 !important;
  color: var(--sf-on-surface) !important;
  padding: 10px 8px !important;
}

:deep(.gjs-field) {
  background: var(--sf-surface-variant) !important;
  border: 1px solid var(--sf-outline-variant) !important;
  border-radius: 6px !important;
}

:deep(.gjs-field input),
:deep(.gjs-field select) {
  color: var(--sf-on-surface) !important;
  font-size: 12px !important;
}

:deep(.gjs-color-warn) {
  color: var(--sf-on-surface-variant) !important;
}

:deep(.gjs-sm-label) {
  font-size: 11px !important;
  color: var(--sf-on-surface-variant) !important;
}

/* Scrollbar */
::-webkit-scrollbar {
  width: 6px;
}

::-webkit-scrollbar-track {
  background: transparent;
}

::-webkit-scrollbar-thumb {
  background: var(--sf-outline-variant);
  border-radius: 3px;
}

::-webkit-scrollbar-thumb:hover {
  background: var(--sf-outline);
}
</style>