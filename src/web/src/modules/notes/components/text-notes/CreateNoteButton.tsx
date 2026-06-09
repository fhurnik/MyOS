"use client"

import { useState } from "react"
import { Plus } from "lucide-react"
import { useTranslations } from "next-intl"
import { Button } from "@/shared/components/ui/button"
import { CreateNoteModal } from "./CreateNoteModal"

export function CreateNoteButton() {
  const t = useTranslations("notes.textNotes")
  const [open, setOpen] = useState(false)

  return (
    <>
      <Button onClick={() => setOpen(true)} size="sm">
        <Plus className="h-4 w-4" />
        {t("createButton")}
      </Button>
      <CreateNoteModal open={open} onOpenChange={setOpen} />
    </>
  )
}
