<template>
  <div class="studio-editor" :class="`theme-${theme}`">
    <header class="studio-topbar">
      <div class="topbar-left">
        <button @click="backToWorkspace" class="chrome-button back-button" type="button">←</button>
        <button @click="showCode = true" class="chrome-button code-button" type="button">{{ t('editor.code') }}</button>
      </div>

      <div class="topbar-center">
        <div class="device-menu">
          <button :class="{ active: device === 'Desktop' }" @click="setDevice('Desktop')" type="button">{{ t('editor.desktop') }}</button>
          <button :class="{ active: device === 'Tablet' }" @click="setDevice('Tablet')" type="button">{{ t('editor.tablet') }}</button>
          <button :class="{ active: device === 'Mobile portrait' }" @click="setDevice('Mobile portrait')" type="button">{{ t('editor.mobile') }}</button>
        </div>
        <button class="tool-button" @click="togglePreview" type="button">{{ previewMode ? t('common.edit') : t('editor.preview') }}</button>
        <button class="tool-button icon-only undo" @click="undo" type="button" aria-label="Undo"></button>
        <button class="tool-button icon-only redo" @click="redo" type="button" aria-label="Redo"></button>
      </div>

      <div class="topbar-right">
        <button class="theme-toggle language-toggle" @click="localeStore.toggleLocale()" type="button" :title="t('common.language')">
          {{ locale === 'en' ? '繁' : 'EN' }}
        </button>
        <button class="theme-toggle" @click="themeStore.toggle()" type="button" :title="theme === 'dark' ? t('common.switchToLight') : t('common.switchToDark')">
          {{ theme === 'dark' ? '☀️' : '🌙' }}
        </button>
        <button class="tool-button save-button" @click="savePage" :disabled="saving || !editorReady" type="button">
          {{ saving ? t('editor.saving') : hasUnsavedChanges ? t('editor.save') : t('editor.saved') }}
        </button>
        <button class="publish-button" @click="publishSite" :disabled="publishing" type="button">
          {{ publishing ? t('editor.publishing') : t('editor.publish') }}
        </button>
        <button class="upgrade-button" type="button">{{ t('editor.upgrade') }}</button>
      </div>
    </header>

    <main
      ref="workbenchEl"
      class="studio-workbench"
      :class="{ resizing: resizingPane }"
      :style="workbenchGridStyle"
    >
      <aside class="editor-rail">
        <button :class="{ active: isLeftPanelOpen && activePanel === 'blocks' }" @click="toggleLeftPanel('blocks')" type="button" :aria-label="t('editor.blocks')">
          <span class="rail-icon plus"></span>
        </button>
        <button :class="{ active: isLeftPanelOpen && activePanel === 'project' }" @click="toggleLeftPanel('project')" type="button" :aria-label="t('editor.pages')">
          <span class="rail-icon layers"></span>
        </button>
        <button :class="{ active: isLeftPanelOpen && activePanel === 'global' }" @click="toggleLeftPanel('global')" type="button" :aria-label="t('editor.globalStyles')">
          <span class="rail-icon palette"></span>
        </button>
        <button :class="{ active: isLeftPanelOpen && activePanel === 'assets' }" @click="toggleLeftPanel('assets')" type="button" :aria-label="t('editor.assets')">
          <span class="rail-icon image"></span>
        </button>
        <button :class="{ active: isLeftPanelOpen && activePanel === 'data' }" @click="toggleLeftPanel('data')" type="button" :aria-label="t('editor.dataSources')">
          <span class="rail-icon database"></span>
        </button>
        <button :class="{ active: isLeftPanelOpen && activePanel === 'ai' }" @click="toggleLeftPanel('ai')" type="button" :aria-label="t('editor.aiAssistant')">
          <span class="rail-icon spark"></span>
        </button>
        <button class="rail-bottom" @click="backToWorkspace" type="button" :aria-label="t('editor.workspace')">
          <span class="rail-icon home"></span>
        </button>
      </aside>

      <aside ref="leftPanelEl" class="studio-left-panel" :class="{ collapsed: !isLeftPanelOpen }" :aria-hidden="!isLeftPanelOpen">
        <section v-show="activePanel === 'project'" class="studio-panel project-panel" :style="projectSplitStyle">
          <div class="project-pane">
            <header class="panel-title-row">
              <h2>{{ t('editor.projectTree') }}</h2>
              <button @click="createPage" class="panel-icon-button" type="button" :aria-label="t('workspace.createPage')">+</button>
            </header>
            <div class="project-tree">
              <div class="tree-root">
                <span class="tree-caret">▾</span>
                <span class="tree-site-icon"></span>
                <span>{{ site?.name || t('editor.website') }}</span>
              </div>
              <button
                v-for="item in treePages"
                :key="item.id"
                class="tree-node"
                :class="{ active: item.id === pageId }"
                @click="openPage(item.id)"
                type="button"
              >
                <span class="tree-line"></span>
                <span class="tree-file-icon" :class="{ home: item.isHome }"></span>
                <span class="tree-label">
                  <strong>{{ item.title }}</strong>
                  <small>/{{ item.slug }}</small>
                </span>
                <em v-if="item.isHome">{{ t('common.home') }}</em>
              </button>
            </div>
          </div>

          <button
            class="stack-resizer project-layer-resizer"
            :class="{ active: resizingPane === 'project-layers' }"
            type="button"
            role="separator"
            aria-orientation="horizontal"
            :aria-valuemin="MIN_PROJECT_TREE"
            :aria-valuemax="Math.round(projectTreeMax)"
            :aria-valuenow="Math.round(projectSplit.projectTree)"
            :aria-label="t('editor.adjustProjectLayers')"
            :title="t('editor.adjustProjectLayers')"
            @pointerdown="startProjectSplitResize"
            @keydown="resizeProjectSplitByKeyboard"
          >
            <span class="stack-resizer-track" aria-hidden="true">
              <span></span>
              <span></span>
              <span></span>
            </span>
          </button>

          <div class="layers-pane">
            <header class="panel-title-row">
              <h2>{{ t('editor.layers') }}</h2>
            </header>
            <div id="layers-panel" class="gjs-panel-host layers-host"></div>
          </div>
        </section>

        <section v-show="activePanel === 'blocks'" class="studio-panel">
          <header class="panel-title-row">
            <h2>{{ t('editor.blocks') }}</h2>
            <button class="panel-icon-button" type="button" @click="closeLeftPanel" :aria-label="t('editor.closePanel')">×</button>
          </header>
          <div class="segmented">
            <button class="active" type="button">{{ t('editor.regular') }}</button>
            <button type="button">{{ t('editor.symbols') }}</button>
          </div>
          <input v-model="blockSearch" class="panel-search" type="search" :placeholder="t('editor.search')" />
          <div class="block-section-title">{{ t('editor.basic') }}</div>
          <div id="blocks-panel" class="gjs-panel-host blocks-host"></div>
          <button class="add-blocks-button" type="button">{{ t('editor.addMoreBlocks') }}</button>
        </section>

        <section v-show="activePanel === 'assets'" class="studio-panel">
          <header class="panel-title-row">
            <h2>{{ t('editor.assets') }}</h2>
            <button class="panel-icon-button" type="button" @click="closeLeftPanel" :aria-label="t('editor.closePanel')">×</button>
          </header>
          <div class="asset-controls">
            <input v-model="assetSearch" class="panel-search" type="search" :placeholder="t('editor.search')" />
            <button class="filter-chip" type="button">{{ t('editor.projectAssets') }}</button>
          </div>
          <button class="upload-button" @click="registerAssetUrl" type="button">{{ t('editor.registerUrl') }}</button>
          <input v-model="assetUrl" class="panel-search asset-url-input" type="url" placeholder="https://example.com/image.jpg" />
          <div class="asset-grid">
            <button
              v-for="asset in filteredAssets"
              :key="asset.publicUrl || asset"
              class="asset-tile"
              type="button"
              @click="selectAsset(asset.publicUrl || asset)"
            >
              <img :src="asset.publicUrl || asset" :alt="asset.altText || asset.fileName || t('editor.assetPreview')" />
              <span>{{ asset.fileName || filenameFromUrl(asset.publicUrl || asset) }}</span>
            </button>
          </div>
          <button class="panel-action" @click="openAssetManager" type="button">{{ t('editor.openAssetManager') }}</button>
        </section>

        <section v-show="activePanel === 'global'" class="studio-panel">
          <header class="panel-title-row">
            <h2>{{ t('editor.globalStyles') }}</h2>
            <button class="panel-icon-button" type="button" @click="closeLeftPanel" :aria-label="t('editor.closePanel')">×</button>
          </header>
          <div class="style-group open">
            <button type="button">{{ t('editor.colors') }} <span>⌃</span></button>
            <label v-for="token in colorTokens" :key="token.name" class="token-row">
              <span>{{ token.name }}</span>
              <input v-model="token.value" type="color" @input="applyGlobalTokens" />
              <code>{{ token.value }}</code>
            </label>
          </div>
          <div class="style-group open">
            <button type="button">{{ t('editor.body') }} <span>⌃</span></button>
            <label class="field-row">
              <span>{{ t('editor.fontFamily') }}</span>
              <select v-model="fontFamily" @change="applyGlobalTokens">
                <option>Inter</option>
                <option>Arial</option>
                <option>Georgia</option>
                <option>system-ui</option>
              </select>
            </label>
            <label class="field-row">
              <span>{{ t('editor.lineHeight') }}</span>
              <input v-model="lineHeight" type="number" step="0.05" min="1" @input="applyGlobalTokens" />
            </label>
          </div>
        </section>

        <section v-show="activePanel === 'data'" class="studio-panel">
          <header class="panel-title-row">
            <h2>{{ t('editor.dataSources') }}</h2>
            <button class="panel-icon-button" type="button">+</button>
          </header>
          <div class="data-empty">
            <span class="data-icon"></span>
            <h3>{{ t('editor.createTable') }}</h3>
            <button @click="createSampleTable" class="panel-action" type="button">{{ t('editor.createSampleTable') }}</button>
          </div>
          <div v-if="sampleTable.length" class="data-table">
            <div v-for="row in sampleTable" :key="row.key">
              <span>{{ row.key }}</span>
              <strong>{{ row.value }}</strong>
            </div>
          </div>
        </section>

        <section v-show="activePanel === 'ai'" class="studio-panel ai-panel">
          <header class="panel-title-row">
            <h2>{{ t('editor.aiAssistant') }}</h2>
            <button class="panel-icon-button" type="button" @click="closeLeftPanel" :aria-label="t('editor.closePanel')">×</button>
          </header>
          <div class="ai-suggestions">
            <button v-for="suggestion in aiSuggestions" :key="suggestion" type="button" @click="aiPrompt = suggestion">
              {{ suggestion }}
            </button>
          </div>
          <div class="ai-generate-controls">
            <label>
              {{ t('editor.template') }}
              <select v-model="aiTemplateKey" @change="applyEditorTemplate">
                <option value="">{{ t('editor.aiPromptOnly') }}</option>
                <option v-for="template in pageTemplates" :key="template.key" :value="template.key">
                  {{ template.label }}
                </option>
              </select>
            </label>
            <label>
              {{ t('editor.pageType') }}
              <select v-model="aiPageType">
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
            <label>
              {{ t('editor.style') }}
              <select v-model="aiStyle">
                <option value="studio">Studio</option>
                <option value="tech">Tech</option>
                <option value="premium">Premium</option>
                <option value="eco">Eco</option>
                <option value="fashion">Fashion</option>
              </select>
            </label>
          </div>
          <div class="ai-input-row">
            <button type="button">+</button>
            <textarea v-model="aiPrompt" :placeholder="t('editor.askAnything')"></textarea>
            <button type="button" @click="generateCurrentPage" :disabled="generatingCurrentPage || !editorReady">↑</button>
          </div>
          <button class="panel-action ai-apply-button" type="button" @click="generateCurrentPage" :disabled="generatingCurrentPage || !editorReady">
            {{ generatingCurrentPage ? t('editor.generatingCurrentPage') : t('editor.generateCurrentPage') }}
          </button>
        </section>
      </aside>

      <button
        class="pane-resizer left-resizer"
        :class="{ active: resizingPane === 'left' }"
        type="button"
        role="separator"
        aria-orientation="vertical"
        :aria-hidden="!isLeftPanelOpen"
        :tabindex="isLeftPanelOpen ? 0 : -1"
        :aria-valuemin="MIN_LEFT_PANEL"
        :aria-valuemax="Math.round(leftPaneMax)"
        :aria-valuenow="Math.round(paneLayout.left)"
        :aria-label="t('editor.adjustLeftCanvas')"
        :title="t('editor.adjustLeftCanvas')"
        @pointerdown="startPaneResize('left', $event)"
        @keydown="resizePaneByKeyboard('left', $event)"
      >
        <span class="pane-resizer-track" aria-hidden="true">
          <span></span>
          <span></span>
          <span></span>
        </span>
      </button>

      <section class="canvas-stage">
        <div class="canvas-meta">
          <span>{{ site?.name || 'SiteForge' }}</span>
          <strong>{{ page?.title || t('editor.pageEditor') }}</strong>
          <em v-if="hasUnsavedChanges">{{ t('editor.unsavedChanges') }}</em>
        </div>
        <div v-if="loading || editorError" class="editor-state">
          <strong>{{ editorError || t('editor.loadingEditor') }}</strong>
        </div>
        <div id="gjs" class="editor-canvas"></div>
      </section>

      <button
        class="pane-resizer right-resizer"
        :class="{ active: resizingPane === 'right' }"
        type="button"
        role="separator"
        aria-orientation="vertical"
        :aria-valuemin="MIN_RIGHT_PANEL"
        :aria-valuemax="Math.round(rightPaneMax)"
        :aria-valuenow="Math.round(paneLayout.right)"
        :aria-label="t('editor.adjustCanvasRight')"
        :title="t('editor.adjustCanvasRight')"
        @pointerdown="startPaneResize('right', $event)"
        @keydown="resizePaneByKeyboard('right', $event)"
      >
        <span class="pane-resizer-track" aria-hidden="true">
          <span></span>
          <span></span>
          <span></span>
        </span>
      </button>

      <aside class="studio-right-panel">
        <div class="right-tabs">
          <button :class="{ active: rightTab === 'styles' }" @click="rightTab = 'styles'" type="button">{{ t('editor.styles') }}</button>
          <button :class="{ active: rightTab === 'properties' }" @click="rightTab = 'properties'" type="button">{{ t('editor.properties') }}</button>
        </div>

        <section v-show="rightTab === 'styles'" class="right-content">
          <div class="selection-card">
            <header>
              <span>{{ t('editor.selection') }}</span>
              <strong>{{ selectedComponentName || t('editor.none') }}</strong>
            </header>
            <p v-if="!selectedComponentName">{{ t('editor.selectionHelp') }}</p>
          </div>
          <div id="selectors-panel" class="gjs-panel-host compact"></div>
          <div id="styles-panel" class="gjs-panel-host"></div>
        </section>

        <section v-show="rightTab === 'properties'" class="right-content">
          <div class="selection-card">
            <header>
              <span>{{ t('editor.page') }}</span>
              <strong>{{ page?.slug ? `/${page.slug}` : t('common.draft') }}</strong>
            </header>
            <p>{{ page?.metaDescription || t('editor.propertiesHelp') }}</p>
          </div>
          <div id="traits-panel" class="gjs-panel-host"></div>
          <div class="property-summary">
            <label>
              {{ t('editor.pageTitle') }}
              <input :value="page?.title || ''" readonly />
            </label>
            <label>
              {{ t('editor.slug') }}
              <input :value="page?.slug || ''" readonly />
            </label>
          </div>
        </section>
      </aside>
    </main>

    <div v-if="showCode" class="code-overlay" @click.self="showCode = false">
      <section class="code-dialog">
        <header>
          <div>
            <p class="sf-kicker">{{ t('editor.export') }}</p>
            <h2>{{ t('editor.currentPageCode') }}</h2>
          </div>
          <button @click="showCode = false" type="button">×</button>
        </header>
        <div class="code-grid">
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
import { computed, nextTick, onMounted, onUnmounted, reactive, ref } from 'vue'
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
const pageId = ref(route.params.pageId || '')
const PANE_STORAGE_KEY = 'siteforge.editor.panes.v5'
const RAIL_WIDTH = 44
const RESIZER_WIDTH = 8
const MIN_LEFT_PANEL = 150
const MIN_CANVAS = 720
const MIN_RIGHT_PANEL = 300
const DEFAULT_CANVAS_SHARE = 0.62
const DEFAULT_LEFT_SIDE_SHARE = 0.54
const DEFAULT_LEFT_PANEL = 400
const DEFAULT_RIGHT_PANEL = 340
const PROJECT_SPLIT_STORAGE_KEY = 'siteforge.editor.projectSplit.v1'
const MIN_PROJECT_TREE = 120
const MIN_LAYERS = 140
const PROJECT_RESIZER_HEIGHT = 14
const DEFAULT_PROJECT_TREE = 360

const site = ref(null)
const page = ref(null)
const pages = ref([])
const templates = ref([])
const assets = ref([])
const loading = ref(true)
const saving = ref(false)
const publishing = ref(false)
const editorReady = ref(false)
const editorError = ref('')
const hasUnsavedChanges = ref(false)
const activePanel = ref('project')
const isLeftPanelOpen = ref(true)
const rightTab = ref('styles')
const device = ref('Desktop')
const previewMode = ref(false)
const showCode = ref(false)
const aiPrompt = ref('')
const aiPageType = ref('home')
const aiStyle = ref('studio')
const aiTemplates = ref([])
const aiTemplateKey = ref('')
const generatingCurrentPage = ref(false)
const blockSearch = ref('')
const assetSearch = ref('')
const assetUrl = ref('')
const selectedComponentName = ref('')
const fontFamily = ref('Inter')
const lineHeight = ref(1.55)
const sampleTable = ref([])
const workbenchEl = ref(null)
const leftPanelEl = ref(null)
const resizingPane = ref('')
const paneLayout = reactive({
  left: DEFAULT_LEFT_PANEL,
  right: DEFAULT_RIGHT_PANEL
})
const projectSplit = reactive({
  projectTree: DEFAULT_PROJECT_TREE
})
const colorTokens = reactive([
  { name: 'Primary', value: '#8358ed' },
  { name: 'Secondary', value: '#fc549e' },
  { name: 'Accent', value: '#ffcb47' },
  { name: 'Success', value: '#28a745' },
  { name: 'Error', value: '#dc3545' }
])

const defaultAssetUrls = [
  'https://images.unsplash.com/photo-1497366216548-37526070297c?w=1200',
  'https://images.unsplash.com/photo-1522071820081-009f0129c71c?w=1200',
  'https://images.unsplash.com/photo-1557683316-973673baf926?w=1200',
  'https://images.unsplash.com/photo-1557804506-669a67965ba0?w=1200'
]
const aiSuggestions = computed(() => [
  t('editor.aiSuggestionHero'),
  t('editor.aiSuggestionFeatures'),
  t('editor.aiSuggestionContact'),
  t('editor.aiSuggestionTestimonials')
])

const currentHtml = computed(() => editor ? editor.getHtml() : '')
const currentCss = computed(() => editor ? editor.getCss() : '')
const leftPaneMax = computed(() => maxLeftWidth())
const rightPaneMax = computed(() => maxRightWidth())
const workbenchGridStyle = computed(() => ({
  '--sf-left-panel-width': isLeftPanelOpen.value ? `${Math.round(paneLayout.left)}px` : '0px',
  '--sf-left-resizer-width': isLeftPanelOpen.value ? `${RESIZER_WIDTH}px` : '0px',
  '--sf-right-panel-width': `${Math.round(paneLayout.right)}px`
}))
const projectTreeMax = computed(() => maxProjectTreeHeight())
const projectSplitStyle = computed(() => ({
  '--sf-project-tree-height': `${Math.round(projectSplit.projectTree)}px`
}))
const pageTemplates = computed(() => aiTemplates.value.filter((template) => template.kind === 'page'))
const treePages = computed(() => {
  return [...pages.value].sort((a, b) => {
    if (a.isHome && !b.isHome) return -1
    if (!a.isHome && b.isHome) return 1
    return (a.displayOrder || 0) - (b.displayOrder || 0)
  })
})
const filteredAssets = computed(() => {
  const allAssets = [
    ...assets.value,
    ...defaultAssetUrls.map((url) => ({ publicUrl: url, fileName: filenameFromUrl(url), altText: 'Template asset' }))
  ]
  const query = assetSearch.value.trim().toLowerCase()
  if (!query) return allAssets
  return allAssets.filter((asset) => {
    const value = `${asset.fileName || ''} ${asset.altText || ''} ${asset.publicUrl || ''}`.toLowerCase()
    return value.includes(query)
  })
})
let editor = null
let resizeState = null
let projectSplitResizeState = null

onMounted(async () => {
  restorePaneLayout()
  restoreProjectSplit()
  window.addEventListener('resize', fitPaneLayout)
  window.addEventListener('resize', fitProjectSplit)
  await loadEditorData()
  await nextTick()
  fitPaneLayout()
  fitProjectSplit()
  initGrapesJS()
})

onUnmounted(() => {
  window.removeEventListener('resize', fitPaneLayout)
  window.removeEventListener('resize', fitProjectSplit)
  stopPaneResize()
  stopProjectSplitResize()
  if (editor) {
    editor.destroy()
    editor = null
  }
})

function restorePaneLayout() {
  if (typeof window === 'undefined') return
  const defaults = defaultPaneLayout()
  try {
    const saved = JSON.parse(window.localStorage.getItem(PANE_STORAGE_KEY) || '{}')
    paneLayout.left = Number.isFinite(saved.left) ? clamp(saved.left, MIN_LEFT_PANEL, 680) : defaults.left
    paneLayout.right = Number.isFinite(saved.right) ? clamp(saved.right, MIN_RIGHT_PANEL, 560) : defaults.right
  } catch {
    paneLayout.left = defaults.left
    paneLayout.right = defaults.right
  }
  fitPaneLayout()
}

function persistPaneLayout() {
  if (typeof window === 'undefined') return
  window.localStorage.setItem(PANE_STORAGE_KEY, JSON.stringify({
    left: Math.round(paneLayout.left),
    right: Math.round(paneLayout.right)
  }))
}

function toggleLeftPanel(panel) {
  if (activePanel.value === panel && isLeftPanelOpen.value) {
    closeLeftPanel()
    return
  }
  activePanel.value = panel
  isLeftPanelOpen.value = true
  nextTick(() => {
    fitPaneLayout()
    fitProjectSplit()
    editor?.refresh?.()
  })
}

function closeLeftPanel() {
  isLeftPanelOpen.value = false
  stopPaneResize()
  stopProjectSplitResize()
  nextTick(() => editor?.refresh?.())
}

function restoreProjectSplit() {
  if (typeof window === 'undefined') return
  try {
    const saved = JSON.parse(window.localStorage.getItem(PROJECT_SPLIT_STORAGE_KEY) || '{}')
    if (Number.isFinite(saved.projectTree)) {
      projectSplit.projectTree = clamp(saved.projectTree, MIN_PROJECT_TREE, maxProjectTreeHeight())
    }
  } catch {
    projectSplit.projectTree = DEFAULT_PROJECT_TREE
  }
  fitProjectSplit()
}

function persistProjectSplit() {
  if (typeof window === 'undefined') return
  window.localStorage.setItem(PROJECT_SPLIT_STORAGE_KEY, JSON.stringify({
    projectTree: Math.round(projectSplit.projectTree)
  }))
}

function startPaneResize(target, event) {
  const bounds = workbenchEl.value?.getBoundingClientRect()
  if (!bounds) return
  event.preventDefault()
  event.currentTarget?.setPointerCapture?.(event.pointerId)
  resizeState = {
    target,
    startX: event.clientX,
    startLeft: paneLayout.left,
    startRight: paneLayout.right
  }
  resizingPane.value = target
  window.addEventListener('pointermove', handlePaneResize)
  window.addEventListener('pointerup', stopPaneResize)
  window.addEventListener('pointercancel', stopPaneResize)
}

function handlePaneResize(event) {
  if (!resizeState) return
  const delta = event.clientX - resizeState.startX
  if (resizeState.target === 'left') {
    paneLayout.left = clamp(resizeState.startLeft + delta, MIN_LEFT_PANEL, maxLeftWidth())
  } else {
    paneLayout.right = clamp(resizeState.startRight - delta, MIN_RIGHT_PANEL, maxRightWidth())
  }
  fitPaneLayout()
}

function startProjectSplitResize(event) {
  const bounds = leftPanelEl.value?.getBoundingClientRect()
  if (!bounds) return
  event.preventDefault()
  event.currentTarget?.setPointerCapture?.(event.pointerId)
  projectSplitResizeState = {
    startY: event.clientY,
    startProjectTree: projectSplit.projectTree
  }
  resizingPane.value = 'project-layers'
  window.addEventListener('pointermove', handleProjectSplitResize)
  window.addEventListener('pointerup', stopProjectSplitResize)
  window.addEventListener('pointercancel', stopProjectSplitResize)
}

function handleProjectSplitResize(event) {
  if (!projectSplitResizeState) return
  const delta = event.clientY - projectSplitResizeState.startY
  projectSplit.projectTree = clamp(
    projectSplitResizeState.startProjectTree + delta,
    MIN_PROJECT_TREE,
    maxProjectTreeHeight()
  )
  fitProjectSplit()
}

function resizePaneByKeyboard(target, event) {
  const smallStep = event.shiftKey ? 48 : 16
  const largeStep = 96
  let handled = true

  if (target === 'left') {
    if (event.key === 'ArrowLeft') {
      paneLayout.left = clamp(paneLayout.left - smallStep, MIN_LEFT_PANEL, maxLeftWidth())
    } else if (event.key === 'ArrowRight') {
      paneLayout.left = clamp(paneLayout.left + smallStep, MIN_LEFT_PANEL, maxLeftWidth())
    } else if (event.key === 'PageUp') {
      paneLayout.left = clamp(paneLayout.left + largeStep, MIN_LEFT_PANEL, maxLeftWidth())
    } else if (event.key === 'PageDown') {
      paneLayout.left = clamp(paneLayout.left - largeStep, MIN_LEFT_PANEL, maxLeftWidth())
    } else if (event.key === 'Home') {
      paneLayout.left = MIN_LEFT_PANEL
    } else if (event.key === 'End') {
      paneLayout.left = maxLeftWidth()
    } else {
      handled = false
    }
  } else if (target === 'right') {
    if (event.key === 'ArrowLeft') {
      paneLayout.right = clamp(paneLayout.right + smallStep, MIN_RIGHT_PANEL, maxRightWidth())
    } else if (event.key === 'ArrowRight') {
      paneLayout.right = clamp(paneLayout.right - smallStep, MIN_RIGHT_PANEL, maxRightWidth())
    } else if (event.key === 'PageUp') {
      paneLayout.right = clamp(paneLayout.right + largeStep, MIN_RIGHT_PANEL, maxRightWidth())
    } else if (event.key === 'PageDown') {
      paneLayout.right = clamp(paneLayout.right - largeStep, MIN_RIGHT_PANEL, maxRightWidth())
    } else if (event.key === 'Home') {
      paneLayout.right = MIN_RIGHT_PANEL
    } else if (event.key === 'End') {
      paneLayout.right = maxRightWidth()
    } else {
      handled = false
    }
  }

  if (!handled) return
  event.preventDefault()
  fitPaneLayout()
  persistPaneLayout()
  editor?.refresh?.()
}

function resizeProjectSplitByKeyboard(event) {
  const smallStep = event.shiftKey ? 48 : 16
  const largeStep = 96
  let handled = true

  if (event.key === 'ArrowUp') {
    projectSplit.projectTree = clamp(projectSplit.projectTree - smallStep, MIN_PROJECT_TREE, maxProjectTreeHeight())
  } else if (event.key === 'ArrowDown') {
    projectSplit.projectTree = clamp(projectSplit.projectTree + smallStep, MIN_PROJECT_TREE, maxProjectTreeHeight())
  } else if (event.key === 'PageUp') {
    projectSplit.projectTree = clamp(projectSplit.projectTree - largeStep, MIN_PROJECT_TREE, maxProjectTreeHeight())
  } else if (event.key === 'PageDown') {
    projectSplit.projectTree = clamp(projectSplit.projectTree + largeStep, MIN_PROJECT_TREE, maxProjectTreeHeight())
  } else if (event.key === 'Home') {
    projectSplit.projectTree = MIN_PROJECT_TREE
  } else if (event.key === 'End') {
    projectSplit.projectTree = maxProjectTreeHeight()
  } else {
    handled = false
  }

  if (!handled) return
  event.preventDefault()
  fitProjectSplit()
  persistProjectSplit()
}

function stopPaneResize() {
  if (!resizeState) return
  resizeState = null
  resizingPane.value = ''
  window.removeEventListener('pointermove', handlePaneResize)
  window.removeEventListener('pointerup', stopPaneResize)
  window.removeEventListener('pointercancel', stopPaneResize)
  persistPaneLayout()
  editor?.refresh?.()
}

function stopProjectSplitResize() {
  if (!projectSplitResizeState) return
  projectSplitResizeState = null
  resizingPane.value = ''
  window.removeEventListener('pointermove', handleProjectSplitResize)
  window.removeEventListener('pointerup', stopProjectSplitResize)
  window.removeEventListener('pointercancel', stopProjectSplitResize)
  persistProjectSplit()
}

function fitPaneLayout() {
  if (typeof window === 'undefined') return
  const available = (workbenchEl.value?.getBoundingClientRect().width || window.innerWidth) - RAIL_WIDTH - activeResizerWidth()
  const maxSideTotal = Math.max(MIN_LEFT_PANEL + MIN_RIGHT_PANEL, available - MIN_CANVAS)
  const currentSideTotal = paneLayout.left + paneLayout.right
  if (currentSideTotal <= maxSideTotal) return

  const overflow = currentSideTotal - maxSideTotal
  const leftShare = paneLayout.left / currentSideTotal
  paneLayout.left = Math.max(MIN_LEFT_PANEL, paneLayout.left - (overflow * leftShare))
  paneLayout.right = Math.max(MIN_RIGHT_PANEL, paneLayout.right - (overflow * (1 - leftShare)))
}

function fitProjectSplit() {
  projectSplit.projectTree = clamp(projectSplit.projectTree, MIN_PROJECT_TREE, maxProjectTreeHeight())
}

function defaultPaneLayout() {
  const available = (workbenchEl.value?.getBoundingClientRect().width || window.innerWidth) - RAIL_WIDTH - (RESIZER_WIDTH * 2)
  const sideTotal = Math.max(MIN_LEFT_PANEL + MIN_RIGHT_PANEL, available * (1 - DEFAULT_CANVAS_SHARE))
  const left = clamp(sideTotal * DEFAULT_LEFT_SIDE_SHARE, MIN_LEFT_PANEL, 680)
  const right = clamp(sideTotal - left, MIN_RIGHT_PANEL, 560)
  return { left, right }
}

function maxLeftWidth() {
  const available = (workbenchEl.value?.getBoundingClientRect().width || window.innerWidth) - RAIL_WIDTH - activeResizerWidth()
  return Math.max(MIN_LEFT_PANEL, available - paneLayout.right - MIN_CANVAS)
}

function maxRightWidth() {
  const available = (workbenchEl.value?.getBoundingClientRect().width || window.innerWidth) - RAIL_WIDTH - activeResizerWidth()
  const leftWidth = isLeftPanelOpen.value ? paneLayout.left : 0
  return Math.max(MIN_RIGHT_PANEL, available - leftWidth - MIN_CANVAS)
}

function activeResizerWidth() {
  return isLeftPanelOpen.value ? RESIZER_WIDTH * 2 : RESIZER_WIDTH
}

function maxProjectTreeHeight() {
  const panelHeight = leftPanelEl.value?.getBoundingClientRect().height || window.innerHeight - 52
  return Math.max(MIN_PROJECT_TREE, panelHeight - MIN_LAYERS - PROJECT_RESIZER_HEIGHT)
}

function clamp(value, min, max) {
  return Math.min(max, Math.max(min, value))
}

async function loadEditorData() {
  loading.value = true
  editorError.value = ''
  try {
    const [siteResponse, pagesResponse, templatesResponse, assetsResponse, aiTemplatesResponse] = await Promise.all([
      api.get(`/Sites/${siteId}`),
      api.get(`/Pages/site/${siteId}`),
      api.get('/WidgetTemplates'),
      api.get(`/Assets/site/${siteId}`).catch(() => ({ data: { data: [] } })),
      api.get('/AiConversations/templates?kind=page')
    ])
    site.value = unwrap(siteResponse)
    pages.value = (unwrap(pagesResponse) || []).sort((a, b) => a.displayOrder - b.displayOrder)
    templates.value = unwrap(templatesResponse) || []
    assets.value = unwrap(assetsResponse) || []
    aiTemplates.value = unwrap(aiTemplatesResponse) || []

    if (!pageId.value) {
      const home = pages.value.find((item) => item.isHome) || pages.value[0]
      if (home) {
        pageId.value = home.id
        router.replace(`/editor/${siteId}/${home.id}`)
      }
    }

    if (pageId.value) {
      const pageResponse = await api.get(`/Pages/${pageId.value}`)
      page.value = unwrap(pageResponse)
    }
  } catch (e) {
    editorError.value = errorMessage(e, t('common.operationFailed'))
  } finally {
    loading.value = false
  }
}

function initGrapesJS() {
  if (editorError.value) return
  if (typeof window === 'undefined' || !window.grapesjs) {
    editorError.value = t('editor.grapesMissing')
    return
  }

  editor = window.grapesjs.init({
    container: '#gjs',
    height: '100%',
    fromElement: false,
    storageManager: false,
    noticeOnUnload: false,
    panels: { defaults: [] },
    blockManager: { appendTo: '#blocks-panel' },
    layerManager: { appendTo: '#layers-panel' },
    selectorManager: { appendTo: '#selectors-panel' },
    styleManager: {
      appendTo: '#styles-panel',
      sectors: defaultStyleSectors()
    },
    traitManager: { appendTo: '#traits-panel' },
    deviceManager: {
      devices: [
        { name: 'Desktop', width: '' },
        { name: 'Tablet', width: '768px' },
        { name: 'Mobile portrait', width: '390px' }
      ]
    },
    assetManager: {
      upload: false,
      assets: filteredAssets.value.map((asset) => asset.publicUrl || asset)
    },
    canvas: {
      styles: ['https://cdn.jsdelivr.net/npm/tailwindcss@2.2.19/dist/tailwind.min.css']
    }
  })

  registerBlocks()
  loadPageIntoEditor()
  editor.setDevice(device.value)
  editor.on('update', () => {
    hasUnsavedChanges.value = true
  })
  editor.on('component:selected', (component) => {
    selectedComponentName.value = component.getName?.() || component.get('tagName') || component.getId?.() || 'Component'
    rightTab.value = 'styles'
  })
  editor.on('component:deselected', () => {
    selectedComponentName.value = ''
  })
  editorReady.value = true
}

function registerBlocks() {
  templates.value
    .filter((template) => template.isActive !== false)
    .sort((a, b) => a.displayOrder - b.displayOrder)
    .forEach((template) => {
      editor.BlockManager.add(`template-${template.id}`, {
        label: template.name,
        category: template.category || 'Content',
        media: blockIcon(template.category || template.name),
        content: template.defaultContent,
        attributes: { title: template.description || template.name }
      })
    })

  const basicBlocks = [
    {
      id: 'basic-section',
      label: t('editor.blockSection'),
      media: blockIcon('section'),
      content: `<section class="py-16 px-6"><div class="max-w-6xl mx-auto"><h2 class="text-3xl font-bold text-gray-900">${t('editor.basicSectionTitle')}</h2><p class="mt-3 text-gray-600">${t('editor.basicSectionBody')}</p></div></section>`
    },
    {
      id: 'basic-text',
      label: t('editor.blockText'),
      media: blockIcon('text'),
      content: `<p class="text-lg leading-7 text-gray-700">${t('editor.basicTextContent')}</p>`
    },
    {
      id: 'basic-button',
      label: t('editor.blockButton'),
      media: blockIcon('button'),
      content: `<a href="#" class="inline-block px-5 py-3 bg-blue-600 text-white font-semibold rounded-md">${t('editor.basicButtonText')}</a>`
    },
    {
      id: 'basic-image',
      label: t('editor.blockImage'),
      media: blockIcon('image'),
      content: `<img src="https://images.unsplash.com/photo-1522071820081-009f0129c71c?w=1200" alt="${t('editor.blockTeamAlt')}" class="w-full rounded-lg" />`
    },
    {
      id: 'basic-form',
      label: t('editor.blockForm'),
      media: blockIcon('form'),
      content: `<form class="grid gap-3 max-w-md"><input class="border rounded-md px-4 py-3" placeholder="${t('editor.formName')}" /><input class="border rounded-md px-4 py-3" placeholder="Email" /><button class="bg-gray-900 text-white rounded-md px-4 py-3">${t('editor.formSubmit')}</button></form>`
    }
  ]
  basicBlocks.forEach((block) => editor.BlockManager.add(block.id, { ...block, category: t('editor.basic') }))
}

function blockIcon(value = '') {
  const key = value.toLowerCase()
  if (key.includes('hero')) return '<span class="sf-block-icon hero"></span>'
  if (key.includes('footer')) return '<span class="sf-block-icon footer"></span>'
  if (key.includes('feature') || key.includes('content')) return '<span class="sf-block-icon features"></span>'
  if (key.includes('about') || key.includes('team')) return '<span class="sf-block-icon text"></span>'
  if (key.includes('contact') || key.includes('form')) return '<span class="sf-block-icon form"></span>'
  if (key.includes('button')) return '<span class="sf-block-icon button"></span>'
  if (key.includes('image')) return '<span class="sf-block-icon image"></span>'
  if (key.includes('text')) return '<span class="sf-block-icon text"></span>'
  return '<span class="sf-block-icon section"></span>'
}

function loadPageIntoEditor() {
  const currentPage = page.value
  if (!currentPage) {
    editor.setComponents(defaultBlankPage())
    hasUnsavedChanges.value = true
    return
  }

  const hasComponents = currentPage.components && currentPage.components !== '[]'
  const hasStyles = currentPage.styles && currentPage.styles !== '[]'

  if (hasComponents) {
    try {
      editor.setComponents(JSON.parse(currentPage.components))
    } catch {
      editor.setComponents(currentPage.htmlContent || defaultBlankPage())
    }
  } else {
    editor.setComponents(currentPage.htmlContent || defaultBlankPage())
  }

  if (hasStyles) {
    try {
      editor.setStyle(JSON.parse(currentPage.styles))
    } catch {
      editor.setStyle(currentPage.cssContent || '')
    }
  } else if (currentPage.cssContent) {
    editor.setStyle(currentPage.cssContent)
  }

  applyTemplateHeadAssets(currentPage.jsContent || '')
  hasUnsavedChanges.value = false
}

async function savePage() {
  if (!editor || !pageId.value) return
  saving.value = true
  try {
    const response = await api.put(`/Pages/${pageId.value}`, {
      htmlContent: editor.getHtml(),
      cssContent: editor.getCss(),
      jsContent: preserveTemplateHead(page.value?.jsContent || '', editor.getJs?.() || ''),
      components: JSON.stringify(editor.getComponents().toJSON()),
      styles: JSON.stringify(editor.getStyle())
    })
    page.value = { ...page.value, ...unwrap(response) }
    hasUnsavedChanges.value = false
  } catch (e) {
    alert(errorMessage(e, t('common.operationFailed')))
  } finally {
    saving.value = false
  }
}

async function publishSite() {
  publishing.value = true
  try {
    if (hasUnsavedChanges.value) await savePage()
    await api.post(`/Sites/${siteId}/publish`, { taskType: 'full_publish', targetUrl: '' })
    alert(t('editor.publishSuccess'))
  } catch (e) {
    alert(errorMessage(e, t('common.operationFailed')))
  } finally {
    publishing.value = false
  }
}

async function createPage() {
  const title = prompt(t('editor.newPagePrompt'), t('editor.newPageDefault'))
  if (!title) return
  try {
    const response = await api.post(`/Pages/site/${siteId}`, {
      title,
      slug: slugify(title),
      pageType: 'custom',
      isHome: false
    })
    const created = unwrap(response)
    await loadEditorData()
    if (created?.id) await openPage(created.id)
  } catch (e) {
    alert(errorMessage(e, t('common.operationFailed')))
  }
}

async function openPage(targetPageId) {
  if (targetPageId === pageId.value) return
  if (hasUnsavedChanges.value && !confirm(t('editor.unsavedLeavePage'))) return
  router.push(`/editor/${siteId}/${targetPageId}`)
  pageId.value = targetPageId
  const response = await api.get(`/Pages/${targetPageId}`)
  page.value = unwrap(response)
  if (editor) loadPageIntoEditor()
}

async function registerAssetUrl() {
  const url = assetUrl.value.trim()
  if (!url) {
    openAssetManager()
    return
  }
  try {
    const response = await api.post('/Assets', {
      siteId,
      fileName: filenameFromUrl(url),
      mimeType: 'image/*',
      fileSize: 0,
      storagePath: url,
      publicUrl: url,
      source: 'external'
    })
    const asset = unwrap(response)
    assets.value = [asset, ...assets.value]
    editor?.AssetManager.add(url)
    assetUrl.value = ''
  } catch (e) {
    alert(errorMessage(e, t('common.operationFailed')))
  }
}

async function generateCurrentPage() {
  if (!editor || !pageId.value) return
  generatingCurrentPage.value = true
  try {
    const response = await api.post('/AiConversations/generate-page', {
      siteId,
      pageId: pageId.value,
      pageName: page.value?.title || 'Generated Page',
      pageType: aiPageType.value,
      templateKey: aiTemplateKey.value,
      prompt: aiPrompt.value || `Generate a polished ${aiPageType.value} page for ${site.value?.name || 'this website'}.`,
      style: aiStyle.value,
      contentLength: 'medium'
    })
    const generated = unwrap(response)
    page.value = {
      ...page.value,
      title: generated.pageName,
      slug: generated.slug,
      pageType: generated.pageType,
      htmlContent: generated.htmlContent,
      cssContent: generated.cssContent,
      jsContent: generated.jsContent,
      components: generated.components,
      styles: generated.styles
    }
    editor.setComponents(generated.htmlContent || defaultBlankPage())
    editor.setStyle(generated.cssContent || '')
    applyTemplateHeadAssets(generated.jsContent || '')
    hasUnsavedChanges.value = false
    selectedComponentName.value = ''
    alert(t('editor.generatedApplied'))
  } catch (e) {
    alert(errorMessage(e, t('common.operationFailed')))
  } finally {
    generatingCurrentPage.value = false
  }
}

function applyEditorTemplate() {
  const template = pageTemplates.value.find((item) => item.key === aiTemplateKey.value)
  if (!template) return

  aiPageType.value = template.pageTypes?.[0] || aiPageType.value
  if (!aiPrompt.value) aiPrompt.value = template.description
}

function extractTemplateHead(js) {
  const start = '/*SITEFORGE_TEMPLATE_HEAD_START'
  const end = 'SITEFORGE_TEMPLATE_HEAD_END*/'
  const startIndex = js.indexOf(start)
  if (startIndex < 0) return ''
  const contentStart = startIndex + start.length
  const endIndex = js.indexOf(end, contentStart)
  if (endIndex < 0) return ''
  return js.slice(contentStart, endIndex).trim()
}

function preserveTemplateHead(originalJs, nextJs) {
  const head = extractTemplateHead(originalJs)
  return head ? `/*SITEFORGE_TEMPLATE_HEAD_START\n${head}\nSITEFORGE_TEMPLATE_HEAD_END*/\n${nextJs || ''}` : nextJs
}

function applyTemplateHeadAssets(js) {
  const headHtml = extractTemplateHead(js)
  const canvasDoc = editor?.Canvas?.getDocument?.()
  if (!headHtml || !canvasDoc?.head) return

  canvasDoc.head.querySelectorAll('[data-siteforge-template-head]').forEach((node) => node.remove())

  const template = document.createElement('template')
  template.innerHTML = headHtml
  Array.from(template.content.childNodes).forEach((node) => {
    if (node.nodeType !== Node.ELEMENT_NODE) return
    const element = node
    let copy
    if (element.tagName?.toLowerCase() === 'script') {
      copy = canvasDoc.createElement('script')
      Array.from(element.attributes).forEach((attr) => copy.setAttribute(attr.name, attr.value))
      copy.textContent = element.textContent || ''
    } else {
      copy = element.cloneNode(true)
    }
    copy.setAttribute('data-siteforge-template-head', 'true')
    canvasDoc.head.appendChild(copy)
  })
}

function selectAsset(url) {
  editor?.AssetManager.add(url)
  editor?.runCommand('open-assets')
}

function setDevice(targetDevice) {
  device.value = targetDevice
  editor?.setDevice(targetDevice)
}

function togglePreview() {
  if (!editor) return
  previewMode.value = !previewMode.value
  if (previewMode.value) {
    editor.runCommand('preview')
  } else {
    editor.stopCommand('preview')
  }
}

function undo() {
  editor?.UndoManager?.undo()
}

function redo() {
  editor?.UndoManager?.redo()
}

function openAssetManager() {
  editor?.runCommand('open-assets')
}

function applyGlobalTokens() {
  if (!editor) return
  editor.addStyle(`
    body {
      font-family: ${fontFamily.value}, ui-sans-serif, system-ui, sans-serif;
      line-height: ${lineHeight.value};
    }
    a, button, .sf-primary {
      --siteforge-primary: ${colorTokens[0].value};
    }
  `)
  hasUnsavedChanges.value = true
}

function createSampleTable() {
  sampleTable.value = [
    { key: 'title', value: page.value?.title || t('common.home') },
    { key: 'slug', value: page.value?.slug || 'home' },
    { key: 'status', value: page.value?.isPublished ? t('common.published') : t('common.draft') }
  ]
}

function backToWorkspace() {
  if (hasUnsavedChanges.value && !confirm(t('editor.unsavedBackWorkspace'))) return
  router.push(`/sites/${siteId}`)
}

function filenameFromUrl(url) {
  try {
    const pathname = new URL(url).pathname
    return decodeURIComponent(pathname.split('/').filter(Boolean).pop() || 'asset')
  } catch {
    return 'asset'
  }
}

function slugify(value) {
  return value
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9\u4e00-\u9fa5]+/g, '-')
    .replace(/^-+|-+$/g, '') || `page-${Date.now()}`
}

function defaultBlankPage() {
  return `
    <main>
      <section class="py-20 px-6 bg-white">
        <div class="max-w-5xl mx-auto">
          <p class="text-sm font-semibold text-blue-600 mb-3">${t('editor.defaultBlankKicker')}</p>
          <h1 class="text-5xl font-bold text-gray-900 mb-6">${t('editor.defaultBlankTitle')}</h1>
          <p class="text-xl text-gray-600 max-w-2xl">${t('editor.defaultBlankBody')}</p>
        </div>
      </section>
    </main>
  `
}

function defaultStyleSectors() {
  return [
    {
      name: t('editor.sectorLayout'),
      open: true,
      buildProps: ['display', 'position', 'width', 'min-height', 'margin', 'padding']
    },
    {
      name: t('editor.sectorSize'),
      open: true,
      buildProps: ['width', 'height', 'min-width', 'min-height', 'max-width', 'max-height']
    },
    {
      name: t('editor.sectorTypography'),
      open: false,
      buildProps: ['font-family', 'font-size', 'font-weight', 'line-height', 'color', 'text-align', 'text-decoration']
    },
    {
      name: t('editor.sectorBackground'),
      open: false,
      buildProps: ['background-color', 'background-image', 'background-size', 'background-position']
    },
    {
      name: t('editor.sectorBorders'),
      open: false,
      buildProps: ['border-radius', 'border-width', 'border-style', 'border-color']
    },
    {
      name: t('editor.sectorEffects'),
      open: false,
      buildProps: ['opacity', 'box-shadow', 'transform']
    },
    {
      name: t('editor.sectorFlex'),
      open: false,
      buildProps: ['flex-direction', 'justify-content', 'align-items', 'gap']
    }
  ]
}
</script>

<style scoped>
.studio-editor {
  width: 100vw;
  min-width: 100%;
  height: 100vh;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  background: var(--sf-page);
  color: var(--sf-ink);
  transition: background 280ms ease, color 280ms ease;
}

.studio-topbar {
  min-height: 52px;
  display: grid;
  grid-template-columns: 1fr auto 1fr;
  align-items: center;
  gap: 12px;
  border-bottom: 1px solid var(--sf-header-line);
  background: var(--sf-header-bg);
  backdrop-filter: blur(14px);
  -webkit-backdrop-filter: blur(14px);
  padding: 6px 12px;
  transition: border-color 280ms ease, background 280ms ease;
}

.topbar-left,
.topbar-center,
.topbar-right {
  display: flex;
  align-items: center;
  gap: 9px;
}

.topbar-center {
  justify-content: center;
}

.topbar-right {
  justify-content: flex-end;
}

.chrome-button,
.tool-button,
.publish-button,
.upgrade-button,
.device-menu button,
.panel-icon-button,
.panel-action,
.filter-chip,
.upload-button {
  border: 1px solid var(--sf-line);
  border-radius: 6px;
  background: var(--sf-surface);
  color: var(--sf-ink);
  cursor: pointer;
  font-weight: 780;
  transition: border-color 160ms ease, background 160ms ease, color 160ms ease;
}

.chrome-button,
.tool-button,
.publish-button,
.upgrade-button {
  min-height: 36px;
  padding: 0 14px;
}

.back-button {
  min-width: 38px;
  padding: 0;
}

.code-button {
  color: #bfc2cb;
}

.device-menu {
  display: flex;
  border: 1px solid var(--sf-line);
  border-radius: 7px;
  padding: 2px;
  background: var(--sf-surface);
  transition: border-color 280ms ease, background 280ms ease;
}

.device-menu button {
  min-height: 30px;
  border: 0;
  padding: 0 12px;
  background: transparent;
}

.device-menu button.active {
  background: var(--sf-primary-soft);
  color: var(--sf-primary-strong);
}

.publish-button {
  border-color: var(--sf-primary);
  background: var(--sf-primary);
  color: #fff;
}

.theme-dark .publish-button {
  color: #101828;
}

.upgrade-button {
  border-color: #ffce57;
  background: #ffce57;
  color: #4a3310;
}

button:disabled {
  cursor: not-allowed;
  opacity: 0.55;
}

.studio-workbench {
  width: 100%;
  flex: 1;
  display: grid;
  grid-template-columns:
    44px
    var(--sf-left-panel-width, 400px)
    var(--sf-left-resizer-width, 8px)
    minmax(720px, 1fr)
    8px
    var(--sf-right-panel-width, 340px);
  min-height: 0;
}

.studio-workbench.resizing {
  cursor: col-resize;
}

.studio-workbench.resizing * {
  user-select: none;
}

.editor-rail {
  display: flex;
  flex-direction: column;
  gap: 6px;
  border-right: 1px solid var(--sf-line);
  background: var(--sf-sidebar-bg);
  padding: 6px;
  transition: border-color 280ms ease, background 280ms ease;
}

.editor-rail button {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  border: 0;
  border-radius: 6px;
  background: transparent;
  color: var(--sf-sidebar-text);
  cursor: pointer;
}

.editor-rail button.active,
.editor-rail button:hover {
  background: var(--sf-primary);
  color: #fff;
}

.theme-dark .editor-rail button.active,
.theme-dark .editor-rail button:hover {
  color: #101828;
}

.rail-bottom {
  margin-top: auto;
}

.rail-icon {
  position: relative;
  width: 24px;
  height: 24px;
  display: block;
  transform: scale(0.67);
  transform-origin: center;
}

.rail-icon.plus::before,
.rail-icon.plus::after,
.rail-icon.home::before,
.rail-icon.home::after,
.rail-icon.database::before,
.rail-icon.database::after,
.rail-icon.spark::before,
.rail-icon.spark::after {
  content: "";
  position: absolute;
  background: currentColor;
}

.rail-icon.plus::before {
  width: 18px;
  height: 4px;
  left: 3px;
  top: 10px;
}

.rail-icon.plus::after {
  width: 4px;
  height: 18px;
  left: 10px;
  top: 3px;
}

.rail-icon.layers {
  background:
    linear-gradient(currentColor 0 0) 3px 5px / 18px 3px no-repeat,
    linear-gradient(currentColor 0 0) 5px 10px / 14px 3px no-repeat,
    linear-gradient(currentColor 0 0) 7px 15px / 10px 3px no-repeat;
}

.rail-icon.palette {
  border: 3px solid currentColor;
  border-radius: 50%;
}

.rail-icon.palette::after {
  content: "";
  position: absolute;
  width: 8px;
  height: 8px;
  right: -2px;
  bottom: -1px;
  border-radius: 50%;
  background: currentColor;
}

.rail-icon.image {
  border: 3px solid currentColor;
  border-radius: 3px;
}

.rail-icon.image::after {
  content: "";
  position: absolute;
  left: 4px;
  bottom: 4px;
  border-left: 6px solid transparent;
  border-right: 6px solid transparent;
  border-bottom: 8px solid currentColor;
}

.rail-icon.database::before {
  width: 20px;
  height: 8px;
  left: 2px;
  top: 3px;
  border-radius: 50%;
}

.rail-icon.database::after {
  width: 20px;
  height: 14px;
  left: 2px;
  top: 7px;
  border-radius: 0 0 50% 50%;
}

.rail-icon.spark::before {
  width: 4px;
  height: 22px;
  left: 10px;
  top: 1px;
}

.rail-icon.spark::after {
  width: 22px;
  height: 4px;
  left: 1px;
  top: 10px;
}

.rail-icon.home::before {
  width: 17px;
  height: 13px;
  left: 4px;
  top: 9px;
}

.rail-icon.home::after {
  width: 14px;
  height: 14px;
  left: 5px;
  top: 4px;
  transform: rotate(45deg);
}

.studio-left-panel,
.studio-right-panel {
  min-width: 0;
  overflow: auto;
  background: var(--sf-surface);
  transition: background 280ms ease;
}

.studio-left-panel {
  border-right: 1px solid var(--sf-line);
  transition: border-color 280ms ease;
}

.studio-left-panel.collapsed {
  overflow: hidden;
  border-right: 0;
  pointer-events: none;
}

.studio-right-panel {
  border-left: 1px solid var(--sf-line);
  transition: border-color 280ms ease;
}

.pane-resizer {
  position: relative;
  z-index: 8;
  width: 8px;
  min-width: 8px;
  border: 0;
  border-left: 1px solid transparent;
  border-right: 1px solid transparent;
  padding: 0;
  background: var(--sf-stage-bg);
  cursor: col-resize;
  outline: none;
  transition: background 160ms ease, border-color 160ms ease;
}

.pane-resizer::before {
  content: "";
  position: absolute;
  inset: 0 -6px;
}

.pane-resizer-track {
  content: "";
  position: absolute;
  top: 50%;
  left: 50%;
  width: 16px;
  height: 74px;
  display: grid;
  place-items: center;
  gap: 5px;
  transform: translate(-50%, -50%);
  border-radius: 999px;
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: #292b33;
  box-shadow: 0 10px 28px rgba(0, 0, 0, 0.18);
  opacity: 0.86;
  transition: background 160ms ease, border-color 160ms ease, opacity 160ms ease, transform 160ms ease;
}

.pane-resizer-track span {
  width: 4px;
  height: 4px;
  border-radius: 999px;
  background: #9da0aa;
  transition: background 160ms ease, transform 160ms ease;
}

.pane-resizer:hover,
.pane-resizer:focus-visible,
.pane-resizer.active {
  border-color: rgba(168, 136, 255, 0.4);
  background: rgba(131, 88, 237, 0.16);
}

.pane-resizer:hover .pane-resizer-track,
.pane-resizer:focus-visible .pane-resizer-track,
.pane-resizer.active .pane-resizer-track {
  background: var(--sf-primary);
  border-color: rgba(255, 255, 255, 0.18);
  opacity: 1;
  transform: translate(-50%, -50%) scale(1.04);
}

.pane-resizer:hover .pane-resizer-track span,
.pane-resizer:focus-visible .pane-resizer-track span,
.pane-resizer.active .pane-resizer-track span {
  background: #fff;
  transform: scale(1.08);
}

.left-resizer[aria-hidden="true"] {
  overflow: hidden;
  pointer-events: none;
  opacity: 0;
}

.studio-panel,
.right-content {
  padding: 14px;
}

.project-panel {
  height: 100%;
  display: grid;
  grid-template-rows:
    minmax(120px, var(--sf-project-tree-height, 360px))
    14px
    minmax(0, 1fr);
  overflow: hidden;
  padding: 0;
}

.project-pane,
.layers-pane {
  min-height: 0;
  overflow: auto;
  padding: 14px;
}

.project-pane {
  padding-bottom: 8px;
}

.layers-pane {
  padding-top: 8px;
}

.stack-resizer {
  position: relative;
  z-index: 7;
  width: 100%;
  height: 14px;
  border: 0;
  border-top: 1px solid transparent;
  border-bottom: 1px solid transparent;
  padding: 0;
  background: var(--sf-surface);
  cursor: row-resize;
  outline: none;
  transition: background 160ms ease, border-color 160ms ease;
}

.stack-resizer::before {
  content: "";
  position: absolute;
  inset: -7px 0;
}

.stack-resizer-track {
  position: absolute;
  top: 50%;
  left: 50%;
  width: 96px;
  height: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  transform: translate(-50%, -50%);
  border-radius: 999px;
  background: #292b33;
  opacity: 0.88;
  transition: background 160ms ease, opacity 160ms ease, transform 160ms ease;
}

.stack-resizer-track::before {
  content: "";
  position: absolute;
  left: -62px;
  right: -62px;
  top: 50%;
  height: 1px;
  transform: translateY(-50%);
  background: #30313a;
}

.stack-resizer-track span {
  position: relative;
  z-index: 1;
  width: 4px;
  height: 4px;
  border-radius: 999px;
  background: #9da0aa;
}

.stack-resizer:hover,
.stack-resizer:focus-visible,
.stack-resizer.active {
  border-color: rgba(168, 136, 255, 0.36);
  background: rgba(131, 88, 237, 0.12);
}

.stack-resizer:hover .stack-resizer-track,
.stack-resizer:focus-visible .stack-resizer-track,
.stack-resizer.active .stack-resizer-track {
  background: var(--sf-primary);
  opacity: 1;
  transform: translate(-50%, -50%) scale(1.03);
}

.stack-resizer:hover .stack-resizer-track span,
.stack-resizer:focus-visible .stack-resizer-track span,
.stack-resizer.active .stack-resizer-track span {
  background: #fff;
}

.panel-title-row {
  min-height: 38px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.panel-title-row h2 {
  color: #bfc2cc;
  font-size: 18px;
  font-weight: 760;
}

.panel-icon-button {
  width: 34px;
  height: 34px;
  display: grid;
  place-items: center;
  font-size: 22px;
  line-height: 1;
}

.segmented {
  display: grid;
  grid-template-columns: 1fr 1fr;
  margin: 8px 0 12px;
  border-radius: 999px;
  background: #42444e;
  padding: 4px;
}

.segmented button {
  min-height: 34px;
  border: 0;
  border-radius: 999px;
  background: transparent;
  color: #b8bbc6;
  cursor: pointer;
  font-weight: 760;
}

.segmented button.active {
  background: #18191e;
  color: #b998ff;
}

.panel-search,
.property-summary input,
.field-row input,
.field-row select {
  width: 100%;
  min-height: 42px;
  border: 1px solid #3a3b45;
  border-radius: 5px;
  background: #15161b;
  color: #dfe1e8;
  padding: 0 12px;
}

.panel-search::placeholder {
  color: #777a84;
}

.project-tree {
  display: grid;
  gap: 4px;
  padding: 5px 0 2px;
}

.tree-root,
.tree-node {
  display: grid;
  align-items: center;
  border-radius: 7px;
  color: #c7cad4;
}

.tree-root {
  grid-template-columns: 18px 22px minmax(0, 1fr);
  min-height: 32px;
  padding: 0 9px;
  background: #1a1b21;
  font-weight: 850;
}

.tree-caret {
  color: #9b9faa;
  font-size: 14px;
}

.tree-site-icon,
.tree-file-icon {
  position: relative;
  width: 16px;
  height: 18px;
  display: block;
  color: currentColor;
}

.tree-site-icon::before,
.tree-file-icon::before {
  content: "";
  position: absolute;
  inset: 2px 1px 1px;
  border: 2px solid currentColor;
  border-radius: 3px;
}

.tree-site-icon::after {
  content: "";
  position: absolute;
  left: 4px;
  right: 4px;
  top: 7px;
  height: 2px;
  background: currentColor;
  box-shadow: 0 4px 0 currentColor;
}

.tree-file-icon::after {
  content: "";
  position: absolute;
  right: 1px;
  top: 2px;
  width: 6px;
  height: 6px;
  border-left: 2px solid currentColor;
  border-bottom: 2px solid currentColor;
  background: #15161b;
}

.tree-file-icon.home::before {
  border-color: #b998ff;
}

.tree-node {
  width: 100%;
  grid-template-columns: 16px 22px minmax(0, 1fr) auto;
  gap: 5px;
  min-height: 38px;
  border: 1px solid #2f3038;
  background: #15161b;
  padding: 5px 8px;
  cursor: pointer;
  text-align: left;
}

.tree-node:hover,
.tree-node.active {
  border-color: #8b62f6;
  background: #24242d;
}

.tree-line {
  width: 10px;
  height: 18px;
  border-left: 1px solid #4b4d57;
  border-bottom: 1px solid #4b4d57;
  align-self: center;
  margin-left: 5px;
}

.tree-label {
  min-width: 0;
  display: flex;
  align-items: baseline;
  gap: 7px;
}

.tree-label strong,
.tree-label small {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.tree-label strong {
  color: #dfe1e8;
  flex: 0 1 auto;
  font-weight: 860;
}

.tree-label small {
  color: #969aa5;
  flex: 1 1 auto;
  font-size: 12px;
}

.tree-node em {
  border: 1px solid #4d3b78;
  border-radius: 999px;
  padding: 1px 6px;
  color: #d9c8ff;
  font-size: 10px;
  font-style: normal;
  font-weight: 850;
}

.block-section-title {
  margin: 12px -14px 8px;
  border-top: 1px solid #30313a;
  border-bottom: 1px solid #30313a;
  padding: 8px 14px;
  color: #9da0aa;
  font-size: 16px;
  font-weight: 780;
}

.add-blocks-button,
.panel-action,
.upload-button {
  width: 100%;
  min-height: 42px;
}

.add-blocks-button {
  position: sticky;
  bottom: 0;
  border: 0;
  border-radius: 5px;
  background: #8358ed;
  color: white;
  cursor: pointer;
  font-weight: 820;
}

.studio-list {
  display: grid;
  gap: 7px;
}

.studio-list button {
  display: block;
  width: 100%;
  border: 1px solid #30313a;
  border-radius: 5px;
  background: transparent;
  color: #b8bbc5;
  cursor: pointer;
  padding: 10px 11px;
  text-align: left;
}

.studio-list button.active,
.studio-list button:hover {
  background: #3b3c45;
  color: #fff;
}

.studio-list span,
.studio-list small {
  display: block;
}

.studio-list span {
  font-weight: 780;
}

.studio-list small {
  color: #878a95;
}

.panel-divider {
  height: 1px;
  margin: 16px -14px 12px;
  background: #30313a;
}

.asset-controls {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 160px;
  gap: 10px;
  margin-bottom: 10px;
}

.filter-chip {
  padding: 0 10px;
}

.upload-button {
  margin-bottom: 10px;
  background: #8358ed;
  border-color: #8358ed;
  color: #fff;
}

.asset-url-input {
  margin-bottom: 12px;
}

.asset-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
}

.asset-tile {
  overflow: hidden;
  border: 1px solid #3a3b45;
  border-radius: 5px;
  background: #1d1e24;
  color: #b9bcc7;
  cursor: pointer;
  text-align: left;
}

.asset-tile img {
  width: 100%;
  aspect-ratio: 1.45;
  display: block;
  object-fit: cover;
  background: #242631;
}

.asset-tile span {
  display: block;
  overflow: hidden;
  padding: 7px;
  text-overflow: ellipsis;
  white-space: nowrap;
  font-size: 12px;
}

.style-group {
  margin-top: 10px;
  border: 1px solid #30313a;
  border-radius: 5px;
}

.style-group > button {
  width: 100%;
  min-height: 40px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border: 0;
  border-bottom: 1px solid #30313a;
  background: #26272d;
  color: #bfc2cc;
  padding: 0 10px;
  font-weight: 780;
}

.token-row,
.field-row,
.property-summary label {
  display: grid;
  grid-template-columns: 1fr auto auto;
  align-items: center;
  gap: 9px;
  padding: 9px;
  color: #aeb1bb;
}

.token-row input[type="color"] {
  width: 36px;
  height: 28px;
  border: 0;
  background: transparent;
}

.token-row code {
  color: #9da0aa;
}

.field-row {
  grid-template-columns: 1fr 130px;
}

.data-empty {
  min-height: 420px;
  display: grid;
  align-content: center;
  justify-items: center;
  gap: 12px;
  color: #a2a5af;
  text-align: center;
}

.data-icon {
  width: 54px;
  height: 36px;
  border-radius: 50%;
  background: #a8abb5;
  box-shadow: 0 11px 0 -1px #a8abb5, 0 22px 0 -2px #a8abb5;
}

.data-empty h3 {
  color: #bfc2cc;
  font-size: 22px;
}

.data-table {
  display: grid;
  gap: 8px;
}

.data-table div {
  display: flex;
  justify-content: space-between;
  border: 1px solid #30313a;
  border-radius: 5px;
  padding: 9px;
  background: #1c1d23;
}

.ai-panel {
  min-height: 100%;
  display: grid;
  grid-template-rows: auto minmax(120px, 1fr) auto auto auto;
  gap: 12px;
}

.ai-suggestions {
  display: grid;
  align-content: center;
  gap: 10px;
}

.ai-suggestions button {
  min-height: 42px;
  border: 1px solid #3a3b45;
  border-radius: 5px;
  background: transparent;
  color: #aeb1bb;
  cursor: pointer;
  padding: 0 12px;
}

.ai-generate-controls {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px;
}

.ai-generate-controls label {
  display: grid;
  gap: 6px;
  color: #aeb1bb;
  font-size: 12px;
  font-weight: 780;
}

.ai-generate-controls select {
  min-height: 38px;
  border: 1px solid #3a3b45;
  border-radius: 5px;
  background: #15161b;
  color: #dfe1e8;
  padding: 0 9px;
}

.ai-input-row {
  display: grid;
  grid-template-columns: 38px minmax(0, 1fr) 38px;
  gap: 8px;
  border-top: 1px solid #30313a;
  padding-top: 12px;
}

.ai-apply-button {
  background: #8358ed;
  border-color: #8358ed;
  color: #fff;
}

.ai-input-row button {
  height: 38px;
  border: 1px solid #3a3b45;
  border-radius: 50%;
  background: #1c1d23;
  color: #c7cad3;
}

.ai-input-row textarea {
  min-height: 88px;
  resize: vertical;
  border: 1px solid #3a3b45;
  border-radius: 5px;
  background: #15161b;
  color: #dfe1e8;
  padding: 10px;
}

.canvas-stage {
  position: relative;
  min-width: 0;
  background: var(--sf-stage-bg);
  transition: background 280ms ease;
}

.canvas-meta {
  position: absolute;
  z-index: 4;
  left: 18px;
  top: 14px;
  display: flex;
  align-items: center;
  gap: 9px;
  border: 1px solid rgba(255, 255, 255, 0.09);
  border-radius: 999px;
  padding: 6px 10px;
  background: rgba(20, 21, 26, 0.84);
  color: #bfc2cc;
  backdrop-filter: blur(10px);
}

.canvas-meta span,
.canvas-meta em {
  color: #8f929d;
  font-size: 12px;
  font-style: normal;
}

.canvas-meta strong {
  font-size: 12px;
}

.editor-canvas {
  height: 100%;
}

.editor-state {
  position: absolute;
  inset: 0;
  display: grid;
  place-items: center;
  z-index: 5;
  background: #202127;
  color: #dfe1e8;
}

.right-tabs {
  display: grid;
  grid-template-columns: 1fr 1fr;
  border-bottom: 1px solid #30313a;
}

.right-tabs button {
  min-height: 52px;
  border: 0;
  border-bottom: 3px solid transparent;
  background: transparent;
  color: #9da0aa;
  cursor: pointer;
  font-size: 16px;
  font-weight: 820;
}

.right-tabs button.active {
  border-bottom-color: #a888ff;
  color: #a888ff;
}

.selection-card {
  border-bottom: 1px solid #30313a;
  margin: -14px -14px 14px;
  padding: 14px;
}

.selection-card header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  margin-bottom: 12px;
}

.selection-card span {
  color: #9da0aa;
  font-size: 16px;
  font-weight: 780;
}

.selection-card strong {
  overflow: hidden;
  border: 1px solid #3a3b45;
  border-radius: 5px;
  padding: 5px 8px;
  color: #ffc2dd;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.selection-card p {
  color: #a7aab4;
}

.property-summary {
  display: grid;
  gap: 10px;
  border-top: 1px solid #30313a;
  padding-top: 14px;
}

.property-summary label {
  grid-template-columns: 1fr;
  padding: 0;
}

.code-overlay {
  position: fixed;
  inset: 0;
  z-index: 100;
  display: grid;
  place-items: center;
  padding: 22px;
  background: rgba(0, 0, 0, 0.64);
}

.code-dialog {
  width: min(1100px, 100%);
  max-height: 86vh;
  overflow: auto;
  border: 1px solid #3a3b45;
  border-radius: 8px;
  background: #17181d;
  color: #dfe1e8;
  box-shadow: 0 24px 70px rgba(0, 0, 0, 0.5);
}

.code-dialog header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #30313a;
  padding: 18px;
}

.code-dialog h2 {
  margin-top: 3px;
}

.code-dialog header button {
  width: 38px;
  height: 38px;
  border: 1px solid #3a3b45;
  border-radius: 6px;
  background: #1c1d23;
  color: #dfe1e8;
  cursor: pointer;
  font-size: 24px;
}

.code-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 14px;
  padding: 18px;
}

.code-grid label {
  display: grid;
  gap: 8px;
  color: #aeb1bb;
  font-weight: 780;
}

.code-grid textarea {
  min-height: 460px;
  resize: vertical;
  border: 1px solid #30313a;
  border-radius: 6px;
  background: #0f1014;
  color: #e4e6ed;
  padding: 12px;
  font-family: ui-monospace, SFMono-Regular, Menlo, Consolas, monospace;
  font-size: 12px;
}

.gjs-panel-host {
  min-height: 80px;
}

.gjs-panel-host.compact {
  min-height: 20px;
}

.theme-toggle {
  width: 36px;
  height: 36px;
  display: grid;
  place-items: center;
  border: 1px solid var(--sf-line);
  border-radius: 6px;
  background: var(--sf-surface);
  color: var(--sf-muted);
  cursor: pointer;
  font-size: 16px;
  transition: border-color 160ms ease, background 160ms ease, color 160ms ease;
}

.theme-toggle:hover {
  border-color: var(--sf-line-strong);
  color: var(--sf-ink);
}

@media (max-width: 1180px) {
  .studio-workbench {
    grid-template-columns: 44px var(--sf-left-panel-width, 360px) minmax(0, 1fr);
  }

  .pane-resizer,
  .studio-right-panel {
    display: none;
  }
}

@media (max-width: 820px) {
  .studio-topbar {
    grid-template-columns: 1fr;
  }

  .topbar-left,
  .topbar-center,
  .topbar-right {
    justify-content: flex-start;
    flex-wrap: wrap;
  }

  .studio-workbench {
    grid-template-columns: 44px minmax(0, 1fr);
  }

  .studio-left-panel {
    display: none;
  }

  .code-grid {
    grid-template-columns: 1fr;
  }
}
</style>

<style>
.gjs-one-bg {
  background-color: #17181d;
}

.gjs-two-color {
  color: #aeb1bb;
}

.gjs-three-bg {
  background-color: #8358ed;
}

.gjs-four-color,
.gjs-four-color-h:hover {
  color: #a888ff;
}

.gjs-block {
  width: calc(50% - 8px);
  min-height: 92px;
  border: 1px solid #3a3b45;
  border-radius: 5px;
  margin: 0 8px 10px 0;
  background: #17181d;
  box-shadow: none;
}

.gjs-block:hover {
  border-color: #8358ed;
}

.gjs-block__media {
  min-height: 38px;
  display: grid;
  place-items: center;
  margin-bottom: 8px;
  color: #b998ff;
}

.sf-block-icon {
  position: relative;
  width: 34px;
  height: 26px;
  display: block;
  color: currentColor;
}

.sf-block-icon::before,
.sf-block-icon::after {
  content: "";
  position: absolute;
  box-sizing: border-box;
}

.sf-block-icon.section::before,
.sf-block-icon.hero::before,
.sf-block-icon.footer::before,
.sf-block-icon.text::before,
.sf-block-icon.form::before {
  inset: 2px;
  border: 2px solid currentColor;
  border-radius: 4px;
}

.sf-block-icon.section::after {
  left: 8px;
  right: 8px;
  top: 8px;
  height: 2px;
  background: currentColor;
  box-shadow: 0 6px 0 currentColor;
}

.sf-block-icon.hero::after {
  left: 7px;
  right: 7px;
  bottom: 7px;
  height: 8px;
  border-radius: 999px;
  background: currentColor;
}

.sf-block-icon.footer::after {
  left: 6px;
  right: 6px;
  bottom: 6px;
  height: 3px;
  background: currentColor;
  box-shadow: 0 -6px 0 -1px currentColor;
}

.sf-block-icon.features {
  background:
    linear-gradient(currentColor 0 0) 3px 3px / 10px 8px no-repeat,
    linear-gradient(currentColor 0 0) 21px 3px / 10px 8px no-repeat,
    linear-gradient(currentColor 0 0) 3px 15px / 10px 8px no-repeat,
    linear-gradient(currentColor 0 0) 21px 15px / 10px 8px no-repeat;
}

.sf-block-icon.text::after {
  left: 8px;
  right: 8px;
  top: 8px;
  height: 2px;
  background: currentColor;
  box-shadow: 0 5px 0 currentColor, 0 10px 0 currentColor;
}

.sf-block-icon.button::before {
  left: 5px;
  right: 5px;
  top: 8px;
  height: 12px;
  border: 2px solid currentColor;
  border-radius: 999px;
}

.sf-block-icon.image::before {
  inset: 2px;
  border: 2px solid currentColor;
  border-radius: 4px;
}

.sf-block-icon.image::after {
  left: 8px;
  bottom: 6px;
  border-left: 8px solid transparent;
  border-right: 8px solid transparent;
  border-bottom: 10px solid currentColor;
}

.sf-block-icon.form::after {
  left: 8px;
  right: 8px;
  top: 8px;
  height: 2px;
  background: currentColor;
  box-shadow: 0 6px 0 currentColor;
}

.gjs-block-label {
  color: #bfc2cc;
  font-size: 13px;
}

.gjs-cv-canvas {
  width: calc(100% - 44px);
  height: calc(100% - 42px);
  top: 22px;
  left: 22px;
  border-radius: 4px;
  background: #2a2b31;
}

.gjs-frame-wrapper {
  border-radius: 4px;
}

.gjs-sm-sector,
.gjs-clm-tags {
  border-color: #30313a;
  background: transparent;
}

.gjs-sm-sector-title,
.gjs-clm-header,
.gjs-layer-title {
  background: #26272d;
  color: #aeb1bb;
}

.gjs-field,
.gjs-field input,
.gjs-field select {
  background: #15161b;
  color: #dfe1e8;
}
</style>
