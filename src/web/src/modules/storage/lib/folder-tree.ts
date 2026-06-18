import type { FolderDto } from "@/modules/storage/types/storage.types"

// Builds the ancestor chain (root → … → current) for breadcrumb rendering.
export function getBreadcrumbPath(folders: FolderDto[], currentFolderId: string | null): FolderDto[] {
  if (!currentFolderId) return []
  const byId = new Map(folders.map((f) => [f.id, f]))
  const path: FolderDto[] = []
  const visited = new Set<string>()
  let id: string | null = currentFolderId
  while (id) {
    const folder: FolderDto | undefined = byId.get(id)
    if (!folder || visited.has(id)) break
    visited.add(id)
    path.unshift(folder)
    id = folder.parentId
  }
  return path
}

// All transitive descendants of a folder — used to forbid moving a folder into its own subtree.
export function getDescendantFolderIds(folders: FolderDto[], folderId: string): Set<string> {
  const childrenByParent = new Map<string | null, FolderDto[]>()
  for (const folder of folders) {
    const list = childrenByParent.get(folder.parentId) ?? []
    list.push(folder)
    childrenByParent.set(folder.parentId, list)
  }
  const result = new Set<string>()
  const stack = [folderId]
  while (stack.length) {
    const id = stack.pop()!
    for (const child of childrenByParent.get(id) ?? []) {
      if (!result.has(child.id)) {
        result.add(child.id)
        stack.push(child.id)
      }
    }
  }
  return result
}
