"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { useTranslations } from "next-intl"
import { FileText } from "lucide-react"
import { useTextNotes } from "@/modules/notes/hooks/text-notes/useTextNotes"
import type { PagingList } from "@/shared/types/api.types"
import type { TextNoteDto } from "@/modules/notes/types/notes.types"
import { TextNoteCard } from "./TextNoteCard"

interface TextNoteListProps {
  initialData: PagingList<TextNoteDto>
}

export function TextNoteList({ initialData }: TextNoteListProps) {
  const t = useTranslations("notes.textNotes")
  const pathname = usePathname()
  const locale = pathname.split("/")[1] ?? "en"

  const { data, isLoading } = useTextNotes({ initialData })

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">…</p>
  }

  if (!data?.items.length) {
    return (
      <div className="flex flex-col items-center gap-3 py-20 text-muted-foreground">
        <FileText className="h-10 w-10 opacity-25" />
        <p className="text-sm">{t("empty")}</p>
      </div>
    )
  }

  return (
    <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
      {data.items.map((note) => (
        <Link key={note.id} href={`/${locale}/notes/${note.id}`}>
          <TextNoteCard note={note} />
        </Link>
      ))}
    </div>
  )
}
