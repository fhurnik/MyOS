import { getTranslations } from "next-intl/server"
import { getWorkoutsApi } from "@/modules/fitness/api/workouts.api"
import { getServerToken } from "@/shared/lib/session"
import { WorkoutList } from "@/modules/fitness/components/workouts/WorkoutList"
import { CreateWorkoutButton } from "@/modules/fitness/components/workouts/CreateWorkoutButton"
import { AppBreadcrumbs } from "@/shared/components/layout/AppBreadcrumbs"

const VALID_ORDER_BY = ["date", "createdAtUtc"] as const
type WorkoutOrderBy = (typeof VALID_ORDER_BY)[number]

interface PageProps {
  params: Promise<{ locale: string }>
  searchParams: Promise<{ page?: string; pageSize?: string; orderBy?: string; orderByDesc?: string }>
}

export default async function WorkoutsPage({ params, searchParams }: PageProps) {
  await params
  const { page, pageSize, orderBy, orderByDesc } = await searchParams
  const pageNum = Math.max(1, parseInt(page ?? "1", 10))
  const pageSizeNum = [5, 10, 25, 100].includes(parseInt(pageSize ?? "", 10))
    ? parseInt(pageSize!, 10)
    : 10
  // Default order: date desc.
  const hasOrder = VALID_ORDER_BY.includes(orderBy as WorkoutOrderBy)
  const orderByParam: WorkoutOrderBy = hasOrder ? (orderBy as WorkoutOrderBy) : "date"
  const orderByDescParam = hasOrder ? orderByDesc === "true" : true

  const tNav = await getTranslations("navigation")
  const t = await getTranslations("fitness.workouts")
  const token = await getServerToken()
  const initialData = await getWorkoutsApi(
    { page: pageNum, pageSize: pageSizeNum, orderBy: orderByParam, orderByDesc: orderByDescParam || undefined },
    token ?? undefined
  )

  return (
    <div className="space-y-6">
      <AppBreadcrumbs items={[{ label: tNav("fitness") }, { label: tNav("fitnessWorkouts") }]} />
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold tracking-tight">{t("title")}</h1>
        <CreateWorkoutButton />
      </div>
      <WorkoutList
        initialData={initialData}
        initialPage={pageNum}
        initialPageSize={pageSizeNum}
        initialOrderBy={orderByParam}
        initialOrderByDesc={orderByDescParam}
      />
    </div>
  )
}
