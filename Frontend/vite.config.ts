import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Overridable so several copies of the app can each talk to their own backend.
const apiTarget = process.env.API_TARGET ?? 'http://localhost:5219'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: { '/api': apiTarget },
  },
})
