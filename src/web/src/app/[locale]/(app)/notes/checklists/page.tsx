import { getTranslations } from "next-intl/server"
import { getCheckListsApi } from "@/modules/notes/api/check-lists.api"
import { getServerToken } from "@/shared/lib/session"
import { CheckListList } from "@/modules/notes/components/check-lists/CheckListList"
import { CreateCheckListButton } from "@/modules/notes/components/check-lists/CreateCheckListButton"
import { NotesTabBar } from "@/modules/notes/components/NotesTabBar"

export default async function CheckListsPage() {
  const t = await getTranslations("navigation")
  const token = await getServerToken()
  const initialData = await getCheckListsApi({}, token ?? undefined)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold tracking-tight">{t("notes")}</h1>
        <CreateCheckListButton />
      </div>
      <NotesTabBar />
      <CheckListList initialData={initialData} />
    </div>
  )
}
