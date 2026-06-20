"use client"

import { useState } from "react"
import { useTranslations } from "next-intl"
import { Pencil, Plus, Trash2 } from "lucide-react"
import { useWorkoutEntryMutations } from "@/modules/fitness/hooks/workouts/useWorkoutEntryMutations"
import { SetFormDialog } from "./SetFormDialog"
import { DurationDialog } from "./DurationDialog"
import { Button } from "@/shared/components/ui/button"
import { Card, CardContent, CardHeader } from "@/shared/components/ui/card"
import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog"
import { formatDuration, formatWeight } from "@/modules/fitness/lib/fitness-format"
import type {
  WorkoutExerciseDto,
  ExerciseSetDto,
} from "@/modules/fitness/types/fitness.types"

interface WorkoutExerciseEntryProps {
  workoutId: string
  entry: WorkoutExerciseDto
}

export function WorkoutExerciseEntry({ workoutId, entry }: WorkoutExerciseEntryProps) {
  const t = useTranslations("fitness.workouts")
  const tSets = useTranslations("fitness.sets")
  const tEnum = useTranslations("fitness.exercises.enums")

  const { removeExercise, removeSet } = useWorkoutEntryMutations(workoutId)

  const [setDialogOpen, setSetDialogOpen] = useState(false)
  const [editingSet, setEditingSet] = useState<ExerciseSetDto | undefined>(undefined)
  const [durationOpen, setDurationOpen] = useState(false)
  const [confirmRemoveEntry, setConfirmRemoveEntry] = useState(false)
  const [pendingDeleteSetId, setPendingDeleteSetId] = useState<string | null>(null)

  const isCardio = entry.activityType === "cardio"
  const category = entry.strengthCategory

  const typeText = isCardio
    ? tEnum("activityType.cardio")
    : `${tEnum("activityType.strength")}${category ? ` · ${tEnum(`strengthCategory.${category}`)}` : ""}`

  function setSummary(set: ExerciseSetDto): string {
    if (category === "weighted") {
      return `${set.reps} × ${formatWeight(set.weight ?? 0)}`
    }
    // bodyweight
    const extras: string[] = []
    if (set.addedWeight != null) extras.push(`+${formatWeight(set.addedWeight)}`)
    if (set.negatives != null) extras.push(`${set.negatives} ${tSets("negativesLabel").toLowerCase()}`)
    const suffix = extras.length ? ` (${extras.join(", ")})` : ""
    return `${set.reps} ${tSets("repsLabel").toLowerCase()}${suffix}`
  }

  function openAddSet() {
    setEditingSet(undefined)
    setSetDialogOpen(true)
  }

  function openEditSet(set: ExerciseSetDto) {
    setEditingSet(set)
    setSetDialogOpen(true)
  }

  return (
    <Card>
      <CardHeader className="flex flex-row items-start justify-between">
        <div>
          <p className="font-medium">
            <span className="text-muted-foreground">{entry.position}.</span> {entry.exerciseName}
          </p>
          <p className="text-xs text-muted-foreground">{typeText}</p>
        </div>
        <button
          onClick={() => setConfirmRemoveEntry(true)}
          className="rounded p-1 text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
        >
          <Trash2 className="h-4 w-4" />
        </button>
      </CardHeader>

      <CardContent className="space-y-3">
        {isCardio ? (
          <div className="flex items-center justify-between">
            <span className="text-sm">
              {t("durationLabel")}: <span className="font-medium">{formatDuration(entry.duration)}</span>
            </span>
            <Button variant="outline" size="sm" onClick={() => setDurationOpen(true)}>
              <Pencil className="h-3.5 w-3.5" />
              {t("editDuration")}
            </Button>
          </div>
        ) : (
          <>
            {entry.sets.length === 0 ? (
              <p className="text-sm text-muted-foreground">{tSets("empty")}</p>
            ) : (
              <ul className="divide-y">
                {entry.sets.map((set) => (
                  <li key={set.id} className="flex items-center justify-between py-1.5 text-sm">
                    <span>
                      <span className="text-muted-foreground">{tSets("set")} {set.position}:</span>{" "}
                      {setSummary(set)}
                      {set.rir != null && (
                        <span className="text-muted-foreground"> · {tSets("rirLabel")} {set.rir}</span>
                      )}
                    </span>
                    <span className="flex items-center gap-1">
                      <button
                        onClick={() => openEditSet(set)}
                        className="rounded p-1 text-muted-foreground hover:text-foreground hover:bg-accent transition-colors"
                      >
                        <Pencil className="h-3.5 w-3.5" />
                      </button>
                      <button
                        onClick={() => setPendingDeleteSetId(set.id)}
                        className="rounded p-1 text-muted-foreground hover:text-destructive hover:bg-destructive/10 transition-colors"
                      >
                        <Trash2 className="h-3.5 w-3.5" />
                      </button>
                    </span>
                  </li>
                ))}
              </ul>
            )}
            <Button variant="outline" size="sm" onClick={openAddSet}>
              <Plus className="h-4 w-4" />
              {tSets("addSet")}
            </Button>
          </>
        )}
      </CardContent>

      {/* Dialogs */}
      {category && (
        <SetFormDialog
          open={setDialogOpen}
          onOpenChange={setSetDialogOpen}
          workoutId={workoutId}
          workoutExerciseId={entry.id}
          category={category}
          set={editingSet}
        />
      )}
      {isCardio && (
        <DurationDialog
          open={durationOpen}
          onOpenChange={setDurationOpen}
          workoutId={workoutId}
          workoutExerciseId={entry.id}
          currentDuration={entry.duration}
        />
      )}
      <ConfirmDialog
        open={confirmRemoveEntry}
        onOpenChange={setConfirmRemoveEntry}
        title={t("removeExerciseConfirm")}
        onConfirm={() =>
          removeExercise.mutate(entry.id, { onSuccess: () => setConfirmRemoveEntry(false) })
        }
        isPending={removeExercise.isPending}
      />
      <ConfirmDialog
        open={pendingDeleteSetId !== null}
        onOpenChange={(open) => {
          if (!open) setPendingDeleteSetId(null)
        }}
        title={tSets("deleteConfirm")}
        onConfirm={() =>
          removeSet.mutate(
            { workoutExerciseId: entry.id, setId: pendingDeleteSetId! },
            { onSuccess: () => setPendingDeleteSetId(null) }
          )
        }
        isPending={removeSet.isPending}
      />
    </Card>
  )
}
