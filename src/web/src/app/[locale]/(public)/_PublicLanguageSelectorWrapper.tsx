"use client"

import { useLocale, useTranslations } from "next-intl"
import { usePathname, useRouter } from "next/navigation"
import { Globe } from "lucide-react"
import { LanguageSelector } from "@/shared/components/layout/LanguageSelector"

export function PublicLanguageSelectorWrapper() {
  const currentLocale = useLocale()
  const pathname = usePathname()
  const router = useRouter()
  const t = useTranslations("identity.language")

  function handleChange(newLocale: string) {
    document.cookie = `preferred_locale=${newLocale}; path=/; max-age=${60 * 60 * 24 * 365}; SameSite=Lax`
    const pagePath = pathname.split("/").slice(2).join("/")
    router.replace(`/${newLocale}/${pagePath}`)
  }

  return (
    <div className="flex items-center gap-2 text-muted-foreground">
      <Globe className="h-4 w-4 shrink-0" />
      <span className="text-sm">{t("selectorLabel")}</span>
      <LanguageSelector currentLocale={currentLocale} onChange={handleChange} compact />
    </div>
  )
}
