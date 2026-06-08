export interface PagingList<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface PagingRequest {
  page?: number
  pageSize?: number
  orderBy?: string
  orderByDesc?: boolean
}

export interface ProblemDetails {
  status: number
  title: string
  detail: string
  instance: string
  traceId: string
  correlationId: string
  errorCode: string
}
