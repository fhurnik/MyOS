import { NextResponse } from "next/server"
import { decodeJwt } from "jose"
import { loginApi } from "@/modules/identity/api/auth.api"
import { ApiError } from "@/shared/lib/api-error"
import type { LoginBody, SessionPayload } from "@/modules/identity/types/identity.types"

const isProd = process.env.NODE_ENV === "production"

const COOKIE_OPTIONS = {
  httpOnly: true,
  secure: isProd,
  sameSite: "lax" as const,
  path: "/",
}

export async function POST(request: Request) {
  try {
    const body: LoginBody = await request.json()
    const tokens = await loginApi(body)

    const payload = decodeJwt(tokens.accessToken)
    const session: SessionPayload = {
      userId: payload.sub ?? "",
      email: (payload.email as string) ?? "",
      language: parseInt((payload.language as string) ?? "0", 10) as 0 | 1,
    }

    const response = NextResponse.json(session)
    response.cookies.set("access_token", tokens.accessToken, COOKIE_OPTIONS)
    response.cookies.set("refresh_token", tokens.refreshToken, COOKIE_OPTIONS)
    return response
  } catch (error) {
    if (error instanceof ApiError) {
      return NextResponse.json(
        { detail: error.detail, errorCode: error.code },
        { status: error.status }
      )
    }
    return NextResponse.json(
      { detail: "Login failed. Check your credentials.", errorCode: "UnknownError" },
      { status: 500 }
    )
  }
}
