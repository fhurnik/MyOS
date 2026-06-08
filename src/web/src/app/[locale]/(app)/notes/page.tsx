import { getTranslations } from "next-intl/server"
import { getTextNotesApi } from "@/modules/notes/api/text-notes.api"
import { getServerToken } from "@/shared/lib/session"
import { TextNoteList } from "@/modules/notes/components/text-notes/TextNoteList"
import { CreateNoteButton } from "@/modules/notes/components/text-notes/CreateNoteButton"
import { NotesTabBar } from "@/modules/notes/components/NotesTabBar"

export default async function NotesPage() {
  const t = await getTranslations("navigation")
  const token = await getServerToken()
  const initialData = await getTextNotesApi({}, token ?? undefined)

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-semibold tracking-tight">{t("notes")}</h1>
        <CreateNoteButton />
      </div>
      <NotesTabBar />
      <TextNoteList initialData={initialData} />
    </div>
  )
}
