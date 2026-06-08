import { NextResponse } from "next/server"
import { decodeJwt } from "jose"
import { refreshTokenApi } from "@/modules/identity/api/auth.api"

const isProd = process.env.NODE_ENV === "production"

const COOKIE_OPTIONS = {
  httpOnly: true,
  secure: isProd,
  sameSite: "lax" as const,
  path: "/",
}

export async function POST(request: Request) {
  const { refreshToken } = await request.json()

  const tokens = await refreshTokenApi(refreshToken)

  const payload = decodeJwt(tokens.accessToken)
  const session = {
    userId: payload.sub ?? "",
    email: (payload.email as string) ?? "",
    language: parseInt((payload.language as string) ?? "0", 10),
  }

  const response = NextResponse.json(session)
  response.cookies.set("access_token", tokens.accessToken, COOKIE_OPTIONS)
  response.cookies.set("refresh_token", tokens.refreshToken, COOKIE_OPTIONS)

  return response
}
