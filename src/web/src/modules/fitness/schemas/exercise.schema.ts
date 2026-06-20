import { z } from "zod"

export interface ExerciseSchemaErrors {
  nameRequired: string
  distancePositive: string
  categoryRequired: string
}

// Polymorphic on activityType — cardio carries distance (meters), strength carries category.
// The inferred output matches `CreateExerciseBody`, so a parsed value can be submitted directly.
export function createExerciseSchema(errors: ExerciseSchemaErrors) {
  return z.discriminatedUnion("activityType", [
    z.object({
      activityType: z.literal("cardio"),
      name: z.string().min(1, { error: errors.nameRequired }).max(200),
      distance: z
        .number({ error: errors.distancePositive })
        .positive({ error: errors.distancePositive }),
    }),
    z.object({
      activityType: z.literal("strength"),
      name: z.string().min(1, { error: errors.nameRequired }).max(200),
      category: z.enum(["weighted", "bodyweight"], { error: errors.categoryRequired }),
    }),
  ])
}

export type ExerciseFormValues = z.infer<ReturnType<typeof createExerciseSchema>>
