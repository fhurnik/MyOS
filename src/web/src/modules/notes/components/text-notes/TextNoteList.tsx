"use client"

import { useState } from "react"
import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { FileText, Trash2 } from "lucide-react"
import { useTextNotes } from "@/modules/notes/hooks/text-notes/useTextNotes"
import { useDeleteTextNote } from "@/modules/notes/hooks/text-notes/useDeleteTextNote"
import { PaginatedList } from "@/shared/components/ui/paginated-list"
import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog"
import { usePaginatedNavigation } from "@/shared/hooks/usePaginatedNavigation"
import type { PagingList } from "@/shared/types/api.types"
import type { TextNoteDto } from "@/modules/notes/types/notes.types"
import { formatDate } from "@/shared/lib/format"

type TextNoteOrderBy = "title" | "createdAtUtc"

interface TextNoteListProps {
  initialData: PagingList<TextNoteDto>
  initialPage: number
  initialPageSize: number
  initialOrderBy?: TextNoteOrderBy
  initialOrderByDesc?: boolean
}

export function TextNoteList({
  initialData,
  initialPage,
  initialPageSize,
  initialOrderBy,
  initialOrderByDesc,
}: TextNoteListProps) {
  const t = useTranslations("notes.textNotes")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"
  const [pendingDeleteId, setPendingDeleteId] = useState<string | null>(null)

  const { page, pageSize, orderBy, orderByDesc, goToPage, handlePageSizeChange, handleSortChange, listRef } =
    usePaginatedNavigation<TextNoteOrderBy>({
      initialPage,
      initialPageSize,
      initialOrderBy,
      initialOrderByDesc,
    })

  const { data, isLoading, isError } = useTextNotes({
    params: { page, pageSize, orderBy, orderByDesc },
    initialData:
      page === initialPage &&
      pageSize === initialPageSize &&
      orderBy === initialOrderBy &&
      orderByDesc === (initialOrderByDesc ?? false)
        ? initialData
        : undefined,
  })

  const { mutate: del, isPending: deleting } = useDeleteTextNote()

  return (
    <>
      <PaginatedList
        data={data}
        isLoading={isLoading}
        isError={isError}
        page={page}
        pageSize={pageSize}
        onGoToPage={goToPage}
        onPageSizeChange={handlePageSizeChange}
        listRef={listRef}
        orderBy={orderBy}
        orderByDesc={orderByDesc}
        onSortChange={handleSortChange}
        onRowClick={(note) => router.push(`/${locale}/notes/${note.id}`)}
        rowActions={(note) => (
          <button
            onClick={() => setPendingDeleteId(note.id)}
            className="rounded p-1 text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
          >
            <Trash2 className="h-4 w-4" />
          </button>
        )}
        keyExtractor={(note) => note.id}
        columns={[
          {
            key: "title",
            label: tCommon("sortColumns.title"),
            sortable: true,
            render: (note) => (
              <Link href={`/${locale}/notes/${note.id}`} className="font-medium hover:underline">
                {note.title}
              </Link>
            ),
          },
          {
            key: "createdAtUtc",
            label: tCommon("sortColumns.createdAt"),
            sortable: true,
            headerClassName: "text-right",
            cellClassName: "text-right text-muted-foreground whitespace-nowrap",
            render: (note) => formatDate(note.createdAtUtc),
          },
        ]}
        emptyState={
          <div className="flex flex-col items-center gap-3 py-20 text-muted-foreground">
            <FileText className="h-10 w-10 opacity-25" />
            <p className="text-sm">{t("empty")}</p>
          </div>
        }
      />
      <ConfirmDialog
        open={pendingDeleteId !== null}
        onOpenChange={(open) => { if (!open) setPendingDeleteId(null) }}
        title={t("deleteConfirm")}
        onConfirm={() => del(pendingDeleteId!, { onSuccess: () => setPendingDeleteId(null) })}
        isPending={deleting}
      />
    </>
  )
}
