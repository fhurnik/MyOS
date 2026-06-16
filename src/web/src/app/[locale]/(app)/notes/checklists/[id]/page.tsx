import { getTranslations } from "next-intl/server"
import { getCheckListApi } from "@/modules/notes/api/check-lists.api"
import { getServerToken } from "@/shared/lib/session"
import { CheckListDetail } from "@/modules/notes/components/check-lists/CheckListDetail"
import { AppBreadcrumbs } from "@/shared/components/layout/AppBreadcrumbs"

type Props = { params: Promise<{ locale: string; id: string }> }

export default async function CheckListDetailPage({ params }: Props) {
  const { locale, id } = await params
  const token = await getServerToken()
  const initialData = id === "new" ? undefined : await getCheckListApi(id, token ?? undefined)
  const t = await getTranslations("navigation")

  return (
    <div className="flex flex-col gap-6 h-[calc(100vh-3rem)]">
      <AppBreadcrumbs items={[
        { label: t("checkLists"), href: `/${locale}/notes/checklists` },
        { label: initialData?.title ?? "" },
      ]} />
      <CheckListDetail id={id} initialData={initialData} />
    </div>
  )
}
