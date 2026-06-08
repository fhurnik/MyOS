import type { ProblemDetails } from "@/shared/types/api.types"

export class ApiError extends Error {
  readonly status: number
  readonly code: string
  readonly detail: string
  readonly traceId: string
  readonly correlationId: string

  constructor(problem: ProblemDetails) {
    super(problem.detail || problem.title)
    this.name = "ApiError"
    this.status = problem.status
    this.code = problem.errorCode
    this.detail = problem.detail
    this.traceId = problem.traceId
    this.correlationId = problem.correlationId
  }

  static isApiError(error: unknown): error is ApiError {
    return error instanceof ApiError
  }
}
