import type { CheckListSummaryDto } from "@/modules/notes/types/notes.types"
import { cn } from "@/shared/lib/utils"

interface CheckListCardProps {
  list: CheckListSummaryDto
  className?: string
}

export function CheckListCard({ list, className }: CheckListCardProps) {
  return (
    <div
      className={cn(
        "rounded-lg border bg-card p-4 shadow-sm transition-all hover:-translate-y-0.5 hover:shadow-md",
        className
      )}
    >
      <h3 className="font-medium leading-tight">{list.title}</h3>
      <p className="mt-2 text-xs text-muted-foreground/70">
        {new Date(list.createdAtUtc).toLocaleDateString()}
      </p>
    </div>
  )
}
