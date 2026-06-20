"use client"

import { useEffect, useMemo } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useTranslations } from "next-intl"
import { createSetSchema, type SetFormValues } from "@/modules/fitness/schemas/set.schema"
import { useWorkoutEntryMutations } from "@/modules/fitness/hooks/workouts/useWorkoutEntryMutations"
import type {
  ExerciseSetDto,
  SetBody,
  StrengthCategory,
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

interface SetFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  workoutId: string
  workoutExerciseId: string
  category: StrengthCategory
  set?: ExerciseSetDto // present → edit mode
}

// Empty string → null; otherwise a number. Used for optional numeric inputs.
const nullableNumber = { setValueAs: (v: unknown) => (v === "" || v == null ? null : Number(v)) }

function defaultsFor(category: StrengthCategory, set?: ExerciseSetDto): Partial<SetFormValues> {
  if (category === "weighted") {
    return {
      category: "weighted",
      reps: set?.reps,
      weight: set?.weight ?? undefined,
      rir: set?.rir ?? null,
    } as Partial<SetFormValues>
  }
  return {
    category: "bodyweight",
    reps: set?.reps,
    addedWeight: set?.addedWeight ?? null,
    negatives: set?.negatives ?? null,
    rir: set?.rir ?? null,
  } as Partial<SetFormValues>
}

export function SetFormDialog({
  open,
  onOpenChange,
  workoutId,
  workoutExerciseId,
  category,
  set,
}: SetFormDialogProps) {
  const t = useTranslations("fitness.sets")
  const tCommon = useTranslations("common")
  const isEdit = !!set

  const schema = useMemo(
    () =>
      createSetSchema({
        repsPositive: t("validation.repsPositive"),
        weightRequired: t("validation.weightRequired"),
        weightNonNegative: t("validation.weightNonNegative"),
        addedWeightNonNegative: t("validation.addedWeightNonNegative"),
        negativesNonNegative: t("validation.negativesNonNegative"),
        rirRange: t("validation.rirRange"),
      }),
    [t]
  )

  const { addSet, updateSet } = useWorkoutEntryMutations(workoutId)
  const isPending = addSet.isPending || updateSet.isPending

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<SetFormValues>({
    resolver: zodResolver(schema),
    defaultValues: defaultsFor(category, set) as SetFormValues,
  })

  useEffect(() => {
    if (open) reset(defaultsFor(category, set) as SetFormValues)
  }, [open, category, set, reset])

  const errorFor = (key: string) =>
    (errors as Record<string, { message?: string }>)[key]?.message

  function onSubmit(values: SetFormValues) {
    const body = values as SetBody
    if (set) {
      updateSet.mutate(
        { workoutExerciseId, setId: set.id, body },
        { onSuccess: () => onOpenChange(false) }
      )
    } else {
      addSet.mutate({ workoutExerciseId, body }, { onSuccess: () => onOpenChange(false) })
    }
  }

  return (
    <Dialog open={open} onOpenChange={(v) => !isPending && onOpenChange(v)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{isEdit ? t("edit") : t("addSet")}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Discriminant — seeded from defaultValues, kept in form state for the schema. */}
          <input type="hidden" {...register("category")} />

          <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
            <div className="space-y-1.5">
              <Label htmlFor="set-reps">{t("repsLabel")}</Label>
              <Input
                id="set-reps"
                type="number"
                min={1}
                step={1}
                autoFocus
                aria-invalid={!!errorFor("reps")}
                {...register("reps", { valueAsNumber: true })}
              />
              {errorFor("reps") && <p className="text-sm text-destructive">{errorFor("reps")}</p>}
            </div>

            {category === "weighted" ? (
              <div className="space-y-1.5">
                <Label htmlFor="set-weight">{t("weightLabel")}</Label>
                <Input
                  id="set-weight"
                  type="number"
                  min={0}
                  step="0.5"
                  aria-invalid={!!errorFor("weight")}
                  {...register("weight", { valueAsNumber: true })}
                />
                {errorFor("weight") && (
                  <p className="text-sm text-destructive">{errorFor("weight")}</p>
                )}
              </div>
            ) : (
              <>
                <div className="space-y-1.5">
                  <Label htmlFor="set-added-weight">{t("addedWeightLabel")}</Label>
                  <Input
                    id="set-added-weight"
                    type="number"
                    min={0}
                    step="0.5"
                    aria-invalid={!!errorFor("addedWeight")}
                    {...register("addedWeight", nullableNumber)}
                  />
                  {errorFor("addedWeight") && (
                    <p className="text-sm text-destructive">{errorFor("addedWeight")}</p>
                  )}
                </div>
                <div className="space-y-1.5">
                  <Label htmlFor="set-negatives">{t("negativesLabel")}</Label>
                  <Input
                    id="set-negatives"
                    type="number"
                    min={0}
                    step={1}
                    aria-invalid={!!errorFor("negatives")}
                    {...register("negatives", nullableNumber)}
                  />
                  {errorFor("negatives") && (
                    <p className="text-sm text-destructive">{errorFor("negatives")}</p>
                  )}
                </div>
              </>
            )}

            <div className="space-y-1.5">
              <Label htmlFor="set-rir">{t("rirLabel")}</Label>
              <Input
                id="set-rir"
                type="number"
                min={0}
                max={10}
                step={1}
                aria-invalid={!!errorFor("rir")}
                {...register("rir", nullableNumber)}
              />
              {errorFor("rir") ? (
                <p className="text-sm text-destructive">{errorFor("rir")}</p>
              ) : (
                <p className="text-xs text-muted-foreground">{t("rirHint")}</p>
              )}
            </div>
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
