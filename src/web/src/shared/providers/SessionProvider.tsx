"use client"

import { createContext, useContext, type ReactNode } from "react"
import type { Language } from "@/shared/types/common.types"

export interface Session {
  userId: string
  email: string
  language: Language
}

const SessionContext = createContext<Session | null>(null)

export function SessionProvider({
  session,
  children,
}: {
  session: Session | null
  children: ReactNode
}) {
  return (
    <SessionContext.Provider value={session}>
      {children}
    </SessionContext.Provider>
  )
}

export function useSession(): Session | null {
  return useContext(SessionContext)
}

export function useRequiredSession(): Session {
  const session = useContext(SessionContext)
  if (!session) throw new Error("Session required but not available")
  return session
}
