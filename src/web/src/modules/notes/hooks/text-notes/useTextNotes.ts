"use client"

import { useQuery } from "@tanstack/react-query"
import { getTextNotesApi } from "@/modules/notes/api/text-notes.api"
import type { PagingList } from "@/shared/types/api.types"
import type { TextNoteDto } from "@/modules/notes/types/notes.types"
import type { PagingRequest } from "@/shared/types/api.types"
import { textNoteKeys } from "./query-keys"

interface UseTextNotesOptions {
  params?: PagingRequest
  initialData?: PagingList<TextNoteDto>
}

export function useTextNotes({ params = {}, initialData }: UseTextNotesOptions = {}) {
  return useQuery({
    queryKey: textNoteKeys.list(params),
    queryFn: () => getTextNotesApi(params),
    initialData,
  })
}
