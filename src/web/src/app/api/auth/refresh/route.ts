import { NextResponse } from "next/server"
import { decodeJwt } from "jose"
import { refreshTokenApi } from "@/modules/identity/api/auth.api"
import { ApiError } from "@/shared/lib/api-error"
import { authCookieOptions, requestIsHttps } from "@/shared/lib/auth-cookies"

export async function POST(request: Request) {
  try {
    const { refreshToken } = await request.json()

    const tokens = await refreshTokenApi(refreshToken)

    const COOKIE_OPTIONS = authCookieOptions(
      requestIsHttps(request.url, request.headers.get("x-forwarded-proto"))
    )

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
  } catch (error) {
    if (error instanceof ApiError) {
      return NextResponse.json(
        { detail: error.detail, errorCode: error.code },
        { status: error.status }
      )
    }
    return NextResponse.json(
      { detail: "Token refresh failed.", errorCode: "UnknownError" },
      { status: 500 }
    )
  }
}
