# SiteForge Manual Media

本資料夾放置使用手冊用的動態教學媒體。

| 檔案 | 說明 |
|---|---|
| `siteforge-editor-tutorial.mp4` | 10 張 Editor 截圖組成的 34 秒教學影片。 |
| `siteforge-editor-tutorial.gif` | 同內容的 GIF 版本，方便 Markdown 預覽。 |
| `siteforge-sofa-editor-demo.mp4` | E2E 錄製慢速課程版沙發產品頁教學，逐步拖拉 Section / Image / Text / Button，並分段標示落點與設定參數。 |
| `siteforge-sofa-editor-demo.gif` | 同內容的 GIF 版本，方便直接嵌入手冊。 |

重新產生方式：

```bash
rtk node scripts/build-editor-tutorial-media.mjs
rtk node scripts/record-sofa-editor-demo.mjs
```
