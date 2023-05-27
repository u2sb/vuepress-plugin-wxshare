export const init = (url, page) => {
    fetch("/api/wx/signature", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({
            url: location.href.split("#")[0],
        }),
    })
        .then((res) => res.json())
        .then((res) => {
            if (res["code"] === 0) {
                const data = res["data"];
                import("./jweixin-1.6.0.esm.js").then((w) => {
                    const wx = w.default;
                    wx.config({
                        debug: false,
                        appId: data.appId,
                        timestamp: data.timestamp,
                        nonceStr: data.nonceStr,
                        signature: data.signature,
                        jsApiList: [
                            "updateAppMessageShareData",
                            "updateTimelineShareData",
                        ],
                    });
                    wx.ready(function () {
                        wx.updateAppMessageShareData({
                            title: page.title || "",
                            desc: page.desc || "",
                            link: url,
                            imgUrl: page.imgUrl || "",
                        });
                        wx.updateTimelineShareData({
                            title: page.title || "",
                            desc: page.desc || "",
                            link: url,
                            imgUrl: page.imgUrl || "",
                        });
                    });
                });
            }
        });
}