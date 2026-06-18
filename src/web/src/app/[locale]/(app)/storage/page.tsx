import { getServerToken } from "@/shared/lib/session"
import { getFoldersApi } from "@/modules/storage/api/folders.api"
import { getFilesApi } from "@/modules/storage/api/files.api"
import { getQuotaApi, getAllowedFileTypesApi } from "@/modules/storage/api/storage.api"
import { StorageExplorer } from "@/modules/storage/components/StorageExplorer"

export default async function StoragePage() {
  const token = await getServerToken()

  const [folders, files, quota, allowedTypes] = await Promise.all([
    getFoldersApi(token ?? undefined),
    getFilesApi(token ?? undefined),
    getQuotaApi(token ?? undefined),
    getAllowedFileTypesApi(token ?? undefined),
  ])

  return (
    <StorageExplorer
      initialFolders={folders}
      initialFiles={files}
      initialQuota={quota}
      initialAllowedTypes={allowedTypes}
    />
  )
}
