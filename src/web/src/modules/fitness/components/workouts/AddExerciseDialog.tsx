"use client"

import { useEffect, useMemo, useState } from "react"
import { useTranslations } from "next-intl"
import { Plus, Trash2 } from "lucide-react"
import { useExercises } from "@/modules/fitness/hooks/exercises/useExercises"
import { useWorkoutEntryMutations } from "@/modules/fitness/hooks/workouts/useWorkoutEntryMutations"
import {
  DurationFields,
  emptyDuration,
  partsToSeconds,
  type DurationParts,
} from "./DurationFields"
import type {
  ExerciseDto,
  InlineSet,
  AddExerciseToWorkoutBody,
} from "@/modules/fitness/types/fitness.types"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/shared/components/ui/dialog"
import { Button } from "@/shared/components/ui/button"
import { Input } from "@/shared/components/ui/input"
import { Label } from "@/shared/components/ui/label"

interface AddExerciseDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  workoutId: string
}

interface SetRow {
  reps: string
  weight: string
  addedWeight: string
  negatives: string
  rir: string
}

const emptyRow: SetRow = { reps: "", weight: "", addedWeight: "", negatives: "", rir: "" }
const num = (s: string): number | null => (s.trim() === "" ? null : Number(s))

export function AddExerciseDialog({ open, onOpenChange, workoutId }: AddExerciseDialogProps) {
  const t = useTranslations("fitness.workouts")
  const tSets = useTranslations("fitness.sets")
  const tEnum = useTranslations("fitness.exercises.enums")
  const tCommon = useTranslations("common")

  const { data: exercisesData } = useExercises({ params: { pageSize: 100, orderBy: "name" } })
  const exercises = exercisesData?.items ?? []
  const { addExercise } = useWorkoutEntryMutations(workoutId)

  const [exerciseId, setExerciseId] = useState("")
  const [duration, setDuration] = useState<DurationParts>(emptyDuration)
  const [rows, setRows] = useState<SetRow[]>([])
  const [error, setError] = useState<string | null>(null)

  const selected: ExerciseDto | undefined = useMemo(
    () => exercises.find((e) => e.id === exerciseId),
    [exercises, exerciseId]
  )

  useEffect(() => {
    if (open) {
      setExerciseId("")
      setDuration(emptyDuration)
      setRows([])
      setError(null)
    }
  }, [open])

  // Reset the type-specific inputs when the selected exercise changes.
  useEffect(() => {
    setDuration(emptyDuration)
    setRows([])
    setError(null)
  }, [exerciseId])

  const totalSeconds = partsToSeconds(duration)
  const isCardio = selected?.activityType === "cardio"
  const isStrength = selected?.activityType === "strength"
  const isWeighted = selected?.strengthCategory === "weighted"

  function updateRow(index: number, patch: Partial<SetRow>) {
    setRows((prev) => prev.map((r, i) => (i === index ? { ...r, ...patch } : r)))
  }

  function buildStrengthSets(): InlineSet[] | null {
    const sets: InlineSet[] = []
    for (const row of rows) {
      const reps = num(row.reps)
      if (reps == null && row.weight === "" && row.addedWeight === "" && row.negatives === "" && row.rir === "") {
        continue // skip fully empty rows
      }
      if (reps == null || reps <= 0) {
        setError(tSets("validation.repsPositive"))
        return null
      }
      if (isWeighted) {
        const weight = num(row.weight)
        if (weight == null) {
          setError(tSets("validation.weightRequired"))
          return null
        }
        sets.push({ reps, weight, rir: num(row.rir) })
      } else {
        sets.push({
          reps,
          addedWeight: num(row.addedWeight),
          negatives: num(row.negatives),
          rir: num(row.rir),
        })
      }
    }
    return sets
  }

  function onSubmit() {
    setError(null)
    if (!selected) {
      setError(t("validation.exerciseRequired"))
      return
    }
    let body: AddExerciseToWorkoutBody
    if (isCardio) {
      if (totalSeconds <= 0) {
        setError(t("validation.durationPositive"))
        return
      }
      body = { activityType: "cardio", exerciseId: selected.id, duration: totalSeconds }
    } else {
      const sets = buildStrengthSets()
      if (sets == null) return
      body = { activityType: "strength", exerciseId: selected.id, sets }
    }
    addExercise.mutate(body, { onSuccess: () => onOpenChange(false) })
  }

  return (
    <Dialog open={open} onOpenChange={(v) => !addExercise.isPending && onOpenChange(v)}>
      <DialogContent className="max-h-[85dvh] overflow-y-auto sm:max-w-2xl">
        <DialogHeader>
          <DialogTitle>{t("addExerciseTitle")}</DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          {/* Exercise picker */}
          <div className="space-y-1.5">
            <Label htmlFor="pick-exercise">{t("pickExercise")}</Label>
            {exercises.length === 0 ? (
              <p className="text-sm text-muted-foreground">{t("noExercises")}</p>
            ) : (
              <select
                id="pick-exercise"
                value={exerciseId}
                onChange={(e) => setExerciseId(e.target.value)}
                className="h-9 w-full rounded-lg border border-input bg-transparent px-2.5 text-sm outline-none focus:border-ring"
              >
                <option value="">{t("pickExercisePlaceholder")}</option>
                {exercises.map((ex) => (
                  <option key={ex.id} value={ex.id}>
                    {ex.name} ({tEnum(`activityType.${ex.activityType}`)}
                    {ex.strengthCategory ? ` · ${tEnum(`strengthCategory.${ex.strengthCategory}`)}` : ""})
                  </option>
                ))}
              </select>
            )}
          </div>

          {/* Cardio: duration */}
          {isCardio && (
            <div className="space-y-1.5">
              <Label>{t("durationLabel")}</Label>
              <DurationFields value={duration} onChange={setDuration} idPrefix="add-duration" />
            </div>
          )}

          {/* Strength: optional inline sets */}
          {isStrength && (
            <div className="space-y-3">
              <div className="flex items-center justify-between">
                <Label>{tSets("title")}</Label>
                <Button
                  type="button"
                  variant="outline"
                  size="sm"
                  onClick={() => setRows((prev) => [...prev, { ...emptyRow }])}
                >
                  <Plus className="h-4 w-4" />
                  {tSets("addSet")}
                </Button>
              </div>
              <p className="text-xs text-muted-foreground">{t("inlineSetsHint")}</p>

              {rows.map((row, i) => (
                <div key={i} className="flex flex-wrap items-end gap-3 rounded-lg border p-3">
                  <div className="w-20 space-y-1">
                    <Label className="text-xs">{tSets("repsLabel")}</Label>
                    <Input
                      type="number"
                      min={1}
                      inputMode="numeric"
                      placeholder="0"
                      value={row.reps}
                      onChange={(e) => updateRow(i, { reps: e.target.value })}
                    />
                  </div>
                  {isWeighted ? (
                    <div className="min-w-28 flex-1 space-y-1">
                      <Label className="text-xs">{tSets("weightLabel")}</Label>
                      <Input
                        type="number"
                        min={0}
                        step="0.5"
                        inputMode="decimal"
                        placeholder="0"
                        value={row.weight}
                        onChange={(e) => updateRow(i, { weight: e.target.value })}
                      />
                    </div>
                  ) : (
                    <>
                      <div className="min-w-28 flex-1 space-y-1">
                        <Label className="text-xs">{tSets("addedWeightLabel")}</Label>
                        <Input
                          type="number"
                          min={0}
                          step="0.5"
                          inputMode="decimal"
                          placeholder="0"
                          value={row.addedWeight}
                          onChange={(e) => updateRow(i, { addedWeight: e.target.value })}
                        />
                      </div>
                      <div className="w-24 space-y-1">
                        <Label className="text-xs">{tSets("negativesLabel")}</Label>
                        <Input
                          type="number"
                          min={0}
                          inputMode="numeric"
                          placeholder="0"
                          value={row.negatives}
                          onChange={(e) => updateRow(i, { negatives: e.target.value })}
                        />
                      </div>
                    </>
                  )}
                  <div className="w-20 space-y-1">
                    <Label className="text-xs">{tSets("rirLabel")}</Label>
                    <Input
                      type="number"
                      min={0}
                      max={10}
                      inputMode="numeric"
                      placeholder="—"
                      value={row.rir}
                      onChange={(e) => updateRow(i, { rir: e.target.value })}
                    />
                  </div>
                  <button
                    type="button"
                    onClick={() => setRows((prev) => prev.filter((_, idx) => idx !== i))}
                    className="mb-0.5 rounded p-1.5 text-muted-foreground hover:text-destructive hover:bg-destructive/10"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              ))}
            </div>
          )}

          {error && <p className="text-sm text-destructive">{error}</p>}
        </div>

        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={addExercise.isPending}
          >
            {tCommon("cancel")}
          </Button>
          <Button onClick={onSubmit} disabled={addExercise.isPending || !selected}>
            {addExercise.isPending ? "…" : t("addExercise")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
