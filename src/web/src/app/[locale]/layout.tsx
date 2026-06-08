import { NextIntlClientProvider } from "next-intl"
import { getMessages } from "next-intl/server"
import { notFound } from "next/navigation"
import { routing } from "@/i18n/routing"
import { SessionProvider } from "@/shared/providers/SessionProvider"
import { getServerSession } from "@/shared/lib/session"

type Props = {
  children: React.ReactNode
  params: Promise<{ locale: string }>
}

export default async function LocaleLayout({ children, params }: Props) {
  const { locale } = await params

  if (!routing.locales.includes(locale as "en" | "pl")) {
    notFound()
  }

  const messages = await getMessages()
  const session = await getServerSession()

  return (
    <NextIntlClientProvider messages={messages}>
      <SessionProvider session={session}>{children}</SessionProvider>
    </NextIntlClientProvider>
  )
}
