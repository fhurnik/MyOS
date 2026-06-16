import { apiClient } from "@/shared/lib/api-client"
import { buildPagingParams } from "@/shared/lib/paging"
import type { PagingList, PagingRequest } from "@/shared/types/api.types"
import type {
  TextNoteDto,
  CreateTextNoteBody,
  UpdateTextNoteBody,
} from "@/modules/notes/types/notes.types"

const BASE = "/api/v1/notes/text"

export async function getTextNotesApi(
  params: PagingRequest = {},
  token?: string
): Promise<PagingList<TextNoteDto>> {
  return apiClient<PagingList<TextNoteDto>>(`${BASE}${buildPagingParams(params)}`, { token })
}

export async function getTextNoteApi(id: string, token?: string): Promise<TextNoteDto> {
  return apiClient<TextNoteDto>(`${BASE}/${id}`, { token })
}

export async function createTextNoteApi(body: CreateTextNoteBody): Promise<string> {
  return apiClient<string>(BASE, { method: "POST", body })
}

export async function updateTextNoteApi(id: string, body: UpdateTextNoteBody): Promise<void> {
  return apiClient<void>(`${BASE}/${id}`, { method: "PUT", body })
}

export async function deleteTextNoteApi(id: string): Promise<void> {
  return apiClient<void>(`${BASE}/${id}`, { method: "DELETE" })
}
