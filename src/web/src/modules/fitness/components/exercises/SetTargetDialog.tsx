"use client"

import { useEffect, useMemo } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useTranslations } from "next-intl"
import {
  createTargetSchema,
  type TargetFormValues,
} from "@/modules/fitness/schemas/target.schema"
import { useSetExerciseTarget } from "@/modules/fitness/hooks/exercises/useExerciseMutations"
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
  exerciseId: string
  currentValue: number | null
}

export function SetTargetDialog({
  open,
  onOpenChange,
  exerciseId,
  currentValue,
}: SetTargetDialogProps) {
  const t = useTranslations("fitness.exercises")
  const tCommon = useTranslations("common")

  const schema = useMemo(
    () => createTargetSchema({ targetPositive: t("validation.targetPositive") }),
    [t]
  )

  const { mutate: setTarget, isPending } = useSetExerciseTarget(exerciseId)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<TargetFormValues>({
    resolver: zodResolver(schema),
    defaultValues: { value: currentValue ?? 0 },
  })

  useEffect(() => {
    if (open) reset({ value: currentValue ?? 0 })
  }, [open, currentValue, reset])

  function onSubmit(values: TargetFormValues) {
    setTarget(values, { onSuccess: () => onOpenChange(false) })
  }

  return (
    <Dialog open={open} onOpenChange={(v) => !isPending && onOpenChange(v)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("setTarget")}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="target-value">{t("targetValue")}</Label>
            <Input
              id="target-value"
              type="number"
              step="0.01"
              min={0}
              autoFocus
              aria-invalid={!!errors.value}
              {...register("value", { valueAsNumber: true })}
            />
            {errors.value && <p className="text-sm text-destructive">{errors.value.message}</p>}
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)} disabled={isPending}>
              {tCommon("cancel")}
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? "…" : tCommon("save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
