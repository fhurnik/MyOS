import Link from "next/link"
import { getTranslations } from "next-intl/server"
import { FileText, HardDrive, GraduationCap, Wallet, Dumbbell, ChevronRight } from "lucide-react"
import { getServerSession, getServerToken } from "@/shared/lib/session"
import { getTextNotesApi } from "@/modules/notes/api/text-notes.api"
import { getCheckListsApi } from "@/modules/notes/api/check-lists.api"
import { getQuotaApi } from "@/modules/storage/api/storage.api"
import { getDashboardApi, getWeeklySetsApi } from "@/modules/fitness/api/stats.api"
import { formatBytes } from "@/shared/lib/format"
import { TextNoteCard } from "@/modules/notes/components/text-notes/TextNoteCard"
import { DashboardCards } from "@/modules/fitness/components/stats/DashboardCards"
import { WeeklyVolumeChart } from "@/modules/fitness/components/stats/WeeklyVolumeChart"
import { Card, CardContent, CardHeader, CardTitle } from "@/shared/components/ui/card"
import type { TextNoteDto } from "@/modules/notes/types/notes.types"
import type { ReactNode } from "react"

type Props = { params: Promise<{ locale: string }> }

function getGreetingKey(): "goodMorning" | "goodAfternoon" | "goodEvening" {
  const hour = new Date().getHours()
  if (hour < 12) return "goodMorning"
  if (hour < 18) return "goodAfternoon"
  return "goodEvening"
}

function formatDate(locale: string): string {
  const localeStr = locale === "pl" ? "pl-PL" : "en-GB"
  return new Date().toLocaleDateString(localeStr, {
    weekday: "long",
    day: "numeric",
    month: "long",
    year: "numeric",
  })
}

interface ActiveModuleCardProps {
  href: string
  icon: ReactNode
  title: string
  description: string
}

function ActiveModuleCard({ href, icon, title, description }: ActiveModuleCardProps) {
  return (
    <Link
      href={href}
      className="group flex items-center gap-4 rounded-xl border bg-card p-5 shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md"
    >
      <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
        {icon}
      </div>
      <div className="min-w-0 flex-1">
        <p className="font-semibold leading-tight">{title}</p>
        <p className="mt-0.5 text-sm text-muted-foreground">{description}</p>
      </div>
      <ChevronRight className="h-4 w-4 shrink-0 text-muted-foreground transition-transform group-hover:translate-x-0.5" />
    </Link>
  )
}

interface ComingSoonCardProps {
  icon: ReactNode
  title: string
  badge: string
}

function ComingSoonCard({ icon, title, badge }: ComingSoonCardProps) {
  return (
    <div className="flex items-center gap-4 rounded-xl border bg-card p-5 opacity-60">
      <div className="flex h-11 w-11 shrink-0 items-center justify-center rounded-lg bg-muted text-muted-foreground">
        {icon}
      </div>
      <div className="min-w-0 flex-1">
        <p className="font-semibold leading-tight text-muted-foreground">{title}</p>
      </div>
      <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">{badge}</span>
    </div>
  )
}

export default async function HomePage({ params }: Props) {
  const { locale } = await params
  const t = await getTranslations("home")
  const tNav = await getTranslations("navigation")
  const tFit = await getTranslations("fitness.dashboard")

  const session = await getServerSession()
  const token = await getServerToken()
  const username = session?.email.split("@")[0] ?? ""

  const [notesData, checkListsData, quota, dashboard, weeklySets] = await Promise.all([
    getTextNotesApi({ pageSize: 3 }, token ?? undefined),
    getCheckListsApi({ pageSize: 1 }, token ?? undefined),
    getQuotaApi(token ?? undefined),
    getDashboardApi(token ?? undefined),
    getWeeklySetsApi(undefined, token ?? undefined),
  ])

  const notesTotal = notesData.totalCount
  const listsTotal = checkListsData.totalCount
  const notesDescription = `${t("notesCount", { count: notesTotal })} · ${t("checkListsCount", { count: listsTotal })}`
  const storageDescription = `${formatBytes(quota.usedBytes)} / ${formatBytes(quota.maxBytes)}`
  const fitnessDescription = `${tFit("workoutsThisWeek")}: ${dashboard.workoutsThisWeek}`

  return (
    <div className="space-y-8">
      {/* Greeting */}
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">
          {t(getGreetingKey())}, {username}
        </h1>
        <p className="mt-1 text-sm text-muted-foreground">{formatDate(locale)}</p>
      </div>

      {/* Modules */}
      <section>
        <h2 className="mb-3 text-xs font-semibold uppercase tracking-wider text-muted-foreground/60">
          {t("modules")}
        </h2>
        <div className="grid gap-3 sm:grid-cols-2">
          <ActiveModuleCard
            href={`/${locale}/notes`}
            icon={<FileText className="h-5 w-5" />}
            title={tNav("notes")}
            description={notesDescription}
          />
          <ActiveModuleCard
            href={`/${locale}/storage`}
            icon={<HardDrive className="h-5 w-5" />}
            title={tNav("storage")}
            description={storageDescription}
          />
          <ComingSoonCard
            icon={<GraduationCap className="h-5 w-5" />}
            title={tNav("learning")}
            badge={t("comingSoon")}
          />
          <ActiveModuleCard
            href={`/${locale}/fitness/workouts`}
            icon={<Dumbbell className="h-5 w-5" />}
            title={tNav("fitness")}
            description={fitnessDescription}
          />
          <ComingSoonCard
            icon={<Wallet className="h-5 w-5" />}
            title={tNav("finance")}
            badge={t("comingSoon")}
          />
        </div>
      </section>

      {/* Fitness dashboard */}
      <section>
        <div className="mb-3 flex items-center justify-between">
          <h2 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground/60">
            {tNav("fitness")}
          </h2>
          <Link
            href={`/${locale}/fitness/workouts`}
            className="flex items-center gap-1 text-xs text-muted-foreground transition-colors hover:text-foreground"
          >
            {t("viewAll")}
            <ChevronRight className="h-3 w-3" />
          </Link>
        </div>
        <div className="space-y-4">
          <DashboardCards data={dashboard} />
          <Card>
            <CardHeader>
              <CardTitle className="text-sm">{tFit("weeklyVolume")}</CardTitle>
            </CardHeader>
            <CardContent>
              <WeeklyVolumeChart data={weeklySets} />
            </CardContent>
          </Card>
        </div>
      </section>

      {/* Recent notes */}
      {notesData.items.length > 0 && (
        <section>
          <div className="mb-3 flex items-center justify-between">
            <h2 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground/60">
              {t("recentNotes")}
            </h2>
            <Link
              href={`/${locale}/notes`}
              className="flex items-center gap-1 text-xs text-muted-foreground transition-colors hover:text-foreground"
            >
              {t("viewAll")}
              <ChevronRight className="h-3 w-3" />
            </Link>
          </div>
          <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
            {notesData.items.map((note: TextNoteDto) => (
              <Link key={note.id} href={`/${locale}/notes/${note.id}`}>
                <TextNoteCard note={note} />
              </Link>
            ))}
          </div>
        </section>
      )}
    </div>
  )
}
