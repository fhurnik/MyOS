"use client"

import type { ReactNode } from "react"
import { MoreVertical } from "lucide-react"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/shared/components/ui/dropdown-menu"

export interface TileAction {
  key: string
  label: string
  icon: ReactNode
  onSelect?: () => void
  href?: string
  download?: string
  variant?: "default" | "destructive"
}

export function TileActionsMenu({ actions, label }: { actions: TileAction[]; label: string }) {
  return (
    <DropdownMenu>
      <DropdownMenuTrigger
        aria-label={label}
        // Stop the drag sensor (attached on the tile root) from capturing the menu interaction.
        onClick={(e) => e.stopPropagation()}
        onPointerDown={(e) => e.stopPropagation()}
        className="rounded-md p-1 text-muted-foreground opacity-70 transition-colors hover:bg-muted hover:text-foreground"
      >
        <MoreVertical className="h-4 w-4" />
      </DropdownMenuTrigger>
      {/* Menu content is portaled but events still bubble through the React tree to the
          tile's onClick — stop them here so selecting an action never navigates the tile. */}
      <DropdownMenuContent align="end" onClick={(e) => e.stopPropagation()}>
        {actions.map((action) =>
          action.href ? (
            <DropdownMenuItem
              key={action.key}
              variant={action.variant}
              render={<a href={action.href} download={action.download} />}
            >
              {action.icon}
              {action.label}
            </DropdownMenuItem>
          ) : (
            <DropdownMenuItem key={action.key} variant={action.variant} onClick={action.onSelect}>
              {action.icon}
              {action.label}
            </DropdownMenuItem>
          )
        )}
      </DropdownMenuContent>
    </DropdownMenu>
  )
}
