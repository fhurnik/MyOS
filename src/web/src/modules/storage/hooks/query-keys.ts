export const storageKeys = {
  all: ["storage"] as const,
  files: () => [...storageKeys.all, "files"] as const,
  folders: () => [...storageKeys.all, "folders"] as const,
  quota: () => [...storageKeys.all, "quota"] as const,
  allowedTypes: () => [...storageKeys.all, "allowed-types"] as const,
}
