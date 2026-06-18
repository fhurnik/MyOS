import { apiClient } from "@/shared/lib/api-client"
import type {
  FolderDto,
  CreateFolderBody,
  RenameFolderBody,
  MoveFolderBody,
} from "@/modules/storage/types/storage.types"

const BASE = "/api/v1/storage/folders"

export async function getFoldersApi(token?: string): Promise<FolderDto[]> {
  return apiClient<FolderDto[]>(BASE, { token })
}

export async function createFolderApi(body: CreateFolderBody): Promise<string> {
  return apiClient<string>(BASE, { method: "POST", body })
}

export async function renameFolderApi(id: string, body: RenameFolderBody): Promise<void> {
  return apiClient<void>(`${BASE}/${id}`, { method: "PUT", body })
}

export async function moveFolderApi(id: string, body: MoveFolderBody): Promise<void> {
  return apiClient<void>(`${BASE}/${id}/move`, { method: "PUT", body })
}

export async function deleteFolderApi(id: string): Promise<void> {
  return apiClient<void>(`${BASE}/${id}`, { method: "DELETE" })
}
