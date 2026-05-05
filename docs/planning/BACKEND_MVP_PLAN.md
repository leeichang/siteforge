# SiteForge Backend MVP Plan

> Updated: 2026-05-05
> Scope: `backend/SiteForge`

## Summary

This plan tracks the backend MVP skeleton for SiteForge. The goal is to provide a buildable .NET 8 API foundation that supports authentication, site/page CRUD, template/theme/layout lookup, and minimal supporting APIs for assets, domains, AI conversations, widgets, and publish tasks.

The backend follows the existing direction in `PLAN.md`: Template First, AI Assist, Vue/GrapesJS frontend later, and .NET 8 + SqlSugar + PostgreSQL on the server.

## Current Implementation

Implemented backend skeleton:

- Solution/project structure:
  - `SiteForge.sln`
  - `SiteForge.Core`
  - `SiteForge.Infrastructure`
  - `SiteForge.Api`
- Core layer:
  - Existing entities, enums, and DTOs retained.
  - Added service interfaces.
  - Added `PasswordHelper` using PBKDF2.
  - Added `JwtHelper` using JWT bearer tokens.
  - Added service implementations using `*ServiceImpl` naming.
- Infrastructure layer:
  - Existing `AppDbContext` retained and adjusted for Guid inserts.
  - Existing `BaseRepository<T>` retained and adjusted for Guid inserts.
  - Added repository implementations for user, site, page, widget template, widget base, theme, layout, layout zone, site domain, AI conversation, AI message, asset, and publish task.
- API layer:
  - Added `Program.cs` with controllers, Swagger, CORS, JWT auth, SqlSugar DI, repository/service DI, and database initialization.
  - Added global `ExceptionMiddleware`.
  - Added controllers under `Controllers/Api` using `[Area("api")]` and `[Route("[area]/[controller]")]`.

## API Surface

Auth and user profile:

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/refresh`
- `GET /api/users/profile`
- `PUT /api/users/profile`

Main MVP CRUD:

- `GET /api/sites`
- `GET /api/sites/{id}`
- `POST /api/sites`
- `PUT /api/sites/{id}`
- `DELETE /api/sites/{id}`
- `POST /api/sites/{id}/publish`
- `GET /api/pages/site/{siteId}`
- `GET /api/pages/{id}`
- `POST /api/pages/site/{siteId}`
- `PUT /api/pages/{id}`
- `DELETE /api/pages/{id}`

Template and editor support:

- `GET /api/widgettemplates`
- `GET /api/widgettemplates/{id}`
- `GET /api/themes`
- `GET /api/themes/{id}`
- `GET /api/layouts`
- `GET /api/layouts/{id}`
- `GET /api/widgets/page/{pageId}`
- `POST /api/widgets/page/{pageId}`
- `PUT /api/widgets/{id}`
- `DELETE /api/widgets/{id}`

Supporting MVP endpoints:

- `GET /api/assets/site/{siteId}`
- `POST /api/assets`
- `GET /api/domains/site/{siteId}`
- `POST /api/domains/site/{siteId}`
- `POST /api/domains/{id}/verify`
- `DELETE /api/domains/{id}`
- `GET /api/aiconversations/site/{siteId}`
- `POST /api/aiconversations`
- `GET /api/aiconversations/{conversationId}/messages`
- `POST /api/aiconversations/{conversationId}/messages`
- `GET /api/publishtasks/site/{siteId}`
- `GET /api/publishtasks/site/{siteId}/latest`
- `POST /api/publishtasks/{id}/retry`

## Implementation Decisions

- Repository interfaces keep the existing `R{Entity}Repository` naming.
- Service interfaces use names without an `I` prefix.
- Service implementations use `*ServiceImpl` to avoid interface/class name collisions.
- Register/login/refresh are anonymous.
- System templates, themes, and layouts are anonymous for frontend bootstrap.
- Site and page APIs require JWT.
- Page APIs verify site ownership through the owning site before reading or mutating pages.
- AI, publish, domain, and asset features are MVP data skeletons only. They do not integrate external LLM, DNS, SSL, CDN, or file storage yet.

## Verification Plan

The local machine currently does not have `dotnet` in PATH, so build verification still needs to be run in an environment with .NET 8 SDK installed.

Required commands once SDK is available:

```bash
dotnet restore backend/SiteForge/SiteForge.sln
dotnet build backend/SiteForge/SiteForge.sln
```

API smoke test after build succeeds:

1. Start the API.
2. Confirm Swagger opens in development mode.
3. Register a user.
4. Login and copy the JWT.
5. Create a site.
6. Fetch site list.
7. Create a page for that site.
8. Fetch page list for that site.
9. Confirm seed themes, widget templates, and layouts are returned and are not duplicated across restarts.

## Next Backend Work

- Install or expose .NET 8 SDK in this workspace and run restore/build.
- Fix any compile errors found by the real SDK.
- Add integration tests for Auth, Sites, Pages, and seeded lookups.
- Add authorization checks to supporting APIs that currently accept `siteId` directly: assets, domains, AI conversations, publish tasks, and widgets.
- Replace placeholder asset registration with real upload/storage.
- Replace AI conversation message storage with real AI action handling.
- Add publish worker/service that renders static HTML from page content.
- Move secrets out of `appsettings.json` into user secrets or environment variables before deployment.
