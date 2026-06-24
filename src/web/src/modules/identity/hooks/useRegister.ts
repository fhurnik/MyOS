"use client"

import { useMutation } from "@tanstack/react-query"
import { useRouter } from "next/navigation"
import { useLocale } from "next-intl"
import { registerApi } from "@/modules/identity/api/auth.api"
import type { RegisterBody } from "@/modules/identity/types/identity.types"

export function useRegister() {
  const router = useRouter()
  const locale = useLocale()

  return useMutation({
    meta: { suppressToast: true },
    mutationFn: (values: RegisterBody) => registerApi(values, locale),
    onSuccess: () => {
      router.replace(`/${locale}/login`)
    },
  })
}
