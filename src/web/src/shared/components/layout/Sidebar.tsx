"use client"

import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import {
  Home,
  FileText,
  ListTodo,
  Settings2,
  GraduationCap,
  Wallet,
  Dumbbell,
  LogOut,
} from "lucide-react"
import { useRequiredSession } from "@/shared/hooks/useSession"
import { cn } from "@/shared/lib/utils"

const NOTES_SUB_LINKS = [
  { href: "/notes", labelKey: "textNotes", Icon: FileText },
  { href: "/notes/checklists", labelKey: "checkLists", Icon: ListTodo },
] as const

const NAV_LINKS = [
  { href: "/settings", labelKey: "settings", Icon: Settings2 },
] as const

const COMING_SOON = [
  { labelKey: "learning", Icon: GraduationCap },
  { labelKey: "finance", Icon: Wallet },
  { labelKey: "fitness", Icon: Dumbbell },
] as const

function getInitials(email: string): string {
  return (email.split("@")[0] ?? "").slice(0, 2).toUpperCase()
}

export function Sidebar() {
  const t = useTranslations("navigation")
  const pathname = usePathname()
  const router = useRouter()
  const session = useRequiredSession()

  const locale = pathname.split("/")[1] ?? "en"
  const isChecklistsPath = pathname.startsWith(`/${locale}/notes/checklists`)
  const isHomePath = pathname === `/${locale}/home`

  async function handleLogout() {
    await fetch("/api/auth/logout", { method: "DELETE" })
    router.replace(`/${locale}/login`)
    router.refresh()
  }

  return (
    <aside className="flex w-56 shrink-0 flex-col border-r bg-card px-3 py-4">
      {/* User header */}
      <div className="mb-6 flex items-center gap-2.5 px-1">
        <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-full bg-primary text-xs font-semibold text-primary-foreground">
          {getInitials(session.email)}
        </div>
        <div className="min-w-0">
          <p className="text-sm font-semibold leading-none">MyOS</p>
          <p className="mt-0.5 truncate text-xs text-muted-foreground">{session.email}</p>
        </div>
      </div>

      <nav className="flex flex-1 flex-col gap-0.5">
        {/* Home */}
        <Link
          href={`/${locale}/home`}
          className={cn(
            "flex items-center gap-2.5 rounded-md border-l-2 px-2 py-1.5 text-sm transition-colors",
            isHomePath
              ? "border-primary bg-primary/10 font-medium text-primary"
              : "border-transparent text-muted-foreground hover:bg-muted hover:text-foreground"
          )}
        >
          <Home className="h-4 w-4 shrink-0" />
          {t("home")}
        </Link>

        <div className="my-1" />

        {/* Notes module with sub-links */}
        <div>
          <p className="px-2 pb-1 pt-0.5 text-xs font-medium uppercase tracking-wider text-muted-foreground/50">
            {t("notes")}
          </p>
          {NOTES_SUB_LINKS.map(({ href, labelKey, Icon }) => {
            const fullPath = `/${locale}${href}`
            const isActive =
              href === "/notes/checklists"
                ? isChecklistsPath
                : pathname.startsWith(`/${locale}/notes`) && !isChecklistsPath
            return (
              <Link
                key={href}
                href={fullPath}
                className={cn(
                  "flex items-center gap-2 rounded-md border-l-2 py-1.5 pl-5 pr-2 text-sm transition-colors",
                  isActive
                    ? "border-primary bg-primary/10 font-medium text-primary"
                    : "border-transparent text-muted-foreground hover:bg-muted hover:text-foreground"
                )}
              >
                <Icon className="h-3.5 w-3.5 shrink-0" />
                {t(labelKey)}
              </Link>
            )
          })}
        </div>

        <div className="my-1" />

        {/* Top-level links */}
        {NAV_LINKS.map(({ href, labelKey, Icon }) => {
          const fullPath = `/${locale}${href}`
          const isActive = pathname === fullPath || pathname.startsWith(fullPath + "/")
          return (
            <Link
              key={href}
              href={fullPath}
              className={cn(
                "flex items-center gap-2.5 rounded-md border-l-2 px-2 py-1.5 text-sm transition-colors",
                isActive
                  ? "border-primary bg-primary/10 font-medium text-primary"
                  : "border-transparent text-muted-foreground hover:bg-muted hover:text-foreground"
              )}
            >
              <Icon className="h-4 w-4 shrink-0" />
              {t(labelKey)}
            </Link>
          )
        })}

        <div className="my-2 border-t" />

        {COMING_SOON.map(({ labelKey, Icon }) => (
          <div
            key={labelKey}
            className="flex items-center gap-2.5 rounded-md border-l-2 border-transparent px-2 py-1.5 text-sm text-muted-foreground/50"
          >
            <Icon className="h-4 w-4 shrink-0" />
            <span className="flex-1">{t(labelKey)}</span>
            <span className="rounded bg-muted px-1.5 py-0.5 text-xs text-muted-foreground/60">
              soon
            </span>
          </div>
        ))}
      </nav>

      <button
        onClick={handleLogout}
        className="mt-auto flex items-center gap-2.5 rounded-md px-2 py-1.5 text-left text-sm text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
      >
        <LogOut className="h-4 w-4 shrink-0" />
        {t("logout")}
      </button>
    </aside>
  )
}
