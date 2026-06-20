"use client"

import { useQuery } from "@tanstack/react-query"
import { getWeeklySetsApi } from "@/modules/fitness/api/stats.api"
import { statsKeys } from "@/modules/fitness/api/query-keys"
import type { WeeklySetsDto } from "@/modules/fitness/types/fitness.types"

interface UseWeeklySetsOptions {
  exerciseId?: string
  initialData?: WeeklySetsDto[]
}

export function useWeeklySets({ exerciseId, initialData }: UseWeeklySetsOptions = {}) {
  return useQuery({
    queryKey: statsKeys.weeklySets(exerciseId),
    queryFn: () => getWeeklySetsApi(exerciseId),
    initialData,
  })
}
