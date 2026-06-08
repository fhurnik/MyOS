"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { useTranslations } from "next-intl"
import { FileText, ListTodo } from "lucide-react"
import { cn } from "@/shared/lib/utils"

export function NotesTabBar() {
  const t = useTranslations("navigation")
  const pathname = usePathname()
  const locale = pathname.split("/")[1] ?? "en"

  const isChecklists = pathname.startsWith(`/${locale}/notes/checklists`)

  const tabs = [
    { href: `/${locale}/notes`, label: t("textNotes"), Icon: FileText, active: !isChecklists },
    { href: `/${locale}/notes/checklists`, label: t("checkLists"), Icon: ListTodo, active: isChecklists },
  ]

  return (
    <div className="flex gap-1 border-b">
      {tabs.map(({ href, label, Icon, active }) => (
        <Link
          key={href}
          href={href}
          className={cn(
            "-mb-px flex items-center gap-1.5 border-b-2 px-1 pb-2.5 text-sm transition-colors",
            active
              ? "border-primary font-medium text-primary"
              : "border-transparent text-muted-foreground hover:text-foreground"
          )}
        >
          <Icon className="h-3.5 w-3.5" />
          {label}
        </Link>
      ))}
    </div>
  )
}
