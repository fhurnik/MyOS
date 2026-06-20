"use client"

import { useQuery } from "@tanstack/react-query"
import { getWorkoutsApi } from "@/modules/fitness/api/workouts.api"
import { workoutKeys } from "@/modules/fitness/api/query-keys"
import type { PagingList, PagingRequest } from "@/shared/types/api.types"
import type { WorkoutSummaryDto } from "@/modules/fitness/types/fitness.types"

interface UseWorkoutsOptions {
  params?: PagingRequest
  initialData?: PagingList<WorkoutSummaryDto>
}

export function useWorkouts({ params = {}, initialData }: UseWorkoutsOptions = {}) {
  return useQuery({
    queryKey: workoutKeys.list(params),
    queryFn: () => getWorkoutsApi(params),
    initialData,
  })
}
