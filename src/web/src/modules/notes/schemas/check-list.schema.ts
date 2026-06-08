import { z } from "zod"

export const checkListSchema = z.object({
  title: z.string().min(1, "Title required").max(500),
})

export const checkListItemSchema = z.object({
  text: z.string().min(1, "Item text required"),
})

export type CheckListFormValues = z.infer<typeof checkListSchema>
export type CheckListItemFormValues = z.infer<typeof checkListItemSchema>
