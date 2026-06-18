"use client"

import { useQuery } from "@tanstack/react-query"
import { getFoldersApi } from "@/modules/storage/api/folders.api"
import type { FolderDto } from "@/modules/storage/types/storage.types"
import { storageKeys } from "./query-keys"

export function useFolders(initialData?: FolderDto[]) {
  return useQuery({
    queryKey: storageKeys.folders(),
    queryFn: () => getFoldersApi(),
    initialData,
  })
}
