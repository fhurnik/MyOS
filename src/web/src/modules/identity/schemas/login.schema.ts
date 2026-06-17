import { z } from "zod"

export interface LoginSchemaErrors {
  emailInvalid: string
  passwordRequired: string
}

export function createLoginSchema(errors: LoginSchemaErrors) {
  return z.object({
    email: z.string().email(errors.emailInvalid),
    password: z.string().min(1, errors.passwordRequired),
  })
}

export type LoginFormValues = z.infer<ReturnType<typeof createLoginSchema>>
