import { getTranslations } from "next-intl/server"
import {
  getExerciseApi,
  getExerciseProgressionApi,
} from "@/modules/fitness/api/exercises.api"
import { getServerToken } from "@/shared/lib/session"
import { ExerciseDetail } from "@/modules/fitness/components/exercises/ExerciseDetail"
import { AppBreadcrumbs } from "@/shared/components/layout/AppBreadcrumbs"

type Props = { params: Promise<{ locale: string; id: string }> }

export default async function ExerciseDetailPage({ params }: Props) {
  const { locale, id } = await params
  const token = await getServerToken()
  const [exercise, progression] = await Promise.all([
    getExerciseApi(id, token ?? undefined),
    getExerciseProgressionApi(id, token ?? undefined),
  ])
  const tNav = await getTranslations("navigation")

  return (
    <div className="space-y-6">
      <AppBreadcrumbs
        items={[
          { label: tNav("fitnessExercises"), href: `/${locale}/fitness/exercises` },
          { label: exercise.name },
        ]}
      />
      <ExerciseDetail id={id} initialExercise={exercise} initialProgression={progression} />
    </div>
  )
}
