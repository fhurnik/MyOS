import { FileText, ListTodo, Settings2, GraduationCap, Wallet, Dumbbell } from "lucide-react"

export const NOTES_SUB_LINKS = [
  { href: "/notes", labelKey: "textNotes", Icon: FileText },
  { href: "/notes/checklists", labelKey: "checkLists", Icon: ListTodo },
] as const

export const NAV_LINKS = [
  { href: "/settings", labelKey: "settings", Icon: Settings2 },
] as const

export const COMING_SOON = [
  { labelKey: "learning", Icon: GraduationCap },
  { labelKey: "finance", Icon: Wallet },
  { labelKey: "fitness", Icon: Dumbbell },
] as const

export function getInitials(email: string): string {
  return (email.split("@")[0] ?? "").slice(0, 2).toUpperCase()
}
