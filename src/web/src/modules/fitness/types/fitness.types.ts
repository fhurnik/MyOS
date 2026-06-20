// Mirrors the backend `fitness` module DTOs and request payloads.
// Enums are camelCase strings on the wire; dates (`date`, progression point `date`) are
// "YYYY-MM-DD" (DateOnly), timestamps (`createdAtUtc`/`updatedAtUtc`) are full ISO datetimes.

export type ActivityType = "cardio" | "strength"
export type StrengthCategory = "weighted" | "bodyweight"

// ── Exercises ─────────────────────────────────────────────────────────────────

export interface ExerciseDto {
  id: string
  name: string
  activityType: ActivityType
  strengthCategory: StrengthCategory | null // null for cardio
  distance: number | null // meters — cardio only, else null
  createdAtUtc: string
  updatedAtUtc: string | null
}

// ── Workouts ──────────────────────────────────────────────────────────────────

export interface WorkoutSummaryDto {
  id: string
  userId: string
  date: string // "YYYY-MM-DD"
  notes: string | null
  createdAtUtc: string
  updatedAtUtc: string | null
}

export interface WorkoutDto {
  id: string
  userId: string
  date: string // "YYYY-MM-DD"
  notes: string | null
  createdAtUtc: string
  updatedAtUtc: string | null
  exercises: WorkoutExerciseDto[] // ordered by position
}

export interface WorkoutExerciseDto {
  id: string // workoutExerciseId — use for set/duration ops
  workoutId: string
  exerciseId: string
  exerciseName: string // denormalized; survives exercise soft-delete
  activityType: ActivityType
  strengthCategory: StrengthCategory | null
  position: number
  duration: number | null // seconds — cardio only, else null
  sets: ExerciseSetDto[] // ordered by position; empty for cardio
}

export interface ExerciseSetDto {
  id: string
  workoutExerciseId: string
  position: number
  reps: number
  weight: number | null // kg — weighted only, else null
  addedWeight: number | null // kg — bodyweight only (extra load), else null
  negatives: number | null // bodyweight only (eccentric reps count), else null
  rir: number | null // 0..10 reps in reserve; null = not provided
}

// ── Stats & dashboard ─────────────────────────────────────────────────────────

export interface ProgressionPoint {
  date: string // "YYYY-MM-DD"
  value: number | null // kg / reps / seconds depending on exercise type; null = no sets logged
}

export interface ProgressionDto {
  exerciseId: string
  targetValue: number | null
  points: ProgressionPoint[] // ordered by date
}

export interface WeeklySetsDto {
  exerciseId: string
  exerciseName: string
  isoYear: number
  isoWeek: number
  setCount: number
}

export interface UserDashboardDto {
  lastWorkoutDate: string | null
  daysSinceLastWorkout: number | null
  workoutsThisWeek: number
  setsThisWeek: number
}

// ── Request payloads ──────────────────────────────────────────────────────────

// Create / update exercise — discriminated on activityType.
export interface CreateCardioExerciseBody {
  activityType: "cardio"
  name: string
  distance: number // meters
}
export interface CreateStrengthExerciseBody {
  activityType: "strength"
  name: string
  category: StrengthCategory
}
export type CreateExerciseBody = CreateCardioExerciseBody | CreateStrengthExerciseBody
// PUT uses the same body shape; activityType must match the stored exercise.
export type UpdateExerciseBody = CreateExerciseBody

export interface SetTargetBody {
  value: number
}

// Workout header.
export interface CreateWorkoutBody {
  date: string // "YYYY-MM-DD"
  notes: string | null
}
export type UpdateWorkoutBody = CreateWorkoutBody

// Add exercise to workout — discriminated on activityType.
// Inline set is a flat shape; category is fixed by the referenced exercise.
export interface InlineSet {
  reps: number
  weight?: number | null
  addedWeight?: number | null
  negatives?: number | null
  rir?: number | null
}
export interface AddCardioExerciseToWorkoutBody {
  activityType: "cardio"
  exerciseId: string
  duration: number // seconds
}
export interface AddStrengthExerciseToWorkoutBody {
  activityType: "strength"
  exerciseId: string
  sets: InlineSet[] // may be empty
}
export type AddExerciseToWorkoutBody =
  | AddCardioExerciseToWorkoutBody
  | AddStrengthExerciseToWorkoutBody

export interface UpdateDurationBody {
  duration: number // seconds
}

// Add / update set — discriminated on category.
export interface WeightedSetBody {
  category: "weighted"
  reps: number
  weight: number
  rir?: number | null
}
export interface BodyweightSetBody {
  category: "bodyweight"
  reps: number
  addedWeight?: number | null
  negatives?: number | null
  rir?: number | null
}
export type SetBody = WeightedSetBody | BodyweightSetBody

// List filter params for the exercise catalog.
export interface ExerciseFilterParams {
  activityType?: ActivityType
  strengthCategory?: StrengthCategory
}
