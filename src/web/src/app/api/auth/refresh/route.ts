import { NextResponse } from "next/server"
import { decodeJwt } from "jose"
import { refreshTokenApi } from "@/modules/identity/api/auth.api"
import { ApiError } from "@/shared/lib/api-error"

const isProd = process.env.NODE_ENV === "production"

const COOKIE_OPTIONS = {
  httpOnly: true,
  secure: isProd,
  sameSite: "lax" as const,
  path: "/",
}

export async function POST(request: Request) {
  try {
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
