import { apiClient } from "@/shared/lib/api-client"
import type { AuthTokens, ChangeLanguageBody } from "@/modules/identity/types/identity.types"

export async function changeLanguageApi(
  body: ChangeLanguageBody,
  token?: string
): Promise<AuthTokens> {
  return apiClient<AuthTokens>("/api/v1/users/me/language", {
    method: "PATCH",
    body,
    token,
  })
}
