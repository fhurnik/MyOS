"use client"

import { useMutation } from "@tanstack/react-query"
import { useRouter } from "next/navigation"
import { registerApi } from "@/modules/identity/api/auth.api"
import type { RegisterFormValues } from "@/modules/identity/schemas/register.schema"

export function useRegister() {
  const router = useRouter()

  return useMutation({
    meta: { suppressToast: true },
    mutationFn: (values: RegisterFormValues) => registerApi(values),
    onSuccess: () => {
      router.replace("/en/login")
    },
  })
}
