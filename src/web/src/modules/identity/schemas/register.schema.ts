import { z } from "zod"

export const registerSchema = z.object({
  firstName: z.string().min(1, "First name required").max(100),
  lastName: z.string().min(1, "Last name required").max(100),
  email: z.string().email("Valid email required").max(255),
  password: z.string().min(8, "Minimum 8 characters").max(200),
})

export type RegisterFormValues = z.infer<typeof registerSchema>
