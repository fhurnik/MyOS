"use client"

import { useEffect, useState } from "react"
import { useTranslations } from "next-intl"
import { useWorkoutEntryMutations } from "@/modules/fitness/hooks/workouts/useWorkoutEntryMutations"
import {
  DurationFields,
  durationToParts,
  partsToSeconds,
  type DurationParts,
} from "./DurationFields"
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/shared/components/ui/dialog"
import { Button } from "@/shared/components/ui/button"

interface DurationDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  workoutId: string
  workoutExerciseId: string
  currentDuration: number | null
}

export function DurationDialog({
  open,
  onOpenChange,
  workoutId,
  workoutExerciseId,
  currentDuration,
}: DurationDialogProps) {
  const t = useTranslations("fitness.workouts")
  const tCommon = useTranslations("common")
  const { updateDuration } = useWorkoutEntryMutations(workoutId)

  const [parts, setParts] = useState<DurationParts>(durationToParts(currentDuration))

  useEffect(() => {
    if (open) setParts(durationToParts(currentDuration))
  }, [open, currentDuration])

  const totalSeconds = partsToSeconds(parts)

  function onSubmit() {
    if (totalSeconds <= 0) return
    updateDuration.mutate(
      { workoutExerciseId, duration: totalSeconds },
      { onSuccess: () => onOpenChange(false) }
    )
  }

  return (
    <Dialog open={open} onOpenChange={(v) => !updateDuration.isPending && onOpenChange(v)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("editDuration")}</DialogTitle>
        </DialogHeader>
        <DurationFields value={parts} onChange={setParts} idPrefix="edit-duration" />
        {totalSeconds <= 0 && (
          <p className="text-sm text-destructive">{t("validation.durationPositive")}</p>
        )}
        <DialogFooter>
          <Button
            type="button"
            variant="outline"
            onClick={() => onOpenChange(false)}
            disabled={updateDuration.isPending}
          >
            {tCommon("cancel")}
          </Button>
          <Button onClick={onSubmit} disabled={updateDuration.isPending || totalSeconds <= 0}>
            {updateDuration.isPending ? "…" : tCommon("save")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
