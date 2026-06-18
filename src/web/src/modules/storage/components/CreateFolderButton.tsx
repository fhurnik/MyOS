"use client"

import { useMemo, useState } from "react"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { useTranslations } from "next-intl"
import { FolderPlus } from "lucide-react"
import { createFolderSchema, type FolderFormValues } from "@/modules/storage/schemas/folder.schema"
import { useCreateFolder } from "@/modules/storage/hooks/useFolderMutations"
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

export function CreateFolderButton({ parentId }: { parentId: string | null }) {
  const t = useTranslations("storage")
  const tCommon = useTranslations("common")
  const [open, setOpen] = useState(false)
  const { mutate: create, isPending } = useCreateFolder()

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

  function onSubmit(values: FolderFormValues) {
    create({ name: values.name, parentId }, {
      onSuccess: () => {
        setOpen(false)
        reset()
      },
    })
  }

  function handleOpenChange(value: boolean) {
    if (!isPending) {
      reset()
      setOpen(value)
    }
  }

  return (
    <>
      <Button size="sm" onClick={() => setOpen(true)} className="shrink-0">
        <FolderPlus className="h-4 w-4" />
        {t("newFolder")}
      </Button>
      <Dialog open={open} onOpenChange={handleOpenChange}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{t("newFolder")}</DialogTitle>
          </DialogHeader>
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <div className="space-y-1.5">
              <Label htmlFor="create-folder-name">{t("folderName")}</Label>
              <Input id="create-folder-name" autoFocus aria-invalid={!!errors.name} {...register("name")} />
              {errors.name && <p className="text-sm text-destructive">{errors.name.message}</p>}
            </div>
            <DialogFooter>
              <Button type="button" variant="outline" onClick={() => handleOpenChange(false)} disabled={isPending}>
                {tCommon("cancel")}
              </Button>
              <Button type="submit" disabled={isPending}>
                {isPending ? "…" : tCommon("save")}
              </Button>
            </DialogFooter>
          </form>
        </DialogContent>
      </Dialog>
    </>
  )
}
