import { defineComponent, h, onMounted, ref, watch } from "vue";
import { usePageData, useSiteData } from "@vuepress/client";
import type { WxSharePluginOptions } from "../options.js";
import { checkIsMobile, checkIsWeChat } from "../utils/utils.js";
import { share as wxShare } from "../utils/wx.js";

import type { VNode } from "vue";

import "../style/wxShare.scss";

// @ts-ignore
const wspo = WX_SHARE_PLUGIN_OPTIONS as WxSharePluginOptions;

export default defineComponent({
  setup() {
    const pageData = usePageData();
    const siteData = useSiteData();
    const frontmatter = pageData.value.frontmatter;

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
        frontmatter.title || pageData.value.title || siteData.value.title;
      desc.value =
        (frontmatter.wxdescription as string) ||
        frontmatter.description?.substring(0, 60) ||
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
        if (checkIsWeChat(navigator.userAgent)) {
          wxShare(wspo, {
            title: title.value,
            desc: desc.value,
            url: url.value,
            imgUrl: imgUrl || "",
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
