"use client"

import { useTranslations } from "next-intl"
import { useRequiredSession } from "@/shared/hooks/useSession"
import { LanguageSelector } from "@/shared/components/layout/LanguageSelector"
import { useChangeLanguage } from "@/modules/identity/hooks/useChangeLanguage"
import { LANGUAGE_TO_LOCALE, LOCALE_TO_LANGUAGE, DEFAULT_LOCALE } from "@/shared/types/common.types"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"

export function LanguageSettings() {
  const t = useTranslations("settings")
  const session = useRequiredSession()
  const { mutate: changeLanguage, isPending } = useChangeLanguage()

  const currentLocale = LANGUAGE_TO_LOCALE[session.language] ?? DEFAULT_LOCALE

  function handleChange(locale: string) {
    const language = LOCALE_TO_LANGUAGE[locale]
    if (language !== undefined && language !== session.language) {
      changeLanguage(language)
    }
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("language.title")}</CardTitle>
        <CardDescription>{t("language.description")}</CardDescription>
      </CardHeader>
      <CardContent>
        <LanguageSelector
          currentLocale={currentLocale}
          onChange={handleChange}
          disabled={isPending}
        />
      </CardContent>
    </Card>
  )
}
