"use client"

import { useMutation, useQueryClient } from "@tanstack/react-query"
import { createTextNoteApi } from "@/modules/notes/api/text-notes.api"
import { textNoteKeys } from "./query-keys"

export function useCreateTextNote() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: createTextNoteApi,
    onSuccess: () => qc.invalidateQueries({ queryKey: textNoteKeys.lists() }),
  })
}
