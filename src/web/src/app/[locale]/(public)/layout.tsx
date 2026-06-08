import { Layers } from "lucide-react"

export default function PublicLayout({ children }: { children: React.ReactNode }) {
  return (
    <div className="grid min-h-screen lg:grid-cols-2">
      {/* Branding panel — desktop only */}
      <div className="hidden flex-col justify-between bg-primary p-12 text-primary-foreground lg:flex">
        <div className="flex items-center gap-2">
          <Layers className="h-5 w-5" />
          <span className="text-base font-bold tracking-tight">MyOS</span>
        </div>
        <div className="space-y-3">
          <p className="text-3xl font-semibold leading-snug">
            Your personal<br />operating system.
          </p>
          <p className="text-sm text-primary-foreground/60">
            Notes, tasks, finance, fitness — all in one place.
          </p>
        </div>
        <p className="text-xs text-primary-foreground/30">© 2026 MyOS</p>
      </div>

      {/* Form panel */}
      <div className="flex items-center justify-center p-8">
        {children}
      </div>
    </div>
  )
}
