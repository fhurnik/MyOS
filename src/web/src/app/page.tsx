import { redirect } from "next/navigation"
import { cookies } from "next/headers"
import { SUPPORTED_LOCALES, DEFAULT_LOCALE } from "@/shared/types/common.types"

export default async function RootPage() {
  const cookieStore = await cookies()
  const preferred = cookieStore.get("preferred_locale")?.value
  const locale =
    preferred && (SUPPORTED_LOCALES as readonly string[]).includes(preferred)
      ? preferred
      : DEFAULT_LOCALE
  redirect(`/${locale}`)
}
