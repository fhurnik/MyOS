import type { Language } from "@/shared/types/common.types"

export interface AuthTokens {
  accessToken: string
  refreshToken: string
}

export interface RegisterBody {
  firstName: string
  lastName: string
  email: string
  password: string
}

export interface LoginBody {
  email: string
  password: string
}

export interface ChangeLanguageBody {
  language: Language
  refreshToken: string
}

export interface SessionPayload {
  userId: string
  email: string
  language: Language
}
