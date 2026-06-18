"use client"

import { useMemo } from "react"
import { useTranslations } from "next-intl"
import { Folder, HardDrive } from "lucide-react"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/shared/components/ui/dialog"
import { cn } from "@/shared/lib/utils"
import { getDescendantFolderIds } from "@/modules/storage/lib/folder-tree"
import { useMoveFile } from "@/modules/storage/hooks/useFileMutations"
import { useMoveFolder } from "@/modules/storage/hooks/useFolderMutations"
import type { FolderDto } from "@/modules/storage/types/storage.types"

export interface MoveTarget {
  kind: "file" | "folder"
  id: string
  name: string
  currentParentId: string | null
}

interface MoveToModalProps {
  target: MoveTarget | null
  folders: FolderDto[]
  onOpenChange: (open: boolean) => void
}

interface TreeRow {
  folder: FolderDto | null
  depth: number
  disabled: boolean
}

export function MoveToModal({ target, folders, onOpenChange }: MoveToModalProps) {
  const t = useTranslations("storage")
  const moveFile = useMoveFile()
  const moveFolder = useMoveFolder()
  const isPending = moveFile.isPending || moveFolder.isPending

  const rows = useMemo<TreeRow[]>(() => {
    if (!target) return []
    const excluded =
      target.kind === "folder"
        ? new Set<string>([target.id, ...getDescendantFolderIds(folders, target.id)])
        : new Set<string>()

    const childrenByParent = new Map<string | null, FolderDto[]>()
    for (const folder of folders) {
      const list = childrenByParent.get(folder.parentId) ?? []
      list.push(folder)
      childrenByParent.set(folder.parentId, list)
    }
    for (const list of childrenByParent.values()) list.sort((a, b) => a.name.localeCompare(b.name))

    const out: TreeRow[] = [{ folder: null, depth: 0, disabled: target.currentParentId === null }]
    const walk = (parentId: string | null, depth: number) => {
      for (const folder of childrenByParent.get(parentId) ?? []) {
        if (excluded.has(folder.id)) continue
        out.push({ folder, depth, disabled: target.currentParentId === folder.id })
        walk(folder.id, depth + 1)
      }
    }
    walk(null, 1)
    return out
  }, [target, folders])

  function handleSelect(folderId: string | null) {
    if (!target) return
    const onSuccess = () => onOpenChange(false)
    if (target.kind === "file") moveFile.mutate({ id: target.id, folderId }, { onSuccess })
    else moveFolder.mutate({ id: target.id, parentId: folderId }, { onSuccess })
  }

  return (
    <Dialog open={target !== null} onOpenChange={(o) => { if (!isPending && !o) onOpenChange(false) }}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("moveTo")}</DialogTitle>
        </DialogHeader>
        <p className="text-sm text-muted-foreground">{t("selectFolder")}</p>
        <div className="max-h-72 space-y-0.5 overflow-y-auto">
          {rows.map((row) => (
            <button
              key={row.folder?.id ?? "root"}
              type="button"
              disabled={row.disabled || isPending}
              onClick={() => handleSelect(row.folder?.id ?? null)}
              style={{ paddingLeft: `${row.depth * 1 + 0.5}rem` }}
              className={cn(
                "flex w-full items-center gap-2 rounded-md py-1.5 pr-2 text-left text-sm transition-colors",
                row.disabled ? "cursor-not-allowed text-muted-foreground/50" : "hover:bg-muted"
              )}
            >
              {row.folder ? (
                <Folder className="h-4 w-4 shrink-0 text-primary" />
              ) : (
                <HardDrive className="h-4 w-4 shrink-0 text-primary" />
              )}
              <span className="truncate">{row.folder ? row.folder.name : t("moveToRoot")}</span>
            </button>
          ))}
        </div>
      </DialogContent>
    </Dialog>
  )
}
