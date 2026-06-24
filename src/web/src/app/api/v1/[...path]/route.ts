import { type NextRequest, NextResponse } from "next/server"
import { cookies } from "next/headers"
import * as https from "node:https"
import * as http from "node:http"
import type { IncomingMessage } from "node:http"
import { Readable } from "node:stream"

const API_BASE = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5042"

// Native fetch (undici) ignores NODE_TLS_REJECT_UNAUTHORIZED in dev.
// Use node:https directly with rejectUnauthorized: false for local self-signed certs.
const devHttpsAgent =
  process.env.NODE_ENV !== "production"
    ? new https.Agent({ rejectUnauthorized: false })
    : undefined

// Hop-by-hop headers must not be forwarded; the rest (incl. Accept-Ranges, Content-Range,
// Content-Length, Content-Disposition) are passed through so range requests / downloads work.
const HOP_BY_HOP = new Set([
  "connection",
  "keep-alive",
  "transfer-encoding",
  "te",
  "trailer",
  "upgrade",
  "proxy-authenticate",
  "proxy-authorization",
])

interface BackendResponse {
  status: number
  headers: Headers
  body: ReadableStream<Uint8Array> | null
}

function nodeRequest(
  url: URL,
  method: string,
  headers: Record<string, string>,
  body?: ReadableStream<Uint8Array> | null
): Promise<BackendResponse> {
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
      (res: IncomingMessage) => {
        const status = res.statusCode ?? 200

        const resHeaders = new Headers()
        for (const [k, v] of Object.entries(res.headers)) {
          if (!v || HOP_BY_HOP.has(k.toLowerCase())) continue
          resHeaders.set(k, Array.isArray(v) ? v.join(", ") : v)
        }

        if ([204, 205, 304].includes(status)) {
          res.resume() // drain
          resolve({ status, headers: resHeaders, body: null })
          return
        }

        // Stream the response (with basic backpressure) so large media isn't buffered in memory
        // and 206/Range responses pass through intact.
        const stream = new ReadableStream<Uint8Array>({
          start(controller) {
            res.on("data", (chunk: Buffer) => {
              controller.enqueue(new Uint8Array(chunk))
              if (controller.desiredSize !== null && controller.desiredSize <= 0) res.pause()
            })
            res.on("end", () => controller.close())
            res.on("error", (err) => controller.error(err))
          },
          pull() {
            res.resume()
          },
          cancel() {
            res.destroy()
          },
        })

        resolve({ status, headers: resHeaders, body: stream })
      }
    )

    req.on("error", reject)

    if (body) {
      // Stream the request body straight through instead of buffering it in memory,
      // so large uploads (phone photos/videos up to the backend's 1 GB limit) don't
      // exhaust the Next.js server's heap.
      const nodeBody = Readable.fromWeb(body as Parameters<typeof Readable.fromWeb>[0])
      nodeBody.on("error", (err) => req.destroy(err))
      nodeBody.pipe(req)
    } else {
      req.end()
    }
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
  // Preserve Content-Length so the streamed body isn't forced into chunked encoding.
  const contentLength = request.headers.get("content-length")
  if (contentLength) headers["content-length"] = contentLength
  if (accessToken) headers["authorization"] = `Bearer ${accessToken}`
  const acceptLanguage = request.headers.get("accept-language")
  if (acceptLanguage) headers["accept-language"] = acceptLanguage
  // Forward range headers so audio/video seeking and resumable downloads work.
  const range = request.headers.get("range")
  if (range) headers["range"] = range
  const ifRange = request.headers.get("if-range")
  if (ifRange) headers["if-range"] = ifRange

  const hasBody = request.method !== "GET" && request.method !== "HEAD"
  const body = hasBody ? request.body : undefined

  const response = await nodeRequest(targetUrl, request.method, headers, body)

  return new NextResponse(response.body, {
    status: response.status,
    headers: response.headers,
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
