import createIntlMiddleware from "next-intl/middleware"
import { type NextRequest, NextResponse } from "next/server"
import { decodeJwt } from "jose"
import { SUPPORTED_LOCALES, DEFAULT_LOCALE } from "@/shared/types/common.types"

const intlMiddleware = createIntlMiddleware({
  locales: SUPPORTED_LOCALES,
  defaultLocale: DEFAULT_LOCALE,
  localePrefix: "always",
})

// Path segments that require authentication (after the locale prefix)
const PROTECTED_SEGMENTS = ["/home", "/notes", "/storage", "/settings", "/learning", "/finance", "/fitness"]

// Public-only paths (redirect to app if already authenticated)
const PUBLIC_AUTH_SEGMENTS = ["/login", "/register"]

function getPathAfterLocale(pathname: string): string {
  for (const locale of SUPPORTED_LOCALES) {
    if (pathname.startsWith(`/${locale}/`) || pathname === `/${locale}`) {
      return pathname.slice(locale.length + 1) || "/"
    }
  }
  return pathname
}

function isProtectedPath(pathname: string): boolean {
  const path = getPathAfterLocale(pathname)
  return PROTECTED_SEGMENTS.some((seg) => path.startsWith(seg))
}

function isPublicAuthPath(pathname: string): boolean {
  const path = getPathAfterLocale(pathname)
  return PUBLIC_AUTH_SEGMENTS.some((seg) => path.startsWith(seg))
}

function isTokenValid(token: string): boolean {
  try {
    const { exp } = decodeJwt(token)
    return !!exp && exp * 1000 > Date.now()
  } catch {
    return false
  }
}

function getLocaleFromPath(pathname: string): string {
  for (const locale of SUPPORTED_LOCALES) {
    if (pathname.startsWith(`/${locale}/`) || pathname === `/${locale}`) {
      return locale
    }
  }
  return DEFAULT_LOCALE
}

function redirectToLogin(request: NextRequest, clearCookies = false): NextResponse {
  const locale = getLocaleFromPath(request.nextUrl.pathname)
  const loginUrl = new URL(`/${locale}/login`, request.url)
  const response = NextResponse.redirect(loginUrl)
  if (clearCookies) {
    response.cookies.delete("access_token")
    response.cookies.delete("refresh_token")
  }
  return response
}

export async function proxy(request: NextRequest): Promise<NextResponse> {
  const { pathname } = request.nextUrl

  // Versioned API proxy: inject Authorization header so the Next.js rewrite can forward it to the backend.
  // The browser can't read httpOnly cookies, so it can't set Bearer itself.
  if (pathname.startsWith("/api/v")) {
    const accessToken = request.cookies.get("access_token")?.value
    if (accessToken) {
      const requestHeaders = new Headers(request.headers)
      requestHeaders.set("Authorization", `Bearer ${accessToken}`)
      return NextResponse.next({ request: { headers: requestHeaders } })
    }
    return NextResponse.next()
  }

  if (isProtectedPath(pathname)) {
    const accessToken = request.cookies.get("access_token")?.value
    const refreshToken = request.cookies.get("refresh_token")?.value

    // Valid access token → continue
    if (accessToken && isTokenValid(accessToken)) {
      return intlMiddleware(request)
    }

    // No refresh token → redirect to login
    if (!refreshToken) {
      return redirectToLogin(request)
    }

    // Access token expired — attempt refresh
    try {
      const apiBase = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:7255"
      const res = await fetch(`${apiBase}/api/v1/auth/refresh`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ token: refreshToken }),
      })

      if (!res.ok) {
        return redirectToLogin(request, true)
      }

      const { accessToken: newAccess, refreshToken: newRefresh } = await res.json()
      const isProd = process.env.NODE_ENV === "production"

      const response = intlMiddleware(request)
      response.cookies.set("access_token", newAccess, {
        httpOnly: true,
        secure: isProd,
        sameSite: "lax",
        path: "/",
      })
      response.cookies.set("refresh_token", newRefresh, {
        httpOnly: true,
        secure: isProd,
        sameSite: "lax",
        path: "/",
      })
      return response
    } catch {
      return redirectToLogin(request, true)
    }
  }

  // Redirect authenticated users away from login/register
  if (isPublicAuthPath(pathname)) {
    const accessToken = request.cookies.get("access_token")?.value
    if (accessToken && isTokenValid(accessToken)) {
      const locale = getLocaleFromPath(pathname)
      return NextResponse.redirect(new URL(`/${locale}/home`, request.url))
    }
  }

  return intlMiddleware(request)
}

export const config = {
  matcher: [
    "/api/v(.*)",                                           // versioned API paths — inject Authorization header
    "/((?!api|_next/static|_next/image|favicon.ico).*)",   // page paths — auth guard + locale routing
  ],
}
