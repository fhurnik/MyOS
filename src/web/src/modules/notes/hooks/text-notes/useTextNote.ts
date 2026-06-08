"use client"

import { useQuery } from "@tanstack/react-query"
import { getTextNoteApi } from "@/modules/notes/api/text-notes.api"
import type { TextNoteDto } from "@/modules/notes/types/notes.types"
import { textNoteKeys } from "./query-keys"

export function useTextNote(id: string, initialData?: TextNoteDto) {
  return useQuery({
    queryKey: textNoteKeys.detail(id),
    queryFn: () => getTextNoteApi(id),
    initialData,
    enabled: !!id,
  })
}
