# SiteForge Frontend MVP Plan

> Updated: 2026-05-05
> Scope: frontend application, planned path `frontend/siteforge-web`

## Summary

Build the SiteForge frontend as a Vue 3 + TypeScript + Vite application that connects to the .NET backend MVP and provides the first usable website builder flow:

1. User registers/logs in.
2. User sees a dashboard of sites.
3. User creates/selects a site.
4. User manages pages for the site.
5. User opens a GrapesJS-based editor.
6. User edits page content with blocks/templates.
7. User saves page HTML/CSS/JS/components/styles back to the backend.

This frontend phase does not implement full AI generation, real publishing, real asset upload, DNS, SSL, or production billing. Those remain later phases.

## Product Scope

Frontend MVP includes:

- Auth screens:
  - Login
  - Register
  - Token persistence
  - Logout
- App shell:
  - Top bar
  - Left navigation
  - User/profile menu
  - API error handling
- Dashboard:
  - List sites from `GET /api/sites`
  - Create site via `POST /api/sites`
  - Open site workspace
- Site workspace:
  - Site overview
  - Page list from `GET /api/pages/site/{siteId}`
  - Create page via `POST /api/pages/site/{siteId}`
  - Open page editor
- Editor MVP:
  - GrapesJS editor canvas
  - Block manager populated from backend `GET /api/widgettemplates`
  - Device switcher: desktop/tablet/mobile
  - Layers panel
  - Style manager
  - Trait manager
  - Save button writing to `PUT /api/pages/{id}`
  - Basic preview mode
- Public lookup screens:
  - Themes from `GET /api/themes`
  - Layouts from `GET /api/layouts`

Out of scope for this phase:

- AI prompt-to-page generation.
- Streaming AI UI.
- Real image upload/storage.
- Real publish output.
- Domain verification UI beyond simple list/status.
- Team collaboration, roles, billing, analytics, marketplace, template marketplace.

## Tech Stack

- Framework: Vue 3 + Composition API + TypeScript.
- Build tool: Vite.
- Routing: Vue Router.
- State: Pinia.
- HTTP: Axios.
- UI library: Element Plus for MVP speed and consistency.
- Editor: GrapesJS.
- Editor CSS: Tailwind CSS injected into GrapesJS canvas iframe.
- Icons: Element Plus icons or lucide-vue if added later.
- Tests:
  - Vitest for unit tests.
  - Playwright for smoke/e2e once screens exist.

Recommended initial dependencies:

```bash
npm create vite@latest frontend/siteforge-web -- --template vue-ts
cd frontend/siteforge-web
npm install vue-router pinia axios element-plus @element-plus/icons-vue grapesjs
npm install -D vitest @vue/test-utils jsdom playwright
```

## Route Plan

Use Vue Router history mode for app pages. The editor may use query params for focused resources.

- `/login`
  - Login form.
  - On success store access token and refresh token.
- `/register`
  - Register form.
  - On success store tokens and redirect to dashboard.
- `/`
  - Redirect to `/dashboard` if authenticated, otherwise `/login`.
- `/dashboard`
  - Site cards and create-site action.
- `/sites/:siteId`
  - Site workspace overview.
  - Pages list, settings summary, publish task placeholder.
- `/sites/:siteId/pages/:pageId/editor`
  - GrapesJS editor.
- `/settings/profile`
  - Profile view/edit using `/api/users/profile`.

## API Client Plan

Create a single Axios client with:

- `baseURL` from `VITE_API_BASE_URL`, default `http://localhost:5000`.
- `Authorization: Bearer <token>` request interceptor.
- 401 response handling:
  - Try refresh once using `/api/auth/refresh`.
  - If refresh fails, clear tokens and redirect to `/login`.
- Normalize backend response envelope:
  - Backend returns `{ success, message, data, errors }`.
  - API client should return `data` for success and throw a typed error for failure.

Recommended modules:

- `src/api/http.ts`
- `src/api/auth.ts`
- `src/api/sites.ts`
- `src/api/pages.ts`
- `src/api/templates.ts`
- `src/api/themes.ts`
- `src/api/layouts.ts`
- `src/api/widgets.ts`

## State Plan

Use Pinia stores:

- `authStore`
  - user, accessToken, refreshToken, isAuthenticated.
  - login, register, refresh, logout, loadProfile.
- `siteStore`
  - sites, activeSite, loading states.
  - fetchSites, createSite, fetchSite.
- `pageStore`
  - pages, activePage.
  - fetchPages, createPage, fetchPage, savePage.
- `editorStore`
  - editor instance metadata only.
  - selected device, dirty state, lastSavedAt.

Avoid storing the full GrapesJS editor object in Pinia if it causes serialization/devtools issues. Keep the editor instance local to the editor component or in a shallow ref composable.

## UI Layout Plan

Use an operational SaaS layout rather than a landing page.

Dashboard:

- Header with product name, user menu, logout.
- Dense site card grid or table.
- Create Site dialog.
- Empty state when no sites exist.

Site workspace:

- Left sidebar for site sections: Pages, Themes, Domains, Publish.
- Main area starts with Pages.
- Pages use a compact list/table with title, slug, status, and edit action.

Editor:

- Full-screen app surface.
- Top bar:
  - Page title.
  - Desktop/tablet/mobile segmented control.
  - Preview toggle.
  - Save button.
  - Back to site button.
- Left vertical tool rail:
  - Project/pages.
  - Layers.
  - Blocks.
  - Assets placeholder.
  - Global theme placeholder.
  - AI placeholder.
- Center:
  - GrapesJS canvas.
- Right panel:
  - GrapesJS style manager, trait manager, layers/blocks depending active tool.

Keep controls compact and editor-first. Do not build a marketing landing page as the first screen.

## GrapesJS Integration Plan

Create `EditorView.vue` that owns the GrapesJS lifecycle:

- On mount:
  - Fetch page detail from `GET /api/pages/{pageId}`.
  - Fetch templates from `GET /api/widgettemplates`.
  - Initialize GrapesJS into a stable container.
  - Load existing page content:
    - Prefer `components` + `styles` if present.
    - Fallback to `htmlContent` + `cssContent`.
    - Fallback to a simple blank page section.
  - Register backend templates as GrapesJS blocks.
  - Inject Tailwind CDN into canvas iframe.
- On save:
  - Read `editor.getHtml()`.
  - Read `editor.getCss()`.
  - Read `editor.getJs()` if available.
  - Serialize components and styles.
  - Send `PUT /api/pages/{pageId}` with:
    - `htmlContent`
    - `cssContent`
    - `jsContent`
    - `components`
    - `styles`
- On unmount:
  - Destroy GrapesJS instance.

Template mapping:

- Backend `WidgetTemplateDto.defaultContent` becomes GrapesJS block `content`.
- `name` becomes block label.
- `category` becomes block category.
- `defaultStyle` can be appended to canvas CSS or bundled with block content later.
- `editableProps` is reserved for later custom traits.

Canvas CSS:

- Inject Tailwind CDN for MVP.
- Add a small reset for body margin and responsive canvas width.
- Later replace CDN injection with compiled Tailwind if needed.

## File Structure

Recommended initial structure:

```text
frontend/siteforge-web/
├── index.html
├── package.json
├── vite.config.ts
├── src/
│   ├── main.ts
│   ├── App.vue
│   ├── router/
│   │   └── index.ts
│   ├── api/
│   │   ├── http.ts
│   │   ├── auth.ts
│   │   ├── sites.ts
│   │   ├── pages.ts
│   │   ├── templates.ts
│   │   ├── themes.ts
│   │   └── layouts.ts
│   ├── stores/
│   │   ├── auth.ts
│   │   ├── sites.ts
│   │   ├── pages.ts
│   │   └── editor.ts
│   ├── layouts/
│   │   └── AppShell.vue
│   ├── views/
│   │   ├── LoginView.vue
│   │   ├── RegisterView.vue
│   │   ├── DashboardView.vue
│   │   ├── SiteWorkspaceView.vue
│   │   ├── EditorView.vue
│   │   └── ProfileView.vue
│   ├── components/
│   │   ├── app/
│   │   ├── sites/
│   │   ├── pages/
│   │   └── editor/
│   └── styles/
│       └── main.css
```

## Milestones

### Milestone 1 — Frontend Foundation

- Scaffold Vue 3 + Vite + TypeScript.
- Add router, Pinia, Element Plus, Axios.
- Add environment config.
- Add app shell.
- Add auth store and route guards.
- Implement login/register/profile/logout.

Acceptance:

- User can register/login against backend.
- JWT is attached to authenticated API calls.
- Refresh token flow is wired.
- Protected routes redirect unauthenticated users to login.

### Milestone 2 — Dashboard and Site Workspace

- Implement dashboard site list.
- Implement create-site dialog.
- Implement site workspace.
- Implement page list and create-page dialog.
- Add basic error/loading/empty states.

Acceptance:

- User can create a site and see it in dashboard.
- Opening a site shows its pages.
- User can create a page and open editor route.

### Milestone 3 — GrapesJS Editor MVP

- Install and initialize GrapesJS.
- Load page detail.
- Register backend widget templates as blocks.
- Add Tailwind canvas injection.
- Add save flow to backend page update API.
- Add dirty state and save feedback.
- Add device switching.

Acceptance:

- Editor opens for a real backend page.
- Backend templates appear as draggable blocks.
- User can drag blocks into canvas.
- User can save.
- Reloading editor restores saved content.

### Milestone 4 — Polish and Smoke Tests

- Add UI polish for dashboard/workspace/editor.
- Add API error toasts.
- Add Vitest coverage for API envelope handling and stores.
- Add Playwright smoke test for login → create site → create page → editor opens.

Acceptance:

- MVP flow is repeatable without manual console fixes.
- Basic responsive layout works on desktop and tablet widths.
- No obvious text overflow or broken editor panels.

## Backend Dependencies

Frontend implementation depends on:

- Backend build passing.
- Backend running locally.
- Swagger available at `/swagger`.
- PostgreSQL database reachable.
- Seeded templates/themes/layouts available.

Known backend gaps that may affect frontend:

- No real upload endpoint yet; asset UI should be placeholder/register-only.
- AI endpoints only store messages; AI assistant panel should be placeholder.
- Publish only queues tasks; publish UI should show task state but not promise live deployment.
- Supporting APIs that accept `siteId` need stronger authorization later.

## Verification Commands

Once the frontend project exists:

```bash
cd frontend/siteforge-web
npm install
npm run typecheck
npm run test
npm run dev
```

If Playwright is added:

```bash
npm run test:e2e
```

Manual smoke test:

1. Start backend API.
2. Start frontend dev server.
3. Register a user.
4. Login.
5. Create a site.
6. Open the site workspace.
7. Create a page.
8. Open editor.
9. Drag a backend template block into canvas.
10. Save page.
11. Reload editor and confirm content persists.

## Open Decisions

- Whether to use Element Plus only or add lucide-vue for editor tool icons.
- Whether to keep Tailwind only inside GrapesJS canvas or also use Tailwind for the app shell.
- Whether frontend path should be exactly `frontend/siteforge-web` or another name.
- Whether editor route should remain full-screen or nested inside the site workspace shell.
