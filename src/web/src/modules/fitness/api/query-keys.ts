import type { PagingRequest } from "@/shared/types/api.types"
import type { ExerciseFilterParams } from "@/modules/fitness/types/fitness.types"

export const exerciseKeys = {
  all: ["fitness", "exercises"] as const,
  lists: () => [...exerciseKeys.all, "list"] as const,
  list: (params: PagingRequest & ExerciseFilterParams) =>
    [...exerciseKeys.lists(), params] as const,
  detail: (id: string) => [...exerciseKeys.all, "detail", id] as const,
  progression: (id: string) => [...exerciseKeys.all, "progression", id] as const,
}

export const workoutKeys = {
  all: ["fitness", "workouts"] as const,
  lists: () => [...workoutKeys.all, "list"] as const,
  list: (params: PagingRequest) => [...workoutKeys.lists(), params] as const,
  detail: (id: string) => [...workoutKeys.all, "detail", id] as const,
}

export const statsKeys = {
  all: ["fitness", "stats"] as const,
  dashboard: () => [...statsKeys.all, "dashboard"] as const,
  weeklySets: (exerciseId?: string) =>
    [...statsKeys.all, "weekly-sets", exerciseId ?? null] as const,
}
