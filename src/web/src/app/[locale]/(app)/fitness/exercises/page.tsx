import { getTranslations } from "next-intl/server"
import { getExercisesApi } from "@/modules/fitness/api/exercises.api"
import { getServerToken } from "@/shared/lib/session"
import { ExerciseList } from "@/modules/fitness/components/exercises/ExerciseList"
import { CreateExerciseButton } from "@/modules/fitness/components/exercises/CreateExerciseButton"
import { AppBreadcrumbs } from "@/shared/components/layout/AppBreadcrumbs"

const VALID_ORDER_BY = ["name", "createdAtUtc"] as const
type ExerciseOrderBy = (typeof VALID_ORDER_BY)[number]

interface PageProps {
  params: Promise<{ locale: string }>
  searchParams: Promise<{ page?: string; pageSize?: string; orderBy?: string; orderByDesc?: string }>
}

export default async function ExercisesPage({ params, searchParams }: PageProps) {
  await params
  const { page, pageSize, orderBy, orderByDesc } = await searchParams
  const pageNum = Math.max(1, parseInt(page ?? "1", 10))
  const pageSizeNum = [5, 10, 25, 100].includes(parseInt(pageSize ?? "", 10))
    ? parseInt(pageSize!, 10)
    : 10
  const orderByParam = VALID_ORDER_BY.includes(orderBy as ExerciseOrderBy)
    ? (orderBy as ExerciseOrderBy)
    : undefined
  const orderByDescParam = orderByDesc === "true"

  const tNav = await getTranslations("navigation")
  const t = await getTranslations("fitness.exercises")
  const token = await getServerToken()
  const initialData = await getExercisesApi(
    { page: pageNum, pageSize: pageSizeNum, orderBy: orderByParam, orderByDesc: orderByDescParam || undefined },
    token ?? undefined
  )

  return (
    <div className="space-y-6">
      <AppBreadcrumbs items={[{ label: tNav("fitness") }, { label: tNav("fitnessExercises") }]} />
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold tracking-tight">{t("title")}</h1>
        <CreateExerciseButton />
      </div>
      <ExerciseList
        initialData={initialData}
        initialPage={pageNum}
        initialPageSize={pageSizeNum}
        initialOrderBy={orderByParam}
        initialOrderByDesc={orderByDescParam}
      />
    </div>
  )
}
