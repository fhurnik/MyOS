"use client"

import { useEffect, useState } from "react"
import { useTranslations } from "next-intl"
import { useSetExerciseTarget } from "@/modules/fitness/hooks/exercises/useExerciseMutations"
import { progressionUnit } from "@/modules/fitness/lib/fitness-format"
import {
  DurationFields,
  durationToParts,
  partsToSeconds,
  type DurationParts,
} from "@/modules/fitness/components/workouts/DurationFields"
import type { ExerciseDto } from "@/modules/fitness/types/fitness.types"
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

interface SetTargetDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  exercise: ExerciseDto
  currentValue: number | null
}

export function SetTargetDialog({ open, onOpenChange, exercise, currentValue }: SetTargetDialogProps) {
  const t = useTranslations("fitness.exercises")
  const tCommon = useTranslations("common")
  const unit = progressionUnit(exercise.activityType, exercise.strengthCategory)

  const { mutate: setTarget, isPending } = useSetExerciseTarget(exercise.id)

  // For time the value is seconds (h/m/s parts); for kg/reps it is a plain number string.
  const [parts, setParts] = useState<DurationParts>(durationToParts(currentValue))
  const [numberValue, setNumberValue] = useState<string>(
    currentValue != null ? String(currentValue) : ""
  )
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (open) {
      setParts(durationToParts(currentValue))
      setNumberValue(currentValue != null ? String(currentValue) : "")
      setError(null)
    }
  }, [open, currentValue])

  function onSubmit() {
    const value = unit === "time" ? partsToSeconds(parts) : Number(numberValue)
    if (!Number.isFinite(value) || value <= 0) {
      setError(t("validation.targetPositive"))
      return
    }
    setTarget({ value }, { onSuccess: () => onOpenChange(false) })
  }

  return (
    <Dialog open={open} onOpenChange={(v) => !isPending && onOpenChange(v)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("setTarget")}</DialogTitle>
        </DialogHeader>

        {unit === "time" ? (
          <div className="space-y-1.5">
            <Label>{t("targetTimeLabel")}</Label>
            <DurationFields value={parts} onChange={setParts} idPrefix="target" />
          </div>
        ) : (
          <div className="space-y-1.5">
            <Label htmlFor="target-value">
              {unit === "kg" ? t("targetWeightLabel") : t("targetRepsLabel")}
            </Label>
            <Input
              id="target-value"
              type="number"
              min={0}
              step={unit === "kg" ? "0.5" : "1"}
              inputMode={unit === "kg" ? "decimal" : "numeric"}
              placeholder="0"
              autoFocus
              value={numberValue}
              onChange={(e) => setNumberValue(e.target.value)}
            />
          </div>
        )}

        {error && <p className="text-sm text-destructive">{error}</p>}

        <DialogFooter>
          <Button type="button" variant="outline" onClick={() => onOpenChange(false)} disabled={isPending}>
            {tCommon("cancel")}
          </Button>
          <Button onClick={onSubmit} disabled={isPending}>
            {isPending ? "…" : tCommon("save")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
