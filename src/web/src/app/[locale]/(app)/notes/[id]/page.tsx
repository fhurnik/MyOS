import { getTranslations } from "next-intl/server"
import { getTextNoteApi } from "@/modules/notes/api/text-notes.api"
import { getServerToken } from "@/shared/lib/session"
import { TextNoteDetail } from "@/modules/notes/components/text-notes/TextNoteDetail"
import { AppBreadcrumbs } from "@/shared/components/layout/AppBreadcrumbs"

type Props = { params: Promise<{ locale: string; id: string }> }

export default async function TextNoteDetailPage({ params }: Props) {
  const { locale, id } = await params
  const token = await getServerToken()
  const initialData = id === "new" ? undefined : await getTextNoteApi(id, token ?? undefined)
  const t = await getTranslations("navigation")
  const tNotes = await getTranslations("notes.textNotes")

  return (
    <div className="space-y-6">
      <AppBreadcrumbs items={[
        { label: t("textNotes"), href: `/${locale}/notes` },
        { label: initialData?.title ?? tNotes("newTitle") },
      ]} />
      <TextNoteDetail id={id} initialData={initialData} />
    </div>
  )
}
