import { z } from "zod"

export type TextNoteFormValues = { title: string; text: string }

export interface TextNoteSchemaErrors {
  titleRequired: string
  contentRequired: string
}

export function createTextNoteSchema(errors: TextNoteSchemaErrors) {
  return z.object({
    title: z.string().min(1, errors.titleRequired).max(500),
    text: z.string().min(1, errors.contentRequired),
  })
}
