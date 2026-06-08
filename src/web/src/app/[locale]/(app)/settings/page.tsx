import { getTranslations } from "next-intl/server"

export default async function SettingsPage() {
  const t = await getTranslations("settings")
  return (
    <div className="space-y-4">
      <h1 className="text-xl font-semibold">{t("title")}</h1>
      <p className="text-muted-foreground">Coming soon</p>
    </div>
  )
}
