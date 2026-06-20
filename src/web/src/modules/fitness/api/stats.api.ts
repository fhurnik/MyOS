import { apiClient } from "@/shared/lib/api-client"
import type { WeeklySetsDto, UserDashboardDto } from "@/modules/fitness/types/fitness.types"

const BASE = "/api/v1/fitness"

// Plain array, NOT paginated.
export async function getWeeklySetsApi(
  exerciseId?: string,
  token?: string
): Promise<WeeklySetsDto[]> {
  const qs = exerciseId ? `?exerciseId=${exerciseId}` : ""
  return apiClient<WeeklySetsDto[]>(`${BASE}/stats/weekly-sets${qs}`, { token })
}

export async function getDashboardApi(token?: string): Promise<UserDashboardDto> {
  return apiClient<UserDashboardDto>(`${BASE}/dashboard`, { token })
}
