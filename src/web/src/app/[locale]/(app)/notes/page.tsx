import { getTranslations } from "next-intl/server"
import { getTextNotesApi } from "@/modules/notes/api/text-notes.api"
import { getServerToken } from "@/shared/lib/session"
import { TextNoteList } from "@/modules/notes/components/text-notes/TextNoteList"
import { CreateNoteButton } from "@/modules/notes/components/text-notes/CreateNoteButton"
import { AppBreadcrumbs } from "@/shared/components/layout/AppBreadcrumbs"

const VALID_ORDER_BY = ["title", "createdAtUtc"] as const
type TextNoteOrderBy = typeof VALID_ORDER_BY[number]

interface PageProps {
  params: Promise<{ locale: string }>
  searchParams: Promise<{ page?: string; pageSize?: string; orderBy?: string; orderByDesc?: string }>
}

export default async function NotesPage({ params, searchParams }: PageProps) {
  const { locale } = await params
  const { page, pageSize, orderBy, orderByDesc } = await searchParams
  const pageNum = Math.max(1, parseInt(page ?? "1", 10))
  const pageSizeNum = [5, 10, 25, 100].includes(parseInt(pageSize ?? "", 10))
    ? parseInt(pageSize!, 10)
    : 10
  const orderByParam = VALID_ORDER_BY.includes(orderBy as TextNoteOrderBy)
    ? (orderBy as TextNoteOrderBy)
    : undefined
  const orderByDescParam = orderByDesc === "true"
  const t = await getTranslations("navigation")
  const token = await getServerToken()
  const initialData = await getTextNotesApi(
    { page: pageNum, pageSize: pageSizeNum, orderBy: orderByParam, orderByDesc: orderByDescParam || undefined },
    token ?? undefined
  )

  return (
    <div className="space-y-6">
      <AppBreadcrumbs items={[
        { label: t("notes") },
        { label: t("textNotes") },
      ]} />
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold tracking-tight">{t("textNotes")}</h1>
        <CreateNoteButton />
      </div>
      <TextNoteList
        initialData={initialData}
        initialPage={pageNum}
        initialPageSize={pageSizeNum}
        initialOrderBy={orderByParam}
        initialOrderByDesc={orderByDescParam}
      />
    </div>
  )
}
