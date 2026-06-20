"use client"

import { useState } from "react"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { CalendarDays, Trash2 } from "lucide-react"
import { useWorkouts } from "@/modules/fitness/hooks/workouts/useWorkouts"
import { useDeleteWorkout } from "@/modules/fitness/hooks/workouts/useWorkoutMutations"
import { PaginatedList } from "@/shared/components/ui/paginated-list"
import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog"
import { usePaginatedNavigation } from "@/shared/hooks/usePaginatedNavigation"
import { formatDate } from "@/shared/lib/format"
import type { PagingList } from "@/shared/types/api.types"
import type { WorkoutSummaryDto } from "@/modules/fitness/types/fitness.types"

type WorkoutOrderBy = "date" | "createdAtUtc"

interface WorkoutListProps {
  initialData: PagingList<WorkoutSummaryDto>
  initialPage: number
  initialPageSize: number
  initialOrderBy?: WorkoutOrderBy
  initialOrderByDesc?: boolean
}

export function WorkoutList({
  initialData,
  initialPage,
  initialPageSize,
  initialOrderBy,
  initialOrderByDesc,
}: WorkoutListProps) {
  const t = useTranslations("fitness.workouts")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"
  const [pendingDeleteId, setPendingDeleteId] = useState<string | null>(null)

  const { page, pageSize, orderBy, orderByDesc, goToPage, handlePageSizeChange, handleSortChange, listRef } =
    usePaginatedNavigation<WorkoutOrderBy>({
      initialPage,
      initialPageSize,
      initialOrderBy,
      initialOrderByDesc,
    })

  const { data, isLoading, isError } = useWorkouts({
    params: { page, pageSize, orderBy, orderByDesc },
    initialData:
      page === initialPage &&
      pageSize === initialPageSize &&
      orderBy === initialOrderBy &&
      orderByDesc === (initialOrderByDesc ?? false)
        ? initialData
        : undefined,
  })

  const { mutate: del, isPending: deleting } = useDeleteWorkout()

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
        onRowClick={(w) => router.push(`/${locale}/fitness/workouts/${w.id}`)}
        rowActions={(w) => (
          <button
            onClick={() => setPendingDeleteId(w.id)}
            className="rounded p-1 text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
          >
            <Trash2 className="h-4 w-4" />
          </button>
        )}
        keyExtractor={(w) => w.id}
        columns={[
          {
            key: "date",
            label: t("dateLabel"),
            sortable: true,
            render: (w) => <span className="font-medium">{formatDate(w.date)}</span>,
          },
          {
            key: "notes",
            label: t("notesLabel"),
            cellClassName: "text-muted-foreground",
            render: (w) => <span className="line-clamp-1">{w.notes ?? "—"}</span>,
          },
          {
            key: "createdAtUtc",
            label: tCommon("sortColumns.createdAt"),
            sortable: true,
            headerClassName: "text-right",
            cellClassName: "text-right text-muted-foreground whitespace-nowrap",
            render: (w) => formatDate(w.createdAtUtc),
          },
        ]}
        emptyState={
          <div className="flex flex-col items-center gap-3 py-20 text-muted-foreground">
            <CalendarDays className="h-10 w-10 opacity-25" />
            <p className="text-sm">{t("empty")}</p>
          </div>
        }
      />

      <ConfirmDialog
        open={pendingDeleteId !== null}
        onOpenChange={(open) => {
          if (!open) setPendingDeleteId(null)
        }}
        title={t("deleteConfirm")}
        onConfirm={() => del(pendingDeleteId!, { onSuccess: () => setPendingDeleteId(null) })}
        isPending={deleting}
      />
    </>
  )
}
