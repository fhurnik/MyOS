"use client"

import { useQuery } from "@tanstack/react-query"
import { getExerciseApi } from "@/modules/fitness/api/exercises.api"
import { exerciseKeys } from "@/modules/fitness/api/query-keys"
import type { ExerciseDto } from "@/modules/fitness/types/fitness.types"

export function useExercise(id: string, initialData?: ExerciseDto) {
  return useQuery({
    queryKey: exerciseKeys.detail(id),
    queryFn: () => getExerciseApi(id),
    initialData,
    enabled: !!id,
  })
}
