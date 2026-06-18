import { getTranslations } from "next-intl/server"
import { LanguageSettings } from "@/modules/identity/components/LanguageSettings"
import { AllowedFileTypesSection } from "@/modules/storage/components/settings/AllowedFileTypesSection"

export default async function SettingsPage() {
  const t = await getTranslations("settings")
  return (
    <div className="space-y-6 max-w-lg">
      <h1 className="text-xl font-semibold">{t("title")}</h1>
      <LanguageSettings />
      <AllowedFileTypesSection />
    </div>
  )
}
