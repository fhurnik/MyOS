"use client"

import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { useEffect } from "react"
import { textNoteSchema, type TextNoteFormValues } from "@/modules/notes/schemas/text-note.schema"
import { useTextNote } from "@/modules/notes/hooks/text-notes/useTextNote"
import { useCreateTextNote } from "@/modules/notes/hooks/text-notes/useCreateTextNote"
import { useUpdateTextNote } from "@/modules/notes/hooks/text-notes/useUpdateTextNote"
import { useDeleteTextNote } from "@/modules/notes/hooks/text-notes/useDeleteTextNote"
import type { TextNoteDto } from "@/modules/notes/types/notes.types"
import { Button } from "@/shared/components/ui/button"
import { Input } from "@/shared/components/ui/input"
import { Label } from "@/shared/components/ui/label"

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
  const isNew = id === "new"

  const { data: note } = useTextNote(isNew ? "" : id, initialData)
  const { mutate: create, isPending: creating } = useCreateTextNote()
  const { mutate: update, isPending: updating } = useUpdateTextNote(id)
  const { mutate: del, isPending: deleting } = useDeleteTextNote()

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isDirty },
  } = useForm<TextNoteFormValues>({
    resolver: zodResolver(textNoteSchema),
    defaultValues: { title: note?.title ?? "", text: note?.text ?? "" },
  })

  useEffect(() => {
    if (note) reset({ title: note.title, text: note.text })
  }, [note, reset])

  function onSubmit(values: TextNoteFormValues) {
    if (isNew) {
      create(values, {
        onSuccess: (newId) => router.replace(`/${locale}/notes/${newId}`),
      })
    } else {
      update(values)
    }
  }

  function handleDelete() {
    if (!confirm(t("deleteConfirm"))) return
    del(id, { onSuccess: () => router.replace(`/${locale}/notes`) })
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold">{isNew ? t("newTitle") : t("editTitle")}</h1>
        {!isNew && (
          <Button variant="destructive" size="sm" onClick={handleDelete} disabled={deleting}>
            {tCommon("delete")}
          </Button>
        )}
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
          <Button type="submit" disabled={creating || updating || (!isDirty && !isNew)}>
            {creating || updating ? "…" : tCommon("save")}
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
    </div>
  )
}
