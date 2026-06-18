import {
  Home,
  FileText,
  ListTodo,
  NotebookText,
  HardDrive,
  Settings2,
  GraduationCap,
  Wallet,
  Dumbbell,
  type LucideIcon,
} from "lucide-react"

export interface NavLink {
  href: string
  labelKey: string
  Icon: LucideIcon
}

export interface NavModule extends NavLink {
  subLinks: readonly NavLink[]
}

// Utility links shown above the Modules section.
export const TOP_LINKS: readonly NavLink[] = [
  { href: "/home", labelKey: "home", Icon: Home },
  { href: "/settings", labelKey: "settings", Icon: Settings2 },
]

// Active modules — each rendered uniformly (icon + name, with optional indented sub-links).
// Adding a new module's nav entry means adding an item here; both Sidebar and MobileNav read it.
export const MODULES: readonly NavModule[] = [
  {
    href: "/notes",
    labelKey: "notes",
    Icon: NotebookText,
    subLinks: [
      { href: "/notes", labelKey: "textNotes", Icon: FileText },
      { href: "/notes/checklists", labelKey: "checkLists", Icon: ListTodo },
    ],
  },
  {
    href: "/storage",
    labelKey: "storage",
    Icon: HardDrive,
    subLinks: [],
  },
]

export const COMING_SOON = [
  { labelKey: "learning", Icon: GraduationCap },
  { labelKey: "finance", Icon: Wallet },
  { labelKey: "fitness", Icon: Dumbbell },
] as const

export function isLinkActive(href: string, pathname: string, locale: string): boolean {
  const full = `/${locale}${href}`
  return pathname === full || pathname.startsWith(`${full}/`)
}

// The sub-link whose path is the longest active prefix (so /notes/checklists wins over /notes).
export function activeSubLinkHref(
  subLinks: readonly NavLink[],
  pathname: string,
  locale: string
): string | null {
  let best: string | null = null
  for (const link of subLinks) {
    if (isLinkActive(link.href, pathname, locale) && link.href.length > (best?.length ?? -1)) {
      best = link.href
    }
  }
  return best
}

export function getInitials(email: string): string {
  return (email.split("@")[0] ?? "").slice(0, 2).toUpperCase()
}
