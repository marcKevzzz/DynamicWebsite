import { defineConfig } from 'vite'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [tailwindcss()],
  publicDir: false,
  base: './',
  build: {
    outDir: 'wwwroot/assets',
    emptyOutDir: true,
    cssCodeSplit: false,
    rollupOptions: {
      input: 'Frontend/site.js',
      output: {
        entryFileNames: 'site.js',
        assetFileNames: asset => asset.names.includes('style.css') ? 'site.css' : '[name]-[hash][extname]',
      },
    },
  },
})
