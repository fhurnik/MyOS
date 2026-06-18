"use client"

import { useMutation, useQueryClient } from "@tanstack/react-query"
import { moveFileApi, deleteFileApi } from "@/modules/storage/api/files.api"
import { storageKeys } from "./query-keys"

export function useMoveFile() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, folderId }: { id: string; folderId: string | null }) =>
      moveFileApi(id, { folderId }),
    onSuccess: () => qc.invalidateQueries({ queryKey: storageKeys.files() }),
  })
}

export function useDeleteFile() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: deleteFileApi,
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: storageKeys.files() })
      qc.invalidateQueries({ queryKey: storageKeys.quota() })
    },
  })
}
