import { apiClient } from "@/shared/lib/api-client"
import { buildPagingParams } from "@/shared/lib/paging"
import type { PagingList, PagingRequest } from "@/shared/types/api.types"
import type {
  ExerciseDto,
  ExerciseFilterParams,
  CreateExerciseBody,
  UpdateExerciseBody,
  SetTargetBody,
  ProgressionDto,
} from "@/modules/fitness/types/fitness.types"

const BASE = "/api/v1/fitness/exercises"

function buildExerciseQuery(params: PagingRequest & ExerciseFilterParams): string {
  const qs = buildPagingParams(params)
  const extra = new URLSearchParams()
  if (params.activityType) extra.set("activityType", params.activityType)
  if (params.strengthCategory) extra.set("strengthCategory", params.strengthCategory)
  const extraStr = extra.toString()
  if (!extraStr) return qs
  return qs ? `${qs}&${extraStr}` : `?${extraStr}`
}

export async function getExercisesApi(
  params: PagingRequest & ExerciseFilterParams = {},
  token?: string
): Promise<PagingList<ExerciseDto>> {
  return apiClient<PagingList<ExerciseDto>>(`${BASE}${buildExerciseQuery(params)}`, { token })
}

export async function getExerciseApi(id: string, token?: string): Promise<ExerciseDto> {
  return apiClient<ExerciseDto>(`${BASE}/${id}`, { token })
}

export async function createExerciseApi(body: CreateExerciseBody): Promise<string> {
  return apiClient<string>(BASE, { method: "POST", body })
}

export async function updateExerciseApi(id: string, body: UpdateExerciseBody): Promise<void> {
  return apiClient<void>(`${BASE}/${id}`, { method: "PUT", body })
}

export async function deleteExerciseApi(id: string): Promise<void> {
  return apiClient<void>(`${BASE}/${id}`, { method: "DELETE" })
}

export async function setExerciseTargetApi(id: string, body: SetTargetBody): Promise<void> {
  return apiClient<void>(`${BASE}/${id}/target`, { method: "PUT", body })
}

export async function getExerciseProgressionApi(
  id: string,
  token?: string
): Promise<ProgressionDto> {
  return apiClient<ProgressionDto>(`${BASE}/${id}/progression`, { token })
}
