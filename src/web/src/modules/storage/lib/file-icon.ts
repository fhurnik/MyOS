import { FileText, FileVideo, FileAudio, FileArchive, FileImage, File, type LucideIcon } from "lucide-react"

// Maps an AllowedFileType category (audio/video/document/archive/text/image) to a lucide icon.
const CATEGORY_ICONS: Record<string, LucideIcon> = {
  text: FileText,
  document: FileText,
  image: FileImage,
  audio: FileAudio,
  video: FileVideo,
  archive: FileArchive,
}

export function getFileIconByCategory(category: string | undefined): LucideIcon {
  return (category && CATEGORY_ICONS[category]) || File
}
