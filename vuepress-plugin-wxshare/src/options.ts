export interface WxSharePluginOptions {
  host?: string;
  server?: string;
  imgUrl?: string;
  desc?: string;
  signatureApi?: string;
  directConnection?: boolean;
}

export const WxSharePluginOptionsDefault: WxSharePluginOptions = {
  directConnection: false,
};
