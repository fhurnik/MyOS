"use client"

import { Check } from "lucide-react"
import { useTranslations } from "next-intl"
import { Button } from "@/shared/components/ui/button"
import { SUPPORTED_LOCALES } from "@/shared/types/common.types"

const LOCALE_LABELS: Record<string, string> = { en: "English", pl: "Polski" }
const LOCALE_LABELS_SHORT: Record<string, string> = { en: "EN", pl: "PL" }

interface LanguageSelectorProps {
  currentLocale: string
  onChange: (locale: string) => void
  disabled?: boolean
  compact?: boolean
}

export function LanguageSelector({ currentLocale, onChange, disabled, compact }: LanguageSelectorProps) {
  const t = useTranslations("identity.language")
  const labels = compact ? LOCALE_LABELS_SHORT : LOCALE_LABELS

  return (
    <div className="flex gap-2" role="group" aria-label="Language">
      {SUPPORTED_LOCALES.map((locale) => {
        const isActive = locale === currentLocale
        return (
          <Button
            key={locale}
            size="sm"
            variant={isActive ? "default" : "outline"}
            disabled={disabled}
            onClick={() => onChange(locale)}
            aria-label={t(locale === "en" ? "english" : "polish")}
            aria-pressed={isActive}
            className={compact ? undefined : "min-w-28 gap-1.5"}
          >
            {!compact && isActive && <Check className="size-3.5" />}
            {labels[locale]}
          </Button>
        )
      })}
    </div>
  )
}
