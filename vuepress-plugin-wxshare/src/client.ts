import { defineClientConfig } from "@vuepress/client";
import WxShare from "./components/WxShare.js";

export default defineClientConfig({
  rootComponents: [WxShare],
});
