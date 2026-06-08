import { getCheckListApi } from "@/modules/notes/api/check-lists.api"
import { getServerToken } from "@/shared/lib/session"
import { CheckListDetail } from "@/modules/notes/components/check-lists/CheckListDetail"

type Props = { params: Promise<{ locale: string; id: string }> }

export default async function CheckListDetailPage({ params }: Props) {
  const { id } = await params
  const token = await getServerToken()
  const initialData = id === "new" ? undefined : await getCheckListApi(id, token ?? undefined)

  return <CheckListDetail id={id} initialData={initialData} />
}
