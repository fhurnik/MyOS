import { z } from "zod"

export type FolderFormValues = { name: string }

export interface FolderSchemaErrors {
  nameRequired: string
}

export function createFolderSchema(errors: FolderSchemaErrors) {
  return z.object({
    name: z.string().min(1, errors.nameRequired).max(255),
  })
}
