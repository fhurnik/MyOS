"use client"

import { useMutation, useQueryClient } from "@tanstack/react-query"
import {
  addExerciseToWorkoutApi,
  removeExerciseFromWorkoutApi,
  updateDurationApi,
  addSetApi,
  updateSetApi,
  removeSetApi,
} from "@/modules/fitness/api/workouts.api"
import { workoutKeys, statsKeys, exerciseKeys } from "@/modules/fitness/api/query-keys"
import type {
  AddExerciseToWorkoutBody,
  SetBody,
} from "@/modules/fitness/types/fitness.types"

// Matches every exercise progression query regardless of id.
const PROGRESSION_PREFIX = [...exerciseKeys.all, "progression"] as const

/**
 * Grouped mutations for editing a single workout's exercise entries and sets.
 * Every mutation invalidates the workout detail plus stats/progression, since logging
 * a set changes both the workout graph and the derived progression/volume charts.
 */
export function useWorkoutEntryMutations(workoutId: string) {
  const qc = useQueryClient()

  function invalidateAll() {
    qc.invalidateQueries({ queryKey: workoutKeys.detail(workoutId) })
    qc.invalidateQueries({ queryKey: statsKeys.all })
    qc.invalidateQueries({ queryKey: PROGRESSION_PREFIX })
  }

  const addExercise = useMutation({
    mutationFn: (body: AddExerciseToWorkoutBody) => addExerciseToWorkoutApi(workoutId, body),
    onSuccess: invalidateAll,
  })

  const removeExercise = useMutation({
    mutationFn: (workoutExerciseId: string) =>
      removeExerciseFromWorkoutApi(workoutId, workoutExerciseId),
    onSuccess: invalidateAll,
  })

  const updateDuration = useMutation({
    mutationFn: (vars: { workoutExerciseId: string; duration: number }) =>
      updateDurationApi(workoutId, vars.workoutExerciseId, { duration: vars.duration }),
    onSuccess: invalidateAll,
  })

  const addSet = useMutation({
    mutationFn: (vars: { workoutExerciseId: string; body: SetBody }) =>
      addSetApi(workoutId, vars.workoutExerciseId, vars.body),
    onSuccess: invalidateAll,
  })

  const updateSet = useMutation({
    mutationFn: (vars: { workoutExerciseId: string; setId: string; body: SetBody }) =>
      updateSetApi(workoutId, vars.workoutExerciseId, vars.setId, vars.body),
    onSuccess: invalidateAll,
  })

  const removeSet = useMutation({
    mutationFn: (vars: { workoutExerciseId: string; setId: string }) =>
      removeSetApi(workoutId, vars.workoutExerciseId, vars.setId),
    onSuccess: invalidateAll,
  })

  return { addExercise, removeExercise, updateDuration, addSet, updateSet, removeSet }
}
