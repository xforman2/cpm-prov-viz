import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      // The Java provenance service. The Aspire AppHost injects its address as PROVENANCE_SERVICE_HTTP.
      '/api': {
        target: process.env.PROVENANCE_SERVICE_HTTP,
        changeOrigin: true,
      },
    },
  },
})
