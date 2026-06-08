import type { NextConfig } from "next"
import createNextIntlPlugin from "next-intl/plugin"

const withNextIntl = createNextIntlPlugin("./src/i18n/request.ts")

const nextConfig: NextConfig = {
  async rewrites() {
    const apiUrl = process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5042"
    return [
      {
        source: "/api/v:version/:path*",
        destination: `${apiUrl}/api/v:version/:path*`,
      },
    ]
  },
}

export default withNextIntl(nextConfig)
