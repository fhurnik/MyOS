"use client"

import { useState } from "react"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { Pencil, Target } from "lucide-react"
import { useExercise } from "@/modules/fitness/hooks/exercises/useExercise"
import { useExerciseProgression } from "@/modules/fitness/hooks/exercises/useExerciseProgression"
import { useDeleteExercise } from "@/modules/fitness/hooks/exercises/useExerciseMutations"
import { ExerciseFormDialog } from "./ExerciseFormDialog"
import { SetTargetDialog } from "./SetTargetDialog"
import { ProgressionChart } from "@/modules/fitness/components/stats/ProgressionChart"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card"
import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog"
import {
  progressionUnit,
  formatProgressionValue,
  formatDistance,
} from "@/modules/fitness/lib/fitness-format"
import type { ExerciseDto, ProgressionDto } from "@/modules/fitness/types/fitness.types"

interface ExerciseDetailProps {
  id: string
  initialExercise: ExerciseDto
  initialProgression?: ProgressionDto
}

export function ExerciseDetail({ id, initialExercise, initialProgression }: ExerciseDetailProps) {
  const t = useTranslations("fitness.exercises")
  const tEnum = useTranslations("fitness.exercises.enums")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"

  const [editOpen, setEditOpen] = useState(false)
  const [targetOpen, setTargetOpen] = useState(false)
  const [confirmOpen, setConfirmOpen] = useState(false)

  const { data: exercise } = useExercise(id, initialExercise)
  const { data: progression } = useExerciseProgression(id, initialProgression)
  const { mutate: del, isPending: deleting } = useDeleteExercise()

  if (!exercise) return null

  const unit = progressionUnit(exercise.activityType, exercise.strengthCategory)
  const typeText =
    exercise.activityType === "cardio"
      ? `${tEnum("activityType.cardio")}${exercise.distance != null ? ` · ${formatDistance(exercise.distance)}` : ""}`
      : `${tEnum("activityType.strength")}${exercise.strengthCategory ? ` · ${tEnum(`strengthCategory.${exercise.strengthCategory}`)}` : ""}`

  const targetValue = progression?.targetValue ?? null

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">{exercise.name}</h1>
          <p className="mt-1 text-sm text-muted-foreground">{typeText}</p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm" onClick={() => setEditOpen(true)}>
            <Pencil className="h-4 w-4" />
            {tCommon("edit")}
          </Button>
          <Button variant="destructive" size="sm" onClick={() => setConfirmOpen(true)} disabled={deleting}>
            {tCommon("delete")}
          </Button>
        </div>
      </div>

      {/* Target */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="flex items-center gap-2">
            <Target className="h-4 w-4 text-muted-foreground" />
            {t("targetLabel")}
          </CardTitle>
          <Button variant="outline" size="sm" onClick={() => setTargetOpen(true)}>
            {t("setTarget")}
          </Button>
        </CardHeader>
        <CardContent>
          <p className="text-lg font-medium">
            {targetValue != null ? formatProgressionValue(targetValue, unit) : (
              <span className="text-sm font-normal text-muted-foreground">{t("noTarget")}</span>
            )}
          </p>
        </CardContent>
      </Card>

      {/* Progression */}
      <Card>
        <CardHeader>
          <CardTitle>{t("progression")}</CardTitle>
        </CardHeader>
        <CardContent>
          {progression && progression.points.some((p) => p.value != null) ? (
            <ProgressionChart exercise={exercise} progression={progression} />
          ) : (
            <p className="py-12 text-center text-sm text-muted-foreground">{t("progressionEmpty")}</p>
          )}
        </CardContent>
      </Card>

      <ExerciseFormDialog open={editOpen} onOpenChange={setEditOpen} exercise={exercise} />
      <SetTargetDialog
        open={targetOpen}
        onOpenChange={setTargetOpen}
        exerciseId={id}
        currentValue={targetValue}
      />
      <ConfirmDialog
        open={confirmOpen}
        onOpenChange={setConfirmOpen}
        title={t("deleteConfirm")}
        onConfirm={() => del(id, { onSuccess: () => router.replace(`/${locale}/fitness/exercises`) })}
        isPending={deleting}
      />
    </div>
  )
}
