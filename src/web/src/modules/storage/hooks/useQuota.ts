"use client"

import { useQuery } from "@tanstack/react-query"
import { getQuotaApi } from "@/modules/storage/api/storage.api"
import type { QuotaDto } from "@/modules/storage/types/storage.types"
import { storageKeys } from "./query-keys"

export function useQuota(initialData?: QuotaDto) {
  return useQuery({
    queryKey: storageKeys.quota(),
    queryFn: () => getQuotaApi(),
    initialData,
  })
}
