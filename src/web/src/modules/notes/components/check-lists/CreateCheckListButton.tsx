"use client"

import { Plus } from "lucide-react"
import { usePathname } from "next/navigation"
import { useTranslations } from "next-intl"
import { useCreateCheckList } from "@/modules/notes/hooks/check-lists/useCheckListMutations"
import { Button } from "@/shared/components/ui/button"

export function CreateCheckListButton() {
  const t = useTranslations("notes.checkLists")
  const pathname = usePathname()
  const locale = pathname.split("/")[1] ?? "en"
  const { mutate: create, isPending } = useCreateCheckList()

  function handleCreate() {
    create(
      { title: "New list" },
      {
        onSuccess: (id) => {
          window.location.href = `/${locale}/notes/checklists/${id}`
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
