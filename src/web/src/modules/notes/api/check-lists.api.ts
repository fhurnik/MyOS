import { apiClient } from "@/shared/lib/api-client"
import type { PagingList, PagingRequest } from "@/shared/types/api.types"
import type {
  CheckListDto,
  CheckListSummaryDto,
  CreateCheckListBody,
  UpdateCheckListTitleBody,
  AddCheckListItemBody,
  UpdateCheckListItemBody,
  ReorderCheckListItemBody,
} from "@/modules/notes/types/notes.types"

const BASE = "/api/v1/notes/checklists"

export async function getCheckListsApi(
  params: PagingRequest = {},
  token?: string
): Promise<PagingList<CheckListSummaryDto>> {
  const query = new URLSearchParams()
  if (params.page) query.set("page", String(params.page))
  if (params.pageSize) query.set("pageSize", String(params.pageSize))
  if (params.orderBy) query.set("orderBy", params.orderBy)
  const qs = query.toString()
  return apiClient<PagingList<CheckListSummaryDto>>(`${BASE}${qs ? `?${qs}` : ""}`, { token })
}

export async function getCheckListApi(id: string, token?: string): Promise<CheckListDto> {
  return apiClient<CheckListDto>(`${BASE}/${id}`, { token })
}

export async function createCheckListApi(body: CreateCheckListBody): Promise<string> {
  return apiClient<string>(BASE, { method: "POST", body })
}

export async function updateCheckListTitleApi(
  id: string,
  body: UpdateCheckListTitleBody
): Promise<void> {
  return apiClient<void>(`${BASE}/${id}`, { method: "PUT", body })
}

export async function deleteCheckListApi(id: string): Promise<void> {
  return apiClient<void>(`${BASE}/${id}`, { method: "DELETE" })
}

export async function addCheckListItemApi(id: string, body: AddCheckListItemBody): Promise<string> {
  return apiClient<string>(`${BASE}/${id}/items`, { method: "POST", body })
}

export async function updateCheckListItemApi(
  id: string,
  itemId: string,
  body: UpdateCheckListItemBody
): Promise<void> {
  return apiClient<void>(`${BASE}/${id}/items/${itemId}`, { method: "PUT", body })
}

export async function deleteCheckListItemApi(id: string, itemId: string): Promise<void> {
  return apiClient<void>(`${BASE}/${id}/items/${itemId}`, { method: "DELETE" })
}

export async function toggleCheckListItemApi(id: string, itemId: string): Promise<void> {
  return apiClient<void>(`${BASE}/${id}/items/${itemId}/toggle`, { method: "PATCH" })
}

export async function reorderCheckListItemApi(
  id: string,
  itemId: string,
  body: ReorderCheckListItemBody
): Promise<void> {
  return apiClient<void>(`${BASE}/${id}/items/${itemId}/reorder`, { method: "PATCH", body })
}
