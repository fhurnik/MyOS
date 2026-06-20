"use client"

import { useQuery } from "@tanstack/react-query"
import { getExerciseProgressionApi } from "@/modules/fitness/api/exercises.api"
import { exerciseKeys } from "@/modules/fitness/api/query-keys"
import type { ProgressionDto } from "@/modules/fitness/types/fitness.types"

export function useExerciseProgression(id: string, initialData?: ProgressionDto) {
  return useQuery({
    queryKey: exerciseKeys.progression(id),
    queryFn: () => getExerciseProgressionApi(id),
    initialData,
    enabled: !!id,
  })
}
