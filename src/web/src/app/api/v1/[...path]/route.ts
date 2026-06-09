import { type NextRequest, NextResponse } from "next/server"
import { cookies } from "next/headers"
import * as https from "node:https"
import * as http from "node:http"

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5042"

// Native fetch (undici) ignores NODE_TLS_REJECT_UNAUTHORIZED in dev.
// Use node:https directly with rejectUnauthorized: false for local self-signed certs.
const devHttpsAgent =
  process.env.NODE_ENV !== "production"
    ? new https.Agent({ rejectUnauthorized: false })
    : undefined

function nodeRequest(
  url: URL,
  method: string,
  headers: Record<string, string>,
  body?: ArrayBuffer
): Promise<Response> {
  return new Promise((resolve, reject) => {
    const isHttps = url.protocol === "https:"
    const mod = isHttps ? https : http
    const agent = isHttps && devHttpsAgent ? devHttpsAgent : undefined

    const req = mod.request(
      {
        hostname: url.hostname,
        port: url.port || (isHttps ? 443 : 80),
        path: url.pathname + url.search,
        method,
        headers,
        agent,
      },
      (res) => {
        const chunks: Buffer[] = []
        res.on("data", (chunk: Buffer) => chunks.push(chunk))
        res.on("end", () => {
          const status = res.statusCode ?? 200
          const resHeaders = new Headers()
          for (const [k, v] of Object.entries(res.headers)) {
            if (v) resHeaders.set(k, Array.isArray(v) ? v.join(", ") : v)
          }
          const isNullBody = [204, 205, 304].includes(status)
          resolve(
            new Response(isNullBody ? null : Buffer.concat(chunks), {
              status,
              headers: resHeaders,
            })
          )
        })
        res.on("error", reject)
      }
    )

    req.on("error", reject)
    if (body && body.byteLength > 0) req.write(Buffer.from(body))
    req.end()
  })
}

async function proxyToBackend(request: NextRequest, path: string[]): Promise<NextResponse> {
  const targetUrl = new URL(`${API_BASE}/api/v1/${path.join("/")}`)
  targetUrl.search = request.nextUrl.search

  const cookieStore = await cookies()
  const accessToken = cookieStore.get("access_token")?.value

  const headers: Record<string, string> = {}
  const contentType = request.headers.get("content-type")
  if (contentType) headers["content-type"] = contentType
  if (accessToken) headers["authorization"] = `Bearer ${accessToken}`

  const hasBody = request.method !== "GET" && request.method !== "HEAD"
  const body = hasBody ? await request.arrayBuffer() : undefined

  const response = await nodeRequest(targetUrl, request.method, headers, body)

  if (response.status === 204) {
    return new NextResponse(null, { status: 204 })
  }

  const responseBody = await response.arrayBuffer()
  const responseHeaders = new Headers()
  const responseContentType = response.headers.get("content-type")
  if (responseContentType) responseHeaders.set("content-type", responseContentType)

  return new NextResponse(responseBody, {
    status: response.status,
    headers: responseHeaders,
  })
}

export async function GET(request: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
  const { path } = await params
  return proxyToBackend(request, path)
}

export async function POST(request: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
  const { path } = await params
  return proxyToBackend(request, path)
}

export async function PUT(request: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
  const { path } = await params
  return proxyToBackend(request, path)
}

export async function PATCH(request: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
  const { path } = await params
  return proxyToBackend(request, path)
}

export async function DELETE(request: NextRequest, { params }: { params: Promise<{ path: string[] }> }) {
  const { path } = await params
  return proxyToBackend(request, path)
}
