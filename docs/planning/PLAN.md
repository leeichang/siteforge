# SiteForge — 網站建構器專案規劃

> 類比 Wix / GoDaddy / Bricks Builder 的 AI 網站建構器
> **Template First, AI Assist** — 從模板快速啟動，AI 幫你微調細節
> 技術棧：Vue 3 + .NET 8 + SqlSugar + PostgreSQL + AI

---

## 一、專案願景

讓使用者從專業模板開始，搭配 AI 微調與視覺編輯器，快速建立專業網站。不需要從零設計。

### 核心理念：Template First, AI Assist
1. **選擇模板** → 從內建產業模板開始（防偽驗證 / 掃碼集點 / 產品介紹 / 生產履歷 / DPP）
2. **AI 微調** → 用自然語言描述小修改（「圖片縮到 1/3」、「文字間距加大」）
3. **視覺編輯** → GrapesJS 精細調整（拖拽、改樣式、換圖片）

### 核心價值
- **快速啟動**：專業模板讓你 5 分鐘有雛形
- **AI 輔助**：自然語言微調，不用手動調 CSS
- **所見即所得**：即時預覽，編輯與成果一致
- **一鍵部署**：從編輯到上線一條龍

---

## 二、技術架構

```
┌──────────────────────────────────────────────────────┐
│                   Frontend (Vue 3)                    │
│  Vue Router / Pinia / Element Plus                   │
│  GrapesJS (拖拽視覺編輯器) / Axios                   │
│  iFrame Sandbox (即時預覽) + Tailwind CSS            │
└─────────────────────┬────────────────────────────────┘
                      │ REST API + SignalR (即時推送)
┌─────────────────────▼────────────────────────────────┐
│              Backend Gateway (.NET 8)                 │
│  ┌─────────────┐ ┌──────────┐ ┌────────────────────┐ │
│  │ Core Service│ │ AI Assist│ │ Deploy Service     │ │
│  │ Page CRUD   │ │ Prompt  │ │ Static Render      │ │
│  │ Section Mgr │ │ Refine  │ │ DNS + SSL          │ │
│  │ Theme Mgr   │ │ Adjust  │ │ CDN Push           │ │
│  │ Widget Temp │ │ Layout  │ │                    │ │
│  └──────┬──────┘ └────┬─────┘ └────────────────────┘ │
│         └─────────────┴─────────────────────────┐    │
│  SqlSugar ORM + PostgreSQL                     │    │
└──────────────────────────────────────────────────────┘
```

### 前端技術棧

| 技術 | 用途 |
|------|------|
| Vue 3 + Composition API + TypeScript | 框架 |
| Vue Router | 路由 (hash mode for editor) |
| Pinia | 狀態管理 (PageStore, SiteStore, AIStore) |
| **GrapesJS** | 拖拽視覺編輯器核心 (取代自製 Canvas) |
| Element Plus | UI 元件庫 (Button, Dialog, Tabs, Form) |
| Tailwind CSS | Canvas iframe 內樣式 (注入到編輯器) |
| Axios | HTTP Client |
| Vite | 建置工具 |

### 後端技術棧

| 技術 | 用途 |
|------|------|
| .NET 8 | Web API (Controller-based) |
| SqlSugar | ORM（取代 EF Core，更輕量高效） |
| PostgreSQL 15+ | 資料庫 (varchar/jsonb/text) |
| **Azure OpenAI SDK** | LLM 整合 (Chat Completion) |
| SignalR | 即時通訊 (AI 生成進度推送) |
| Swagger / Scalar | API 文件 |

### 專案三層架構 (參考 GrapesJS)

```
SiteForge.sln
├── SiteForge.Api/           # Web API (Controllers, Program.cs)
├── SiteForge.Core/          # 核心業務層
│   ├── Entities/            # 資料實體 (SqlSugar POCO)
│   ├── DTOs/                # 請求/回應 DTOs
│   ├── Interfaces/          # 服務與 Repository 介面
│   ├── Services/            # 業務邏輯實作
│   ├── Enums/               # 列舉
│   └── Utilities/           # 工具類 (JWT, Password)
├── SiteForge.Infrastructure/ # 基礎設施層
│   ├── Data/                # SqlSugar DbContext
│   ├── Repositories/        # Repository 實作
│   └── Configurations/      # 配置類
└── SiteForge.Tests/         # 單元/整合測試
```

---

## 三、UI 布局

參考 GrapesJS 預設 UI + Bricks Builder 的三欄式設計：

```
┌──────────────────────────────────────────────────────────────┐
│  Header: [📄 Page Name]  [Desktop▼] [Undo][Redo] [⚙️] [Publish]│
├──────┬──────────────────────────────┬────────────────────────┤
│ ICON │      CANVAS (GrapesJS)       │    PROPERTIES PANEL    │
│ BAR  │                              │                       │
│ 🧩   │  ┌────────────────────────┐  │  ┌──────────────────┐ │
│ 🗂️   │  │  GrapesJS Editor Canvas│  │  │  Style Manager   │ │
│ ⚡   │  │  iFrame Sandbox        │  │  │  (GrapesJS 原生)  │ │
│ 🤖   │  │  Tailwind CSS 注入     │  │  │  • Background    │ │
│ 🎨   │  │  拖拽編輯區塊          │  │  │  • Typography    │ │
│ 📱   │  │  響應式預覽            │  │  │  • Margin/Padding│ │
│      │  │  (Desk/Tablet/Mobile)  │  │  │  • Border/Shadow │ │
│      │  │                        │  │  │                  │ │
│      │  │                        │  │  │  ┌──────────────┐│ │
│      │  │                        │  │  │  │ Trait Manager ││ │
│      │  │                        │  │  │  │ (屬性編輯)    ││ │
│      │  │                        │  │  │  └──────────────┘│ │
│      │  └────────────────────────┘  │  │  ┌──────────────┐│ │
│      │                              │  │  │  Layers Panel ││ │
│      │                              │  │  │  (DOM Tree)   ││ │
│      │                              │  │  └──────────────┘│ │
│      │                              │  │  ┌──────────────┐│ │
│      │                              │  │  │  Blocks Panel ││ │
│      │                              │  │  │  (區塊拖拽)   ││ │
│      │                              │  │  └──────────────┘│ │
│      │                              │  │                   │ │
└──────┴──────────────────────────────┴────────────────────────┘
```

### 左側 Icon Bar (重新設計 — Template First)

| 順序 | Icon | 功能 | 對應 GrapesJS Panel / 自訂 |
|:---:|------|------|---------------------------|
| 1 | 📁 Project | **專案管理** | 自訂 — 頁面列表、專案設定、域名配置 |
| 2 | 🗂️ Layers | 層級結構 | `show-layers` — DOM tree 視圖，可選中/重排/刪除區塊 |
| 3 | 🧩 Blocks | **區塊模板** | `show-blocks` — 內建模板區塊 (Hero, Features, Team, FAQ, Contact, Footer...) |
| 4 | 🎨 Global CSS | 全域樣式 | 自訂 — 主題色、字型、間距、圓角設定 |
| 5 | 🖼️ Assets | 資源庫 | `show-assets` — 圖片、影片、SVG 資源管理 |
| 6 | ⚡ Element CSS | 元素樣式 | `show-styles` — 選中元素的 Style Manager |
| 7 | 🤖 AI Chatbot | AI 助手 | 自訂 — 對話式微調面板 |

### Top Bar — Device Selector
放在 Header 右側：
`[Desktop] [Tablet] [Mobile]` — 響應式預覽切換

> **設計原則**：第一個面板永遠是 **Project**，讓使用者先管理頁面結構，再進入編輯。

---

## 四、核心功能模組

### 4.1 GrapesJS 視覺編輯器

我們直接使用 GrapesJS 作為編輯器核心，而不是從零打造 Canvas。

**GrapesJS 方案 vs 自製方案：**

| 功能 | GrapesJS | 自製 |
|------|----------|------|
| 拖拽編輯 | ✅ 內建 Drag & Drop、選擇、移動、縮放 | ❌ 需從頭實作 |
| 區塊系統 | ✅ Block Manager (自訂區塊) | ❌ 需自製 |
| Style Manager | ✅ CSS 屬性面板 (長寬、邊距、字型、背景) | ❌ 全部自製 |
| Layers Panel | ✅ DOM Tree 視圖 | ❌ 需自製 |
| Code Editor | ✅ 內建 HTML 代碼編輯器 | ❌ 需自製 |
| Undo/Redo | ✅ Undo Manager | ❌ 需自製 |
| Device 切換 | ✅ Desktop/Tablet/Mobile 預覽 | ❌ 需自製 |
| Asset Manager | ✅ 圖片/資源管理 | ❌ 需自製 |
| 區塊客製化 | ✅ 自訂 Component Type + Trait | ❌ 需自製 |
| 自訂命令 | ✅ Commands 擴展 | ❌ 需自製 |
| Export | ✅ 匯出 HTML/CSS | ❌ 需自製 |

**GrapesJS 配置重點：**
- 自訂區塊類型 (Component Type)：Hero, Features, About, Team, FAQ, Contact, Footer, Pricing, Stats, Gallery
- 為每個區塊類型定義可編輯屬性 (Traits)：標題、副標題、CTA 文字/連結、背景類型
- 注入 Tailwind CSS 到 Canvas iframe (參考 PageDesigner.vue 的做法)
- 自訂 Link 組件 Trait：支援 URL / 內部頁面 / 錨點 / Email / 電話
- 自訂按鈕組 Trait (button-group) 用於樣式切換
- Tailwind Class 修復器：處理轉義 class (lg\/w-1\/3 → lg:w-1/3)
- Canvas 響應式寬度診斷與診斷

### 4.2 區塊模板系統 (參考 ZKEACMS Widget)

借鑒 ZKEACMS 的元件化架構：

```csharp
// Widget 模板層 - 定義區塊的骨架和可編輯區域
public class WidgetTemplate
{
    public Guid Id { get; set; }
    public string Name { get; set; }          // "Hero", "Features"
    public string Category { get; set; }      // "Layout", "Content", "Media"
    public string ThumbnailUrl { get; set; }  // 縮圖預覽
    public string Description { get; set; }
    public string DefaultContent { get; set; } // 預設 HTML 內容 (GrapesJS component)
    public string EditorView { get; set; }    // 編輯器檢視路徑
    public bool IsSystem { get; set; }        // 是否為系統內建
    public bool IsPublish { get; set; }       // 是否啟用
}

// Widget 實例 - 頁面上的具體區塊
public class WidgetBase
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public Guid TemplateId { get; set; }      // 關聯 WidgetTemplate
    public string Title { get; set; }
    public string Content { get; set; }       // JSON: 區塊內容 (標題, 圖片, 按鈕等)
    public string Style { get; set; }         // JSON: 區塊樣式覆蓋
    public int Order { get; set; }
    public string ZoneName { get; set; }      // 區域名稱: "main", "sidebar", "footer"
    public string? CustomCssClass { get; set; }
    public string? CustomId { get; set; }
}
```

> **注意**: SiteForge 使用 GrapesJS 管理 Canvas 內的區塊內容，但 WidgetTemplate 作為「區塊庫存檔」存放在後端，供使用者新建頁面時快速選用。

### 4.3 AI 生成引擎 (2-Phase 流程，參考 GrapesJS)

借鑒 GrapesJS 的 `AiPageGeneratorService` 設計的兩階段生成模式：

```
Phase 1: 大綱生成 (Outline Generation)
─────────────────────────────────────────
使用者輸入 → LLM → 段落大綱 (Section List)
  例如: "做一個咖啡店 Landing Page"
        → [{type:"hero", title:...}, 
           {type:"about", title:...},
           {type:"menu", title:...},
           {type:"contact", title:...}]

Phase 2: 分段生成 (Chunked Generation)
─────────────────────────────────────────
每個大綱段落 → LLM → HTML + CSS (GrapesJS 相容)
  段落 1: Hero → <section class="hero">...</section>
  段落 2: About → <section class="about">...</section>
  ...

Phase 3 (可選): 圖片搜尋
─────────────────────────────────────────
根據關鍵詞生成 → Unsplash/Pexels API → 插入對應位置
```

**System Prompt 策略 (分層)：**

1. **Outline Prompt**：只生成頁面結構大綱（JSON），不含內容
2. **Section Generation Prompt**：根據大綱生成完整 HTML+CSS
3. **Refresh Prompt**：針對已有區塊進行改寫/編輯
4. **Theme Prompt**：全域主題樣式修改

**串聯方式 (參考 GrapesJS AiPageGeneratorService 實作)：**
```
GeneratePageFromPromptAsync(request)
  ├── 1. 圖片搜尋 (Async) ← Unsplash / Pexels
  ├── 2. 大綱生成 (LLM)
  ├── 3. 分段生成 (LLM Streaming)
  │     ├── 段落 1 → 區塊 1 HTML + CSS
  │     ├── 段落 2 → 區塊 2 HTML + CSS
  │     └── ...
  └── 4. 組裝保存 (PageService)
```

### 4.4 AI Assistant 面板

```
┌──────────────────────────────────────┐
│  🤖 AI Assistant           [×]      │
├──────────────────────────────────────┤
│                                      │
│  💬 對話記錄                          │
│  ──────────────────────────          │
│  使用者：幫我做一個咖啡店的 landing   │
│  🤖：好的，正在生成大綱...            │
│  ── 大綱預覽 ──                      │
│  ✅ Hero - 咖啡品牌形象              │
│  ✅ About - 品牌故事                 │
│  ✅ Menu - 暢銷飲品                  │
│  ✅ Contact - 門市資訊               │
│  ✅ Footer - 頁尾資訊                │
│       [生成完整頁面] [修改大綱]       │
│  ──────────────────────────          │
│  使用者：把主色改成米色 #F5E6D0      │
│  🤖：已套用主題色                    │
│  ──────────────────────────          │
│  使用者：在 Hero 後面插入一個         │
│          特色區塊，放三個咖啡杯圖     │
│  🤖：已生成 Features 區塊            │
│       [插入] [預覽] [重試] [修改]    │
│                                      │
│  ┌──────────────────────────────┐   │
│  │ 輸入描述，Enter 發送...       │   │
│  └──────────────────────────────┘   │
│                                      │
│ 快捷按鈕 (Quick Actions)：           │
│  [生成圖片] [改寫文案] [翻譯] [生成區塊]│
└──────────────────────────────────────┘
```

**AI Actions 清單 (同 GrapesJS Controller 定義)：**

| Action | 端點 | 說明 |
|--------|------|------|
| `generate_section` | `POST /api/ai/generate-section` | 根據描述生成單一區塊 |
| `generate_page` | `POST /api/ai/generate-page` | 生成完整頁面 (2-Phase) |
| `generate_component` | `POST /api/ai/generate-component` | 生成 HTML/CSS 組件 (給編輯器) |
| `rewrite_content` | `POST /api/ai/rewrite` | 改寫選中區塊文字內容 |
| `translate` | `POST /api/ai/translate` | 翻譯內容 |
| `generate_image` | `POST /api/ai/generate-image` | 根據描述生成圖片 (DALL-E) |
| `change_style` | `POST /api/ai/change-style` | 修改選中區塊樣式 |
| `change_theme` | `POST /api/ai/change-theme` | 全域主題修改 |

### 4.5 Layout 頁面佈局區域 (參考 ZKEACMS LayoutZone)

借鑒 ZKEACMS 的 Layout 系統，每個頁面可有多個佈局區域：

```csharp
// Layout 佈局模板
public class Layout
{
    public Guid Id { get; set; }
    public string Name { get; set; }         // "Full Width", "With Sidebar"
    public string Thumbnail { get; set; }    // 佈局預覽圖
    public List<LayoutZone> Zones { get; set; }
}

// 佈局區域
public class LayoutZone
{
    public Guid Id { get; set; }
    public Guid LayoutId { get; set; }
    public string Name { get; set; }         // "main", "sidebar", "header", "footer"
    public int Width { get; set; }           // 寬度比例 (12-column grid)
    public int Order { get; set; }
    public string CssClass { get; set; }     // 容器 CSS class
}
```

> 初版可以先簡化，只支援 Full Width 佈局，後續再擴展 Sidebar 等複雜佈局。

### 4.6 Theme 系統 (參考 ZKEACMS & GrapesJS)

```csharp
public class Theme
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string Name { get; set; }
    
    // JSON 配置
    public string Colors { get; set; }       // JSON: { primary, secondary, accent, background, text }
    public string Fonts { get; set; }        // JSON: { heading: "Inter", body: "Inter" }
    public string Spacing { get; set; }      // JSON: { unit: "px", baseSize: 16 }
    public string BorderRadius { get; set; } // JSON: { sm, md, lg, full }
    public string Shadows { get; set; }      // JSON: { sm, md, lg }
    
    public string? CustomCss { get; set; }   // 自訂全局 CSS
    public string? FontImportUrl { get; set; } // Google Fonts 匯入 URL
    public bool IsActive { get; set; }
}
```

### 4.7 Page Management (參考 GrapesJS)

```csharp
[SugarTable("pages")]
public class Page
{
    [SugarColumn(IsPrimaryKey = true)]
    public Guid Id { get; set; }
    
    public Guid SiteId { get; set; }
    public Guid? ParentId { get; set; }      // 父子頁面 (階層式導航)
    public Guid? LayoutId { get; set; }      // 頁面佈局
    
    public string Title { get; set; }
    public string Slug { get; set; }
    public string PageType { get; set; }     // "home", "about", "product", "blog", "contact"
    
    // GrapesJS 內容儲存
    public string? HtmlContent { get; set; } // 渲染後的 HTML
    public string? CssContent { get; set; }  // 編譯後的 CSS
    public string? JsContent { get; set; }   // 頁面 JS
    public string? Components { get; set; }  // JSONB: GrapesJS 組件結構 (序列化)
    public string? Styles { get; set; }      // JSONB: GrapesJS 樣式
    
    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public string? OgImageUrl { get; set; }
    
    // Status
    public bool IsPublished { get; set; }
    public int DisplayOrder { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

### 4.8 AI 對話管理 (參考 GrapesJS AiConversationEntity)

追蹤使用者與 AI 的對話歷史，支援上下文關聯：

```csharp
public class AiConversation
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public Guid? PageId { get; set; }      // 關聯到頁面
    public string Title { get; set; }      // 對話標題 (自動摘要)
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AiMessage
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Role { get; set; }       // "user", "assistant", "system"
    public string Content { get; set; }
    public string? ActionType { get; set; } // "generate_section", "change_style"
    public string? ActionResult { get; set; } // JSON: action 結果
    public string? Metadata { get; set; }  // JSON: 擴展資訊 (token用量, 生成時間)
    public DateTime CreatedAt { get; set; }
}
```

### 4.9 部署與 Publish (參考 ZKEACMS PublishSystem)

```csharp
public class PublishTask
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public PublishStatus Status { get; set; } // "pending", "publishing", "done", "failed"
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public enum PublishStatus
{
    Pending,
    Publishing,
    Done,
    Failed
}
```

### 4.10 完整資料模型 ERD

```
Site (1) ──┬── (N) Page
           ├── (1) Theme
           └── (N) SiteDomain

Page (1) ──┬── (N) WidgetBase (區塊實例)
           ├── (N) AiConversation
           ├── (0..1) Layout
           └── (0..1) Page (父頁面)

WidgetTemplate (1) ──┬── (N) WidgetBase
                     └── (N) Category

Layout (1) ──┬── (N) LayoutZone
             └── (N) Page

Theme (1) ──┬── (N) Site
```

---

## 五、AI Prompt 架構

### 5.1 大綱生成 Prompt

```
System: 你是一個網站頁面結構規劃助手。
根據使用者的描述，輸出一個頁面區塊大綱 JSON。
每個區塊包含 type、標題和簡短描述。
只回傳 JSON array，不要額外文字。

[{ "type": "hero", "title": "...", "description": "..." }, ...]

支援的區塊類型：
- hero: 首頁英雄區塊 (大標題+副標題+CTA)
- features: 特色功能展示 (2-4 欄)
- about: 關於我們 (左文右圖)
- services: 服務項目列表
- team: 團隊介紹
- blog: 部落格文章列表
- faq: 常見問題 (折疊式)
- contact: 聯絡表單
- pricing: 價格方案
- stats: 統計數字
- gallery: 圖片展示
- testimonials: 客戶推薦
- cta: 行動呼籲區塊
- footer: 頁尾
```

### 5.2 區塊生成 Prompt

```
System: 你是一個網頁區塊 HTML 生成助手。
根據使用者提供的區塊大綱，輸出符合以下條件的 HTML：

1. 使用 Tailwind CSS 類名 (CDN: https://cdn.tailwindcss.com)
2. 包含完整響應式設計 (lg:, md:, sm:)
3. 支援該區塊類型的預設內容填充
4. 只回傳純 HTML，不要包裹 ```html 或說明文字
5. 使用 <section> 包裹，並加上 id 屬性
6. 不使用外部 JavaScript，但可使用 Tailwind 提供的功能
7. 圖片使用 Unsplash placeholder: https://images.unsplash.com/photo-{ID}?w=800
```

### 5.3 完整頁面生成流程 (2-Phase)

```
[使用者輸入] → Phase 1: 大綱生成
               → [大綱 JSON] → 前端顯示預覽
               → 使用者確認後 → Phase 2: 分段生成
               → 每段 LLM Streaming 回傳 HTML
               → 前端即時渲染
               → 組裝保存到 Page.HtmlContent
```

---

## 六、分段生成策略 Chunked Generation

參考 GrapesJS `GeneratePageWithChunksAsync` 的設計：

```csharp
// 流程
1. OutlineGeneration: 使用者描述 → LLM → 區塊大綱列表
2. ImageSearch: 並行搜尋 Unsplash 圖片
3. ChunkGeneration: 每個大綱項目 → LLM → HTML片段
   - 支援並行生成 (多段落同時)
   - 支援 Streaming (即時推送給前端)
   - 內建品質驗證 (HTML 語法檢查 + 必要元素檢查)
   - 失敗重試機制 (最多 3 次)
4. 組裝: 片段 → 完整 HTML + CSS → 保存到 Page

// 優點
- 不會因 token 上限截斷內容
- 每個區塊可以獨立驗證和重試
- 支援 Streaming 即時展示
- 大綱可讓使用者預覽和調整後再生成細節
```

---

## 七、開發階段

### Phase 1 — 核心框架（2-3 週）

**目標**：可運作的骨架，能 CRUD 網站與頁面

- [ ] .NET 8 API 三層專案建立 (Api + Core + Infrastructure)
- [ ] SqlSugar 配置 + PostgreSQL 資料庫
- [ ] 基礎資料模型 + migrations (Site, Page, WidgetTemplate, Theme)
- [ ] API endpoints (Sites CRUD, Pages CRUD, Themes CRUD)
- [ ] Vue 3 + Vite 專案建立 + 路由 + Pinia store
- [ ] Dashboard 頁面 (網站卡片列表)
- [ ] **復用** GrapesJS 的 Auth / JWT 認證系統 (Login, Register)
- [ ] **復用** GrapesJS 的 BaseRepository / BaseEntity 模式

### Phase 2 — GrapesJS 編輯器整合（3-4 週）

**目標**：完整 GrapesJS 編輯器，可拖拽區塊與預覽

- [ ] GrapesJS npm package 安裝 + Vue 組件封裝
- [ ] 編輯器三欄 Layout (Header + Canvas + Properties)
- [ ] GrapesJS Block Manager 自訂區塊 (Hero, Features, About, Team, Contact, Footer 等 8-10 種)
- [ ] **復用** GrapesJS 區塊定義 (Block Definition + Component Type + Trait)
- [ ] 自訂 Component Type 與 Traits (可編輯屬性: 文字、圖片、CTA)
- [ ] Canvas iFrame Sandbox 配置
- [ ] **復用** Tailwind CSS 注入機制 (injectTailwindCSS)
- [ ] **復用** Tailwind Class 修復器 (fixTailwindClasses)
- [ ] **復用** Link 組件客製化 (href-type: URL/Page/Anchor/Email/Phone)
- [ ] **復用** button-group 自訂 Trait 類型
- [ ] Style Manager 配置
- [ ] Device 切換 (Desktop/Tablet/Mobile)
- [ ] **復用** 區塊響應式診斷器
- [ ] Layers Panel 配置
- [ ] Undo/Redo 機制
- [ ] **復用** Canvas 初始化內容載入邏輯 (優先 htmlContent, 後備 components JSON)

### Phase 3 — AI 整合（2-3 週）

**目標**：AI 助手可對話生成區塊與頁面

- [ ] **復用** GrapesJS 後端 AI Service 分層架構
  - AiPageGeneratorService (2-Phase generation)
  - AiContentGenerationService (改寫/翻譯/生成區塊)
  - AiConversationService (對話管理)
- [ ] **復用** Azure OpenAI 配置與 Chat Client 封裝
- [ ] **復用** 分段生成策略 (ChunkedGeneration)
- [ ] **復用** 圖片搜尋服務 (Unsplash/Pexels)
- [ ] **復用** AI 品質驗證與智能重試
- [ ] NL → Section Prompt 工程 (區塊生成)
- [ ] NL → Page 2-Phase 完整頁面生成
- [ ] AI Assistant 面板 UI (參考 GrapesJS AiAssistantPanel.vue)
- [ ] **復用** Streaming 回應處理 (前端 SSE / 後端流式)
- [ ] **復用** Component Preview + Insert 工作流
- [ ] AI 對話記錄 (AiConversation + AiMessage entities)
- [ ] SignalR 即時推送生成進度
- [ ] Quick Actions: [生成圖片] [改寫文案] [翻譯]

### Phase 4 — 部署與上線（2 週）

**目標**：可將網站部署到公開網域

- [ ] 靜態網站生成引擎 (Server-side Render: 合併 HTML + CSS + JS)
- [ ] ZKEACMS 風格的 Publish Task 佇列
- [ ] 自訂網域綁定 + DNS 管理
- [ ] Let's Encrypt SSL 自動申請
- [ ] 靜態檔案上傳 (S3 / CDN)
- [ ] 增量部署 (只更新修改過的頁面)
- [ ] 基本 Analytics (頁面瀏覽數)

### Phase 5 — 強化（持續）

- [ ] Theme Marketplace (WidgetTemplate 生態)
- [ ] 使用者註冊 / 團隊協作
- [ ] 版本控制 / 網站快照
- [ ] E-commerce 擴展 (產品頁 + 購物車)
- [ ] Blog 引擎 (文章管理 + RSS)
- [ ] SEO 工具強化 (Sitemap, Robots.txt, Structured Data)
- [ ] Performance 優化 (SSR, Lazy Load, Image Optimization)
- [ ] 多語言支援

---

## 八、程式碼復用策略

### 直接復用 (GrapesJS)

| 模組 | 檔案 | 復用方式 |
|------|------|---------|
| 後端架構 | Core/Entities, Interfaces, Services | 複製 + 調整命名空間 |
| AI 生成服務 | AiPageGeneratorService | 複製 + 替換 Azure OpenAI 設定 |
| 分段生成 | GeneratePageWithChunksAsync | 直接複製 |
| 品質驗證 | AiQualityCheck entity + logic | 直接複製 |
| Unsplash 搜尋 | UnsplashImageSearchService | 複製 + 提取 Provider 介面 |
| JWT Auth | JwtHelper, AuthService, AuthController | 直接複製 |
| 對話管理 | AiConversationService + entities | 直接複製 |
| 前端 PageDesigner | GrapesJS 配置 + 自訂 Component | 複製 + 調整 Tailwind 設定 |
| AI Assistant Panel | AiAssistantPanel.vue | 複製 + 調整 UI 風格 |
| Tailwind 注入 | injectTailwindCSS + fixTailwindClasses | 直接複製 |

### 參考架構 (ZKEACMS)

| 概念 | 檔案 | 參考方式 |
|------|------|---------|
| Widget Template | WidgetTemplateService | 參考設計區塊模板系統 |
| Widget Base | WidgetBase, WidgetBasePart | 參考設計區塊實例 |
| Layout Zone | LayoutEntity, LayoutHtmlEntity | 參考設計頁面佈局系統 |
| Publish Task | PublishService | 參考設計部署佇列 |
| Theme System | ThemeService, ThemeEntity | 參考設計主題管理 |

---

## 九、目錄結構

```
siteforge/
├── docs/
│   ├── architecture/      # 架構設計
│   ├── features/          # 功能規格
│   ├── ai/                # AI Prompt 設計
│   ├── api/               # API 文件
│   └── planning/          # 專案規劃
│       └── PLAN.md        ← 本文件
├── backend/
│   └── SiteForge/
│       ├── SiteForge.Api/
│       │   ├── Controllers/
│       │   │   ├── SitesController.cs
│       │   │   ├── PagesController.cs
│       │   │   ├── ThemesController.cs
│       │   │   ├── WidgetTemplatesController.cs
│       │   │   ├── AuthController.cs
│       │   │   ├── AiPageGeneratorController.cs
│       │   │   ├── AiContentGenerationController.cs
│       │   │   └── AiConversationsController.cs
│       │   └── Program.cs
│       ├── SiteForge.Core/
│       │   ├── Entities/
│       │   │   ├── Site.cs
│       │   │   ├── Page.cs
│       │   │   ├── WidgetTemplate.cs
│       │   │   ├── WidgetBase.cs
│       │   │   ├── Theme.cs
│       │   │   ├── Layout.cs
│       │   │   ├── LayoutZone.cs
│       │   │   ├── SiteDomain.cs
│       │   │   ├── AiConversation.cs
│       │   │   ├── AiMessage.cs
│       │   │   ├── PublishTask.cs
│       │   │   ├── User.cs
│       │   │   └── BaseEntity.cs
│       │   ├── DTOs/
│       │   │   ├── AI/
│       │   │   ├── Auth/
│       │   │   ├── Pages/
│       │   │   └── Common/
│       │   ├── Interfaces/
│       │   │   ├── Repositories/
│       │   │   └── Services/
│       │   ├── Services/
│       │   │   ├── AiPageGeneratorService.cs
│       │   │   ├── AiContentGenerationService.cs
│       │   │   ├── AiConversationService.cs
│       │   │   ├── UnsplashImageSearchService.cs
│       │   │   ├── PageService.cs
│       │   │   ├── SiteService.cs
│       │   │   ├── ThemeService.cs
│       │   │   └── AuthService.cs
│       │   └── Utilities/
│       │       ├── JwtHelper.cs
│       │       └── PasswordHelper.cs
│       ├── SiteForge.Infrastructure/
│       │   ├── Data/
│       │   │   └── AppDbContext.cs
│       │   ├── Repositories/
│       │   │   └── BaseRepository.cs
│       │   └── Configurations/
│       │       └── AzureOpenAIConfig.cs
│       └── SiteForge.sln
├── frontend/
│   └── siteforge-ui/
│       ├── src/
│       │   ├── views/
│       │   │   ├── Dashboard.vue
│       │   │   ├── PageDesigner.vue
│       │   │   ├── SiteSettings.vue
│       │   │   └── Login.vue / Register.vue
│       │   ├── components/
│       │   │   ├── editor/
│       │   │   │   ├── AiAssistant.vue
│       │   │   │   └── AiAssistantPanel.vue
│       │   │   └── layout/
│       │   ├── stores/
│       │   │   ├── siteStore.ts
│       │   │   ├── pageStore.ts
│       │   │   ├── themeStore.ts
│       │   │   └── aiStore.ts
│       │   ├── router/
│       │   │   └── index.ts
│       │   ├── api/
│       │   │   ├── sites.ts
│       │   │   ├── pages.ts
│       │   │   ├── ai.ts
│       │   │   └── auth.ts
│       │   └── assets/
│       └── package.json
├── docker-compose.yml
└── README.md
```

---

## 十、關鍵技術決策

| 決策 | 選擇 | 原因 |
|------|------|-------|
| 編輯器核心 | GrapesJS | 省去從零打造 Canvas 的大量時間 (拖拽、樣式、層級全部內建) |
| ORM | SqlSugar | 比 EF Core 更輕量，效能更好，支援多 DB |
| 資料庫 | PostgreSQL | 成熟穩定，JSONB 支援好 |
| 即時預覽 | GrapesJS iFrame | 安全性高，CSS/JS 隔離，已內建 |
| 區塊資料 | JSON Column | 靈活，不需要頻繁改 schema |
| AI 串接 | 後端代理 | 保護 API Key，可快取 |
| 即時通訊 | SignalR | .NET 原生支援，WebSocket |
| UI 布局 | GrapesJS 預設 UI + 三欄式 | 重用 GrapesJS UI，搭配自訂 Icon Bar |
| 前端樣式 | Tailwind CSS | GrapesJS Canvas 內使用，注入到 iframe |
| 分段生成 | 2-Phase (Outline + Chunks) | 避免 token 截斷，可即時串流 |

---

## 十一、風險與對策

| 風險 | 對策 |
|------|------|
| GrapesJS 學習曲線 | 已有 PageDesigner.vue 參考實作，可直接複製配置 |
| LLM 生成 HTML 不穩定 | 後端校驗 HTML 語法 + 提供重試 / 手動編輯 |
| AI 延遲 | SignalR 即時推送 + 分段生成逐步渲染 |
| Canvas iframe 樣式衝突 | Tailwind CSS 注入 iframe，不影響主應用 |
| Tailwind Class 轉義 | 復用 fixTailwindClasses 修復器 |
| 瀏覽器相容性 | 產出標準 HTML/CSS + Tailwind |
| 部署複雜度 | 初期產出靜態 HTML 檔案 (Vercel / Netlify 風格) |

---

## 十二、Git Commit 策略建議

```
Phase 1 (Core Framework)
├── feat: init .NET 8 solution with Api/Core/Infrastructure layers
├── feat: add SqlSugar config and base entities (Site, Page, Theme)
├── feat: add CRUD API endpoints for sites and pages
├── feat: add JWT auth (Login, Register)
├── feat: init Vue 3 frontend with router, pinia stores
└── feat: add Dashboard page (site list)

Phase 2 (Editor Integration)
├── feat: integrate GrapesJS editor with Vue 3
├── feat: add custom block templates (Hero, Features, Team...)
├── feat: add Style Manager and Layer Panel configuration
├── feat: add Canvas iframe Tailwind CSS injection
├── feat: add Link component custom traits
└── feat: add page content save/load (htmlContent + components JSON)

Phase 3 (AI)
├── feat: add AiPageGeneratorService (2-phase: outline + chunks)
├── feat: add Unsplash image search service
├── feat: add AI Assistant panel UI
├── feat: add streaming generation with SignalR
└── feat: add AI conversation history

Phase 4 (Deploy)
├── feat: add static site render engine
├── feat: add custom domain binding
├── feat: add Publish Task queue
└── feat: add Let's Encrypt SSL automation
```

---

> 此文件為初期規劃，將隨開發持續更新。
> 最後更新：2026-05-02
> 參考專案：GrapesJS (visual editor + AI generation), ZKEACMS (CMS widget/template/theme system)
