"use client"

import type { CheckListItemDto } from "@/modules/notes/types/notes.types"
import { cn } from "@/shared/lib/utils"

interface CheckListItemProps {
  item: CheckListItemDto
  onToggle: () => void
  onDelete: () => void
}

export function CheckListItem({ item, onToggle, onDelete }: CheckListItemProps) {
  return (
    <li className="flex items-center gap-3 rounded-md px-2 py-1.5 hover:bg-muted/50">
      <input
        type="checkbox"
        checked={item.isChecked}
        onChange={onToggle}
        className="h-4 w-4 cursor-pointer rounded border-input accent-primary"
      />
      <span
        className={cn(
          "flex-1 text-sm",
          item.isChecked && "text-muted-foreground line-through"
        )}
      >
        {item.text}
      </span>
      <button
        onClick={onDelete}
        className="text-muted-foreground/50 transition-colors hover:text-destructive"
        aria-label="Remove item"
      >
        ×
      </button>
    </li>
  )
}
