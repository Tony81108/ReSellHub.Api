import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue()],
  base: '/app/',
  build: {
    outDir: fileURLToPath(new URL('../wwwroot/app', import.meta.url)),
    emptyOutDir: true
  },
  server: {
    port: 5173,
    proxy: {
      '/api': { target: 'https://localhost:7152', changeOrigin: true, secure: false }
    }
  }
})
