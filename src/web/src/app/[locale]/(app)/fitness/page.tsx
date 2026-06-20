import { redirect } from "next/navigation"

// The fitness dashboard lives on the Home page; the module root jumps straight to workouts.
export default async function FitnessPage({ params }: { params: Promise<{ locale: string }> }) {
  const { locale } = await params
  redirect(`/${locale}/fitness/workouts`)
}
