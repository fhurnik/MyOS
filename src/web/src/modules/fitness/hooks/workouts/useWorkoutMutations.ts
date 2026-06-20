"use client"

import { useMutation, useQueryClient } from "@tanstack/react-query"
import {
  createWorkoutApi,
  updateWorkoutApi,
  deleteWorkoutApi,
} from "@/modules/fitness/api/workouts.api"
import { workoutKeys } from "@/modules/fitness/api/query-keys"
import type { CreateWorkoutBody, UpdateWorkoutBody } from "@/modules/fitness/types/fitness.types"

export function useCreateWorkout() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: CreateWorkoutBody) => createWorkoutApi(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: workoutKeys.lists() }),
  })
}

export function useUpdateWorkout(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: UpdateWorkoutBody) => updateWorkoutApi(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: workoutKeys.lists() })
      qc.invalidateQueries({ queryKey: workoutKeys.detail(id) })
    },
  })
}

export function useDeleteWorkout() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => deleteWorkoutApi(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: workoutKeys.lists() }),
  })
}
