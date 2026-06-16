"use client"

import { useState } from "react"
import { usePathname, useRouter } from "next/navigation"
import { useTranslations } from "next-intl"
import { useCheckList } from "@/modules/notes/hooks/check-lists/useCheckList"
import {
  useDeleteCheckList,
  useAddCheckListItem,
  useToggleCheckListItem,
  useDeleteCheckListItem,
} from "@/modules/notes/hooks/check-lists/useCheckListMutations"
import type { CheckListDto } from "@/modules/notes/types/notes.types"
import { Button } from "@/shared/components/ui/button"
import { Input } from "@/shared/components/ui/input"
import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog"
import { CheckListItem } from "./CheckListItem"

interface CheckListDetailProps {
  id: string
  initialData?: CheckListDto
}

export function CheckListDetail({ id, initialData }: CheckListDetailProps) {
  const t = useTranslations("notes.checkLists")
  const tCommon = useTranslations("common")
  const pathname = usePathname()
  const router = useRouter()
  const locale = pathname.split("/")[1] ?? "en"
  const [newItemText, setNewItemText] = useState("")
  const [deleteListOpen, setDeleteListOpen] = useState(false)
  const [deleteItemId, setDeleteItemId] = useState<string | null>(null)

  const { data: list } = useCheckList(id, initialData)
  const { mutate: del, isPending: deleting } = useDeleteCheckList()
  const { mutate: addItem, isPending: addingItem } = useAddCheckListItem(id)
  const { mutate: toggle } = useToggleCheckListItem(id)
  const { mutate: deleteItem, isPending: deletingItem } = useDeleteCheckListItem(id)

  function handleAddItem(e: React.FormEvent) {
    e.preventDefault()
    if (!newItemText.trim()) return
    addItem({ text: newItemText.trim() }, { onSuccess: () => setNewItemText("") })
  }

  function handleConfirmDeleteList() {
    del(id, { onSuccess: () => router.replace(`/${locale}/notes/checklists`) })
  }

  function handleConfirmDeleteItem() {
    if (!deleteItemId) return
    deleteItem(deleteItemId, { onSuccess: () => setDeleteItemId(null) })
  }

  if (!list) return null

  const sortedItems = [...list.items].sort((a, b) => a.order - b.order)

  return (
    <div className="mx-auto w-full max-w-lg flex flex-col flex-1 min-h-0 gap-6">
      <div className="flex items-center justify-between">
        <h1 className="text-xl font-semibold">{list.title}</h1>
        <Button variant="destructive" size="sm" onClick={() => setDeleteListOpen(true)} disabled={deleting}>
          {tCommon("delete")}
        </Button>
      </div>

      <ul className="flex-1 overflow-auto min-h-0 space-y-1">
        {sortedItems.map((item) => (
          <CheckListItem
            key={item.id}
            item={item}
            onToggle={() => toggle(item.id)}
            onDelete={() => setDeleteItemId(item.id)}
          />
        ))}
      </ul>

      <form onSubmit={handleAddItem} className="flex gap-2">
        <Input
          value={newItemText}
          onChange={(e) => setNewItemText(e.target.value)}
          placeholder={t("addItem")}
          className="flex-1"
        />
        <Button type="submit" disabled={addingItem || !newItemText.trim()}>
          +
        </Button>
      </form>

      <ConfirmDialog
        open={deleteListOpen}
        onOpenChange={setDeleteListOpen}
        title={t("deleteConfirm")}
        onConfirm={handleConfirmDeleteList}
        isPending={deleting}
      />

      <ConfirmDialog
        open={deleteItemId !== null}
        onOpenChange={(open) => { if (!open) setDeleteItemId(null) }}
        title={t("deleteItemConfirm")}
        onConfirm={handleConfirmDeleteItem}
        isPending={deletingItem}
      />
    </div>
  )
}
