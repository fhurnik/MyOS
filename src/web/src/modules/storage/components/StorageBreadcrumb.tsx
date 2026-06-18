"use client"

import { useTranslations } from "next-intl"
import { ChevronRight } from "lucide-react"
import { useDroppable } from "@dnd-kit/core"
import { cn } from "@/shared/lib/utils"
import type { FolderDto } from "@/modules/storage/types/storage.types"

interface CrumbProps {
  folderId: string | null
  label: string
  isCurrent: boolean
  onNavigate: (folderId: string | null) => void
}

// Each crumb is also a drop target, so items can be moved to an ancestor folder or the root.
function Crumb({ folderId, label, isCurrent, onNavigate }: CrumbProps) {
  const { setNodeRef, isOver } = useDroppable({ id: `drop:${folderId ?? "root"}`, data: { folderId } })

  return (
    <button
      ref={setNodeRef}
      type="button"
      disabled={isCurrent}
      onClick={() => onNavigate(folderId)}
      className={cn(
        "max-w-40 truncate rounded px-1.5 py-0.5 transition-colors",
        isCurrent
          ? "font-medium text-foreground"
          : "text-muted-foreground hover:bg-muted hover:text-foreground",
        isOver && "bg-primary/15 text-primary ring-1 ring-primary"
      )}
    >
      {label}
    </button>
  )
}

interface StorageBreadcrumbProps {
  path: FolderDto[]
  onNavigate: (folderId: string | null) => void
}

export function StorageBreadcrumb({ path, onNavigate }: StorageBreadcrumbProps) {
  const t = useTranslations("storage")

  return (
    <nav className="flex flex-wrap items-center gap-0.5 text-sm">
      <Crumb folderId={null} label={t("root")} isCurrent={path.length === 0} onNavigate={onNavigate} />
      {path.map((folder, index) => (
        <span key={folder.id} className="flex items-center gap-0.5">
          <ChevronRight className="h-3.5 w-3.5 shrink-0 text-muted-foreground/50" />
          <Crumb
            folderId={folder.id}
            label={folder.name}
            isCurrent={index === path.length - 1}
            onNavigate={onNavigate}
          />
        </span>
      ))}
    </nav>
  )
}
