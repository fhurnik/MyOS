"use client"

import { useMemo } from "react"
import { useTranslations } from "next-intl"
import {
  CartesianGrid,
  Line,
  LineChart,
  ReferenceLine,
  XAxis,
  YAxis,
} from "recharts"
import {
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
  type ChartConfig,
} from "@/shared/components/ui/chart"
import {
  progressionUnit,
  formatProgressionValue,
} from "@/modules/fitness/lib/fitness-format"
import type { ExerciseDto, ProgressionDto } from "@/modules/fitness/types/fitness.types"

interface ProgressionChartProps {
  exercise: ExerciseDto
  progression: ProgressionDto
}

function shortDate(iso: string): string {
  const d = new Date(iso)
  return d.toLocaleDateString(undefined, { month: "short", day: "numeric" })
}

export function ProgressionChart({ exercise, progression }: ProgressionChartProps) {
  const t = useTranslations("fitness.stats")
  const unit = progressionUnit(exercise.activityType, exercise.strengthCategory)

  // Filter null values — they represent sessions with no sets logged.
  const points = useMemo(
    () => progression.points.filter((p) => p.value != null) as { date: string; value: number }[],
    [progression.points]
  )

  const config: ChartConfig = {
    value: { label: t("value"), color: "var(--primary)" },
  }

  if (points.length === 0) {
    return <p className="py-12 text-center text-sm text-muted-foreground">{t("noData")}</p>
  }

  return (
    <ChartContainer config={config} className="aspect-video w-full">
      <LineChart data={points} margin={{ top: 8, right: 12, bottom: 4, left: 4 }}>
        <CartesianGrid vertical={false} />
        <XAxis
          dataKey="date"
          tickLine={false}
          axisLine={false}
          tickMargin={8}
          minTickGap={24}
          tickFormatter={shortDate}
        />
        <YAxis
          tickLine={false}
          axisLine={false}
          width={48}
          tickFormatter={(v: number) => formatProgressionValue(v, unit)}
        />
        {progression.targetValue != null && (
          <ReferenceLine
            y={progression.targetValue}
            stroke="var(--muted-foreground)"
            strokeDasharray="4 4"
            label={{
              value: `${t("target")}: ${formatProgressionValue(progression.targetValue, unit)}`,
              position: "insideTopRight",
              fill: "var(--muted-foreground)",
              fontSize: 11,
            }}
          />
        )}
        <ChartTooltip
          content={
            <ChartTooltipContent
              labelFormatter={(label) => shortDate(String(label))}
              formatter={(value) => formatProgressionValue(Number(value), unit)}
            />
          }
        />
        <Line
          dataKey="value"
          type="monotone"
          stroke="var(--color-value)"
          strokeWidth={2}
          dot={{ r: 3 }}
          activeDot={{ r: 5 }}
        />
      </LineChart>
    </ChartContainer>
  )
}
