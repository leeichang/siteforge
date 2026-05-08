<template>
  <div class="studio-editor" :class="`theme-${theme}`">
    <header class="studio-topbar">
      <div class="topbar-left">
        <button @click="backToWorkspace" class="chrome-button back-button" type="button">←</button>
        <button @click="showCode = true" class="chrome-button code-button" type="button">Code</button>
      </div>

      <div class="topbar-center">
        <div class="device-menu">
          <button :class="{ active: device === 'Desktop' }" @click="setDevice('Desktop')" type="button">Desktop</button>
          <button :class="{ active: device === 'Tablet' }" @click="setDevice('Tablet')" type="button">Tablet</button>
          <button :class="{ active: device === 'Mobile portrait' }" @click="setDevice('Mobile portrait')" type="button">Mobile</button>
        </div>
        <button class="tool-button" @click="togglePreview" type="button">{{ previewMode ? 'Edit' : 'Preview' }}</button>
        <button class="tool-button icon-only undo" @click="undo" type="button" aria-label="Undo"></button>
        <button class="tool-button icon-only redo" @click="redo" type="button" aria-label="Redo"></button>
      </div>

      <div class="topbar-right">
        <button class="theme-toggle" @click="themeStore.toggle()" type="button" :title="theme === 'dark' ? 'Switch to Light' : 'Switch to Dark'">
          {{ theme === 'dark' ? '☀️' : '🌙' }}
        </button>
        <button class="tool-button save-button" @click="savePage" :disabled="saving || !editorReady" type="button">
          {{ saving ? 'Saving' : hasUnsavedChanges ? 'Save' : 'Saved' }}
        </button>
        <button class="publish-button" @click="publishSite" :disabled="publishing" type="button">
          {{ publishing ? 'Publishing' : 'Publish' }}
        </button>
        <button class="upgrade-button" type="button">Upgrade</button>
      </div>
    </header>

    <main class="studio-workbench">
      <aside class="editor-rail">
        <button :class="{ active: activePanel === 'blocks' }" @click="activePanel = 'blocks'" type="button" aria-label="Blocks">
          <span class="rail-icon plus"></span>
        </button>
        <button :class="{ active: activePanel === 'project' }" @click="activePanel = 'project'" type="button" aria-label="Pages">
          <span class="rail-icon layers"></span>
        </button>
        <button :class="{ active: activePanel === 'global' }" @click="activePanel = 'global'" type="button" aria-label="Global styles">
          <span class="rail-icon palette"></span>
        </button>
        <button :class="{ active: activePanel === 'assets' }" @click="activePanel = 'assets'" type="button" aria-label="Assets">
          <span class="rail-icon image"></span>
        </button>
        <button :class="{ active: activePanel === 'data' }" @click="activePanel = 'data'" type="button" aria-label="Data sources">
          <span class="rail-icon database"></span>
        </button>
        <button :class="{ active: activePanel === 'ai' }" @click="activePanel = 'ai'" type="button" aria-label="AI assistant">
          <span class="rail-icon spark"></span>
        </button>
        <button class="rail-bottom" @click="backToWorkspace" type="button" aria-label="Workspace">
          <span class="rail-icon home"></span>
        </button>
      </aside>

      <aside class="studio-left-panel">
        <section v-show="activePanel === 'project'" class="studio-panel">
          <header class="panel-title-row">
            <h2>Pages</h2>
            <button @click="createPage" class="panel-icon-button" type="button" aria-label="Create page">+</button>
          </header>
          <div class="page-list studio-list">
            <button
              v-for="item in pages"
              :key="item.id"
              :class="{ active: item.id === pageId }"
              @click="openPage(item.id)"
              type="button"
            >
              <span>{{ item.title }}</span>
              <small>/{{ item.slug }}</small>
            </button>
          </div>

          <div class="panel-divider"></div>
          <header class="panel-title-row">
            <h2>Layers</h2>
          </header>
          <div id="layers-panel" class="gjs-panel-host layers-host"></div>
        </section>

        <section v-show="activePanel === 'blocks'" class="studio-panel">
          <header class="panel-title-row">
            <h2>Blocks</h2>
            <button class="panel-icon-button" type="button" @click="activePanel = 'project'" aria-label="Close panel">×</button>
          </header>
          <div class="segmented">
            <button class="active" type="button">Regular</button>
            <button type="button">Symbols</button>
          </div>
          <input v-model="blockSearch" class="panel-search" type="search" placeholder="Search..." />
          <div class="block-section-title">Basic</div>
          <div id="blocks-panel" class="gjs-panel-host blocks-host"></div>
          <button class="add-blocks-button" type="button">Add more blocks</button>
        </section>

        <section v-show="activePanel === 'assets'" class="studio-panel">
          <header class="panel-title-row">
            <h2>Assets</h2>
            <button class="panel-icon-button" type="button" @click="activePanel = 'project'" aria-label="Close panel">×</button>
          </header>
          <div class="asset-controls">
            <input v-model="assetSearch" class="panel-search" type="search" placeholder="Search..." />
            <button class="filter-chip" type="button">Project assets</button>
          </div>
          <button class="upload-button" @click="registerAssetUrl" type="button">Register URL</button>
          <input v-model="assetUrl" class="panel-search asset-url-input" type="url" placeholder="https://example.com/image.jpg" />
          <div class="asset-grid">
            <button
              v-for="asset in filteredAssets"
              :key="asset.publicUrl || asset"
              class="asset-tile"
              type="button"
              @click="selectAsset(asset.publicUrl || asset)"
            >
              <img :src="asset.publicUrl || asset" :alt="asset.altText || asset.fileName || 'Asset preview'" />
              <span>{{ asset.fileName || filenameFromUrl(asset.publicUrl || asset) }}</span>
            </button>
          </div>
          <button class="panel-action" @click="openAssetManager" type="button">Open asset manager</button>
        </section>

        <section v-show="activePanel === 'global'" class="studio-panel">
          <header class="panel-title-row">
            <h2>Global Styles</h2>
            <button class="panel-icon-button" type="button" @click="activePanel = 'project'" aria-label="Close panel">×</button>
          </header>
          <div class="style-group open">
            <button type="button">Colors <span>⌃</span></button>
            <label v-for="token in colorTokens" :key="token.name" class="token-row">
              <span>{{ token.name }}</span>
              <input v-model="token.value" type="color" @input="applyGlobalTokens" />
              <code>{{ token.value }}</code>
            </label>
          </div>
          <div class="style-group open">
            <button type="button">Body <span>⌃</span></button>
            <label class="field-row">
              <span>Font Family</span>
              <select v-model="fontFamily" @change="applyGlobalTokens">
                <option>Inter</option>
                <option>Arial</option>
                <option>Georgia</option>
                <option>system-ui</option>
              </select>
            </label>
            <label class="field-row">
              <span>Line Height</span>
              <input v-model="lineHeight" type="number" step="0.05" min="1" @input="applyGlobalTokens" />
            </label>
          </div>
        </section>

        <section v-show="activePanel === 'data'" class="studio-panel">
          <header class="panel-title-row">
            <h2>Data Sources</h2>
            <button class="panel-icon-button" type="button">+</button>
          </header>
          <div class="data-empty">
            <span class="data-icon"></span>
            <h3>Create a table to get started</h3>
            <button @click="createSampleTable" class="panel-action" type="button">Create sample table</button>
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
            <h2>AI Assistant</h2>
            <button class="panel-icon-button" type="button" @click="activePanel = 'project'" aria-label="Close panel">×</button>
          </header>
          <div class="ai-suggestions">
            <button v-for="suggestion in aiSuggestions" :key="suggestion" type="button" @click="aiPrompt = suggestion">
              {{ suggestion }}
            </button>
          </div>
          <div class="ai-generate-controls">
            <label>
              Page type
              <select v-model="aiPageType">
                <option value="home">Home</option>
                <option value="about">About</option>
                <option value="services">Services</option>
                <option value="product">Products</option>
                <option value="portfolio">Portfolio</option>
                <option value="blog">Blog</option>
                <option value="contact">Contact</option>
              </select>
            </label>
            <label>
              Style
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
            <textarea v-model="aiPrompt" placeholder="Ask anything..."></textarea>
            <button type="button" @click="generateCurrentPage" :disabled="generatingCurrentPage || !editorReady">↑</button>
          </div>
          <button class="panel-action ai-apply-button" type="button" @click="generateCurrentPage" :disabled="generatingCurrentPage || !editorReady">
            {{ generatingCurrentPage ? 'Generating current page...' : 'Generate current page' }}
          </button>
        </section>
      </aside>

      <section class="canvas-stage">
        <div class="canvas-meta">
          <span>{{ site?.name || 'SiteForge' }}</span>
          <strong>{{ page?.title || 'Page editor' }}</strong>
          <em v-if="hasUnsavedChanges">Unsaved changes</em>
        </div>
        <div v-if="loading || editorError" class="editor-state">
          <strong>{{ editorError || '載入編輯器中...' }}</strong>
        </div>
        <div id="gjs" class="editor-canvas"></div>
      </section>

      <aside class="studio-right-panel">
        <div class="right-tabs">
          <button :class="{ active: rightTab === 'styles' }" @click="rightTab = 'styles'" type="button">Styles</button>
          <button :class="{ active: rightTab === 'properties' }" @click="rightTab = 'properties'" type="button">Properties</button>
        </div>

        <section v-show="rightTab === 'styles'" class="right-content">
          <div class="selection-card">
            <header>
              <span>Selection</span>
              <strong>{{ selectedComponentName || 'None' }}</strong>
            </header>
            <p v-if="!selectedComponentName">Select an element from the canvas or pick a style from the Style Catalog.</p>
          </div>
          <div id="selectors-panel" class="gjs-panel-host compact"></div>
          <div id="styles-panel" class="gjs-panel-host"></div>
        </section>

        <section v-show="rightTab === 'properties'" class="right-content">
          <div class="selection-card">
            <header>
              <span>Page</span>
              <strong>{{ page?.slug ? `/${page.slug}` : 'Draft' }}</strong>
            </header>
            <p>{{ page?.metaDescription || '管理選取元件的屬性、連結、替代文字與表單欄位。' }}</p>
          </div>
          <div id="traits-panel" class="gjs-panel-host"></div>
          <div class="property-summary">
            <label>
              Page title
              <input :value="page?.title || ''" readonly />
            </label>
            <label>
              Slug
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
            <p class="sf-kicker">Export</p>
            <h2>Current page code</h2>
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
import api, { errorMessage, unwrap } from '../api/client'

const route = useRoute()
const router = useRouter()
const themeStore = useThemeStore()
const theme = computed(() => themeStore.theme)
const siteId = route.params.siteId
const pageId = ref(route.params.pageId || '')

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
const rightTab = ref('styles')
const device = ref('Desktop')
const previewMode = ref(false)
const showCode = ref(false)
const aiPrompt = ref('')
const aiPageType = ref('home')
const aiStyle = ref('studio')
const generatingCurrentPage = ref(false)
const blockSearch = ref('')
const assetSearch = ref('')
const assetUrl = ref('')
const selectedComponentName = ref('')
const fontFamily = ref('Inter')
const lineHeight = ref(1.55)
const sampleTable = ref([])
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
const aiSuggestions = [
  'Create a hero section with a headline and call-to-action',
  'Add a features grid with icons',
  'Create a contact form',
  'Add testimonials section'
]

const currentHtml = computed(() => editor ? editor.getHtml() : '')
const currentCss = computed(() => editor ? editor.getCss() : '')
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

onMounted(async () => {
  await loadEditorData()
  await nextTick()
  initGrapesJS()
})

onUnmounted(() => {
  if (editor) {
    editor.destroy()
    editor = null
  }
})

async function loadEditorData() {
  loading.value = true
  editorError.value = ''
  try {
    const [siteResponse, pagesResponse, templatesResponse, assetsResponse] = await Promise.all([
      api.get(`/Sites/${siteId}`),
      api.get(`/Pages/site/${siteId}`),
      api.get('/WidgetTemplates'),
      api.get(`/Assets/site/${siteId}`).catch(() => ({ data: { data: [] } }))
    ])
    site.value = unwrap(siteResponse)
    pages.value = (unwrap(pagesResponse) || []).sort((a, b) => a.displayOrder - b.displayOrder)
    templates.value = unwrap(templatesResponse) || []
    assets.value = unwrap(assetsResponse) || []

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
    editorError.value = errorMessage(e, '載入編輯器失敗')
  } finally {
    loading.value = false
  }
}

function initGrapesJS() {
  if (editorError.value) return
  if (typeof window === 'undefined' || !window.grapesjs) {
    editorError.value = 'GrapesJS 尚未載入，請確認 CDN 可連線。'
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
        content: template.defaultContent,
        attributes: { title: template.description || template.name }
      })
    })

  const basicBlocks = [
    {
      id: 'basic-section',
      label: 'Section',
      content: '<section class="py-16 px-6"><div class="max-w-6xl mx-auto"><h2 class="text-3xl font-bold text-gray-900">New section</h2><p class="mt-3 text-gray-600">Write section content here.</p></div></section>'
    },
    {
      id: 'basic-text',
      label: 'Text',
      content: '<p class="text-lg leading-7 text-gray-700">輸入段落內容...</p>'
    },
    {
      id: 'basic-button',
      label: 'Button',
      content: '<a href="#" class="inline-block px-5 py-3 bg-blue-600 text-white font-semibold rounded-md">行動按鈕</a>'
    },
    {
      id: 'basic-image',
      label: 'Image',
      content: '<img src="https://images.unsplash.com/photo-1522071820081-009f0129c71c?w=1200" alt="Team" class="w-full rounded-lg" />'
    },
    {
      id: 'basic-form',
      label: 'Form',
      content: '<form class="grid gap-3 max-w-md"><input class="border rounded-md px-4 py-3" placeholder="Name" /><input class="border rounded-md px-4 py-3" placeholder="Email" /><button class="bg-gray-900 text-white rounded-md px-4 py-3">Submit</button></form>'
    }
  ]
  basicBlocks.forEach((block) => editor.BlockManager.add(block.id, { ...block, category: 'Basic' }))
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

  hasUnsavedChanges.value = false
}

async function savePage() {
  if (!editor || !pageId.value) return
  saving.value = true
  try {
    const response = await api.put(`/Pages/${pageId.value}`, {
      htmlContent: editor.getHtml(),
      cssContent: editor.getCss(),
      jsContent: editor.getJs?.() || '',
      components: JSON.stringify(editor.getComponents().toJSON()),
      styles: JSON.stringify(editor.getStyle())
    })
    page.value = { ...page.value, ...unwrap(response) }
    hasUnsavedChanges.value = false
  } catch (e) {
    alert(errorMessage(e, '儲存失敗'))
  } finally {
    saving.value = false
  }
}

async function publishSite() {
  publishing.value = true
  try {
    if (hasUnsavedChanges.value) await savePage()
    await api.post(`/Sites/${siteId}/publish`, { taskType: 'full_publish', targetUrl: '' })
    alert('發佈完成。')
  } catch (e) {
    alert(errorMessage(e, '發佈失敗'))
  } finally {
    publishing.value = false
  }
}

async function createPage() {
  const title = prompt('新頁面名稱', 'New Page')
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
    alert(errorMessage(e, '建立頁面失敗'))
  }
}

async function openPage(targetPageId) {
  if (targetPageId === pageId.value) return
  if (hasUnsavedChanges.value && !confirm('目前頁面尚未儲存，要離開嗎？')) return
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
    alert(errorMessage(e, '資源登錄失敗'))
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
    hasUnsavedChanges.value = false
    selectedComponentName.value = ''
    alert('AI 已產生並套用到目前頁面。')
  } catch (e) {
    alert(errorMessage(e, 'AI 生成目前頁面失敗'))
  } finally {
    generatingCurrentPage.value = false
  }
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
    { key: 'title', value: page.value?.title || 'Home' },
    { key: 'slug', value: page.value?.slug || 'home' },
    { key: 'status', value: page.value?.isPublished ? 'published' : 'draft' }
  ]
}

function backToWorkspace() {
  if (hasUnsavedChanges.value && !confirm('目前頁面尚未儲存，要返回工作區嗎？')) return
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
          <p class="text-sm font-semibold text-blue-600 mb-3">SiteForge Template First</p>
          <h1 class="text-5xl font-bold text-gray-900 mb-6">拖曳左側模板開始編輯</h1>
          <p class="text-xl text-gray-600 max-w-2xl">從 Hero、Features、Contact 等區塊開始，建立後可完整儲存為 GrapesJS project data。</p>
        </div>
      </section>
    </main>
  `
}

function defaultStyleSectors() {
  return [
    {
      name: 'Layout',
      open: true,
      buildProps: ['display', 'position', 'width', 'min-height', 'margin', 'padding']
    },
    {
      name: 'Size',
      open: true,
      buildProps: ['width', 'height', 'min-width', 'min-height', 'max-width', 'max-height']
    },
    {
      name: 'Typography',
      open: false,
      buildProps: ['font-family', 'font-size', 'font-weight', 'line-height', 'color', 'text-align', 'text-decoration']
    },
    {
      name: 'Background',
      open: false,
      buildProps: ['background-color', 'background-image', 'background-size', 'background-position']
    },
    {
      name: 'Borders',
      open: false,
      buildProps: ['border-radius', 'border-width', 'border-style', 'border-color']
    },
    {
      name: 'Effects',
      open: false,
      buildProps: ['opacity', 'box-shadow', 'transform']
    },
    {
      name: 'Flex',
      open: false,
      buildProps: ['flex-direction', 'justify-content', 'align-items', 'gap']
    }
  ]
}
</script>

<style scoped>
.studio-editor {
  height: 100vh;
  display: flex;
  flex-direction: column;
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
  flex: 1;
  display: grid;
  grid-template-columns: 64px 390px minmax(0, 1fr) 350px;
  min-height: 0;
}

.editor-rail {
  display: flex;
  flex-direction: column;
  gap: 9px;
  border-right: 1px solid var(--sf-line);
  background: var(--sf-sidebar-bg);
  padding: 9px 7px;
  transition: border-color 280ms ease, background 280ms ease;
}

.editor-rail button {
  width: 48px;
  height: 48px;
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

.studio-right-panel {
  border-left: 1px solid var(--sf-line);
  transition: border-color 280ms ease;
}

.studio-panel,
.right-content {
  padding: 14px;
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
    grid-template-columns: 64px 330px minmax(0, 1fr);
  }

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
    grid-template-columns: 58px minmax(0, 1fr);
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
