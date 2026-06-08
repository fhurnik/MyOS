export const Language = {
  English: 0,
  Polish: 1,
} as const

export type Language = (typeof Language)[keyof typeof Language]

export const LANGUAGE_TO_LOCALE: Record<Language, string> = {
  [Language.English]: "en",
  [Language.Polish]: "pl",
}

export const LOCALE_TO_LANGUAGE: Record<string, Language> = {
  en: Language.English,
  pl: Language.Polish,
}

export const SUPPORTED_LOCALES = ["en", "pl"] as const
export type Locale = (typeof SUPPORTED_LOCALES)[number]
export const DEFAULT_LOCALE: Locale = "en"
