import { Sidebar } from "@/shared/components/layout/Sidebar"
import { MobileHeader } from "@/shared/components/layout/MobileHeader"
import { SessionProvider } from "@/shared/providers/SessionProvider"
import { UploadProvider } from "@/modules/storage/upload/UploadProvider"
import { UploadPanel } from "@/modules/storage/upload/UploadPanel"
import { getServerSession } from "@/shared/lib/session"

export default async function AppLayout({ children }: { children: React.ReactNode }) {
  const session = await getServerSession()

  return (
    <SessionProvider session={session}>
      <UploadProvider>
        <div className="flex min-h-screen flex-col md:flex-row">
          <MobileHeader />
          <Sidebar />
          <main className="flex-1 overflow-auto p-4 md:p-6">{children}</main>
        </div>
        <UploadPanel />
      </UploadProvider>
    </SessionProvider>
  )
}
