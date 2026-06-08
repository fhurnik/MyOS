"use client"

import { Plus } from "lucide-react"
import { usePathname } from "next/navigation"
import { useTranslations } from "next-intl"
import { useCreateTextNote } from "@/modules/notes/hooks/text-notes/useCreateTextNote"
import { Button } from "@/shared/components/ui/button"

export function CreateNoteButton() {
  const t = useTranslations("notes.textNotes")
  const pathname = usePathname()
  const locale = pathname.split("/")[1] ?? "en"
  const { mutate: create, isPending } = useCreateTextNote()

  function handleCreate() {
    create(
      { title: "New note", text: "" },
      {
        onSuccess: (id) => {
          window.location.href = `/${locale}/notes/${id}`
        },
      }
    )
  }

  return (
    <Button onClick={handleCreate} disabled={isPending} size="sm">
      <Plus className="h-4 w-4" />
      {isPending ? "…" : t("createButton")}
    </Button>
  )
}
