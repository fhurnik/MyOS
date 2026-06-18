import { apiClient } from "@/shared/lib/api-client"
import type { QuotaDto, AllowedFileTypeDto } from "@/modules/storage/types/storage.types"

const BASE = "/api/v1/storage"

export async function getQuotaApi(token?: string): Promise<QuotaDto> {
  return apiClient<QuotaDto>(`${BASE}/quota`, { token })
}

export async function getAllowedFileTypesApi(token?: string): Promise<AllowedFileTypeDto[]> {
  return apiClient<AllowedFileTypeDto[]>(`${BASE}/allowed-file-types`, { token })
}
