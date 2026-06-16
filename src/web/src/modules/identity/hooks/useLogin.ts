"use client"

import { useMutation } from "@tanstack/react-query"
import { useRouter } from "next/navigation"
import type { LoginFormValues } from "@/modules/identity/schemas/login.schema"
import type { SessionPayload } from "@/modules/identity/types/identity.types"
import type { ProblemDetails } from "@/shared/types/api.types"
import { ApiError } from "@/shared/lib/api-error"

export function useLogin() {
  const router = useRouter()

  return useMutation({
    meta: { suppressToast: true },
    mutationFn: async (values: LoginFormValues): Promise<SessionPayload> => {
      const res = await fetch("/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
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
