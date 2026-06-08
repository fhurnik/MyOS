"use client"

import { useMutation } from "@tanstack/react-query"
import { useRouter } from "next/navigation"
import type { LoginFormValues } from "@/modules/identity/schemas/login.schema"
import type { SessionPayload } from "@/modules/identity/types/identity.types"

export function useLogin() {
  const router = useRouter()

  return useMutation({
    mutationFn: async (values: LoginFormValues): Promise<SessionPayload> => {
      const res = await fetch("/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(values),
      })
      if (!res.ok) {
        const problem = await res.json()
        const err = new Error(problem.detail || "Login failed") as Error & { code?: string }
        err.code = problem.errorCode
        throw err
      }
      return res.json()
    },
    onSuccess: (session) => {
      const locale = session.language === 1 ? "pl" : "en"
      router.replace(`/${locale}/home`)
    },
  })
}
