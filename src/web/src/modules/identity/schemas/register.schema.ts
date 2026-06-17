import { z } from "zod"

export interface RegisterSchemaErrors {
  firstNameRequired: string
  lastNameRequired: string
  emailInvalid: string
  passwordMinLength: string
}

export function createRegisterSchema(errors: RegisterSchemaErrors) {
  return z.object({
    firstName: z.string().min(1, errors.firstNameRequired).max(100),
    lastName: z.string().min(1, errors.lastNameRequired).max(100),
    email: z.string().email(errors.emailInvalid).max(255),
    password: z.string().min(8, errors.passwordMinLength).max(200),
  })
}

export type RegisterFormValues = z.infer<ReturnType<typeof createRegisterSchema>>
