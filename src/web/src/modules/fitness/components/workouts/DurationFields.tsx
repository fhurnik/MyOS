"use client"

import type { ChangeEvent } from "react"
import { useTranslations } from "next-intl"
import { Input } from "@/shared/components/ui/input"
import { Label } from "@/shared/components/ui/label"

export interface DurationParts {
  hours: string
  minutes: string
  seconds: string
}

export const emptyDuration: DurationParts = { hours: "", minutes: "", seconds: "" }

// Splits total seconds into h/m/s string parts. Zero components become "" so the inputs
// show a "0" placeholder instead of a sticky literal zero the user has to delete.
export function durationToParts(totalSeconds: number | null | undefined): DurationParts {
  const total = totalSeconds && totalSeconds > 0 ? Math.floor(totalSeconds) : 0
  const h = Math.floor(total / 3600)
  const m = Math.floor((total % 3600) / 60)
  const s = total % 60
  return {
    hours: h ? String(h) : "",
    minutes: m ? String(m) : "",
    seconds: s ? String(s) : "",
  }
}

export function partsToSeconds(p: DurationParts): number {
  return (Number(p.hours) || 0) * 3600 + (Number(p.minutes) || 0) * 60 + (Number(p.seconds) || 0)
}

interface DurationFieldsProps {
  value: DurationParts
  onChange: (value: DurationParts) => void
  idPrefix?: string
}

export function DurationFields({ value, onChange, idPrefix = "duration" }: DurationFieldsProps) {
  const t = useTranslations("fitness.workouts")

  const handle = (key: keyof DurationParts) => (e: ChangeEvent<HTMLInputElement>) =>
    onChange({ ...value, [key]: e.target.value.replace(/[^0-9]/g, "") })

  return (
    <div className="grid grid-cols-3 gap-3">
      <div className="space-y-1.5">
        <Label htmlFor={`${idPrefix}-h`}>{t("durationHours")}</Label>
        <Input
          id={`${idPrefix}-h`}
          type="number"
          min={0}
          inputMode="numeric"
          placeholder="0"
          value={value.hours}
          onChange={handle("hours")}
        />
      </div>
      <div className="space-y-1.5">
        <Label htmlFor={`${idPrefix}-m`}>{t("durationMinutes")}</Label>
        <Input
          id={`${idPrefix}-m`}
          type="number"
          min={0}
          max={59}
          inputMode="numeric"
          placeholder="0"
          value={value.minutes}
          onChange={handle("minutes")}
        />
      </div>
      <div className="space-y-1.5">
        <Label htmlFor={`${idPrefix}-s`}>{t("durationSeconds")}</Label>
        <Input
          id={`${idPrefix}-s`}
          type="number"
          min={0}
          max={59}
          inputMode="numeric"
          placeholder="0"
          value={value.seconds}
          onChange={handle("seconds")}
        />
      </div>
    </div>
  )
}
