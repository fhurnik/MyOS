"use client"

import { useMemo, useState } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { useEffect } from "react"
import { createTextNoteSchema, type TextNoteFormValues } from "@/modules/notes/schemas/text-note.schema"
import { useTextNote } from "@/modules/notes/hooks/text-notes/useTextNote"
import { useUpdateTextNote } from "@/modules/notes/hooks/text-notes/useUpdateTextNote"
import { useDeleteTextNote } from "@/modules/notes/hooks/text-notes/useDeleteTextNote"
import type { TextNoteDto } from "@/modules/notes/types/notes.types"
import { Button } from "@/shared/components/ui/button"
import { Input } from "@/shared/components/ui/input"
import { Label } from "@/shared/components/ui/label"
import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog"

interface TextNoteDetailProps {
  id: string
  initialData?: TextNoteDto
}

export function TextNoteDetail({ id, initialData }: TextNoteDetailProps) {
  const t = useTranslations("notes.textNotes")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"
  const [confirmOpen, setConfirmOpen] = useState(false)

  const { data: note } = useTextNote(id, initialData)
  const { mutate: update, isPending: updating } = useUpdateTextNote(id)
  const { mutate: del, isPending: deleting } = useDeleteTextNote()

  const schema = useMemo(
    () => createTextNoteSchema({
      titleRequired: t("validation.titleRequired"),
      contentRequired: t("validation.contentRequired"),
    }),
    [t]
  )

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<TextNoteFormValues>({
    resolver: zodResolver(schema),
    defaultValues: { title: note?.title ?? "", text: note?.text ?? "" },
  })

  useEffect(() => {
    if (note) reset({ title: note.title, text: note.text })
  }, [note, reset])

  function onSubmit(values: TextNoteFormValues) {
    update(values)
  }

  function handleConfirmDelete() {
    del(id, { onSuccess: () => router.replace(`/${locale}/notes`) })
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold">{t("editTitle")}</h1>
        <Button variant="destructive" size="sm" onClick={() => setConfirmOpen(true)} disabled={deleting}>
          {tCommon("delete")}
        </Button>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        <div className="space-y-1.5">
          <Label htmlFor="title">{t("titleLabel")}</Label>
          <Input id="title" aria-invalid={!!errors.title} {...register("title")} />
          {errors.title && <p className="text-sm text-destructive">{errors.title.message}</p>}
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="text">{t("textLabel")}</Label>
          <textarea
            id="text"
            rows={12}
            className="h-auto w-full rounded-lg border border-input bg-transparent px-2.5 py-2 text-sm outline-none transition-colors focus-visible:border-ring focus-visible:ring-3 focus-visible:ring-ring/50"
            aria-invalid={!!errors.text}
            {...register("text")}
          />
          {errors.text && <p className="text-sm text-destructive">{errors.text.message}</p>}
        </div>

        <div className="flex gap-2">
          <Button type="submit" disabled={updating || !isDirty}>
            {updating ? "…" : tCommon("save")}
          </Button>
          <Button
            type="button"
            variant="outline"
            onClick={() => router.replace(`/${locale}/notes`)}
          >
            {tCommon("cancel")}
          </Button>
        </div>
      </form>

      <ConfirmDialog
        open={confirmOpen}
        onOpenChange={setConfirmOpen}
        title={t("deleteConfirm")}
        onConfirm={handleConfirmDelete}
        isPending={deleting}
      />
    </div>
  )
}
