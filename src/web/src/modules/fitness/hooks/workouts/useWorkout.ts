"use client"

import { useQuery } from "@tanstack/react-query"
import { getWorkoutApi } from "@/modules/fitness/api/workouts.api"
import { workoutKeys } from "@/modules/fitness/api/query-keys"
import type { WorkoutDto } from "@/modules/fitness/types/fitness.types"

export function useWorkout(id: string, initialData?: WorkoutDto) {
  return useQuery({
    queryKey: workoutKeys.detail(id),
    queryFn: () => getWorkoutApi(id),
    initialData,
    enabled: !!id,
  })
}
