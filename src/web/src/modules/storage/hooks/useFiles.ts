"use client"

import { useQuery } from "@tanstack/react-query"
import { getFilesApi } from "@/modules/storage/api/files.api"
import type { StoredFileDto } from "@/modules/storage/types/storage.types"
import { storageKeys } from "./query-keys"

export function useFiles(initialData?: StoredFileDto[]) {
  return useQuery({
    queryKey: storageKeys.files(),
    queryFn: () => getFilesApi(),
    initialData,
  })
}
