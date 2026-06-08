import type { PagingRequest } from "@/shared/types/api.types"

export const textNoteKeys = {
  all: ["text-notes"] as const,
  lists: () => [...textNoteKeys.all, "list"] as const,
  list: (params: PagingRequest) => [...textNoteKeys.lists(), params] as const,
  detail: (id: string) => [...textNoteKeys.all, "detail", id] as const,
}
