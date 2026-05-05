# SiteForge — 後端交接文件

> 交接時間：2026-05-04 22:15
> 目錄：`/Users/leeichang/Documents/siteforge/backend/SiteForge/`

---

## 1. 專案目標

AI 驅動的 Website Builder（類比 Wix / GoDaddy），但核心哲學是 **Template First, AI Assist**：
- 使用者從模版開始，不是從空白頁
- AI 用於細部微調（改圖片大小、調間距、改顏色），不是生成整頁
- 前端用 GrapesJS 拖放編輯，後端提供 API + 儲存

## 2. 技術棧

| 項目 | 選擇 |
|------|------|
| 語言 | C# 12 / .NET 8 |
| ORM | SqlSugar |
| 資料庫 | PostgreSQL（本機） |
| 驗證 | JWT（自建，不含 Identity） |
| 架構 | 三層式：Api → Core（Domain）→ Infrastructure |

## 3. 專案結構

```
backend/SiteForge/
├── SiteForge.sln
├── SiteForge.Core/
│   ├── Entities/         ← 已存在（14 Entities + BaseEntity）
│   ├── Enums/            ← 已存在
│   ├── DTOs/             ← 已存在 3 檔案（ApiResponses, AuthDtos, SiteDtos）
│   │   └── 其餘 DTOs     ← 待建立
│   ├── Interfaces/
│   │   ├── Repositories/ ← 待建立（18 interfaces）
│   │   └── Services/     ← 待建立（18 interfaces）
│   ├── Services/         ← 待建立（18 implementations）
│   └── Utilities/        ← 待建立（JwtHelper, PasswordHelper）
├── SiteForge.Infrastructure/
│   ├── Data/             ← AppDbContext.cs（已存在，含種子資料）
│   └── Repositories/     ← BaseRepository.cs（已存在）
│                           └── 其餘 Repositories 待建立（18 個）
└── SiteForge.Api/
    ├── Controllers/Api/  ← 待建立（12 controllers）
    ├── Middleware/        ← 待建立（ExceptionMiddleware）
    ├── Program.cs         ← 待建立
    └── appsettings.json   ← 待建立
```

## 4. 已完成的檔案（17 個）

### Core/Entities/（15 個）
- `BaseEntity.cs` — 基底：Id (Guid), CreatedAt, UpdatedAt, IsDeleted
- `User.cs` — Email, Username, PasswordHash, DisplayName, Role, RefreshToken
- `Site.cs` — Name, Description, UserId, ActiveThemeId, Status, Slug, Scripts
- `Page.cs` — SiteId, Title, Slug, PageType, Components/Styles(JSONB), HTML/CSS/JS
- `WidgetTemplate.cs` — Name, Category, DefaultContent/Style, EditableProps(JSONB)
- `WidgetBase.cs` — PageId, TemplateId, Content/Style(JSONB), ZoneName, Order
- `Theme.cs` — Colors/Fonts/Spacing/BorderRadius/Shadows 全 JSONB
- `Layout.cs` — Name, BodyHtml, Zones(OneToMany)
- `LayoutZone.cs` — LayoutId, Name, Width, Order, PlaceholderHtml
- `SiteDomain.cs` — SiteId, Domain, SSL, Verification, DnsStatus
- `AiConversation.cs` — SiteId, PageId, Title, Model, TokenUsage(JSONB)
- `AiMessage.cs` — ConversationId, Role, Content, ActionType, ActionResult(JSONB)
- `Asset.cs` — SiteId, FileName, MimeType, StoragePath, Source, Dimensions
- `PublishTask.cs` — SiteId, TaskType, Status, Log, PageCounts

### Core/Enums/
- `Enums.cs` — SiteStatus, PageType, UserRole, PublishStatus, PublishTaskType, AiActionType, AssetSource, DnsStatus

### Infrastructure/Data/
- `AppDbContext.cs` — SqlSugar 建立連線、CodeFirst 自動建表、種子資料（4 主題、7 區塊模板、2 佈局）

### Infrastructure/Repositories/
- `BaseRepository<T>` — 抽象基底：GetById/GetAll/GetPaged/Add/Update/SoftDelete/HardDelete/Restore

## 5. 待建立清單（依順序）

### Phase 1 — DTOs（7 檔案）
建立目錄：`Core/DTOs/`
- [ ] `PageDtos.cs` — CreatePageRequest, UpdatePageRequest, PageDto, PageWidgetDto
- [ ] `WidgetDtos.cs` — CreateWidgetRequest, UpdateWidgetRequest, WidgetTemplateDto, WidgetDto
- [ ] `ThemeDtos.cs` — CreateThemeRequest, UpdateThemeRequest, ThemeDto
- [ ] `LayoutDtos.cs` — CreateLayoutRequest, LayoutDto, LayoutZoneDto
- [ ] `AiDtos.cs` — SendMessageRequest, ConversationDto, MessageDto
- [ ] `AssetDtos.cs` — UploadAssetRequest, AssetDto
- [ ] `PublishDtos.cs` — PublishRequest, PublishTaskDto

### Phase 2 — Repository Interfaces（18 檔案）
建立目錄：`Core/Interfaces/Repositories/`
命名規則：`R{Entity}Repository`（R 前綴）
方法：繼承自 BaseRepository 基底方法的 CRUD + 各 Entity 專用查詢

- [ ] `RUserRepository.cs` — GetByEmailAsync, EmailExistsAsync
- [ ] `RSiteRepository.cs` — GetByUserIdAsync, GetBySlugAsync
- [ ] `RPageRepository.cs` — GetBySiteIdAsync, GetBySlugAsync
- [ ] `RWidgetTemplateRepository.cs` — GetByCategoryAsync
- [ ] `RWidgetBaseRepository.cs` — GetByPageIdAsync, GetByPageAndZoneAsync
- [ ] `RThemeRepository.cs` — GetBySystemAsync
- [ ] `RLayoutRepository.cs`
- [ ] `RLayoutZoneRepository.cs` — GetByLayoutIdAsync
- [ ] `RSiteDomainRepository.cs` — GetBySiteIdAsync, GetPrimaryAsync
- [ ] `RAiConversationRepository.cs` — GetBySiteIdAsync
- [ ] `RAiMessageRepository.cs` — GetByConversationIdAsync
- [ ] `RAssetRepository.cs` — GetBySiteIdAsync
- [ ] `RPublishTaskRepository.cs` — GetBySiteIdAsync
- [ ] `IBaseRepository.cs` — 基底 interface（已存在？確認）

### Phase 3 — Service Interfaces（18 檔案）
建立目錄：`Core/Interfaces/Services/`
命名規則：無 I 前綴（`UserService` interface，不是 `IUserService`）

### Phase 4 — Utilities
- [ ] `Utilities/PasswordHelper.cs` — PBKDF2 hash/verify
- [ ] `Utilities/JwtHelper.cs` — JWT 產生/驗證，含 Config

### Phase 5 — Service Implementations（18 檔案）
建立目錄：`Core/Services/`

### Phase 6 — Repository Implementations（18 檔案）
建立目錄：`Infrastructure/Repositories/`

### Phase 7 — Controllers（12 檔案）
建立目錄：`Api/Controllers/Api/`
全部使用 `[Area("api")]` + `[Route("[area]/[controller]")]` + `[ApiController]`

- [ ] `AuthController.cs` — POST login, register, refresh
- [ ] `SitesController.cs` — CRUD + publish
- [ ] `PagesController.cs` — CRUD + reorder
- [ ] `WidgetsController.cs` — CRUD
- [ ] `WidgetTemplatesController.cs` — List
- [ ] `ThemesController.cs` — CRUD
- [ ] `LayoutsController.cs` — CRUD
- [ ] `AssetsController.cs` — Upload + List
- [ ] `AiConversationsController.cs` — CRUD + send message
- [ ] `PublishTasksController.cs` — Status + retry
- [ ] `DomainsController.cs` — CRUD + verify
- [ ] `UsersController.cs` — Profile

### Phase 8 — Infrastructure
- [ ] `Program.cs` — DI 註冊, CORS, JWT Auth, SqlSugar, Middleware
- [ ] `appsettings.json` — ConnectionStrings, JWT Config
- [ ] `Middleware/ExceptionMiddleware.cs` — 全域錯誤處理

## 6. 關鍵架構決策

### Repository 命名
- Interface: `R{Entity}Repository`（例：`RUserRepository`）
- Implementation: `{Entity}Repository`（例：`UserRepository`）
- 注入 Repository 時用 R 前綴 interface

### Service 命名
- Interface: `{Entity}Service`（無 I 前綴，例：`UserService`）
- Implementation: `{Entity}Service`（類別名稱與 interface 同名）
- 解決方法：檔案放不同目錄（Interfaces/Services vs Services），或實作類別加 `Impl` 後綴

### Controller 位置
- 全部放在 `Controllers/Api/` 資料夾
- 使用 `[Area("api")]` + `[Route("[area]/[controller]")]`

### 驗證
- JWT Bearer Token（自己寫 JwtHelper，用 System.IdentityModel.Tokens.Jwt）
- Password 用 PBKDF2（Rfc2898DeriveBytes）
- 沒有 Microsoft.AspNetCore.Identity

### 資料庫
- SqlSugar, PostgreSQL
- 軟刪除：所有 Entity 繼承 BaseEntity 有 IsDeleted
- JSONB 欄位：在 SugarColumn Attribute 設 `ColumnDataType = "jsonb"`
- CodeFirst 自動建表，無需手動 Migration
- 種子資料在 AppDbContext.cs

### DI 註冊（Program.cs 需做的事）
```
// SqlSugar
builder.Services.AddSingleton<ISqlSugarClient>(sp => {
    var ctx = sp.GetRequiredService<AppDbContext>();
    return ctx.CreateClient();
});
builder.Services.AddScoped<AppDbContext>();

// Repositories
builder.Services.AddScoped<RUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<UserService, UserService>();

// Utilities
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddSingleton<PasswordHelper>();
builder.Services.AddHttpContextAccessor();

// CORS
builder.Services.AddCors(...);

// JWT Auth
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(...);

// Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();
```

## 7. 既有的先行決策（來自奕璋）

- **禁止使用 `Microsoft.AspNetCore.Identity`** → 自建 User/Identity
- **Service Interface 不使用 I 前綴** → 不要有 `IUserService`
- **Repository Interface 使用 R 前綴** → `RUserRepository`
- **不要用 EF Core** → 用 SqlSugar
- **Controller 要放在 `[Area("api")]`** → 用 Area Attribute

## 8. 後續開發方向

- Vue 3 前端（GrapesJS editor）
- 5 種產業模板：防偽驗證、掃碼集點、產品介紹、生產履歷、DPP
- AI Chatbot 對話式微調（非生成整頁）
- Google Stitch 設計模板
- GrapesJS Tailwind plugin（https://gjs.market/products/grapesjs-tailwind）

---

交給接手的人了。有問題問奕璋。
