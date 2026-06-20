"use client"

import { useState } from "react"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { Pencil, Plus } from "lucide-react"
import { useWorkout } from "@/modules/fitness/hooks/workouts/useWorkout"
import { useDeleteWorkout } from "@/modules/fitness/hooks/workouts/useWorkoutMutations"
import { WorkoutFormDialog } from "./WorkoutFormDialog"
import { AddExerciseDialog } from "./AddExerciseDialog"
import { WorkoutExerciseEntry } from "./WorkoutExerciseEntry"
import { Button } from "@/shared/components/ui/button"
import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog"
import { formatDate } from "@/shared/lib/format"
import type { WorkoutDto } from "@/modules/fitness/types/fitness.types"

interface WorkoutDetailProps {
  id: string
  initialData: WorkoutDto
}

export function WorkoutDetail({ id, initialData }: WorkoutDetailProps) {
  const t = useTranslations("fitness.workouts")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"

  const [editOpen, setEditOpen] = useState(false)
  const [addOpen, setAddOpen] = useState(false)
  const [confirmOpen, setConfirmOpen] = useState(false)

  const { data: workout } = useWorkout(id, initialData)
  const { mutate: del, isPending: deleting } = useDeleteWorkout()

  if (!workout) return null

  const entries = [...workout.exercises].sort((a, b) => a.position - b.position)

  return (
    <div className="space-y-6">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-start sm:justify-between">
        <div>
          <h1 className="text-2xl font-semibold tracking-tight">{formatDate(workout.date)}</h1>
          {workout.notes && <p className="mt-1 text-sm text-muted-foreground">{workout.notes}</p>}
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

      <div className="flex justify-end">
        <Button size="sm" onClick={() => setAddOpen(true)}>
          <Plus className="h-4 w-4" />
          {t("addExercise")}
        </Button>
      </div>

      {entries.length === 0 ? (
        <p className="rounded-lg border border-dashed py-16 text-center text-sm text-muted-foreground">
          {t("entryEmpty")}
        </p>
      ) : (
        <div className="space-y-3">
          {entries.map((entry) => (
            <WorkoutExerciseEntry key={entry.id} workoutId={id} entry={entry} />
          ))}
        </div>
      )}

      <WorkoutFormDialog open={editOpen} onOpenChange={setEditOpen} workout={workout} />
      <AddExerciseDialog open={addOpen} onOpenChange={setAddOpen} workoutId={id} />
      <ConfirmDialog
        open={confirmOpen}
        onOpenChange={setConfirmOpen}
        title={t("deleteConfirm")}
        onConfirm={() => del(id, { onSuccess: () => router.replace(`/${locale}/fitness/workouts`) })}
        isPending={deleting}
      />
    </div>
  )
}
