"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { useTranslations } from "next-intl"
import { ListTodo } from "lucide-react"
import { useCheckLists } from "@/modules/notes/hooks/check-lists/useCheckLists"
import type { PagingList } from "@/shared/types/api.types"
import type { CheckListSummaryDto } from "@/modules/notes/types/notes.types"
import { CheckListCard } from "./CheckListCard"

interface CheckListListProps {
  initialData: PagingList<CheckListSummaryDto>
}

export function CheckListList({ initialData }: CheckListListProps) {
  const t = useTranslations("notes.checkLists")
  const pathname = usePathname()
  const locale = pathname.split("/")[1] ?? "en"

  const { data, isLoading } = useCheckLists({ initialData })

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">…</p>
  }

  if (!data?.items.length) {
    return (
      <div className="flex flex-col items-center gap-3 py-20 text-muted-foreground">
        <ListTodo className="h-10 w-10 opacity-25" />
        <p className="text-sm">{t("empty")}</p>
      </div>
    )
  }

  return (
    <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
      {data.items.map((list) => (
        <Link key={list.id} href={`/${locale}/notes/checklists/${list.id}`}>
          <CheckListCard list={list} />
        </Link>
      ))}
    </div>
  )
}
