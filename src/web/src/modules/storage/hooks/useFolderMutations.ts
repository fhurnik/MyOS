"use client"

import { useMutation, useQueryClient } from "@tanstack/react-query"
import {
  createFolderApi,
  renameFolderApi,
  moveFolderApi,
  deleteFolderApi,
} from "@/modules/storage/api/folders.api"
import { storageKeys } from "./query-keys"

export function useCreateFolder() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: createFolderApi,
    onSuccess: () => qc.invalidateQueries({ queryKey: storageKeys.folders() }),
  })
}

export function useRenameFolder() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, name }: { id: string; name: string }) => renameFolderApi(id, { name }),
    onSuccess: () => qc.invalidateQueries({ queryKey: storageKeys.folders() }),
  })
}

export function useMoveFolder() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, parentId }: { id: string; parentId: string | null }) =>
      moveFolderApi(id, { parentId }),
    onSuccess: () => qc.invalidateQueries({ queryKey: storageKeys.folders() }),
  })
}

export function useDeleteFolder() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: deleteFolderApi,
    onSuccess: () => {
      // Cascade soft-deletes descendant folders and their files, freeing quota.
      qc.invalidateQueries({ queryKey: storageKeys.folders() })
      qc.invalidateQueries({ queryKey: storageKeys.files() })
      qc.invalidateQueries({ queryKey: storageKeys.quota() })
    },
  })
}
