# SiteForge Stitch Design System Migration Plan

> Date: 2026-05-10
> Scope: UI/UX redesign based on Stitch design prototypes

## Background

奕璋提供 12 個 Stitch 設計原型（`/Users/leeichang/Documents/stitch_siteforge_ai_website_platform/`），涵蓋完整 SiteForge AI 網站平台的視覺設計。本計劃基於已完整閱讀的 9 個有效設計原型，進行前端 UI/UX 全面重寫。

### 設計系統規格
- **CSS Framework**: Tailwind CSS
- **Typography**: Inter (Google Fonts)
- **Icons**: Material Symbols Outlined (Google Fonts)
- **Color System**: Material 3 (Material You) - 包含 Dark 與 Light 雙主題
- **Component Style**: Surface/Container 層級系統，圓角 `rounded-xl`，陰影 `shadow-md`

### 已閱讀原型清單
1. `siteforge_dashboard` - Dashboard Dark
2. `siteforge_dashboard_light` - Dashboard Light
3. `siteforge_editor_grapesjs` - Editor Dark（三欄式佈局）
4. `siteforge_editor_light` - Editor Light
5. `siteforge_dashboard_light_mode` - Dashboard Light Mode
6. `siteforge_editor_light_mode` - Editor Light Mode
7. `page_management_templates` - Page Management Dark
8. `page_management_light` - Page Management Light
9. `page_management_templates_light_mode` - Page Management Light Mode

### 現有前端狀態
- **已修改**：`theme.css`, `App.vue`, `DashboardView.vue`, `LoginView.vue`
- **尚未修改**：`EditorView.vue`, `SiteWorkspaceView.vue`
- **Git Commit**：`65f4fe8` - WIP: Stitch design system migration

---

## Phase 1: CSS Foundation Fix

### 目標
修復 Light 主題 CSS 變數繼承問題，確保 Dark/Light 主題切換完全正常。

### 已知問題
- `theme.css` 已加入 `[data-theme="light"]` 明確定義
- **問題**：Light 主題 CSS 變數在某些 scoped 元件（App.vue sidebar）未正確響應 `data-theme` 變化
- **根因**：scoped CSS 中的 `var()` 在固定定位元素上繼承異常

### 解法選項
| 方案 | 說明 | 優先級 |
|------|------|--------|
| A | 將 sidebar 等 scoped 元件改為 global CSS | 中 |
| B | 在 `:root` 加上 `transition: none` 繞過緩存 | 低 |
| C | theme store 改用 class 切換而非 data-attribute | **推薦** |

### 實作項目
1. 確認 `theme.css` 中 `[data-theme="light"]` 和 `[data-theme="dark"]` 的 specificity 提高至 `html[data-theme]`
2. 檢查所有 scoped CSS 中的 `var()` 引用
3. 將 App.vue 的 sidebar 樣式改為 global 或提高 specificity
4. 測試：反覆切換 Dark/Light，確認所有元件顏色正確

---

## Phase 2: App.vue Layout Refactor

### 目標
根據路由動態切換佈局：Dashboard 頁面用 TopAppBar，Editor 頁面用編輯器專用佈局。

### 現況 vs Stitch 設計

| 頁面 | 現況 | Stitch 設計 | 修改方式 |
|------|------|-------------|----------|
| **Dashboard** | 280px 固定側欄 | **TopAppBar**（頂部導航列）+ 可選側欄 | 加入 TopAppBar，保留側欄但調整風格 |
| **Editor** | 280px 固定側欄 | **窄圖標欄(64px)** + **可展開面板(280px)** | 完全重寫為編輯器佈局 |

### Dashboard 佈局修改
- **TopAppBar** (64px):
  - 左側：品牌名 "SiteForge AI"（headline-lg, font-black, text-primary）
  - 中間：導航鏈接（Dashboard / Sites / Templates / Marketplace）
    - 當前頁面：text-primary + border-b-2 border-primary
    - 其他頁面：text-on-surface-variant hover:text-primary
  - 右側：
    - [New Project] 按鈕（bg-primary text-on-primary rounded-lg）
    - 通知鈴鐺（notifications icon）
    - 幫助（help icon）
    - 設定（settings icon）
    - 用戶頭像（32px 圓形，bg-secondary-container）

### Editor 佈局修改
- **TopAppBar** (64px): 品牌名 + 通知/幫助/用戶 + New Project 按鈕
- **Editor Workspace**:
  - 左：窄圖標欄（64px）- folder/layers/widgets/palette/image/AI/settings
  - 左二：可展開面板（280px）- 當前選擇的工具分類
  - 中央：Canvas 區域（含畫布 toolbar + 模擬網頁 iframe）
  - 右：屬性面板（320px）- Layout/Typography/Background

---

## Phase 3: DashboardView.vue Polish

### 目標
微調現有 Dashboard，對齊 Stitch 設計細節。

### 修改項目
1. **頂欄區域**:
   - 麵包屑導航：Dashboard / Sites（或省略，直接用 TopAppBar 導航）
   - 標題 "Sites" + [New Project] 按鈕
   - 篩選/排序選項

2. **卡片風格**:
   - 背景：`bg-surface-container-lowest`
   - 圓角：`rounded-xl`
   - 邊框：`border border-outline-variant`
   - Hover：縮圖 `group-hover:scale-105` + `shadow-md`
   - 轉場：`transition-shadow`, `transition-transform duration-500`

3. **卡片內容**:
   - 頂部：縮略圖（aspect-video, object-cover）
   - 右下角狀態標籤：
     - Published：`bg-surface-container-lowest/90 backdrop-blur-sm` + text-primary + 動態脈衝點
     - Draft：`bg-surface-variant/90 backdrop-blur-sm` + text-on-surface-variant
   - 底部：
     - 標題（title-md）+ 描述（body-md, line-clamp-2）
     - 更新時間（label-sm, text-outline）
     - more_vert 按鈕

4. **新增空卡片**:
   - 「Create New Project」卡片
   - `border-2 border-dashed border-outline-variant`
   - Hover: `hover:border-primary hover:bg-surface-container`
   - 中央大加號 icon + 說明文字

5. **搜尋功能**:
   - 頂欄搜尋輸入框（帶 search icon）
   - 或卡片區上方的搜尋欄

---

## Phase 4: EditorView.vue Full Rewrite

### 目標
完全重寫為三欄式編輯器佈局，對齊 Stitch 的 Editor 設計。

### 佈局結構（更新版 - 依第二張圖比例）

```
┌─ Top Toolbar ───────────────────────────────────────────────────────────┐
│ SiteForge AI │ Blocks/Code │ Device▼ │ Undo │ Redo │ Publish │ Upgrade │
├─ Left(220px) ─┬────────── Canvas ───────────┬─ Right(250px) ────────────┤
│  🔍 Search     │                            │  Styles / Properties       │
│  ─────────    │  [寬廣 Canvas 區域]         │  ─ Selection              │
│  Regular      │  網頁預覽                   │  Layout                   │
│  Symbols      │  (佔主要空間)               │  Display / Position       │
│  ─────────    │                            │  Width / Height            │
│  Basic ▼      │                            │  Margin                   │
│  1 Column     │                            │                            │
│  2 Columns    │                            │                            │
│  3 Columns    │                            │                            │
│  Section      │                            │                            │
│  Divider      │                            │                            │
│  Heading      │                            │                            │
│  Text         │                            │                            │
│  Link         │                            │                            │
│  Image        │                            │                            │
│  Video        │                            │                            │
│  [Add more]   │                            │                            │
└───────────────┴────────────────────────────┴────────────────────────────┘
```

### 元件清單

#### 1. TopAppBar (64px)
- 品牌名："SiteForge AI"（headline-sm, font-black, text-primary）
- 右側：
  - 通知按鈕（notifications icon）
  - 幫助按鈕（help icon）
  - 用戶頭像（32px 圓形）
  - [New Project] 按鈕（bg-primary text-on-primary rounded-full）

#### 2. 窄圖標欄 (64px)
- 寬度：64px，垂直排列
- 項目：
  - folder（Project）
  - layers（Layers）
  - widgets（Blocks）- **active 時高亮**
  - palette（Styles）
  - image（Assets）
  - 分隔線
  - auto_awesome（AI Assistant）- 特殊漸層背景
  - settings（Settings）
- 每項：圓角 `rounded-xl`，hover `bg-surface-variant`
- Active：`bg-primary-container text-on-primary-container`

#### 3. 可展開面板 (280px)
- 標題列：h2（title-md）+ 搜尋按鈕
- 內容依當前選擇的工具而變：
  - **Blocks**: 分類摺疊面板（Manufacturing / Layout），每分類內 2x2 圖標網格
  - **Layers**: 樹狀結構清單
  - **Styles**: 樣式選項
  - **Assets**: 圖片清單
- 每個 block item：
  - 背景 `bg-surface-variant`
  - 圓角 `rounded-lg`
  - 圖標（28px, text-primary）+ 標籤（label-sm, text-center）
  - Hover: `bg-surface-container-highest` + `border-outline-variant`

#### 4. Canvas 區域
- **Canvas Toolbar** (48px):
  - 左：頁面名稱（title-md）+ 未保存狀態標籤
  - 中：響應式切換（Desktop / Tablet / Mobile）- 分段按鈕組
  - 右：Undo / Redo + 分隔線 + [Publish] 按鈕
- **Canvas 預覽**:
  - 背景：點陣網格 `[radial-gradient(#353438_1px,transparent_1px)] [background-size:24px_24px]`
  - 模擬 iframe：白色背景 `rounded-xl`，寬度依響應式模式變化
  - 內部：模擬網頁內容（placeholder blocks）
  - 高亮覆蓋層：選中的 section 顯示 `border-2 border-primary bg-primary/10` + 標籤

#### 5. 屬性面板 (320px)
- 標題列：tune icon + "Properties"
- 選擇器：顯示當前選中元素名稱 + ID
- 摺疊面板：
  - **Layout**: Display(Flex/Block/Grid) + Direction + Spacing 視覺化編輯器
  - **Typography**: 字體、大小、顏色（collapsed by default）
  - **Background**: 顏色選擇器 + 輸入框
- 輸入框樣式：`bg-surface border border-outline-variant rounded text-on-surface`
- Focus: `focus:border-primary focus:ring-1 focus:ring-primary`

#### 6. 浮動 AI Prompt Bar
- 位置：置底居中 `absolute bottom-8 left-1/2 transform -translate-x-1/2`
- 寬度：`max-w-2xl`
- 背景：`bg-surface-container-high/80 backdrop-blur-[12px]`
- 邊框：`border border-white/10`
- 圓角：`rounded-full`
- 陰影：`shadow-[0_8px_32px_rgba(0,0,0,0.5)]`
- 內容：
  - auto_awesome icon（text-primary）
  - 輸入框 placeholder: "Describe a section to generate..."
  - 發送按鈕（漸層 bg-primary-container → tertiary-container, rounded-full）

---

## Phase 5: SiteWorkspaceView.vue Full Rewrite

### 目標
重寫為 Page Management 介面，對齊 Stitch 的 page_management 設計。

### 佈局結構
```
TopAppBar（與 Dashboard 相同）
├─ 麵包屑導航：Sites > Acme Corp Redesign
├─ 標題：Page Management + [New Page] 按鈕
├─ Tabs：[Pages] [Templates] [Theme]
├─ 頁面卡片網格（Bento Style）
│  ├─ 每張卡片：圖標(40px圓角) + 名稱 + 類型 + 更新時間 + 發布toggle + more_vert
└─ Templates Section（下方區塊）
   ├─ 標題："Available Templates"
   └─ 模板卡片：預覽縮圖 + 名稱 + 說明 + [Add] 按鈕
```

### 頁面卡片設計
- 背景：`bg-surface-container-lowest rounded-xl border border-outline-variant`
- Padding: `p-md`
- 內容：
  - 頂部：圖標（40px 圓角容器，bg-primary-container 或 bg-surface-variant）+ 名稱（title-lg）+ 類型（label-sm）
  - 右上角：more_vert 按鈕（group-hover 顯示）
  - 底部：更新時間 + 發布狀態 toggle switch
- Hover: `shadow-md`

### 發布狀態 Toggle
- Published：text-primary + 開啟的 toggle（bg-primary）
- Draft：text-on-surface-variant + 關閉的 toggle（bg-surface-variant）

### Templates Section
- 標題：headline-md + body-md 說明
- 模板卡片：
  - 頂部：預覽縮圖（h-40, bg-surface-variant, 漸層覆蓋 + 圖標）
  - 底部：名稱（title-md）+ 說明（label-sm）+ [Add] 按鈕（bg-secondary-container）

---

## Phase 6: Light Theme Final Verification

### 驗證清單
1. **重新 build** `npm run build`
2. **真實瀏覽器測試**（非 Playwright）
3. **Dark/Light 切換測試**：
   - 切換按鈕點擊後所有元件即時更新
   - sidebar、cards、buttons、inputs 顏色正確
4. **各頁面驗證**：
   - Dashboard：卡片背景正確、hover 效果正常
   - Editor：Canvas 網格、工具面板、屬性面板顏色正確
   - Workspace：頁面卡片、Templates 區塊顏色正確
5. **響應式測試**：桌面、平板、手機佈局正常

---

## Technical Recommendations

| 項目 | 建議 |
|------|------|
| **CSS 策略** | 全部改用 global CSS classes（無 scoped），或在 scoped 區塊內也導入 CSS 變數 |
| **Sidebar 切換** | App.vue 根據當前 route 決定顯示 Dashboard Sidebar 還是 Editor Sidebar Layout |
| **Theme 切換** | 保留 `data-theme` 方式，但將 specificity 提高至 `html[data-theme="light"]` |
| **Editor 拆分** | 拆成子元件：`EditorTopBar.vue`, `EditorToolPanel.vue`, `EditorCanvas.vue`, `EditorProperties.vue`, `AiPrompt.vue` |
| **Tailwind 配置** | 已使用 CDN 版本，建議後續遷移至本地配置以支援 JIT |

---

## File Changes Summary

| 階段 | 檔案 | 修改類型 |
|------|------|----------|
| Phase 1 | `src/styles/theme.css` | 修復 |
| Phase 2 | `src/App.vue` | 重構 |
| Phase 2 | `src/stores/theme.js` | 可選修改 |
| Phase 3 | `src/views/DashboardView.vue` | 微調 |
| Phase 4 | `src/views/EditorView.vue` | **完全重寫** |
| Phase 4 | `src/components/editor/*.vue` | **新增** |
| Phase 5 | `src/views/SiteWorkspaceView.vue` | **完全重寫** |
| Phase 6 | 所有視圖 | 驗證 |

---

## Estimated Effort

| 階段 | 預估時間 |
|------|----------|
| Phase 1: CSS 變數修復 | 30 min |
| Phase 2: App.vue 佈局調整 | 1-2 hr |
| Phase 3: DashboardView 微調 | 1 hr |
| Phase 4: EditorView 重寫 | 3-4 hr |
| Phase 5: SiteWorkspaceView 重寫 | 2-3 hr |
| Phase 6: Theme 驗證 | 30 min |
| **總計** | **~8-10 hr** |

---

## References

- 設計原型位置：`/Users/leeichang/Documents/stitch_siteforge_ai_website_platform/`
- 前端專案位置：`/Users/leeichang/Documents/siteforge/frontend/siteforge-ui/`
- 現有 MVP Plan：`/Users/leeichang/Documents/siteforge/docs/planning/FRONTEND_MVP_PLAN.md`
