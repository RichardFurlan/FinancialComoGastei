import type { Config } from 'tailwindcss'

export default {
  content: [],
  plugins: [require('daisyui')],
  daisyui: {
    themes: ['light', 'dark'],
  },
} satisfies Config
