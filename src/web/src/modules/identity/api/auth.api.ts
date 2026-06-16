import { apiClient } from "@/shared/lib/api-client"
import type { AuthTokens, LoginBody, RegisterBody } from "@/modules/identity/types/identity.types"

export async function loginApi(body: LoginBody, acceptLanguage?: string): Promise<AuthTokens> {
  return apiClient<AuthTokens>("/api/v1/auth/login", {
    method: "POST",
    body,
    headers: acceptLanguage ? { "Accept-Language": acceptLanguage } : undefined,
  })
}

export async function registerApi(body: RegisterBody, acceptLanguage?: string): Promise<string> {
  return apiClient<string>("/api/v1/auth/register", {
    method: "POST",
    body,
    headers: acceptLanguage ? { "Accept-Language": acceptLanguage } : undefined,
  })
}

export async function refreshTokenApi(token: string): Promise<AuthTokens> {
  return apiClient<AuthTokens>("/api/v1/auth/refresh", {
    method: "POST",
    body: { token },
  })
}
