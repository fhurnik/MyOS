"use client"

import { useTranslations } from "next-intl"
import { Folder, FolderOpen, Pencil, FolderInput, Trash2 } from "lucide-react"
import { useDraggable, useDroppable } from "@dnd-kit/core"
import { cn } from "@/shared/lib/utils"
import type { FolderDto } from "@/modules/storage/types/storage.types"
import { TileActionsMenu } from "./TileActionsMenu"

interface FolderTileProps {
  folder: FolderDto
  onOpen: () => void
  onRename: () => void
  onMove: () => void
  onDelete: () => void
}

export function FolderTile({ folder, onOpen, onRename, onMove, onDelete }: FolderTileProps) {
  const t = useTranslations("storage")
  const draggable = useDraggable({ id: `folder:${folder.id}`, data: { kind: "folder", id: folder.id } })
  const droppable = useDroppable({ id: `drop:${folder.id}`, data: { folderId: folder.id } })

  const setRefs = (node: HTMLElement | null) => {
    draggable.setNodeRef(node)
    droppable.setNodeRef(node)
  }

  return (
    <div
      ref={setRefs}
      {...draggable.listeners}
      {...draggable.attributes}
      onClick={onOpen}
      className={cn(
        "group relative flex cursor-pointer flex-col items-center gap-2 rounded-xl border bg-card p-4 text-center shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md",
        draggable.isDragging && "opacity-50",
        droppable.isOver && "border-primary bg-primary/10 ring-1 ring-primary"
      )}
    >
      <div className="absolute right-1.5 top-1.5">
        <TileActionsMenu
          label={folder.name}
          actions={[
            { key: "open", label: t("open"), icon: <FolderOpen className="h-4 w-4" />, onSelect: onOpen },
            { key: "rename", label: t("rename"), icon: <Pencil className="h-4 w-4" />, onSelect: onRename },
            { key: "move", label: t("moveTo"), icon: <FolderInput className="h-4 w-4" />, onSelect: onMove },
            {
              key: "delete",
              label: t("delete"),
              icon: <Trash2 className="h-4 w-4" />,
              onSelect: onDelete,
              variant: "destructive",
            },
          ]}
        />
      </div>
      <Folder className="h-10 w-10 text-primary" />
      <span className="line-clamp-2 w-full break-words text-sm font-medium">{folder.name}</span>
    </div>
  )
}
