"use client"

import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from "react"
import { useQueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { storageKeys } from "@/modules/storage/hooks/query-keys"
import { uploadFileXhr } from "./upload.api"
import type { UploadItem } from "./upload.types"

interface UploadContextValue {
  items: UploadItem[]
  enqueue: (files: File[], folderId: string | null) => void
  clearFinished: () => void
}

const UploadContext = createContext<UploadContextValue | null>(null)

export function useUpload(): UploadContextValue {
  const ctx = useContext(UploadContext)
  if (!ctx) throw new Error("useUpload must be used within an UploadProvider")
  return ctx
}

export function UploadProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<UploadItem[]>([])
  const qc = useQueryClient()
  const uploadingRef = useRef(false)

  const enqueue = useCallback((files: File[], folderId: string | null) => {
    if (files.length === 0) return
    setItems((prev) => [
      ...prev,
      ...files.map((file) => ({
        id: crypto.randomUUID(),
        file,
        fileName: file.name,
        sizeBytes: file.size,
        folderId,
        progress: 0,
        status: "queued" as const,
      })),
    ])
  }, [])

  const clearFinished = useCallback(() => {
    setItems((prev) => prev.filter((i) => i.status === "queued" || i.status === "uploading"))
  }, [])

  // Auto-dismiss the panel ~1 min after everything finished. Any new/changed item resets the timer.
  useEffect(() => {
    if (items.length === 0) return
    const active = items.some((i) => i.status === "queued" || i.status === "uploading")
    if (active) return
    const id = setTimeout(() => clearFinished(), 60_000)
    return () => clearTimeout(id)
  }, [items, clearFinished])

  // Process the queue one file at a time. Each status change re-runs this effect,
  // which picks the next queued item once nothing is in flight.
  useEffect(() => {
    if (uploadingRef.current) return
    const next = items.find((i) => i.status === "queued")
    if (!next) return

    uploadingRef.current = true
    setItems((prev) => prev.map((i) => (i.id === next.id ? { ...i, status: "uploading" } : i)))

    uploadFileXhr(next.file, next.folderId, (percent) => {
      setItems((prev) => prev.map((i) => (i.id === next.id ? { ...i, progress: percent } : i)))
    })
      .then(() => {
        setItems((prev) => prev.map((i) => (i.id === next.id ? { ...i, status: "done", progress: 100 } : i)))
        qc.invalidateQueries({ queryKey: storageKeys.files() })
        qc.invalidateQueries({ queryKey: storageKeys.quota() })
      })
      .catch((err: Error) => {
        setItems((prev) => prev.map((i) => (i.id === next.id ? { ...i, status: "error", error: err.message } : i)))
        toast.error(err.message)
      })
      .finally(() => {
        uploadingRef.current = false
      })
  }, [items, qc])

  return (
    <UploadContext.Provider value={{ items, enqueue, clearFinished }}>{children}</UploadContext.Provider>
  )
}
