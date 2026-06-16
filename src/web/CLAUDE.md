# CLAUDE.md — MyOS Frontend

> Single source of truth for AI assistants working on `src/web/`.
> Last verified: 2026-06-08.

---

## Stack & Versions

| Package | Version | Notes |
|---|---|---|
| Next.js | 16.2.7 | **Breaking changes vs 15** — read below |
| React | 19.2.4 | |
| TypeScript | 5.x strict | `"strict": true` in tsconfig |
| Tailwind CSS | 4.x | CSS-first — **no `@tailwind` directives** |
| shadcn/ui | 4.x | Uses `@base-ui/react`, not `@radix-ui` |
| TanStack Query | 5.x | |
| React Hook Form | 7.x | |
| Zod | 4.x | **Breaking: error key is `error`, not `message`** |
| @hookform/resolvers | 5.x | Compatible with Zod 4 |
| next-intl | 4.13 | URL-based locale routing |
| jose | 6.x | JWT decode only — tokens live in httpOnly cookies |
| sonner | latest | Toast notifications — global mutation errors via `MutationCache` |

---

## Things AI Must Never Do

- Use `middleware.ts` — Next.js 16 uses `src/proxy.ts` with `export async function proxy` (not `middleware`)
- Access `params` or `searchParams` synchronously — they are `Promise<…>` in Next.js 16, always `await params`
- Call the backend directly from client-side JavaScript — client-side `apiClient` uses an empty base URL so the browser calls Next.js (same origin), which the rewrite proxies to the backend; CORS is never an issue
- Add an `Authorization` header manually in client-side code — `proxy.ts` reads the httpOnly cookie and injects the header server-side before the rewrite forwards the request
- Store JWT tokens in `localStorage` or `sessionStorage` — tokens live exclusively in httpOnly cookies set by the BFF route handlers
- Use `@tailwind base`, `@tailwind components`, `@tailwind utilities` — Tailwind v4 is CSS-first: `@import "tailwindcss"` in globals.css
- Use Zod's `.min(n, { message: '…' })` — Zod 4 uses `{ error: '…' }` as the key
- Add `'use client'` without a reason — default is Server Component; only add it when you need hooks, event handlers, or browser APIs
- Put page-level data fetching in Client Components — fetch in the Server Component (`page.tsx`) and pass as `initialData` to the Client Component
- Bypass the module slice structure — every domain has its own `api/`, `hooks/`, `components/`, `schemas/`, `types/` under `src/modules/{name}/`
- Add new auth logic to `src/app/api/auth/` without checking what already exists (login, logout, refresh are implemented)
- Use `router.push` after login/logout — use `router.replace` so back navigation doesn't return to the auth page
- Hardcode locale strings (`"en"`, `"pl"`) outside the `language.ts` map — derive from `Language` enum using `LANGUAGE_TO_LOCALE`
- Add shadcn components via `npx shadcn add` without verifying the component lands in `src/shared/components/ui/`
- Add per-mutation `onError` handlers for generic backend error display — `MutationCache.onError` in `query-client.ts` already shows a `toast.error` for every failed mutation; add `onError` only when mutation-specific logic is needed
- Write Zod schemas with hardcoded error strings — schemas that power forms use factory functions accepting a plain error object (`{ titleRequired: string, ... }`); the component calls `t()` and passes the translated strings to the factory
- Wrap `BreadcrumbLink` (or use `AppBreadcrumbs`) in a Server Component — `BreadcrumbLink` uses `useRender` from `@base-ui/react` and requires `"use client"`

---

## Next.js 16 Breaking Changes

These differ from standard Next.js training data:

| Feature | Next.js 15 | **Next.js 16 (this project)** |
|---|---|---|
| Middleware file | `middleware.ts`, export `middleware` | `proxy.ts`, export `proxy` |
| `params` in pages | synchronous `{ id: string }` | `Promise<{ id: string }>` — must `await` |
| `searchParams` in pages | synchronous | `Promise<…>` — must `await` |
| Middleware config | `export const config` | same, but in `proxy.ts` |

---

## Directory Structure

```
src/web/
├── .env.local                  ← NEXT_PUBLIC_API_URL + NODE_TLS_REJECT_UNAUTHORIZED (dev)
├── next.config.ts              ← next-intl plugin + rewrites (proxy /api/v* to backend)
├── components.json             ← shadcn config — aliases point to @/shared/
├── messages/
│   ├── en.json                 ← keys: common, identity, notes, navigation, settings
│   └── pl.json
└── src/
    ├── proxy.ts                ← auth guard + token refresh + Authorization injection
    ├── i18n/
    │   ├── request.ts          ← next-intl server config
    │   └── routing.ts          ← supported locales: en, pl
    ├── app/
    │   ├── layout.tsx          ← root layout: fonts, QueryProvider
    │   ├── globals.css         ← @import "tailwindcss"; @import "tw-animate-css"
    │   ├── api/auth/
    │   │   ├── login/route.ts  ← POST: calls backend, sets httpOnly cookies
    │   │   ├── logout/route.ts ← DELETE: clears cookies
    │   │   └── refresh/route.ts← POST: refreshes tokens, updates cookies
    │   └── [locale]/
    │       ├── layout.tsx      ← NextIntlClientProvider only
    │       ├── (public)/       ← no auth required
    │       │   ├── login/
    │       │   └── register/
    │       └── (app)/          ← auth-guarded (proxy.ts enforces)
    │           ├── layout.tsx  ← SessionProvider + Sidebar layout (re-renders on each (app) entry)
    │           ├── home/
    │           ├── notes/
    │           ├── settings/
    │           ├── learning/   ← stub
    │           ├── finance/    ← stub
    │           └── fitness/    ← stub
    ├── modules/
    │   ├── identity/
    │   │   ├── api/            ← auth.api.ts, users.api.ts
    │   │   ├── hooks/          ← useLogin.ts, useRegister.ts
    │   │   ├── components/     ← LoginForm.tsx, RegisterForm.tsx
    │   │   ├── schemas/        ← login.schema.ts, register.schema.ts
    │   │   └── types/          ← identity.types.ts
    │   └── notes/
    │       ├── api/            ← text-notes.api.ts, check-lists.api.ts
    │       ├── hooks/
    │       │   ├── text-notes/ ← useTextNotes, useTextNote, useCreate/Update/DeleteTextNote
    │       │   └── check-lists/← useCheckLists, useCheckList, useCheckListMutations
    │       ├── components/
    │       │   ├── text-notes/
    │       │   └── check-lists/
    │       ├── schemas/
    │       └── types/          ← notes.types.ts
    └── shared/
        ├── lib/
        │   ├── api-client.ts   ← single fetch wrapper; empty base URL client-side
        │   ├── api-error.ts    ← ApiError class from ProblemDetails
        │   ├── format.ts       ← formatDate(iso) — shared date formatting
        │   ├── paging.ts       ← buildPagingParams(PagingRequest) → query string
        │   ├── session.ts      ← getServerSession(), getServerToken() — server only
        │   ├── language.ts     ← Language enum ↔ locale string map
        │   ├── query-client.ts ← TanStack Query singleton
        │   └── utils.ts        ← cn() = clsx + tailwind-merge
        ├── types/
        │   ├── api.types.ts    ← PagingList<T>, ProblemDetails, PagingRequest
        │   ├── common.types.ts ← Language, SUPPORTED_LOCALES, DEFAULT_LOCALE
        │   └── tanstack.d.ts   ← TanStack Query type augmentation (mutationMeta.suppressToast)
        ├── components/
        │   ├── ui/             ← shadcn copies: button, input, label, card, alert, separator, dialog, breadcrumb, sonner
        │   │   ├── confirm-dialog.tsx  ← reusable delete confirmation modal (wraps Dialog)
        │   │   └── paginated-list.tsx  ← generic PaginatedList<T>; list mode (renderItem) or table mode (columns); sort, row click, row actions
        │   └── layout/
        │       ├── Sidebar.tsx         ← only file to change when adding a new module nav link
        │       └── AppBreadcrumbs.tsx  ← "use client" breadcrumb wrapper; accepts BreadcrumbEntry[]
        ├── hooks/
        │   ├── useSession.ts
        │   └── usePaginatedNavigation.ts  ← page/pageSize/orderBy/orderByDesc state + URL sync + auto-scroll; generic TColumn; use with PaginatedList
        └── providers/
            ├── QueryProvider.tsx
            └── SessionProvider.tsx
```

---

## Request Lifecycle

### Browser → API (authenticated data fetch)

```
Client Component calls apiClient("/api/v1/notes/text")
  → API_BASE = "" (client-side) → relative URL /api/v1/notes/text
  → proxy.ts matches "/api/v(.*)"
  → reads access_token from httpOnly cookie
  → injects Authorization: Bearer <token> into request headers
  → NextResponse.next() with modified headers
  → next.config.ts rewrite: /api/v1/notes/text → https://localhost:7255/api/v1/notes/text
  → backend returns data
```

### Server Component → API

```
Server Component calls apiClient("/api/v1/notes/text", { token })
  → API_BASE = NEXT_PUBLIC_API_URL (server-side) → full URL
  → direct fetch to https://localhost:7255/api/v1/notes/text with Bearer header
  → no rewrite, no middleware
```

### Login (BFF)

```
Browser → POST /api/auth/login (Next.js BFF route handler)
  → BFF calls loginApi() → apiClient → backend https://localhost:7255/api/v1/auth/login
  → backend returns { accessToken, refreshToken }
  → BFF decodes JWT, sets httpOnly cookies, returns { userId, email, language } to browser
  → browser never sees raw tokens
```

---

## Environment Variables

```
# .env.local (not committed)
NEXT_PUBLIC_API_URL=https://localhost:7255
NODE_TLS_REJECT_UNAUTHORIZED=0    # dev only — Node.js ignores OS cert store for .NET dev cert
```

`NEXT_PUBLIC_API_URL` is used server-side only (api-client.ts, rewrites, proxy.ts refresh). Client-side code always uses relative URLs.

---

## Auth Guard — proxy.ts

Protected path segments (after locale prefix): `/home`, `/notes`, `/settings`, `/learning`, `/finance`, `/fitness`

Logic on each request to a protected path:
1. Valid `access_token` cookie → continue
2. Expired access token + valid `refresh_token` → call backend refresh → update cookies → continue
3. No refresh token or refresh failed → delete cookies → redirect to `/{locale}/login`

Public-auth paths (`/login`, `/register`): redirect to `/{locale}/home` if already authenticated.

API paths (`/api/v…`): inject Authorization header only — no redirect logic.

---

## API Client

`src/shared/lib/api-client.ts`:

- **Server-side** (`typeof window === 'undefined'`): `API_BASE = NEXT_PUBLIC_API_URL` — full URL, include `token` param explicitly
- **Client-side**: `API_BASE = ""` — relative URL, middleware injects auth header, rewrite forwards to backend
- Throws `ApiError` (from `api-error.ts`) on non-2xx responses
- Returns `undefined` for 204 responses
- All API functions in `modules/{name}/api/` accept optional `token?: string` for server-side use

---

## Module Slice Convention

```
modules/{name}/
├── api/          ← pure TS functions, one per endpoint, no React
├── hooks/        ← useQuery/useMutation wrappers, one hook per endpoint or grouped by entity
├── components/   ← React components for this module
├── schemas/      ← Zod schemas for forms
└── types/        ← TypeScript interfaces matching backend DTOs
```

**API functions** — accept `token?` for server-side callers, use `apiClient`:
```typescript
export async function getTextNotesApi(params: PagingRequest = {}, token?: string) {
  return apiClient<PagingList<TextNoteDto>>(`${BASE}?${qs}`, { token })
}
```

**Query keys** — one `query-keys.ts` per entity with a structured keys object:
```typescript
export const textNoteKeys = {
  all: ['text-notes'] as const,
  lists: () => [...textNoteKeys.all, 'list'] as const,
  detail: (id: string) => [...textNoteKeys.all, 'detail', id] as const,
}
```

**Hooks** — one hook per operation; mutation hooks invalidate relevant query keys on success.

---

## i18n

- Locales: `en` (default), `pl`
- URL structure: `/{locale}/…` — always present
- Message keys mirror module structure: `notes.textNotes.title`, `identity.login.submit`, etc.
- Language stored in JWT claim `language` as string `"0"` (English) or `"1"` (Polish)
- Map in `src/shared/lib/language.ts`: `{ 0: 'en', 1: 'pl' }`
- After language change (PATCH /users/me/language): backend returns new tokens → BFF updates cookies → client calls `router.replace('/{newLocale}/…')`

---

## Tailwind & shadcn

**globals.css**:
```css
@import "tailwindcss";
@import "tw-animate-css";
/* No @tailwind directives — Tailwind v4 is CSS-first */
```

**shadcn components** — installed to `src/shared/components/ui/`. Add via:
```
npx shadcn add <component>
```
components.json aliases ensure correct placement.

---

## Shared UI Patterns

### Backend error toasts
`MutationCache.onError` in `src/shared/lib/query-client.ts` catches every failed mutation and calls `toast.error(error.detail)`. The `<Toaster />` from `@/shared/components/ui/sonner` is mounted in `src/app/layout.tsx`. No additional setup needed — backend errors surface automatically.

**Suppressing the global toast** — when a mutation has its own inline error display (e.g. an `<Alert>` in a form), the global toast would duplicate it. Add `meta: { suppressToast: true }` to the `useMutation` call to opt out:

```ts
return useMutation({
  meta: { suppressToast: true },
  mutationFn: ...,
})
```

The type for `meta` is declared in `src/shared/types/tanstack.d.ts` — no extra imports needed.

### Paginated lists
Use `usePaginatedNavigation` + `PaginatedList<T>` for every list page. The hook is generic over sort column names (`TColumn extends string`):

```typescript
const { page, pageSize, orderBy, orderByDesc, goToPage, handlePageSizeChange, handleSortChange, listRef } =
  usePaginatedNavigation<"title" | "createdAtUtc">({
    initialPage, initialPageSize,
    initialOrderBy,      // optional — from SSR searchParams
    initialOrderByDesc,  // optional — from SSR searchParams
  })
```

URL syncs automatically: `?page=1&pageSize=10&orderBy=title&orderByDesc=true`.

**List mode** (`renderItem`) — flat list with optional sort bar above:
```tsx
<PaginatedList
  data={data} isLoading={isLoading}
  page={page} pageSize={pageSize}
  onGoToPage={goToPage} onPageSizeChange={handlePageSizeChange}
  listRef={listRef}
  renderItem={(note) => <NoteCard note={note} />}
  keyExtractor={(note) => note.id}
  emptyState={<p>{t("empty")}</p>}
  sortColumns={[{ key: "title", label: t("title") }]}  // optional sort bar
  orderBy={orderBy} orderByDesc={orderByDesc} onSortChange={handleSortChange}
/>
```

**Table mode** (`columns`) — renders `<table>` with sortable headers, row click, and optional actions column:
```tsx
<PaginatedList
  data={data} isLoading={isLoading}
  page={page} pageSize={pageSize}
  onGoToPage={goToPage} onPageSizeChange={handlePageSizeChange}
  listRef={listRef}
  keyExtractor={(note) => note.id}
  emptyState={<p>{t("empty")}</p>}
  orderBy={orderBy} orderByDesc={orderByDesc} onSortChange={handleSortChange}
  onRowClick={(note) => router.push(`/${locale}/notes/${note.id}`)}
  rowActions={(note) => (
    <button onClick={() => setPendingDeleteId(note.id)}><Trash2 /></button>
  )}
  columns={[
    { key: "title", label: tCommon("sortColumns.title"), sortable: true, render: (note) => note.title },
    { key: "createdAtUtc", label: tCommon("sortColumns.createdAt"), sortable: true,
      headerClassName: "text-right", cellClassName: "text-right text-muted-foreground",
      render: (note) => formatDate(note.createdAtUtc) },
  ]}
/>
```

`rowActions` cell automatically stops click propagation — clicks there never trigger `onRowClick`.

Sort column labels live in `common.sortColumns` (`title`, `createdAt`) — shared across all modules.

**SSR sort support** — page Server Components read `orderBy`/`orderByDesc` from `searchParams`, validate against an allowed list, pass to the API call and as `initialOrderBy`/`initialOrderByDesc` props to the list component.

`PaginatedList` always renders the pagination bar (page size selector 5/10/25/100, prev/next, "Page X of Y").

### Confirm dialogs
Use `ConfirmDialog` from `@/shared/components/ui/confirm-dialog` for delete confirmations — never `window.confirm()`.

### Breadcrumbs
Use `AppBreadcrumbs` from `@/shared/components/layout/AppBreadcrumbs` in page Server Components. It requires `"use client"` internally (already applied). Pass `BreadcrumbEntry[]` where entries with `href` render as links, entries without render as current page.

```typescript
<AppBreadcrumbs items={[
  { label: t("textNotes"), href: `/${locale}/notes` },
  { label: note.title },
]} />
```

### Zod schema factories (i18n)
Form schemas that display validation messages must use factory functions — never hardcode English strings:

```typescript
// schemas/text-note.schema.ts
export function createTextNoteSchema(errors: { titleRequired: string; contentRequired: string }) {
  return z.object({ title: z.string().min(1, { error: errors.titleRequired }) })
}

// component
const schema = useMemo(() => createTextNoteSchema({
  titleRequired: t("validation.titleRequired"),
  contentRequired: t("validation.contentRequired"),
}), [t])
```

---

## Adding a New Module

When a new backend module is ready (Finance, Fitness, Learning):

1. Create `src/modules/{name}/types/{name}.types.ts` — interfaces matching backend DTOs
2. Create `src/modules/{name}/api/{name}.api.ts` — one function per endpoint
3. Create `src/modules/{name}/schemas/` — Zod schemas for forms
4. Create `src/modules/{name}/hooks/` — useQuery/useMutation hooks with query keys
5. Create `src/modules/{name}/components/` — React components
6. Create `src/app/[locale]/(app)/{name}/page.tsx` (and sub-routes)
7. Add translation keys to `messages/en.json` and `messages/pl.json`
8. Add nav link in `src/shared/components/layout/Sidebar.tsx` — **only file to touch in shared**
9. Add to proxy.ts `PROTECTED_SEGMENTS` if the route name differs from module name
