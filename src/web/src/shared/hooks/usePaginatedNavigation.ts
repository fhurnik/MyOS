"use client"

import { useRef, useState } from "react"
import { usePathname, useRouter } from "next/navigation"

interface UsePaginatedNavigationOptions<TColumn extends string> {
  initialPage: number
  initialPageSize: number
  initialOrderBy?: TColumn
  initialOrderByDesc?: boolean
}

export function usePaginatedNavigation<TColumn extends string = string>({
  initialPage,
  initialPageSize,
  initialOrderBy,
  initialOrderByDesc = false,
}: UsePaginatedNavigationOptions<TColumn>) {
  const pathname = usePathname()
  const router = useRouter()
  const listRef = useRef<HTMLDivElement>(null)

  const [page, setPage] = useState(initialPage)
  const [pageSize, setPageSize] = useState(initialPageSize)
  const [orderBy, setOrderBy] = useState<TColumn | undefined>(initialOrderBy)
  const [orderByDesc, setOrderByDesc] = useState(initialOrderByDesc)

  function buildUrl(p: number, ps: number, ob?: string, obd?: boolean) {
    const query = new URLSearchParams()
    query.set("page", String(p))
    query.set("pageSize", String(ps))
    if (ob) query.set("orderBy", ob)
    if (obd) query.set("orderByDesc", "true")
    return `${pathname}?${query.toString()}`
  }

  function scrollToListIfNeeded() {
    if (listRef.current) {
      const rect = listRef.current.getBoundingClientRect()
      if (rect.top < 0) {
        listRef.current.scrollIntoView({ behavior: "smooth", block: "start" })
      }
    }
  }

  function goToPage(newPage: number) {
    setPage(newPage)
    router.push(buildUrl(newPage, pageSize, orderBy, orderByDesc), { scroll: false })
    scrollToListIfNeeded()
  }

  function handlePageSizeChange(newSize: number) {
    setPageSize(newSize)
    setPage(1)
    router.push(buildUrl(1, newSize, orderBy, orderByDesc), { scroll: false })
    scrollToListIfNeeded()
  }

  function handleSortChange(column: TColumn) {
    const newDesc = column === orderBy ? !orderByDesc : false
    setOrderBy(column)
    setOrderByDesc(newDesc)
    setPage(1)
    router.push(buildUrl(1, pageSize, column, newDesc), { scroll: false })
    scrollToListIfNeeded()
  }

  return { page, pageSize, orderBy, orderByDesc, goToPage, handlePageSizeChange, handleSortChange, listRef }
}
