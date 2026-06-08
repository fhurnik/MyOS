import type { PagingRequest } from "@/shared/types/api.types"

export const checkListKeys = {
  all: ["check-lists"] as const,
  lists: () => [...checkListKeys.all, "list"] as const,
  list: (params: PagingRequest) => [...checkListKeys.lists(), params] as const,
  detail: (id: string) => [...checkListKeys.all, "detail", id] as const,
}
