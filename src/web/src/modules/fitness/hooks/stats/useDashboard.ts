"use client"

import { useQuery } from "@tanstack/react-query"
import { getDashboardApi } from "@/modules/fitness/api/stats.api"
import { statsKeys } from "@/modules/fitness/api/query-keys"
import type { UserDashboardDto } from "@/modules/fitness/types/fitness.types"

export function useDashboard(initialData?: UserDashboardDto) {
  return useQuery({
    queryKey: statsKeys.dashboard(),
    queryFn: () => getDashboardApi(),
    initialData,
  })
}
