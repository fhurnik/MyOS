import { apiClient } from "@/shared/lib/api-client"
import type { StoredFileDto, MoveFileBody } from "@/modules/storage/types/storage.types"

const BASE = "/api/v1/storage/files"

export async function getFilesApi(token?: string): Promise<StoredFileDto[]> {
  return apiClient<StoredFileDto[]>(BASE, { token })
}

export async function moveFileApi(id: string, body: MoveFileBody): Promise<void> {
  return apiClient<void>(`${BASE}/${id}/move`, { method: "PUT", body })
}

export async function deleteFileApi(id: string): Promise<void> {
  return apiClient<void>(`${BASE}/${id}`, { method: "DELETE" })
}

// Browser-navigable URL for saving a file to disk (proxy injects the Authorization header).
export function fileDownloadUrl(id: string): string {
  return `${BASE}/${id}/download`
}

// Browser-navigable URL for inline streaming (audio/video only — used in Stage 3 preview).
export function fileContentUrl(id: string): string {
  return `${BASE}/${id}/content`
}

// Fetches raw file bytes for client-side preview (image/pdf/text). apiClient only parses JSON,
// so this uses fetch directly; relative URL → proxy injects the Authorization header.
export async function fetchFileBlob(id: string): Promise<Blob> {
  const res = await fetch(fileDownloadUrl(id), { credentials: "include" })
  if (!res.ok) throw new Error("Failed to load file")
  return res.blob()
}
