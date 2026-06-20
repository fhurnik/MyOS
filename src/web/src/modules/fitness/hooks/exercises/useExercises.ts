"use client"

import { useQuery } from "@tanstack/react-query"
import { getExercisesApi } from "@/modules/fitness/api/exercises.api"
import { exerciseKeys } from "@/modules/fitness/api/query-keys"
import type { PagingList, PagingRequest } from "@/shared/types/api.types"
import type { ExerciseDto, ExerciseFilterParams } from "@/modules/fitness/types/fitness.types"

interface UseExercisesOptions {
  params?: PagingRequest & ExerciseFilterParams
  initialData?: PagingList<ExerciseDto>
}

export function useExercises({ params = {}, initialData }: UseExercisesOptions = {}) {
  return useQuery({
    queryKey: exerciseKeys.list(params),
    queryFn: () => getExercisesApi(params),
    initialData,
  })
}
