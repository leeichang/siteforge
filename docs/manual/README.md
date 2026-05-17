# SiteForge 使用手冊

本手冊依照目前前端與 E2E 驗證流程整理，適用於本機開發服務：

- UI：`http://127.0.0.1:5010`
- API：`http://127.0.0.1:8000`

## 建議閱讀順序

1. [快速開始](./GETTING_STARTED.md)
2. [Dashboard 與專案管理](./DASHBOARD_MANUAL.md)
3. [Workspace 與頁面管理](./WORKSPACE_MANUAL.md)
4. [Editor 編輯器使用手冊](./EDITOR_MANUAL.md)
5. [AI Assistant 使用手冊](./AI_ASSISTANT_MANUAL.md)
6. [發佈、公開預覽與 E2E 驗證](./PUBLISHING_AND_E2E.md)
7. [沙發產品頁拖拉實作教學](./SOFA_PRODUCT_PAGE_TUTORIAL.md)

## SiteForge 基本流程

```text
註冊 / 登入
  ↓
Dashboard 建立網站
  ↓
Workspace 管理頁面
  ↓
Editor 編輯內容、樣式與素材
  ↓
Save 儲存頁面
  ↓
Publish 發佈網站
  ↓
公開網址預覽
```

## E2E 驗證截圖

本手冊使用的主要畫面截圖放在：

```text
docs/manual/images/
```

這些截圖來自 `scripts/e2e-ui-template-flow.mjs` 及既有 E2E artifacts，涵蓋登入、Dashboard、套用網站樣板、Workspace、AI 產生頁面、Editor 與面板調整。

## Editor 動態教學

如果想用影片方式快速看完整編輯器流程，可以看：

- [SiteForge Editor 教學 MP4](./media/siteforge-editor-tutorial.mp4)
- [SiteForge Editor 教學 GIF](./media/siteforge-editor-tutorial.gif)

![SiteForge Editor 教學 GIF](./media/siteforge-editor-tutorial.gif)

如果想看實際拖拉 blocks 做出一個產品頁，可以看這支沙發產品頁範例。完整逐步說明請看 [沙發產品頁拖拉實作教學](./SOFA_PRODUCT_PAGE_TUTORIAL.md)。

- [沙發產品頁拖拉實作 MP4](./media/siteforge-sofa-editor-demo.mp4)
- [沙發產品頁拖拉實作 GIF](./media/siteforge-sofa-editor-demo.gif)

![沙發產品頁拖拉實作 GIF](./media/siteforge-sofa-editor-demo.gif)

## 截圖索引

| 流程 | 截圖 |
|---|---|
| 登入頁 | ![登入頁](./images/manual-01-login-default.png) |
| 註冊分頁 | ![註冊分頁](./images/manual-02-login-register.png) |
| Dashboard 空狀態 | ![Dashboard 空狀態](./images/manual-03-dashboard-empty.png) |
| 建立專案視窗 | ![建立專案視窗](./images/manual-04-create-project-blank.png) |
| 選取網站樣板 | ![選取網站樣板](./images/manual-05-create-project-template-selected.png) |
| Workspace Pages | ![Workspace Pages](./images/manual-06-workspace-pages.png) |
| Workspace Templates | ![Workspace Templates](./images/manual-07-workspace-templates.png) |
| Workspace Theme | ![Workspace Theme](./images/manual-08-workspace-theme.png) |
| 頁面更多選單 | ![頁面更多選單](./images/manual-09-workspace-page-menu.png) |
| Editor Project Tree | ![Editor Project Tree](./images/manual-10-editor-project-tree.png) |
| Editor Blocks | ![Editor Blocks](./images/manual-11-editor-blocks.png) |
| Editor Global Styles | ![Editor Global Styles](./images/manual-13-editor-global-styles.png) |
| Editor Assets | ![Editor Assets](./images/manual-14-editor-assets.png) |
| Editor AI Assistant | ![Editor AI Assistant](./images/manual-16-editor-ai-assistant.png) |
| Editor Code 視窗 | ![Editor Code 視窗](./images/manual-18-editor-code-modal.png) |
