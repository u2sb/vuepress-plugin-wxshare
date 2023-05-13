import { defineComponent, h, onMounted, ref, watch } from "vue";
import {
  usePageFrontmatter,
  usePageHeadTitle,
  usePageData,
  useSiteData,
} from "@vuepress/client";
import { WxSharePluginOptions } from "../options.js";
import { checkIsMobile, checkIsWeChat } from "../utils/utils.js";

import type { VNode } from "vue";

import "../style/wxShare.scss";

// @ts-ignore
const wspo = WX_SHARE_PLUGIN_OPTIONS as WxSharePluginOptions;

export default defineComponent({
  setup() {
    const pageData = usePageData();
    const siteData = useSiteData();
    const frontmatter = usePageFrontmatter<{
      title?: string;
      wxdescription?: string;
      description?: string;
    }>();
    const pageHeadTitle = usePageHeadTitle();

    const needIcon = ref(false);
    const updateMobile = (): void => {
      needIcon.value =
        !checkIsMobile(navigator.userAgent) ||
        (checkIsWeChat(navigator.userAgent) && !wspo.directConnection);
    };

    const url = ref("");
    const title = ref("");
    const desc = ref("");
    const imgUrl = wspo.imgUrl;

    const setData = () => {
      url.value = wspo.host + pageData.value.path;
      title.value =
        frontmatter.value.title || pageHeadTitle.value || siteData.value.title;
      desc.value =
        frontmatter.value.wxdescription ||
        frontmatter.value.description?.substring(0, 60) ||
        wspo.desc ||
        siteData.value.description;
    };

    const clickedWxShareButton = () => {
      if (wspo.server) {
        let page = {
          Title: title.value,
          Url: location.href,
          Desc: desc.value,
          ImgUrl: imgUrl,
        };
        fetch(wspo.server + "/api/wx/page/add", {
          method: "POST",
          headers: {
            "Content-Type": "application/json;charset=UTF-8",
          },
          body: JSON.stringify(page),
        })
          .then((res) => res.json())
          .then((res) => {
            if (res["code"] === 0) {
              let id = res["data"];
              location.href = wspo.server + "/api/wx/share/" + id;
            }
          });
      }
    };

    const shareWx = () => {
      if (wspo.directConnection === true) {
        if (/MicroMessenger/i.test(navigator.userAgent.toLowerCase())) {
          fetch(wspo.server + "/api/wx/signature", {
            method: "POST",
            headers: {
              "Content-Type": "application/json;charset=UTF-8",
            },
            body: JSON.stringify({
              url: location.href.split("#")[0],
            }),
          })
            .then((res) => res.json())
            .then((res) => {
              if (res["code"] === 0) {
                const data = res["data"];
                const config = {
                  debug: false,
                  appId: data.appId,
                  timestamp: data.timestamp,
                  nonceStr: data.nonceStr,
                  signature: data.signature,
                  jsApiList: [
                    "updateAppMessageShareData",
                    "updateTimelineShareData",
                  ],
                };
                //@ts-ignore
                import("wechat-jssdk").then(({ default: WechatJSSDK }) => {
                  const wechatObj = new WechatJSSDK(config);
                  wechatObj.initialize().then((w: any) => {
                    w.callWechatApi("updateAppMessageShareData", {
                      title: title.value,
                      desc: desc.value,
                      link: url.value,
                      imgUrl: imgUrl,
                    });
                    w.callWechatApi("updateTimelineShareData", {
                      title: title.value,
                      desc: desc.value,
                      link: url.value,
                      imgUrl: imgUrl,
                    });
                  });
                });
              }
            });
        }
      }
    };

    watch(
      () => pageData.value.path,
      async () => {
        setData();
        shareWx();
      }
    );

    onMounted(() => {
      updateMobile();
      setData();
      shareWx();
    });

    return (): VNode =>
      h(
        "div",
        {
          class: "wxshare",
        },
        needIcon.value
          ? h("button", {
              class: "wxshare-button",
              onClick: clickedWxShareButton,
            })
          : h("div")
      );
  },
});
