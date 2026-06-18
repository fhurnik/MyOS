"use client"

import { useTranslations } from "next-intl"
import { CheckCircle2, AlertCircle, Loader2, X, UploadCloud } from "lucide-react"
import { useUpload } from "./UploadProvider"

export function UploadPanel() {
  const t = useTranslations("storage")
  const { items, clearFinished } = useUpload()

  if (items.length === 0) return null
  const doneCount = items.filter((i) => i.status === "done").length

  return (
    <div className="fixed bottom-4 right-4 z-50 w-[calc(100vw-2rem)] max-w-sm overflow-hidden rounded-xl border bg-card shadow-lg">
      <div className="flex items-center justify-between border-b px-3 py-2">
        <div className="flex items-center gap-2 text-sm font-medium">
          <UploadCloud className="h-4 w-4" />
          {t("upload.title")} {doneCount}/{items.length}
        </div>
        <button
          onClick={clearFinished}
          aria-label={t("upload.clear")}
          title={t("upload.clear")}
          className="rounded p-1 text-muted-foreground transition-colors hover:bg-muted hover:text-foreground"
        >
          <X className="h-4 w-4" />
        </button>
      </div>
      <ul className="max-h-64 divide-y overflow-y-auto">
        {items.map((item) => (
          <li key={item.id} className="flex items-center gap-2 px-3 py-2 text-sm">
            <span className="shrink-0">
              {item.status === "done" ? (
                <CheckCircle2 className="h-4 w-4 text-primary" />
              ) : item.status === "error" ? (
                <AlertCircle className="h-4 w-4 text-destructive" />
              ) : item.status === "uploading" ? (
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
              ) : (
                <Loader2 className="h-4 w-4 text-muted-foreground/40" />
              )}
            </span>
            <div className="min-w-0 flex-1">
              <p className="truncate">{item.fileName}</p>
              {item.status === "uploading" && (
                <div className="mt-1 h-1 w-full overflow-hidden rounded-full bg-muted">
                  <div className="h-full rounded-full bg-primary transition-all" style={{ width: `${item.progress}%` }} />
                </div>
              )}
              {item.status === "error" && <p className="truncate text-xs text-destructive">{item.error}</p>}
            </div>
            <span className="shrink-0 text-xs text-muted-foreground">
              {item.status === "uploading"
                ? `${item.progress}%`
                : item.status === "queued"
                  ? t("upload.queued")
                  : item.status === "done"
                    ? t("upload.done")
                    : t("upload.error")}
            </span>
          </li>
        ))}
      </ul>
    </div>
  )
}
