import type { PagingRequest } from "@/shared/types/api.types"

export function buildPagingParams(params: PagingRequest): string {
  const query = new URLSearchParams()
  if (params.page !== undefined) query.set("page", String(params.page))
  if (params.pageSize !== undefined) query.set("pageSize", String(params.pageSize))
  if (params.orderBy) query.set("orderBy", params.orderBy)
  if (params.orderByDesc) query.set("orderByDesc", "true")
  const qs = query.toString()
  return qs ? `?${qs}` : ""
}
