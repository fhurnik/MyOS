"use client"

import { useQuery } from "@tanstack/react-query"
import { getAllowedFileTypesApi } from "@/modules/storage/api/storage.api"
import type { AllowedFileTypeDto } from "@/modules/storage/types/storage.types"
import { storageKeys } from "./query-keys"

export function useAllowedFileTypes(initialData?: AllowedFileTypeDto[]) {
  return useQuery({
    queryKey: storageKeys.allowedTypes(),
    queryFn: () => getAllowedFileTypesApi(),
    initialData,
    staleTime: 1000 * 60 * 60, // allowed types rarely change
  })
}
