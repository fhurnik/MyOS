"use client"

import { useTranslations } from "next-intl"
import { CalendarDays, Dumbbell, Flame, History } from "lucide-react"
import { Card, CardContent } from "@/shared/components/ui/card"
import { formatDate } from "@/shared/lib/format"
import type { UserDashboardDto } from "@/modules/fitness/types/fitness.types"
import type { ReactNode } from "react"

interface DashboardCardsProps {
  data: UserDashboardDto
}

function MetricCard({ icon, label, value }: { icon: ReactNode; label: string; value: ReactNode }) {
  return (
    <Card>
      <CardContent className="flex items-center gap-3 py-1">
        <div className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg bg-primary/10 text-primary">
          {icon}
        </div>
        <div className="min-w-0">
          <p className="truncate text-xs text-muted-foreground">{label}</p>
          <p className="text-xl font-semibold leading-tight">{value}</p>
        </div>
      </CardContent>
    </Card>
  )
}

export function DashboardCards({ data }: DashboardCardsProps) {
  const t = useTranslations("fitness.dashboard")

  const daysSince =
    data.daysSinceLastWorkout == null
      ? "—"
      : data.daysSinceLastWorkout === 0
        ? t("today")
        : String(data.daysSinceLastWorkout)

  const lastWorkout = data.lastWorkoutDate ? formatDate(data.lastWorkoutDate) : t("never")

  return (
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
      <MetricCard icon={<History className="h-5 w-5" />} label={t("daysSinceLastWorkout")} value={daysSince} />
      <MetricCard icon={<CalendarDays className="h-5 w-5" />} label={t("lastWorkoutDate")} value={lastWorkout} />
      <MetricCard icon={<Dumbbell className="h-5 w-5" />} label={t("workoutsThisWeek")} value={data.workoutsThisWeek} />
      <MetricCard icon={<Flame className="h-5 w-5" />} label={t("setsThisWeek")} value={data.setsThisWeek} />
    </div>
  )
}
