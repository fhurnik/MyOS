import { Sidebar } from "@/shared/components/layout/Sidebar"
import { SessionProvider } from "@/shared/providers/SessionProvider"
import { getServerSession } from "@/shared/lib/session"

export default async function AppLayout({ children }: { children: React.ReactNode }) {
  const session = await getServerSession()

  return (
    <SessionProvider session={session}>
      <div className="flex min-h-screen">
        <Sidebar />
        <main className="flex-1 overflow-auto p-6">{children}</main>
      </div>
    </SessionProvider>
  )
}
