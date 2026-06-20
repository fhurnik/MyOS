"use client"

import { useMemo } from "react"
import { useTranslations } from "next-intl"
import { Bar, BarChart, CartesianGrid, XAxis, YAxis } from "recharts"
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/shared/components/ui/chart"
import { formatIsoWeek } from "@/modules/fitness/lib/fitness-format"
import type { WeeklySetsDto } from "@/modules/fitness/types/fitness.types"

interface WeeklyVolumeChartProps {
  data: WeeklySetsDto[]
}

interface WeekBar {
  week: string
  sets: number
  sortKey: number
}

export function WeeklyVolumeChart({ data }: WeeklyVolumeChartProps) {
  const t = useTranslations("fitness.stats")

  // Group by ISO year+week and sum set counts across exercises.
  const bars = useMemo<WeekBar[]>(() => {
    const map = new Map<string, WeekBar>()
    for (const row of data) {
      const key = `${row.isoYear}-${row.isoWeek}`
      const existing = map.get(key)
      if (existing) {
        existing.sets += row.setCount
      } else {
        map.set(key, {
          week: formatIsoWeek(row.isoYear, row.isoWeek),
          sets: row.setCount,
          sortKey: row.isoYear * 100 + row.isoWeek,
        })
      }
    }
    return [...map.values()].sort((a, b) => a.sortKey - b.sortKey)
  }, [data])

  const config: ChartConfig = {
    sets: { label: t("sets"), color: "var(--primary)" },
  }

  if (bars.length === 0) {
    return <p className="py-12 text-center text-sm text-muted-foreground">{t("noData")}</p>
  }

  return (
    <ChartContainer config={config} className="aspect-video w-full">
      <BarChart data={bars} margin={{ top: 8, right: 12, bottom: 4, left: 4 }}>
        <CartesianGrid vertical={false} />
        <XAxis dataKey="week" tickLine={false} axisLine={false} tickMargin={8} minTickGap={16} />
        <YAxis tickLine={false} axisLine={false} width={32} allowDecimals={false} />
        <ChartTooltip content={<ChartTooltipContent />} />
        <Bar dataKey="sets" fill="var(--color-sets)" radius={[4, 4, 0, 0]} />
      </BarChart>
    </ChartContainer>
  )
}
