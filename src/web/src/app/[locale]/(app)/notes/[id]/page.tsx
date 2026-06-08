import { getTextNoteApi } from "@/modules/notes/api/text-notes.api"
import { getServerToken } from "@/shared/lib/session"
import { TextNoteDetail } from "@/modules/notes/components/text-notes/TextNoteDetail"

type Props = { params: Promise<{ locale: string; id: string }> }

export default async function TextNoteDetailPage({ params }: Props) {
  const { id } = await params
  const token = await getServerToken()
  const initialData = id === "new" ? undefined : await getTextNoteApi(id, token ?? undefined)

  return <TextNoteDetail id={id} initialData={initialData} />
}
