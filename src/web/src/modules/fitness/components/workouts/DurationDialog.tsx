"use client"

import { useEffect, useState } from "react"
import { useTranslations } from "next-intl"
import { useWorkoutEntryMutations } from "@/modules/fitness/hooks/workouts/useWorkoutEntryMutations"
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

  const [minutes, setMinutes] = useState(0)
  const [seconds, setSeconds] = useState(0)

  useEffect(() => {
    if (open) {
      const total = currentDuration ?? 0
      setMinutes(Math.floor(total / 60))
      setSeconds(total % 60)
    }
  }, [open, currentDuration])

  const totalSeconds = minutes * 60 + seconds

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
        <div className="grid grid-cols-2 gap-3">
          <div className="space-y-1.5">
            <Label htmlFor="duration-min">{t("durationMinutes")}</Label>
            <Input
              id="duration-min"
              type="number"
              min={0}
              step={1}
              value={minutes}
              onChange={(e) => setMinutes(Math.max(0, Number(e.target.value) || 0))}
            />
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="duration-sec">{t("durationSeconds")}</Label>
            <Input
              id="duration-sec"
              type="number"
              min={0}
              max={59}
              step={1}
              value={seconds}
              onChange={(e) =>
                setSeconds(Math.min(59, Math.max(0, Number(e.target.value) || 0)))
              }
            />
          </div>
        </div>
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
