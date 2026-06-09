"use client"

import { useMemo } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { createTextNoteSchema, type TextNoteFormValues } from "@/modules/notes/schemas/text-note.schema"
import { useCreateTextNote } from "@/modules/notes/hooks/text-notes/useCreateTextNote"
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

interface CreateNoteModalProps {
  open: boolean
  onOpenChange: (open: boolean) => void
}

export function CreateNoteModal({ open, onOpenChange }: CreateNoteModalProps) {
  const t = useTranslations("notes.textNotes")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"

  const schema = useMemo(
    () => createTextNoteSchema({
      titleRequired: t("validation.titleRequired"),
      contentRequired: t("validation.contentRequired"),
    }),
    [t]
  )
  const { mutate: create, isPending } = useCreateTextNote()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<TextNoteFormValues>({
    resolver: zodResolver(schema),
    defaultValues: { title: "", text: "" },
  })

  function onSubmit(values: TextNoteFormValues) {
    create(values, {
      onSuccess: (id) => {
        onOpenChange(false)
        reset()
        router.push(`/${locale}/notes/${id}`)
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
          <DialogTitle>{t("newTitle")}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="create-note-title">{t("titleLabel")}</Label>
            <Input
              id="create-note-title"
              aria-invalid={!!errors.title}
              autoFocus
              {...register("title")}
            />
            {errors.title && <p className="text-sm text-destructive">{errors.title.message}</p>}
          </div>
          <div className="space-y-1.5">
            <Label htmlFor="create-note-text">{t("textLabel")}</Label>
            <textarea
              id="create-note-text"
              rows={6}
              className="h-auto w-full rounded-lg border border-input bg-transparent px-2.5 py-2 text-sm outline-none transition-colors focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50"
              aria-invalid={!!errors.text}
              {...register("text")}
            />
            {errors.text && <p className="text-sm text-destructive">{errors.text.message}</p>}
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
