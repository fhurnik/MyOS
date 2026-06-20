import { apiClient } from "@/shared/lib/api-client"
import { buildPagingParams } from "@/shared/lib/paging"
import type { PagingList, PagingRequest } from "@/shared/types/api.types"
import type {
  WorkoutSummaryDto,
  WorkoutDto,
  CreateWorkoutBody,
  UpdateWorkoutBody,
  AddExerciseToWorkoutBody,
  UpdateDurationBody,
  SetBody,
} from "@/modules/fitness/types/fitness.types"

const BASE = "/api/v1/fitness/workouts"

export async function getWorkoutsApi(
  params: PagingRequest = {},
  token?: string
): Promise<PagingList<WorkoutSummaryDto>> {
  return apiClient<PagingList<WorkoutSummaryDto>>(`${BASE}${buildPagingParams(params)}`, { token })
}

export async function getWorkoutApi(id: string, token?: string): Promise<WorkoutDto> {
  return apiClient<WorkoutDto>(`${BASE}/${id}`, { token })
}

export async function createWorkoutApi(body: CreateWorkoutBody): Promise<string> {
  return apiClient<string>(BASE, { method: "POST", body })
}

export async function updateWorkoutApi(id: string, body: UpdateWorkoutBody): Promise<void> {
  return apiClient<void>(`${BASE}/${id}`, { method: "PUT", body })
}

export async function deleteWorkoutApi(id: string): Promise<void> {
  return apiClient<void>(`${BASE}/${id}`, { method: "DELETE" })
}

// ── Workout exercise entries ──────────────────────────────────────────────────

export async function addExerciseToWorkoutApi(
  workoutId: string,
  body: AddExerciseToWorkoutBody
): Promise<string> {
  return apiClient<string>(`${BASE}/${workoutId}/exercises`, { method: "POST", body })
}

export async function removeExerciseFromWorkoutApi(
  workoutId: string,
  workoutExerciseId: string
): Promise<void> {
  return apiClient<void>(`${BASE}/${workoutId}/exercises/${workoutExerciseId}`, {
    method: "DELETE",
  })
}

export async function updateDurationApi(
  workoutId: string,
  workoutExerciseId: string,
  body: UpdateDurationBody
): Promise<void> {
  return apiClient<void>(`${BASE}/${workoutId}/exercises/${workoutExerciseId}`, {
    method: "PATCH",
    body,
  })
}

// ── Sets ──────────────────────────────────────────────────────────────────────

export async function addSetApi(
  workoutId: string,
  workoutExerciseId: string,
  body: SetBody
): Promise<string> {
  return apiClient<string>(`${BASE}/${workoutId}/exercises/${workoutExerciseId}/sets`, {
    method: "POST",
    body,
  })
}

export async function updateSetApi(
  workoutId: string,
  workoutExerciseId: string,
  setId: string,
  body: SetBody
): Promise<void> {
  return apiClient<void>(`${BASE}/${workoutId}/exercises/${workoutExerciseId}/sets/${setId}`, {
    method: "PATCH",
    body,
  })
}

export async function removeSetApi(
  workoutId: string,
  workoutExerciseId: string,
  setId: string
): Promise<void> {
  return apiClient<void>(`${BASE}/${workoutId}/exercises/${workoutExerciseId}/sets/${setId}`, {
    method: "DELETE",
  })
}
