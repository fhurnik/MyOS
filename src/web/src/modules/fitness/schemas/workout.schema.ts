import { z } from "zod"

export interface WorkoutSchemaErrors {
  dateRequired: string
}

export type WorkoutFormValues = { date: string; notes?: string }

// date is an ISO "YYYY-MM-DD" string (emitted by a native date input).
export function createWorkoutSchema(errors: WorkoutSchemaErrors) {
  return z.object({
    date: z.string().min(1, { error: errors.dateRequired }),
    notes: z.string().max(2000).optional(),
  })
}
