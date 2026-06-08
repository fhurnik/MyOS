"use client"

import { useMutation, useQueryClient } from "@tanstack/react-query"
import { deleteTextNoteApi } from "@/modules/notes/api/text-notes.api"
import { textNoteKeys } from "./query-keys"

export function useDeleteTextNote() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: deleteTextNoteApi,
    onSuccess: () => qc.invalidateQueries({ queryKey: textNoteKeys.lists() }),
  })
}
