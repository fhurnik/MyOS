import { getTranslations } from "next-intl/server"
import { getServerToken } from "@/shared/lib/session"
import { getAllowedFileTypesApi } from "@/modules/storage/api/storage.api"
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/shared/components/ui/card"

const CATEGORY_ORDER = ["document", "text", "image", "audio", "video", "archive"]

export async function AllowedFileTypesSection() {
  const t = await getTranslations("settings.allowedFileTypes")
  const token = await getServerToken()
  const types = await getAllowedFileTypesApi(token ?? undefined)

  const byCategory = new Map<string, string[]>()
  for (const type of types) {
    const list = byCategory.get(type.category) ?? []
    list.push(type.extension)
    byCategory.set(type.category, list)
  }

  const categories = [...byCategory.keys()].sort((a, b) => {
    const ia = CATEGORY_ORDER.indexOf(a)
    const ib = CATEGORY_ORDER.indexOf(b)
    return (ia < 0 ? 99 : ia) - (ib < 0 ? 99 : ib)
  })

  return (
    <Card>
      <CardHeader>
        <CardTitle>{t("title")}</CardTitle>
        <CardDescription>{t("description")}</CardDescription>
      </CardHeader>
      <CardContent className="space-y-3">
        {categories.map((category) => (
          <div key={category} className="space-y-1.5">
            <p className="text-xs font-medium uppercase tracking-wider text-muted-foreground">
              {t(`categories.${category}`)}
            </p>
            <div className="flex flex-wrap gap-1.5">
              {byCategory
                .get(category)!
                .sort()
                .map((ext) => (
                  <span key={ext} className="rounded-md bg-muted px-2 py-0.5 text-xs font-medium">
                    .{ext}
                  </span>
                ))}
            </div>
          </div>
        ))}
      </CardContent>
    </Card>
  )
}
