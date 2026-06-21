// Auth cookie options shared by the BFF route handlers and proxy.ts.
//
// `secure` cookies are rejected by browsers over plain HTTP (except http://localhost), which
// breaks login when the app is reached over http://<lan-ip> in a homelab. Instead of tying
// `secure` to NODE_ENV, derive it from the actual request scheme: cookies work over http on
// the LAN and automatically become secure once the app is served over https.

export function authCookieOptions(secure: boolean) {
  return {
    httpOnly: true as const,
    secure,
    sameSite: "lax" as const,
    path: "/",
  }
}

// True when the original request used https. Honours x-forwarded-proto so it stays correct
// behind a TLS-terminating reverse proxy; falls back to the request URL scheme otherwise.
export function requestIsHttps(url: string, forwardedProto?: string | null): boolean {
  const proto = forwardedProto?.split(",")[0]?.trim()
  if (proto) return proto === "https"
  return new URL(url).protocol === "https:"
}
