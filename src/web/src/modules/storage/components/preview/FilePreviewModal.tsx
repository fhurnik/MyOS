"use client"

import { useEffect, useRef, useState } from "react"
import { useTranslations } from "next-intl"
import { ChevronLeft, ChevronRight, Download, Maximize2, Loader2, FileQuestion, FileText, ExternalLink } from "lucide-react"
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/shared/components/ui/dialog"
import { Button } from "@/shared/components/ui/button"
import { cn } from "@/shared/lib/utils"
import { fetchFileBlob, fileContentUrl, fileDownloadUrl } from "@/modules/storage/api/files.api"
import type { StoredFileDto } from "@/modules/storage/types/storage.types"

type PreviewMode = "audio" | "video" | "image" | "pdf" | "text" | "none"

function getPreviewMode(extension: string, category: string | undefined): PreviewMode {
  if (extension === "pdf") return "pdf"
  if (category === "audio") return "audio"
  if (category === "video") return "video"
  if (category === "image") return "image"
  if (category === "text") return "text"
  return "none"
}

interface FilePreviewModalProps {
  files: StoredFileDto[]
  index: number | null
  categoryByExt: Map<string, string>
  onIndexChange: (index: number) => void
  onClose: () => void
}

export function FilePreviewModal({ files, index, categoryByExt, onIndexChange, onClose }: FilePreviewModalProps) {
  const t = useTranslations("storage")
  const file = index !== null ? files[index] : undefined
  const mode = file ? getPreviewMode(file.extension, categoryByExt.get(file.extension)) : "none"
  const canPrev = index !== null && index > 0
  const canNext = index !== null && index < files.length - 1

  const [blobUrl, setBlobUrl] = useState<string | null>(null)
  const [text, setText] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState(false)
  const containerRef = useRef<HTMLDivElement | null>(null)

  // Load bytes for blob-based previews (image/pdf/text). Audio/video stream via the content URL.
  useEffect(() => {
    setBlobUrl(null)
    setText(null)
    setError(false)
    if (!file || (mode !== "image" && mode !== "pdf" && mode !== "text")) return

    let cancelled = false
    let createdUrl: string | null = null
    setLoading(true)
    fetchFileBlob(file.id)
      .then(async (blob) => {
        if (cancelled) return
        if (mode === "text") {
          setText(await blob.text())
        } else {
          createdUrl = URL.createObjectURL(blob)
          setBlobUrl(createdUrl)
        }
      })
      .catch(() => {
        if (!cancelled) setError(true)
      })
      .finally(() => {
        if (!cancelled) setLoading(false)
      })

    return () => {
      cancelled = true
      if (createdUrl) URL.revokeObjectURL(createdUrl)
    }
  }, [file?.id, mode])

  // Arrow keys navigate between files (works in fullscreen too, since the listener is on window).
  // Capture phase so the modal Dialog can't swallow the event before us. Audio/video keep
  // their arrows for native seeking.
  useEffect(() => {
    if (file === undefined) return
    function onKey(e: KeyboardEvent) {
      if (mode === "audio" || mode === "video") return
      if (e.key === "ArrowLeft" && index !== null && index > 0) {
        e.preventDefault()
        onIndexChange(index - 1)
      } else if (e.key === "ArrowRight" && index !== null && index < files.length - 1) {
        e.preventDefault()
        onIndexChange(index + 1)
      }
    }
    window.addEventListener("keydown", onKey, true)
    return () => window.removeEventListener("keydown", onKey, true)
  }, [file, mode, index, files.length, onIndexChange])

  return (
    <Dialog open={file !== undefined} onOpenChange={(o) => { if (!o) onClose() }} disablePointerDismissal>
      <DialogContent className="sm:max-w-3xl">
        <DialogHeader>
          <DialogTitle className="truncate pr-8">{file?.originalName}</DialogTitle>
        </DialogHeader>

        <div ref={containerRef} className="flex min-h-[40vh] min-w-0 items-center justify-center overflow-hidden bg-background">
          {loading ? (
            <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
          ) : error ? (
            <p className="text-sm text-destructive">{t("preview.loadError")}</p>
          ) : !file ? null : mode === "audio" ? (
            <audio controls src={fileContentUrl(file.id)} className="w-full" />
          ) : mode === "video" ? (
            <video
              controls
              src={fileContentUrl(file.id)}
              className="max-h-[70vh] w-full max-w-full rounded-lg object-contain [:fullscreen_&]:max-h-screen"
            />
          ) : mode === "image" && blobUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img
              src={blobUrl}
              alt={file.originalName}
              className="max-h-[70vh] w-auto max-w-full object-contain [:fullscreen_&]:max-h-screen"
            />
          ) : mode === "pdf" && blobUrl ? (
            <>
              {/* Desktop has a built-in PDF plugin, so render inline. Mobile browsers (esp. Android
                  Chrome) can't display a PDF in an iframe — they get a tap-to-open fallback that opens
                  the file in a new tab, where the native PDF viewer handles it.
                  #navpanes=0 collapses the built-in viewer's page-thumbnail sidebar by default. */}
              <iframe
                src={`${blobUrl}#navpanes=0`}
                title={file.originalName}
                className="hidden h-[70vh] w-full rounded-lg border md:block"
              />
              <div className="flex flex-col items-center gap-3 p-6 text-muted-foreground md:hidden">
                <FileText className="h-10 w-10 opacity-40" />
                <Button
                  variant="outline"
                  size="sm"
                  nativeButton={false}
                  render={<a href={fileContentUrl(file.id)} target="_blank" rel="noopener noreferrer" />}
                >
                  <ExternalLink className="h-4 w-4" />
                  {t("preview.openPdf")}
                </Button>
              </div>
            </>
          ) : mode === "text" && text !== null ? (
            <pre className="max-h-[70vh] w-full overflow-auto whitespace-pre-wrap rounded-lg bg-muted p-3 text-xs">
              {text}
            </pre>
          ) : (
            <div className="flex flex-col items-center gap-3 text-muted-foreground">
              <FileQuestion className="h-10 w-10 opacity-40" />
              <p className="text-sm">{t("preview.noPreview")}</p>
            </div>
          )}
        </div>

        <DialogFooter className="sm:justify-between">
          <div className="flex items-center gap-1">
            <Button
              variant="outline"
              size="icon-sm"
              aria-label={t("preview.previous")}
              aria-disabled={!canPrev}
              // Not the `disabled` attribute: disabling a focused button bounces focus (and its ring)
              // onto the next control, which made arrow nav look glitchy. Dim + guard instead, and
              // don't take focus on mouse click. Keyboard nav runs through the window listener.
              onMouseDown={(e) => e.preventDefault()}
              onClick={() => index !== null && index > 0 && onIndexChange(index - 1)}
              className={cn("focus-visible:border-border focus-visible:ring-0", !canPrev && "pointer-events-none opacity-40")}
            >
              <ChevronLeft className="h-4 w-4" />
            </Button>
            <Button
              variant="outline"
              size="icon-sm"
              aria-label={t("preview.next")}
              aria-disabled={!canNext}
              onMouseDown={(e) => e.preventDefault()}
              onClick={() => index !== null && index < files.length - 1 && onIndexChange(index + 1)}
              className={cn("focus-visible:border-border focus-visible:ring-0", !canNext && "pointer-events-none opacity-40")}
            >
              <ChevronRight className="h-4 w-4" />
            </Button>
          </div>
          <div className="flex items-center gap-2">
            {(mode === "image" || mode === "video") && (
              <Button
                variant="outline"
                size="icon-sm"
                aria-label={t("preview.fullscreen")}
                onClick={() => containerRef.current?.requestFullscreen?.()}
              >
                <Maximize2 className="h-4 w-4" />
              </Button>
            )}
            {file && (
              <Button variant="outline" size="sm" nativeButton={false} render={<a href={fileDownloadUrl(file.id)} download={file.originalName} />}>
                <Download className="h-4 w-4" />
                {t("download")}
              </Button>
            )}
          </div>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  )
}
