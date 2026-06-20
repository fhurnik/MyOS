"use client"

import { useState } from "react"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { Dumbbell, Trash2 } from "lucide-react"
import { useExercises } from "@/modules/fitness/hooks/exercises/useExercises"
import { useDeleteExercise } from "@/modules/fitness/hooks/exercises/useExerciseMutations"
import { PaginatedList } from "@/shared/components/ui/paginated-list"
import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog"
import { usePaginatedNavigation } from "@/shared/hooks/usePaginatedNavigation"
import { cn } from "@/shared/lib/utils"
import { formatDate } from "@/shared/lib/format"
import { formatDistance } from "@/modules/fitness/lib/fitness-format"
import type { PagingList } from "@/shared/types/api.types"
import type {
  ExerciseDto,
  ExerciseFilterParams,
} from "@/modules/fitness/types/fitness.types"

type ExerciseOrderBy = "name" | "createdAtUtc"
type FilterKey = "all" | "cardio" | "strength" | "weighted" | "bodyweight"

const FILTER_KEYS: FilterKey[] = ["all", "cardio", "strength", "weighted", "bodyweight"]

function filterToParams(key: FilterKey): ExerciseFilterParams {
  switch (key) {
    case "cardio":
      return { activityType: "cardio" }
    case "strength":
      return { activityType: "strength" }
    case "weighted":
      return { activityType: "strength", strengthCategory: "weighted" }
    case "bodyweight":
      return { activityType: "strength", strengthCategory: "bodyweight" }
    default:
      return {}
  }
}

interface ExerciseListProps {
  initialData: PagingList<ExerciseDto>
  initialPage: number
  initialPageSize: number
  initialOrderBy?: ExerciseOrderBy
  initialOrderByDesc?: boolean
}

export function ExerciseList({
  initialData,
  initialPage,
  initialPageSize,
  initialOrderBy,
  initialOrderByDesc,
}: ExerciseListProps) {
  const t = useTranslations("fitness.exercises")
  const tEnum = useTranslations("fitness.exercises.enums")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"
  const [filter, setFilter] = useState<FilterKey>("all")
  const [pendingDeleteId, setPendingDeleteId] = useState<string | null>(null)

  const { page, pageSize, orderBy, orderByDesc, goToPage, handlePageSizeChange, handleSortChange, listRef } =
    usePaginatedNavigation<ExerciseOrderBy>({
      initialPage,
      initialPageSize,
      initialOrderBy,
      initialOrderByDesc,
    })

  const filterParams = filterToParams(filter)
  const isDefaultView =
    filter === "all" &&
    page === initialPage &&
    pageSize === initialPageSize &&
    orderBy === initialOrderBy &&
    orderByDesc === (initialOrderByDesc ?? false)

  const { data, isLoading, isError } = useExercises({
    params: { page, pageSize, orderBy, orderByDesc, ...filterParams },
    initialData: isDefaultView ? initialData : undefined,
  })

  const { mutate: del, isPending: deleting } = useDeleteExercise()

  function typeLabel(ex: ExerciseDto): string {
    if (ex.activityType === "cardio") {
      const dist = ex.distance != null ? ` · ${formatDistance(ex.distance)}` : ""
      return `${tEnum("activityType.cardio")}${dist}`
    }
    const cat = ex.strengthCategory ? ` · ${tEnum(`strengthCategory.${ex.strengthCategory}`)}` : ""
    return `${tEnum("activityType.strength")}${cat}`
  }

  return (
    <>
      {/* Filter chips */}
      <div className="flex flex-wrap gap-2">
        {FILTER_KEYS.map((key) => (
          <button
            key={key}
            onClick={() => {
              setFilter(key)
              goToPage(1)
            }}
            className={cn(
              "rounded-full border px-3 py-1 text-sm transition-colors",
              filter === key
                ? "border-primary bg-primary/10 font-medium text-primary"
                : "border-input text-muted-foreground hover:bg-accent"
            )}
          >
            {t(`filters.${key}`)}
          </button>
        ))}
      </div>

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
        onRowClick={(ex) => router.push(`/${locale}/fitness/exercises/${ex.id}`)}
        rowActions={(ex) => (
          <button
            onClick={() => setPendingDeleteId(ex.id)}
            className="rounded p-1 text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
          >
            <Trash2 className="h-4 w-4" />
          </button>
        )}
        keyExtractor={(ex) => ex.id}
        columns={[
          {
            key: "name",
            label: t("nameLabel"),
            sortable: true,
            render: (ex) => <span className="font-medium">{ex.name}</span>,
          },
          {
            key: "activityType",
            label: t("typeLabel"),
            render: (ex) => <span className="text-muted-foreground">{typeLabel(ex)}</span>,
          },
          {
            key: "createdAtUtc",
            label: tCommon("sortColumns.createdAt"),
            sortable: true,
            headerClassName: "text-right",
            cellClassName: "text-right text-muted-foreground whitespace-nowrap",
            render: (ex) => formatDate(ex.createdAtUtc),
          },
        ]}
        emptyState={
          <div className="flex flex-col items-center gap-3 py-20 text-muted-foreground">
            <Dumbbell className="h-10 w-10 opacity-25" />
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
