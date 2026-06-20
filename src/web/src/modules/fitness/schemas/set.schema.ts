import { z } from "zod"

export interface SetSchemaErrors {
  repsPositive: string
  weightRequired: string
  weightNonNegative: string
  addedWeightNonNegative: string
  negativesNonNegative: string
  rirRange: string
}

// Discriminated on category (fixed by the parent exercise's strengthCategory).
// The inferred output matches `SetBody`, so a parsed value can be submitted directly.
export function createSetSchema(errors: SetSchemaErrors) {
  const reps = z
    .number({ error: errors.repsPositive })
    .positive({ error: errors.repsPositive })
  const rir = z
    .number({ error: errors.rirRange })
    .min(0, { error: errors.rirRange })
    .max(10, { error: errors.rirRange })
    .nullable()
    .optional()

  return z.discriminatedUnion("category", [
    z.object({
      category: z.literal("weighted"),
      reps,
      weight: z
        .number({ error: errors.weightRequired })
        .min(0, { error: errors.weightNonNegative }),
      rir,
    }),
    z.object({
      category: z.literal("bodyweight"),
      reps,
      addedWeight: z
        .number({ error: errors.addedWeightNonNegative })
        .min(0, { error: errors.addedWeightNonNegative })
        .nullable()
        .optional(),
      negatives: z
        .number({ error: errors.negativesNonNegative })
        .min(0, { error: errors.negativesNonNegative })
        .nullable()
        .optional(),
      rir,
    }),
  ])
}

export type SetFormValues = z.infer<ReturnType<typeof createSetSchema>>
