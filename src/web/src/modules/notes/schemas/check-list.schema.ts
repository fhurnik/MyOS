import { z } from "zod"

export type CheckListFormValues = { title: string }
export type CheckListItemFormValues = { text: string }

export interface CheckListSchemaErrors {
  titleRequired: string
}

export interface CheckListItemSchemaErrors {
  itemTextRequired: string
}

export function createCheckListSchema(errors: CheckListSchemaErrors) {
  return z.object({
    title: z.string().min(1, errors.titleRequired).max(500),
  })
}

export function createCheckListItemSchema(errors: CheckListItemSchemaErrors) {
  return z.object({
    text: z.string().min(1, errors.itemTextRequired),
  })
}
