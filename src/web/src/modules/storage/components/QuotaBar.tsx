"use client"

import { useTranslations } from "next-intl"
import { formatBytes } from "@/shared/lib/format"
import { cn } from "@/shared/lib/utils"
import type { QuotaDto } from "@/modules/storage/types/storage.types"

export function QuotaBar({ quota }: { quota: QuotaDto }) {
  const t = useTranslations("storage")
  const percent = quota.maxBytes > 0 ? Math.min(100, (quota.usedBytes / quota.maxBytes) * 100) : 0
  const nearFull = percent >= 90

  return (
    <div className="space-y-1.5">
      <div className="flex items-center justify-between text-xs text-muted-foreground">
        <span>{t("quotaUsed", { used: formatBytes(quota.usedBytes), total: formatBytes(quota.maxBytes) })}</span>
        <span>{percent.toFixed(0)}%</span>
      </div>
      <div className="h-2 w-full overflow-hidden rounded-full bg-muted">
        <div
          className={cn("h-full rounded-full transition-all", nearFull ? "bg-destructive" : "bg-primary")}
          style={{ width: `${percent}%` }}
        />
      </div>
    </div>
  )
}
