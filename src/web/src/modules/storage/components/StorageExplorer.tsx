"use client"

import { useMemo, useState } from "react"
import { usePathname, useRouter, useSearchParams } from "next/navigation"
import { useTranslations } from "next-intl"
import {
  DndContext,
  DragOverlay,
  PointerSensor,
  TouchSensor,
  useSensor,
  useSensors,
} from "@dnd-kit/core"
import type { DragEndEvent, DragStartEvent } from "@dnd-kit/core"
import { Folder, FolderOpen, Pencil, FolderInput, Trash2 } from "lucide-react"
import { getFileIconByCategory } from "@/modules/storage/lib/file-icon"
import { useFolders } from "@/modules/storage/hooks/useFolders"
import { useFiles } from "@/modules/storage/hooks/useFiles"
import { useQuota } from "@/modules/storage/hooks/useQuota"
import { useAllowedFileTypes } from "@/modules/storage/hooks/useAllowedFileTypes"
import { useDeleteFile, useMoveFile } from "@/modules/storage/hooks/useFileMutations"
import { useDeleteFolder, useMoveFolder } from "@/modules/storage/hooks/useFolderMutations"
import { getBreadcrumbPath } from "@/modules/storage/lib/folder-tree"
import { QuotaBar } from "./QuotaBar"
import { StorageBreadcrumb } from "./StorageBreadcrumb"
import { FolderTile } from "./tiles/FolderTile"
import { FileTile } from "./tiles/FileTile"
import { TileActionsMenu } from "./tiles/TileActionsMenu"
import { CreateFolderButton } from "./CreateFolderButton"
import { RenameFolderModal } from "./RenameFolderModal"
import { MoveToModal, type MoveTarget } from "./MoveToModal"
import { FilePreviewModal } from "./preview/FilePreviewModal"
import { ConfirmDialog } from "@/shared/components/ui/confirm-dialog"
import { toast } from "sonner"
import { useUpload } from "@/modules/storage/upload/UploadProvider"
import { UploadButton } from "@/modules/storage/upload/UploadButton"
import { StorageDropZone } from "@/modules/storage/upload/StorageDropZone"
import type {
  AllowedFileTypeDto,
  FolderDto,
  QuotaDto,
  StoredFileDto,
} from "@/modules/storage/types/storage.types"

interface StorageExplorerProps {
  initialFolders: FolderDto[]
  initialFiles: StoredFileDto[]
  initialQuota: QuotaDto
  initialAllowedTypes: AllowedFileTypeDto[]
}

type DeleteTarget = { kind: "file" | "folder"; id: string; name: string }

export function StorageExplorer({
  initialFolders,
  initialFiles,
  initialQuota,
  initialAllowedTypes,
}: StorageExplorerProps) {
  const t = useTranslations("storage")
  const router = useRouter()
  const pathname = usePathname()
  const searchParams = useSearchParams()

  const { data: folders = [] } = useFolders(initialFolders)
  const { data: files = [] } = useFiles(initialFiles)
  const { data: quota } = useQuota(initialQuota)
  const { data: allowedTypes = [] } = useAllowedFileTypes(initialAllowedTypes)

  const moveFile = useMoveFile()
  const moveFolder = useMoveFolder()
  const deleteFile = useDeleteFile()
  const deleteFolder = useDeleteFolder()
  const { enqueue } = useUpload()

  const [activeDrag, setActiveDrag] = useState<{ kind: "file" | "folder"; id: string } | null>(null)
  const [renameTarget, setRenameTarget] = useState<FolderDto | null>(null)
  const [moveTarget, setMoveTarget] = useState<MoveTarget | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<DeleteTarget | null>(null)
  const [previewIndex, setPreviewIndex] = useState<number | null>(null)

  const folderIdParam = searchParams.get("folderId")
  const currentFolderId =
    folderIdParam && folders.some((f) => f.id === folderIdParam) ? folderIdParam : null
  const currentFolder = useMemo(
    () => folders.find((f) => f.id === currentFolderId) ?? null,
    [folders, currentFolderId]
  )

  const path = useMemo(() => getBreadcrumbPath(folders, currentFolderId), [folders, currentFolderId])
  const categoryByExt = useMemo(
    () => new Map(allowedTypes.map((a) => [a.extension, a.category])),
    [allowedTypes]
  )

  const subFolders = useMemo(
    () =>
      folders
        .filter((f) => f.parentId === currentFolderId)
        .sort((a, b) => a.name.localeCompare(b.name)),
    [folders, currentFolderId]
  )
  const folderFiles = useMemo(
    () =>
      files
        .filter((f) => f.folderId === currentFolderId)
        .sort((a, b) => b.createdAtUtc.localeCompare(a.createdAtUtc)),
    [files, currentFolderId]
  )

  const sensors = useSensors(
    useSensor(PointerSensor, { activationConstraint: { distance: 5 } }),
    useSensor(TouchSensor, { activationConstraint: { delay: 200, tolerance: 6 } })
  )

  function navigate(folderId: string | null) {
    router.push(folderId ? `${pathname}?folderId=${folderId}` : pathname)
  }

  function handleDragStart(event: DragStartEvent) {
    setActiveDrag(event.active.data.current as { kind: "file" | "folder"; id: string })
  }

  function handleDragEnd(event: DragEndEvent) {
    setActiveDrag(null)
    const { active, over } = event
    if (!over) return
    const targetData = over.data.current as { folderId: string | null } | undefined
    const targetFolderId = targetData ? targetData.folderId : null
    const activeData = active.data.current as { kind: "file" | "folder"; id: string } | undefined
    if (!activeData) return

    if (activeData.kind === "folder") {
      if (activeData.id === targetFolderId) return
      moveFolder.mutate({ id: activeData.id, parentId: targetFolderId })
    } else {
      moveFile.mutate({ id: activeData.id, folderId: targetFolderId })
    }
  }

  function handleFiles(files: File[]) {
    const allowed = new Set(allowedTypes.map((a) => a.extension.toLowerCase()))
    const valid: File[] = []
    const rejected: string[] = []
    for (const file of files) {
      const ext = file.name.split(".").pop()?.toLowerCase() ?? ""
      if (allowed.has(ext)) valid.push(file)
      else rejected.push(file.name)
    }
    if (rejected.length > 0) toast.error(t("upload.rejectedType", { files: rejected.join(", ") }))
    if (valid.length > 0) enqueue(valid, currentFolderId)
  }

  function confirmDelete() {
    if (!deleteTarget) return
    // Deleting the folder we're currently inside → drop back to its parent afterwards.
    const deletingCurrent = deleteTarget.kind === "folder" && deleteTarget.id === currentFolderId
    const parentOfCurrent = currentFolder?.parentId ?? null
    const onSuccess = () => {
      setDeleteTarget(null)
      if (deletingCurrent) navigate(parentOfCurrent)
    }
    if (deleteTarget.kind === "folder") deleteFolder.mutate(deleteTarget.id, { onSuccess })
    else deleteFile.mutate(deleteTarget.id, { onSuccess })
  }

  const isEmpty = subFolders.length === 0 && folderFiles.length === 0
  const deleting = deleteFile.isPending || deleteFolder.isPending

  return (
    <DndContext
      sensors={sensors}
      onDragStart={handleDragStart}
      onDragEnd={handleDragEnd}
      onDragCancel={() => setActiveDrag(null)}
    >
      <StorageDropZone onFiles={handleFiles}>
      <div className="space-y-5">
        <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
          <StorageBreadcrumb path={path} onNavigate={navigate} />
          <div className="flex shrink-0 items-center gap-1.5">
            {currentFolder && (
              <TileActionsMenu
                label={currentFolder.name}
                actions={[
                  {
                    key: "rename",
                    label: t("rename"),
                    icon: <Pencil className="h-4 w-4" />,
                    onSelect: () => setRenameTarget(currentFolder),
                  },
                  {
                    key: "move",
                    label: t("moveTo"),
                    icon: <FolderInput className="h-4 w-4" />,
                    onSelect: () =>
                      setMoveTarget({
                        kind: "folder",
                        id: currentFolder.id,
                        name: currentFolder.name,
                        currentParentId: currentFolder.parentId,
                      }),
                  },
                  {
                    key: "delete",
                    label: t("delete"),
                    icon: <Trash2 className="h-4 w-4" />,
                    onSelect: () =>
                      setDeleteTarget({ kind: "folder", id: currentFolder.id, name: currentFolder.name }),
                    variant: "destructive",
                  },
                ]}
              />
            )}
            <UploadButton onFiles={handleFiles} />
            <CreateFolderButton parentId={currentFolderId} />
          </div>
        </div>

        {quota && <QuotaBar quota={quota} />}

        {isEmpty ? (
          <div className="flex flex-col items-center gap-3 py-20 text-muted-foreground">
            <FolderOpen className="h-10 w-10 opacity-25" />
            <p className="text-sm">{t("empty")}</p>
          </div>
        ) : (
          <div className="space-y-6">
            {subFolders.length > 0 && (
              <section>
                <h2 className="mb-3 text-xs font-semibold uppercase tracking-wider text-muted-foreground/60">
                  {t("folders")}
                </h2>
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-5 xl:grid-cols-6">
                  {subFolders.map((folder) => (
                    <FolderTile
                      key={folder.id}
                      folder={folder}
                      onOpen={() => navigate(folder.id)}
                      onRename={() => setRenameTarget(folder)}
                      onMove={() =>
                        setMoveTarget({
                          kind: "folder",
                          id: folder.id,
                          name: folder.name,
                          currentParentId: folder.parentId,
                        })
                      }
                      onDelete={() => setDeleteTarget({ kind: "folder", id: folder.id, name: folder.name })}
                    />
                  ))}
                </div>
              </section>
            )}
            {folderFiles.length > 0 && (
              <section>
                <h2 className="mb-3 text-xs font-semibold uppercase tracking-wider text-muted-foreground/60">
                  {t("files")}
                </h2>
                <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 lg:grid-cols-5 xl:grid-cols-6">
                  {folderFiles.map((file, i) => (
                    <FileTile
                      key={file.id}
                      file={file}
                      category={categoryByExt.get(file.extension)}
                      onPreview={() => setPreviewIndex(i)}
                      onMove={() =>
                        setMoveTarget({
                          kind: "file",
                          id: file.id,
                          name: file.originalName,
                          currentParentId: file.folderId,
                        })
                      }
                      onDelete={() => setDeleteTarget({ kind: "file", id: file.id, name: file.originalName })}
                    />
                  ))}
                </div>
              </section>
            )}
          </div>
        )}
      </div>
      </StorageDropZone>

      <DragOverlay>
        {activeDrag &&
          (() => {
            if (activeDrag.kind === "folder") {
              const folder = folders.find((f) => f.id === activeDrag.id)
              if (!folder) return null
              return (
                <div className="flex w-32 flex-col items-center gap-2 rounded-xl border bg-card p-4 text-center shadow-lg">
                  <Folder className="h-10 w-10 text-primary" />
                  <span className="line-clamp-2 w-full break-words text-sm font-medium">{folder.name}</span>
                </div>
              )
            }
            const file = files.find((f) => f.id === activeDrag.id)
            if (!file) return null
            const Icon = getFileIconByCategory(categoryByExt.get(file.extension))
            return (
              <div className="flex w-32 flex-col items-center gap-2 rounded-xl border bg-card p-4 text-center shadow-lg">
                <Icon className="h-10 w-10 text-muted-foreground" />
                <span className="line-clamp-2 w-full break-words text-sm font-medium">{file.originalName}</span>
              </div>
            )
          })()}
      </DragOverlay>

      <RenameFolderModal folder={renameTarget} onOpenChange={(o) => { if (!o) setRenameTarget(null) }} />
      <MoveToModal target={moveTarget} folders={folders} onOpenChange={(o) => { if (!o) setMoveTarget(null) }} />
      <FilePreviewModal
        files={folderFiles}
        index={previewIndex}
        categoryByExt={categoryByExt}
        onIndexChange={setPreviewIndex}
        onClose={() => setPreviewIndex(null)}
      />
      <ConfirmDialog
        open={deleteTarget !== null}
        onOpenChange={(o) => { if (!o) setDeleteTarget(null) }}
        title={deleteTarget?.kind === "folder" ? t("deleteFolderConfirm") : t("deleteFileConfirm")}
        description={deleteTarget?.kind === "folder" ? t("deleteFolderWarning") : undefined}
        onConfirm={confirmDelete}
        isPending={deleting}
      />
    </DndContext>
  )
}
