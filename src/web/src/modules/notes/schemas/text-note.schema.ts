import { z } from "zod"

export const textNoteSchema = z.object({
  title: z.string().min(1, "Title required").max(500),
  text: z.string().min(1, "Content required"),
})

export type TextNoteFormValues = z.infer<typeof textNoteSchema>
