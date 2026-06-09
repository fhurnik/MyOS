"use client"

import type { ReactNode, RefObject } from "react"
import { useTranslations } from "next-intl"
import { ChevronDown, ChevronUp, ChevronsUpDown } from "lucide-react"
import { cn } from "@/shared/lib/utils"
import { Button } from "@/shared/components/ui/button"
import type { PagingList } from "@/shared/types/api.types"

const PAGE_SIZE_OPTIONS = [5, 10, 25, 100]

interface SortColumnDef {
  key: string
  label: string
}

export interface TableColumnDef<T> {
  key: string
  label: string
  render: (item: T) => ReactNode
  sortable?: boolean
  headerClassName?: string
  cellClassName?: string
}

interface SharedProps<T> {
  data: PagingList<T> | undefined
  isLoading: boolean
  page: number
  pageSize: number
  onGoToPage: (page: number) => void
  onPageSizeChange: (size: number) => void
  listRef: RefObject<HTMLDivElement | null>
  keyExtractor: (item: T) => string
  emptyState: ReactNode
  className?: string
  orderBy?: string
  orderByDesc?: boolean
  onSortChange?: (column: string) => void
}

type PaginatedListProps<T> = SharedProps<T> & (
  | { columns: TableColumnDef<T>[]; onRowClick?: (item: T) => void; rowActions?: (item: T) => ReactNode; renderItem?: undefined; itemsClassName?: undefined; sortColumns?: undefined }
  | { columns?: undefined; onRowClick?: undefined; renderItem: (item: T) => ReactNode; itemsClassName?: string; sortColumns?: SortColumnDef[] }
)

function sortIcon(columnKey: string, orderBy: string | undefined, orderByDesc: boolean) {
  if (orderBy !== columnKey) return <ChevronsUpDown className="h-3.5 w-3.5 opacity-40" />
  return orderByDesc
    ? <ChevronDown className="h-3.5 w-3.5" />
    : <ChevronUp className="h-3.5 w-3.5" />
}

export function PaginatedList<T>({
  data,
  isLoading,
  page,
  pageSize,
  onGoToPage,
  onPageSizeChange,
  listRef,
  keyExtractor,
  emptyState,
  className,
  orderBy,
  orderByDesc = false,
  onSortChange,
  columns,
  onRowClick,
  rowActions,
  renderItem,
  itemsClassName,
  sortColumns,
}: PaginatedListProps<T>) {
  const tCommon = useTranslations("common")

  if (isLoading) {
    return <p className="text-sm text-muted-foreground">…</p>
  }

  if (!data?.items.length) {
    return <>{emptyState}</>
  }

  return (
    <div ref={listRef} className={cn("mx-auto max-w-2xl", className)}>
      {columns ? (
        <table className="w-full">
          <thead>
            <tr className="border-b">
              {columns.map((col) => {
                const isSortable = col.sortable && !!onSortChange
                return (
                  <th
                    key={col.key}
                    onClick={isSortable ? () => onSortChange!(col.key) : undefined}
                    className={cn(
                      "px-4 py-2 text-left text-sm font-medium text-muted-foreground",
                      isSortable && "cursor-pointer select-none hover:text-foreground",
                      orderBy === col.key && "text-foreground",
                      col.headerClassName
                    )}
                  >
                    <span className="inline-flex items-center gap-1">
                      {col.label}
                      {isSortable && sortIcon(col.key, orderBy, orderByDesc)}
                    </span>
                  </th>
                )
              })}
              {rowActions && <th className="w-12" />}
            </tr>
          </thead>
          <tbody>
            {data.items.map((item) => (
              <tr
                key={keyExtractor(item)}
                onClick={onRowClick ? () => onRowClick(item) : undefined}
                className={cn("border-b last:border-0 hover:bg-accent/50 transition-colors", onRowClick && "cursor-pointer")}
              >
                {columns.map((col) => (
                  <td key={col.key} className={cn("px-4 py-3 text-sm", col.cellClassName)}>
                    {col.render(item)}
                  </td>
                ))}
                {rowActions && (
                  <td className="w-12 px-2 py-2 text-right" onClick={(e) => e.stopPropagation()}>
                    {rowActions(item)}
                  </td>
                )}
              </tr>
            ))}
          </tbody>
        </table>
      ) : (
        <>
          {sortColumns && onSortChange && (
            <div className="flex items-center gap-1 pb-2">
              {sortColumns.map((col) => {
                const isActive = orderBy === col.key
                return (
                  <button
                    key={col.key}
                    onClick={() => onSortChange(col.key)}
                    className={cn(
                      "flex items-center gap-1 rounded px-2 py-1 text-sm transition-colors hover:bg-accent",
                      isActive ? "text-foreground font-medium" : "text-muted-foreground"
                    )}
                  >
                    {col.label}
                    {isActive ? (
                      orderByDesc
                        ? <ChevronDown className="h-3.5 w-3.5" />
                        : <ChevronUp className="h-3.5 w-3.5" />
                    ) : (
                      <ChevronsUpDown className="h-3.5 w-3.5 opacity-40" />
                    )}
                  </button>
                )
              })}
            </div>
          )}
          <div className={itemsClassName}>
            {data.items.map((item) => (
              <div key={keyExtractor(item)}>{renderItem!(item)}</div>
            ))}
          </div>
        </>
      )}

      <div className="flex items-center justify-between pt-4">
        <select
          value={pageSize}
          onChange={(e) => onPageSizeChange(Number(e.target.value))}
          className="h-8 rounded-md border border-input bg-transparent px-2 text-sm outline-none focus:border-ring"
        >
          {PAGE_SIZE_OPTIONS.map((size) => (
            <option key={size} value={size}>
              {size}
            </option>
          ))}
        </select>

        <div className="flex items-center gap-3">
          <Button
            variant="outline"
            size="sm"
            onClick={() => onGoToPage(page - 1)}
            disabled={!data.hasPreviousPage}
          >
            {tCommon("previousPage")}
          </Button>
          <span className="text-sm text-muted-foreground">
            {tCommon("pageOf", { page, total: data.totalPages })}
          </span>
          <Button
            variant="outline"
            size="sm"
            onClick={() => onGoToPage(page + 1)}
            disabled={!data.hasNextPage}
          >
            {tCommon("nextPage")}
          </Button>
        </div>
      </div>
    </div>
  )
}
