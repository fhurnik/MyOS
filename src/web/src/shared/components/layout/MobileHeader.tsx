"use client"

import { MobileNav } from "./MobileNav"

export function MobileHeader() {
  return (
    <header className="flex md:hidden h-[var(--mobile-header-h)] shrink-0 items-center gap-3 border-b bg-card px-3">
      <MobileNav />
      <span className="text-sm font-semibold">MyOS</span>
    </header>
  )
}
