import type { ProblemDetails } from "@/shared/types/api.types"

// Upload via XMLHttpRequest because fetch cannot report upload progress.
// Relative URL → proxy injects the Authorization header (same as the rest of the client).
export function uploadFileXhr(
  file: File,
  folderId: string | null,
  onProgress: (percent: number) => void
): Promise<void> {
  return new Promise((resolve, reject) => {
    const query = folderId ? `?folderId=${folderId}` : ""
    const xhr = new XMLHttpRequest()
    xhr.open("POST", `/api/v1/storage/files${query}`)

    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable) {
        onProgress(Math.round((event.loaded / event.total) * 100))
      }
    }

    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        resolve()
        return
      }
      let message = "Upload failed"
      try {
        const problem = JSON.parse(xhr.responseText) as ProblemDetails
        message = problem.detail || problem.title || message
      } catch {
        // non-JSON error body — keep the default message
      }
      reject(new Error(message))
    }

    xhr.onerror = () => reject(new Error("Network error"))

    const form = new FormData()
    form.append("file", file)
    // Do not set Content-Type — the browser sets the multipart boundary automatically.
    xhr.send(form)
  })
}
