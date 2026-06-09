"use client"

import { useTranslations } from "next-intl"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/shared/components/ui/dialog"
import { Button } from "@/shared/components/ui/button"

interface ConfirmDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  title: string
  description?: string
  onConfirm: () => void
  isPending?: boolean
}

export function ConfirmDialog({
  open,
  onOpenChange,
  title,
  description,
  onConfirm,
  isPending = false,
}: ConfirmDialogProps) {
  const t = useTranslations("common")

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent showCloseButton={false}>
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          {description && <DialogDescription>{description}</DialogDescription>}
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)} disabled={isPending}>
            {t("cancel")}
          </Button>
          <Button variant="destructive" onClick={onConfirm} disabled={isPending}>
            {t("delete")}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
