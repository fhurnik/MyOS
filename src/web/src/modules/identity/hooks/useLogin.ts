"use client"

import { useMutation } from "@tanstack/react-query"
import { useRouter } from "next/navigation"
import { useLocale } from "next-intl"
import type { LoginFormValues } from "@/modules/identity/schemas/login.schema"
import type { SessionPayload } from "@/modules/identity/types/identity.types"
import type { ProblemDetails } from "@/shared/types/api.types"
import { ApiError } from "@/shared/lib/api-error"

export function useLogin() {
  const router = useRouter()
  const locale = useLocale()

  return useMutation({
    meta: { suppressToast: true },
    mutationFn: async (values: LoginFormValues): Promise<SessionPayload> => {
      const res = await fetch("/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json", "Accept-Language": locale },
        body: JSON.stringify(values),
      })
      if (!res.ok) {
        const problem: ProblemDetails = await res.json()
        throw new ApiError(problem)
      }
      return res.json()
    },
    onSuccess: (session) => {
      const locale = session.language === 1 ? "pl" : "en"
      router.replace(`/${locale}/home`)
    },
  })
}
