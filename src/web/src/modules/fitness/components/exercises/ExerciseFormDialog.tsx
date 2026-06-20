"use client"

import { useEffect, useMemo } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import {
  createExerciseSchema,
  type ExerciseFormValues,
} from "@/modules/fitness/schemas/exercise.schema"
import {
  useCreateExercise,
  useUpdateExercise,
} from "@/modules/fitness/hooks/exercises/useExerciseMutations"
import type {
  ExerciseDto,
  ActivityType,
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
import { cn } from "@/shared/lib/utils"

interface ExerciseFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  exercise?: ExerciseDto // present → edit mode
}

const ACTIVITY_TYPES: ActivityType[] = ["cardio", "strength"]
const CATEGORIES: StrengthCategory[] = ["weighted", "bodyweight"]

function defaultsFor(exercise?: ExerciseDto): ExerciseFormValues {
  if (exercise?.activityType === "cardio") {
    return { activityType: "cardio", name: exercise.name, distance: exercise.distance ?? 0 }
  }
  if (exercise?.activityType === "strength") {
    return {
      activityType: "strength",
      name: exercise.name,
      category: exercise.strengthCategory ?? "weighted",
    }
  }
  return { activityType: "strength", name: "", category: "weighted" }
}

export function ExerciseFormDialog({ open, onOpenChange, exercise }: ExerciseFormDialogProps) {
  const t = useTranslations("fitness.exercises")
  const tEnum = useTranslations("fitness.exercises.enums")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"
  const isEdit = !!exercise

  const schema = useMemo(
    () =>
      createExerciseSchema({
        nameRequired: t("validation.nameRequired"),
        distancePositive: t("validation.distancePositive"),
        categoryRequired: t("validation.categoryRequired"),
      }),
    [t]
  )

  const { mutate: create, isPending: creating } = useCreateExercise()
  const { mutate: update, isPending: updating } = useUpdateExercise(exercise?.id ?? "")
  const isPending = creating || updating

  const {
    register,
    handleSubmit,
    reset,
    watch,
    setValue,
    formState: { errors },
  } = useForm<ExerciseFormValues>({
    resolver: zodResolver(schema),
    defaultValues: defaultsFor(exercise),
  })

  // Re-seed when the dialog opens (or the edited exercise changes).
  useEffect(() => {
    if (open) reset(defaultsFor(exercise))
  }, [open, exercise, reset])

  const activityType = watch("activityType")
  const category = activityType === "strength" ? watch("category") : undefined
  const name = watch("name")

  function handleTypeChange(type: ActivityType) {
    if (isEdit) return // type is immutable
    reset(
      type === "cardio"
        ? { activityType: "cardio", name, distance: 0 }
        : { activityType: "strength", name, category: "weighted" }
    )
  }

  function onSubmit(values: ExerciseFormValues) {
    if (isEdit) {
      update(values, { onSuccess: () => onOpenChange(false) })
    } else {
      create(values, {
        onSuccess: (id) => {
          onOpenChange(false)
          router.push(`/${locale}/fitness/exercises/${id}`)
        },
      })
    }
  }

  function handleOpenChange(value: boolean) {
    if (!isPending) onOpenChange(value)
  }

  const errorFor = (key: keyof ExerciseFormValues) =>
    (errors as Record<string, { message?: string }>)[key]?.message

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{isEdit ? t("editTitle") : t("newTitle")}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Type toggle */}
          <div className="space-y-1.5">
            <Label>{t("typeLabel")}</Label>
            <div className="flex gap-2">
              {ACTIVITY_TYPES.map((type) => (
                <button
                  key={type}
                  type="button"
                  disabled={isEdit}
                  onClick={() => handleTypeChange(type)}
                  className={cn(
                    "flex-1 rounded-lg border px-3 py-2 text-sm transition-colors",
                    activityType === type
                      ? "border-primary bg-primary/10 font-medium text-primary"
                      : "border-input text-muted-foreground hover:bg-accent",
                    isEdit && "cursor-not-allowed opacity-60"
                  )}
                >
                  {tEnum(`activityType.${type}`)}
                </button>
              ))}
            </div>
          </div>

          {/* Name */}
          <div className="space-y-1.5">
            <Label htmlFor="exercise-name">{t("nameLabel")}</Label>
            <Input id="exercise-name" aria-invalid={!!errorFor("name")} autoFocus {...register("name")} />
            {errorFor("name") && <p className="text-sm text-destructive">{errorFor("name")}</p>}
          </div>

          {/* Cardio: distance */}
          {activityType === "cardio" && (
            <div className="space-y-1.5">
              <Label htmlFor="exercise-distance">{t("distanceLabel")}</Label>
              <Input
                id="exercise-distance"
                type="number"
                min={1}
                step={1}
                aria-invalid={!!errorFor("distance")}
                {...register("distance", { valueAsNumber: true })}
              />
              {errorFor("distance") && (
                <p className="text-sm text-destructive">{errorFor("distance")}</p>
              )}
            </div>
          )}

          {/* Strength: category */}
          {activityType === "strength" && (
            <div className="space-y-1.5">
              <Label>{t("categoryLabel")}</Label>
              <div className="flex gap-2">
                {CATEGORIES.map((cat) => (
                  <button
                    key={cat}
                    type="button"
                    onClick={() => setValue("category", cat, { shouldValidate: true })}
                    className={cn(
                      "flex-1 rounded-lg border px-3 py-2 text-sm transition-colors",
                      category === cat
                        ? "border-primary bg-primary/10 font-medium text-primary"
                        : "border-input text-muted-foreground hover:bg-accent"
                    )}
                  >
                    {tEnum(`strengthCategory.${cat}`)}
                  </button>
                ))}
              </div>
              {errorFor("category") && (
                <p className="text-sm text-destructive">{errorFor("category")}</p>
              )}
            </div>
          )}

          {isEdit && <p className="text-xs text-muted-foreground">{t("lockedHint")}</p>}

          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => handleOpenChange(false)} disabled={isPending}>
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
