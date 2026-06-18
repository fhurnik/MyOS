export interface StoredFileDto {
  id: string
  folderId: string | null
  originalName: string
  extension: string
  contentType: string
  sizeBytes: number
  createdAtUtc: string
  updatedAtUtc: string | null
  deletedAtUtc: string | null
}

export interface FolderDto {
  id: string
  parentId: string | null
  name: string
  createdAtUtc: string
  updatedAtUtc: string | null
}

export interface QuotaDto {
  userId: string
  maxBytes: number
  usedBytes: number
  availableBytes: number
}

export interface AllowedFileTypeDto {
  extension: string
  contentType: string
  category: string
}

export interface MoveFileBody {
  folderId: string | null
}

export interface CreateFolderBody {
  name: string
  parentId: string | null
}

export interface RenameFolderBody {
  name: string
}

export interface MoveFolderBody {
  parentId: string | null
}
