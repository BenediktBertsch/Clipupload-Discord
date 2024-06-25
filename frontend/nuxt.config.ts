// https://nuxt.com/docs/api/configuration/nuxt-config
import { vite as vidstack } from 'vidstack/plugins';
import vuetify, { transformAssetUrls } from 'vite-plugin-vuetify'

const development = process.env.NODE_ENV;
export default defineNuxtConfig({
  hooks: {
    close: () => {
      // @see https://github.com/nuxt/cli/issues/169#issuecomment-1729300497
      // Workaround for https://github.com/nuxt/cli/issues/169
      process.exit(0)
    }
  },
  devtools: {
    enabled: true,

    timeline: {
      enabled: true,
    },
  },
  runtimeConfig: {
    public: {
      base_url: "http://localhost:3000/",
      api_url: "https://localhost:7044/",
      discord_url: ""
    },
  },
  vite: {
    vue: {
      template: {
        transformAssetUrls,
      },
    },
    plugins: [vidstack()],
  },
  pages: true,
  modules: [
    (_options, nuxt) => {
      nuxt.hooks.hook('vite:extendConfig', (config) => {
        // @ts-expect-error
        config.plugins.push(vuetify({ autoImport: true }))
      })
    },
  ],
  build: {
    transpile: ['vuetify'],
  },
  typescript: {
    typeCheck: true
  },
  // needed for vidstack
  vue: {
    compilerOptions: {
      isCustomElement: (tag) => tag.startsWith('media-'),
    },
  },
});