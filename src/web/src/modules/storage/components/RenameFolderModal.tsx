"use client"

import { useEffect, useMemo } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useTranslations } from "next-intl"
import { createFolderSchema, type FolderFormValues } from "@/modules/storage/schemas/folder.schema"
import { useRenameFolder } from "@/modules/storage/hooks/useFolderMutations"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog"
import { Button } from "@/shared/components/ui/button"
import { Input } from "@/shared/components/ui/input"
import { Label } from "@/shared/components/ui/label"
import type { FolderDto } from "@/modules/storage/types/storage.types"

interface RenameFolderModalProps {
  folder: FolderDto | null
  onOpenChange: (open: boolean) => void
}

export function RenameFolderModal({ folder, onOpenChange }: RenameFolderModalProps) {
  const t = useTranslations("storage")
  const tCommon = useTranslations("common")
  const { mutate: rename, isPending } = useRenameFolder()

  const schema = useMemo(() => createFolderSchema({ nameRequired: t("validation.nameRequired") }), [t])
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<FolderFormValues>({
    resolver: zodResolver(schema),
    defaultValues: { name: "" },
  })

  useEffect(() => {
    if (folder) reset({ name: folder.name })
  }, [folder, reset])

  function onSubmit(values: FolderFormValues) {
    if (!folder) return
    rename({ id: folder.id, name: values.name }, { onSuccess: () => onOpenChange(false) })
  }

  return (
    <Dialog open={folder !== null} onOpenChange={(o) => { if (!isPending && !o) onOpenChange(false) }}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t("renameFolder")}</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-1.5">
            <Label htmlFor="rename-folder-name">{t("folderName")}</Label>
            <Input id="rename-folder-name" autoFocus aria-invalid={!!errors.name} {...register("name")} />
            {errors.name && <p className="text-sm text-destructive">{errors.name.message}</p>}
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)} disabled={isPending}>
              {tCommon("cancel")}
            </Button>
            <Button type="submit" disabled={isPending}>
              {isPending ? "…" : tCommon("save")}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
