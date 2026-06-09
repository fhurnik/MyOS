"use client"

import { useMemo } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { createCheckListSchema, type CheckListFormValues } from "@/modules/notes/schemas/check-list.schema"
import { useCreateCheckList } from "@/modules/notes/hooks/check-lists/useCheckListMutations"
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

interface CreateCheckListModalProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function CreateCheckListModal({ open, onOpenChange }: CreateCheckListModalProps) {
  const t = useTranslations("notes.checkLists")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"

  const schema = useMemo(
    () => createCheckListSchema({
      titleRequired: t("validation.titleRequired"),
    }),
    [t]
  )
  const { mutate: create, isPending } = useCreateCheckList()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CheckListFormValues>({
    resolver: zodResolver(schema),
    defaultValues: { title: "" },
  })

  function onSubmit(values: CheckListFormValues) {
    create(values, {
      onSuccess: (id) => {
        onOpenChange(false)
        reset()
        router.push(`/${locale}/notes/checklists/${id}`)
      },
    })
  }

  function handleOpenChange(value: boolean) {
    if (!isPending) {
      reset()
      onOpenChange(value)
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("createButton")}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="create-checklist-title">{t("titleLabel")}</Label>
            <Input
              id="create-checklist-title"
              aria-invalid={!!errors.title}
              autoFocus
              {...register("title")}
            />
            {errors.title && <p className="text-sm text-destructive">{errors.title.message}</p>}
          </div>
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
