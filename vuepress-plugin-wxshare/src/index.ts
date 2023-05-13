import { getDirname, path } from "@vuepress/utils";
import { WxSharePluginOptionsDefault } from "./options.js";

import type { Plugin } from "@vuepress/core";
import type { WxSharePluginOptions } from "./options.js";

const __dirname = getDirname(import.meta.url);

const Plugins = (
  options: WxSharePluginOptions = WxSharePluginOptionsDefault
): Plugin => {
  return {
    name: "vuepress-plugin-wxshare",
    define: {
      WX_SHARE_PLUGIN_OPTIONS: options,
    },
    clientConfigFile: path.resolve(__dirname, "client.js"),
    extendsBundlerOptions: (bundlerOptions, app) => {
      // 修改 @vuepress/bundler-vite 的配置项
      if (app.options.bundler.name === "@vuepress/bundler-vite") {
        bundlerOptions.viteOptions ??= {};
        bundlerOptions.viteOptions.optimizeDeps ??= {};
        bundlerOptions.viteOptions.optimizeDeps.include ??= [];
        bundlerOptions.viteOptions.optimizeDeps.include = [
          ...bundlerOptions.viteOptions.optimizeDeps.include,
          "wechat-jssdk",
        ];
      }
    },
  };
};

export default Plugins;
