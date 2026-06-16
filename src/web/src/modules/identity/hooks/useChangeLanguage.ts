"use client"

import { useMutation } from "@tanstack/react-query"
import { useTranslations } from "next-intl"
import { toast } from "sonner"
import { ApiError } from "@/shared/lib/api-error"
import { LANGUAGE_TO_LOCALE, DEFAULT_LOCALE } from "@/shared/types/common.types"
import type { Language } from "@/shared/types/common.types"
import type { SessionPayload } from "@/modules/identity/types/identity.types"
import type { ProblemDetails } from "@/shared/types/api.types"

export function useChangeLanguage() {
  const t = useTranslations("identity.language")

  return useMutation({
    mutationFn: async (language: Language): Promise<SessionPayload> => {
      const res = await fetch("/api/auth/change-language", {
        method: "PATCH",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ language }),
      })
      if (!res.ok) {
        const problem: ProblemDetails = await res.json()
        throw new ApiError(problem)
      }
      return res.json()
    },
    onSuccess: (_session, language) => {
      // Use the requested language (not session response) — JWT claim may lag behind DB update
      const newLocale = LANGUAGE_TO_LOCALE[language] ?? DEFAULT_LOCALE
      toast.success(t("changeSuccess"))
      // Hard navigation: clears RSC cache and re-reads cookies with updated JWT language claim
      window.location.replace(`/${newLocale}/settings`)
    },
  })
}
