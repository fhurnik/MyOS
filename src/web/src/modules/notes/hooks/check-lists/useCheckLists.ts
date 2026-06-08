"use client"

import { useQuery } from "@tanstack/react-query"
import { getCheckListsApi } from "@/modules/notes/api/check-lists.api"
import type { PagingList, PagingRequest } from "@/shared/types/api.types"
import type { CheckListSummaryDto } from "@/modules/notes/types/notes.types"
import { checkListKeys } from "./query-keys"

interface UseCheckListsOptions {
  params?: PagingRequest
  initialData?: PagingList<CheckListSummaryDto>
}

export function useCheckLists({ params = {}, initialData }: UseCheckListsOptions = {}) {
  return useQuery({
    queryKey: checkListKeys.list(params),
    queryFn: () => getCheckListsApi(params),
    initialData,
  })
}
