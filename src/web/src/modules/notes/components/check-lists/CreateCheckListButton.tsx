"use client"

import { useState } from "react"
import { Plus } from "lucide-react"
import { useTranslations } from "next-intl"
import { Button } from "@/shared/components/ui/button"
import { CreateCheckListModal } from "./CreateCheckListModal"

export function CreateCheckListButton() {
  const t = useTranslations("notes.checkLists")
  const [open, setOpen] = useState(false)

  return (
    <>
      <Button onClick={() => setOpen(true)} size="sm">
        <Plus className="h-4 w-4" />
        {t("createButton")}
      </Button>
      <CreateCheckListModal open={open} onOpenChange={setOpen} />
    </>
  )
}
