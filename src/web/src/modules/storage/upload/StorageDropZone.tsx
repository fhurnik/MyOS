"use client"

import { useRef, useState, type DragEvent, type ReactNode } from "react"
import { useTranslations } from "next-intl"
import { UploadCloud } from "lucide-react"

// Native OS file drag&drop (DataTransfer with files) — distinct from @dnd-kit's
// pointer-based tile dragging, so the two never conflict.
function hasFiles(e: DragEvent): boolean {
  return Array.from(e.dataTransfer?.types ?? []).includes("Files")
}

export function StorageDropZone({
  onFiles,
  children,
}: {
  onFiles: (files: File[]) => void
  children: ReactNode
}) {
  const t = useTranslations("storage")
  const [active, setActive] = useState(false)
  const depth = useRef(0)

  return (
    <div
      // Fill the whole main content area so files can be dropped anywhere on the page
      // (not only over the tiles). The navbar/sidebar/mobile header live outside <main>.
      className="relative min-h-[calc(100dvh-var(--mobile-header-h)-2rem)] md:min-h-[calc(100dvh-3rem)]"
      onDragEnter={(e) => {
        if (!hasFiles(e)) return
        e.preventDefault()
        depth.current += 1
        setActive(true)
      }}
      onDragOver={(e) => {
        if (!hasFiles(e)) return
        e.preventDefault()
      }}
      onDragLeave={(e) => {
        if (!hasFiles(e)) return
        depth.current -= 1
        if (depth.current <= 0) {
          depth.current = 0
          setActive(false)
        }
      }}
      onDrop={(e) => {
        if (!hasFiles(e)) return
        e.preventDefault()
        depth.current = 0
        setActive(false)
        onFiles(Array.from(e.dataTransfer.files))
      }}
    >
      {children}
      {active && (
        <div className="pointer-events-none absolute inset-0 z-20 flex flex-col items-center justify-center gap-2 rounded-xl border-2 border-dashed border-primary bg-primary/10 text-sm font-medium text-primary">
          <UploadCloud className="h-8 w-8" />
          {t("upload.dropHint")}
        </div>
      )}
    </div>
  )
}
