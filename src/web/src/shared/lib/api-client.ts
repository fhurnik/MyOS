import { ApiError } from "@/shared/lib/api-error"
import type { ProblemDetails } from "@/shared/types/api.types"

// Server-side: full URL to call backend directly. Client-side: empty → relative URL → Next.js rewrite proxies it.
const API_BASE =
  typeof window === "undefined" ? (process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5042") : ""

interface RequestOptions extends Omit<RequestInit, "body"> {
  token?: string
  body?: unknown
}

export async function apiClient<TResponse>(
  path: string,
  options: RequestOptions = {}
): Promise<TResponse> {
  const { token, body, headers: extraHeaders, ...rest } = options

  const headers: HeadersInit = {
    "Content-Type": "application/json",
    ...extraHeaders,
  }

  if (token) {
    (headers as Record<string, string>)["Authorization"] = `Bearer ${token}`
  }

  const response = await fetch(`${API_BASE}${path}`, {
    ...rest,
    headers,
    body: body !== undefined ? JSON.stringify(body) : undefined,
    credentials: "include",
  })

  if (response.status === 204) {
    return undefined as TResponse
  }

  if (!response.ok) {
    let problem: ProblemDetails
    try {
      problem = await response.json()
    } catch {
      problem = {
        status: response.status,
        title: "Request failed",
        detail: response.statusText,
        instance: path,
        traceId: "",
        correlationId: "",
        errorCode: "UnknownError",
      }
    }
    throw new ApiError(problem)
  }

  return response.json() as Promise<TResponse>
}
