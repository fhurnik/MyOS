"use client"

import { useMutation, useQueryClient } from "@tanstack/react-query"
import { updateTextNoteApi } from "@/modules/notes/api/text-notes.api"
import { textNoteKeys } from "./query-keys"

export function useUpdateTextNote(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: Parameters<typeof updateTextNoteApi>[1]) =>
      updateTextNoteApi(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: textNoteKeys.lists() })
      qc.invalidateQueries({ queryKey: textNoteKeys.detail(id) })
    },
  })
}
