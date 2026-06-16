import { NextResponse } from "next/server"
import { cookies } from "next/headers"
import { decodeJwt } from "jose"
import { changeLanguageApi } from "@/modules/identity/api/users.api"
import { ApiError } from "@/shared/lib/api-error"
import type { Language } from "@/shared/types/common.types"
import type { SessionPayload } from "@/modules/identity/types/identity.types"

const isProd = process.env.NODE_ENV === "production"

const COOKIE_OPTIONS = {
  httpOnly: true,
  secure: isProd,
  sameSite: "lax" as const,
  path: "/",
}

export async function PATCH(request: Request) {
  try {
    const cookieStore = await cookies()
    const refreshToken = cookieStore.get("refresh_token")?.value
    const accessToken = cookieStore.get("access_token")?.value

    if (!refreshToken) {
      return NextResponse.json(
        { detail: "Not authenticated", errorCode: "Unauthorized" },
        { status: 401 }
      )
    }

    const { language }: { language: Language } = await request.json()
    const tokens = await changeLanguageApi({ language, refreshToken }, accessToken)

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
      { detail: "Language change failed.", errorCode: "UnknownError" },
      { status: 500 }
    )
  }
}
