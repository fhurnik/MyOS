import { getTranslations } from "next-intl/server"
import { getWorkoutApi } from "@/modules/fitness/api/workouts.api"
import { getServerToken } from "@/shared/lib/session"
import { WorkoutDetail } from "@/modules/fitness/components/workouts/WorkoutDetail"
import { AppBreadcrumbs } from "@/shared/components/layout/AppBreadcrumbs"
import { formatDate } from "@/shared/lib/format"

type Props = { params: Promise<{ locale: string; id: string }> }

export default async function WorkoutDetailPage({ params }: Props) {
  const { locale, id } = await params
  const token = await getServerToken()
  const workout = await getWorkoutApi(id, token ?? undefined)
  const tNav = await getTranslations("navigation")

  return (
    <div className="space-y-6">
      <AppBreadcrumbs
        items={[
          { label: tNav("fitnessWorkouts"), href: `/${locale}/fitness/workouts` },
          { label: formatDate(workout.date) },
        ]}
      />
      <WorkoutDetail id={id} initialData={workout} />
    </div>
  )
}
