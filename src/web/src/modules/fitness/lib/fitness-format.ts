import type { ActivityType, StrengthCategory } from "@/modules/fitness/types/fitness.types"

/** Seconds → "M:SS" (under an hour) or "H:MM:SS" (e.g. 1800 → "30:00", 5400 → "1:30:00"). */
export function formatDuration(seconds: number | null | undefined): string {
  if (seconds == null || !Number.isFinite(seconds) || seconds < 0) return "—"
  const total = Math.floor(seconds)
  const h = Math.floor(total / 3600)
  const m = Math.floor((total % 3600) / 60)
  const s = total % 60
  const pad = (n: number) => String(n).padStart(2, "0")
  return h > 0 ? `${h}:${pad(m)}:${pad(s)}` : `${m}:${pad(s)}`
}

/** Drops trailing zeros: 100 → "100 kg", 2.5 → "2.5 kg", 100.00 → "100 kg". */
export function formatWeight(kg: number | null | undefined): string {
  if (kg == null || !Number.isFinite(kg)) return "—"
  return `${Number(kg.toFixed(2))} kg`
}

/** Meters → "500 m" or "5 km" (1000+). */
export function formatDistance(meters: number | null | undefined): string {
  if (meters == null || !Number.isFinite(meters)) return "—"
  if (meters >= 1000) return `${Number((meters / 1000).toFixed(2))} km`
  return `${Math.round(meters)} m`
}

/**
 * The single progression metric's unit per exercise type:
 * weighted = max weight (kg), bodyweight = max reps, cardio = duration (time).
 */
export function progressionUnit(
  activityType: ActivityType,
  strengthCategory: StrengthCategory | null
): "kg" | "reps" | "time" {
  if (activityType === "cardio") return "time"
  return strengthCategory === "bodyweight" ? "reps" : "kg"
}

/** Renders a raw progression/value number in its unit (kg / reps / seconds-as-time). */
export function formatProgressionValue(
  value: number,
  unit: "kg" | "reps" | "time"
): string {
  if (unit === "time") return formatDuration(value)
  if (unit === "kg") return formatWeight(value)
  return `${Number(value.toFixed(0))}`
}

/** ISO year+week → compact "2026-W25" label for chart axes. */
export function formatIsoWeek(isoYear: number, isoWeek: number): string {
  return `${isoYear}-W${String(isoWeek).padStart(2, "0")}`
}
