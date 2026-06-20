"use client"

import { useEffect, useMemo } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import {
  createWorkoutSchema,
  type WorkoutFormValues,
} from "@/modules/fitness/schemas/workout.schema"
import {
  useCreateWorkout,
  useUpdateWorkout,
} from "@/modules/fitness/hooks/workouts/useWorkoutMutations"
import type { WorkoutDto, WorkoutSummaryDto } from "@/modules/fitness/types/fitness.types"
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

interface WorkoutFormDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  workout?: WorkoutDto | WorkoutSummaryDto // present → edit mode
}

function todayIso(): string {
  const now = new Date()
  const offset = now.getTimezoneOffset()
  return new Date(now.getTime() - offset * 60000).toISOString().slice(0, 10)
}

export function WorkoutFormDialog({ open, onOpenChange, workout }: WorkoutFormDialogProps) {
  const t = useTranslations("fitness.workouts")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"
  const isEdit = !!workout

  const schema = useMemo(
    () => createWorkoutSchema({ dateRequired: t("validation.dateRequired") }),
    [t]
  )

  const { mutate: create, isPending: creating } = useCreateWorkout()
  const { mutate: update, isPending: updating } = useUpdateWorkout(workout?.id ?? "")
  const isPending = creating || updating

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<WorkoutFormValues>({
    resolver: zodResolver(schema),
    defaultValues: { date: workout?.date ?? todayIso(), notes: workout?.notes ?? "" },
  })

  useEffect(() => {
    if (open) reset({ date: workout?.date ?? todayIso(), notes: workout?.notes ?? "" })
  }, [open, workout, reset])

  function onSubmit(values: WorkoutFormValues) {
    const body = { date: values.date, notes: values.notes?.trim() ? values.notes.trim() : null }
    if (isEdit) {
      update(body, { onSuccess: () => onOpenChange(false) })
    } else {
      create(body, {
        onSuccess: (id) => {
          onOpenChange(false)
          router.push(`/${locale}/fitness/workouts/${id}`)
        },
      })
    }
  }

  return (
    <Dialog open={open} onOpenChange={(v) => !isPending && onOpenChange(v)}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{isEdit ? t("editTitle") : t("newTitle")}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="workout-date">{t("dateLabel")}</Label>
            <Input id="workout-date" type="date" aria-invalid={!!errors.date} {...register("date")} />
            {errors.date && <p className="text-sm text-destructive">{errors.date.message}</p>}
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="workout-notes">{t("notesLabel")}</Label>
            <textarea
              id="workout-notes"
              rows={3}
              placeholder={t("notesPlaceholder")}
              className="h-auto w-full rounded-lg border border-input bg-transparent px-2.5 py-2 text-sm outline-none transition-colors focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50"
              {...register("notes")}
            />
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
