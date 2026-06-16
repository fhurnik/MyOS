"use client"

import { useState } from "react"
import Link from "next/link"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { ListTodo, Trash2 } from "lucide-react"
import { useCheckLists } from "@/modules/notes/hooks/check-lists/useCheckLists"
import { useDeleteCheckList } from "@/modules/notes/hooks/check-lists/useCheckListMutations"
import { PaginatedList } from "@/shared/components/ui/paginated-list"
import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog"
import { usePaginatedNavigation } from "@/shared/hooks/usePaginatedNavigation"
import type { PagingList } from "@/shared/types/api.types"
import type { CheckListSummaryDto } from "@/modules/notes/types/notes.types"
import { formatDate } from "@/shared/lib/format"

type CheckListOrderBy = "title" | "createdAtUtc"

interface CheckListListProps {
  initialData: PagingList<CheckListSummaryDto>
  initialPage: number
  initialPageSize: number
  initialOrderBy?: CheckListOrderBy
  initialOrderByDesc?: boolean
}

export function CheckListList({
  initialData,
  initialPage,
  initialPageSize,
  initialOrderBy,
  initialOrderByDesc,
}: CheckListListProps) {
  const t = useTranslations("notes.checkLists")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"
  const [pendingDeleteId, setPendingDeleteId] = useState<string | null>(null)

  const { page, pageSize, orderBy, orderByDesc, goToPage, handlePageSizeChange, handleSortChange, listRef } =
    usePaginatedNavigation<CheckListOrderBy>({
      initialPage,
      initialPageSize,
      initialOrderBy,
      initialOrderByDesc,
    })

  const { data, isLoading, isError } = useCheckLists({
    params: { page, pageSize, orderBy, orderByDesc },
    initialData:
      page === initialPage &&
      pageSize === initialPageSize &&
      orderBy === initialOrderBy &&
      orderByDesc === (initialOrderByDesc ?? false)
        ? initialData
        : undefined,
  })

  const { mutate: del, isPending: deleting } = useDeleteCheckList()

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
        onRowClick={(list) => router.push(`/${locale}/notes/checklists/${list.id}`)}
        rowActions={(list) => (
          <button
            onClick={() => setPendingDeleteId(list.id)}
            className="rounded p-1 text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
          >
            <Trash2 className="h-4 w-4" />
          </button>
        )}
        keyExtractor={(list) => list.id}
        columns={[
          {
            key: "title",
            label: tCommon("sortColumns.title"),
            sortable: true,
            render: (list) => (
              <Link href={`/${locale}/notes/checklists/${list.id}`} className="font-medium hover:underline">
                {list.title}
              </Link>
            ),
          },
          {
            key: "createdAtUtc",
            label: tCommon("sortColumns.createdAt"),
            sortable: true,
            headerClassName: "text-right",
            cellClassName: "text-right text-muted-foreground whitespace-nowrap",
            render: (list) => formatDate(list.createdAtUtc),
          },
        ]}
        emptyState={
          <div className="flex flex-col items-center gap-3 py-20 text-muted-foreground">
            <ListTodo className="h-10 w-10 opacity-25" />
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
