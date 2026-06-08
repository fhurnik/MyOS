import type { TextNoteDto } from "@/modules/notes/types/notes.types"
import { cn } from "@/shared/lib/utils"

interface TextNoteCardProps {
  note: TextNoteDto
  className?: string
}

export function TextNoteCard({ note, className }: TextNoteCardProps) {
  return (
    <div
      className={cn(
        "rounded-lg border bg-card p-4 shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md",
        className
      )}
    >
      <h3 className="mb-1 font-medium leading-tight">{note.title}</h3>
      <p className="line-clamp-3 text-sm text-muted-foreground">{note.text}</p>
      <p className="mt-2 text-xs text-muted-foreground/70">
        {new Date(note.createdAtUtc).toLocaleDateString()}
      </p>
    </div>
  )
}
