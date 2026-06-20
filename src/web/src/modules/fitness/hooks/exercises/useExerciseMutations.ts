"use client"

import { useMutation, useQueryClient } from "@tanstack/react-query"
import {
  createExerciseApi,
  updateExerciseApi,
  deleteExerciseApi,
  setExerciseTargetApi,
} from "@/modules/fitness/api/exercises.api"
import { exerciseKeys } from "@/modules/fitness/api/query-keys"
import type {
  CreateExerciseBody,
  UpdateExerciseBody,
  SetTargetBody,
} from "@/modules/fitness/types/fitness.types"

export function useCreateExercise() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: CreateExerciseBody) => createExerciseApi(body),
    onSuccess: () => qc.invalidateQueries({ queryKey: exerciseKeys.lists() }),
  })
}

export function useUpdateExercise(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: UpdateExerciseBody) => updateExerciseApi(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: exerciseKeys.lists() })
      qc.invalidateQueries({ queryKey: exerciseKeys.detail(id) })
    },
  })
}

export function useDeleteExercise() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => deleteExerciseApi(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: exerciseKeys.lists() }),
  })
}

export function useSetExerciseTarget(id: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (body: SetTargetBody) => setExerciseTargetApi(id, body),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: exerciseKeys.detail(id) })
      qc.invalidateQueries({ queryKey: exerciseKeys.progression(id) })
    },
  })
}
