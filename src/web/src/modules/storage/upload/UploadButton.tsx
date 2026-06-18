"use client"

import { useRef } from "react"
import { useTranslations } from "next-intl"
import { Upload } from "lucide-react"
import { Button } from "@/shared/components/ui/button"

export function UploadButton({ onFiles }: { onFiles: (files: File[]) => void }) {
  const t = useTranslations("storage")
  const inputRef = useRef<HTMLInputElement>(null)

  return (
    <>
      <Button size="sm" variant="outline" className="shrink-0" onClick={() => inputRef.current?.click()}>
        <Upload className="h-4 w-4" />
        {t("upload.button")}
      </Button>
      <input
        ref={inputRef}
        type="file"
        multiple
        hidden
        onChange={(e) => {
          if (e.target.files) onFiles(Array.from(e.target.files))
          e.target.value = ""
        }}
      />
    </>
  )
}
