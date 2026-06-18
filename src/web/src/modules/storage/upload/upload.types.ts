export type UploadStatus = "queued" | "uploading" | "done" | "error"

export interface UploadItem {
  id: string
  file: File
  fileName: string
  sizeBytes: number
  folderId: string | null
  progress: number
  status: UploadStatus
  error?: string
}
