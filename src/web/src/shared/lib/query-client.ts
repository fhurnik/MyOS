import { MutationCache, QueryClient } from "@tanstack/react-query"
import { toast } from "sonner"
import { ApiError } from "@/shared/lib/api-error"

export function makeQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: 60 * 1000,
        retry: 1,
      },
    },
  })
}

let browserQueryClient: QueryClient | undefined

export function getQueryClient() {
  if (typeof window === "undefined") {
    return makeQueryClient()
  }
  if (!browserQueryClient) {
    browserQueryClient = new QueryClient({
      mutationCache: new MutationCache({
        onError: (error) => {
          toast.error(ApiError.isApiError(error) ? error.detail : "An error occurred")
        },
      }),
      defaultOptions: {
        queries: {
          staleTime: 60 * 1000,
          retry: 1,
        },
      },
    })
  }
  return browserQueryClient
}
