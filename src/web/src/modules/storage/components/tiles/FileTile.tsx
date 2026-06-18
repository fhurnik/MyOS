"use client"

import { useTranslations } from "next-intl"
import { Eye, Download, FolderInput, Trash2 } from "lucide-react"
import { useDraggable } from "@dnd-kit/core"
import { cn } from "@/shared/lib/utils"
import { formatBytes } from "@/shared/lib/format"
import { getFileIconByCategory } from "@/modules/storage/lib/file-icon"
import { fileDownloadUrl } from "@/modules/storage/api/files.api"
import type { StoredFileDto } from "@/modules/storage/types/storage.types"
import { TileActionsMenu } from "./TileActionsMenu"

interface FileTileProps {
  file: StoredFileDto
  category: string | undefined
  onPreview: () => void
  onMove: () => void
  onDelete: () => void
}

export function FileTile({ file, category, onPreview, onMove, onDelete }: FileTileProps) {
  const t = useTranslations("storage")
  const draggable = useDraggable({ id: `file:${file.id}`, data: { kind: "file", id: file.id } })
  const Icon = getFileIconByCategory(category)

  return (
    <div
      ref={draggable.setNodeRef}
      {...draggable.listeners}
      {...draggable.attributes}
      onClick={onPreview}
      className={cn(
        "group relative flex cursor-grab flex-col items-center gap-2 rounded-xl border bg-card p-4 text-center shadow-sm transition-all hover:shadow-md active:cursor-grabbing",
        draggable.isDragging && "opacity-50"
      )}
    >
      <div className="absolute right-1.5 top-1.5">
        <TileActionsMenu
          label={file.originalName}
          actions={[
            {
              key: "preview",
              label: t("preview.action"),
              icon: <Eye className="h-4 w-4" />,
              onSelect: onPreview,
            },
            {
              key: "download",
              label: t("download"),
              icon: <Download className="h-4 w-4" />,
              href: fileDownloadUrl(file.id),
              download: file.originalName,
            },
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
      <Icon className="h-10 w-10 text-muted-foreground" />
      <span className="line-clamp-2 w-full break-words text-sm font-medium">{file.originalName}</span>
      <span className="text-xs text-muted-foreground">{formatBytes(file.sizeBytes)}</span>
    </div>
  )
}
