import { z } from "zod"

export interface TargetSchemaErrors {
  targetPositive: string
}

export type TargetFormValues = { value: number }

export function createTargetSchema(errors: TargetSchemaErrors) {
  return z.object({
    value: z
      .number({ error: errors.targetPositive })
      .positive({ error: errors.targetPositive }),
  })
}
