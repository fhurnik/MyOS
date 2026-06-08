import { cookies } from "next/headers"
import { decodeJwt } from "jose"
import type { Session } from "@/shared/providers/SessionProvider"

export async function getServerSession(): Promise<Session | null> {
  const cookieStore = await cookies()
  const token = cookieStore.get("access_token")?.value
  if (!token) return null

  try {
    const payload = decodeJwt(token)
    if (!payload.sub) return null
    return {
      userId: payload.sub,
      email: (payload.email as string) ?? "",
      language: parseInt((payload.language as string) ?? "0", 10) as 0 | 1,
    }
  } catch {
    return null
  }
}

export async function getServerToken(): Promise<string | null> {
  const cookieStore = await cookies()
  return cookieStore.get("access_token")?.value ?? null
}
