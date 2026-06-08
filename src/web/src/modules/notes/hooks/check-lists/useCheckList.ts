"use client"

import { useQuery } from "@tanstack/react-query"
import { getCheckListApi } from "@/modules/notes/api/check-lists.api"
import type { CheckListDto } from "@/modules/notes/types/notes.types"
import { checkListKeys } from "./query-keys"

export function useCheckList(id: string, initialData?: CheckListDto) {
  return useQuery({
    queryKey: checkListKeys.detail(id),
    queryFn: () => getCheckListApi(id),
    initialData,
    enabled: !!id,
  })
}
