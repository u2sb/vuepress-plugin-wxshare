# vuepress-plugin-wxshare

## 说明

一个用于在微信中分享 vuepress 页面的插件。

本项目分为前端和后端。

## 前端

前端项目仅适用于 vuepress 2.x

### 安装

```bash
pnpm add vuepress-plugin-wxshare
```

### 使用

```js
import wxshare from "vuepress-plugin-wxshare";

export default {
  plugins: [
    wxshare({
      host: "https://www.u2sb.com",
      server: "",
      imgUrl: "https://www.u2sb.com/assets/img/avatar.jpg",
      desc: "帅比网",
      directConnection: false,
    }),
  ],
};
```

## 后端

以 Linux 为例，下载并解压后端

修改配置文件，详见源码

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "UnixSocket": "./wx.share.sock",
  "Port": "13567",

  // pid文件 配合systemd使用 删掉这行注释
  "PidFile": "/run/wx.share.pid",

  // 下面是跨域配置 删掉这行注释
  "WithOrigins": [
    "blog.xxwhite.com",
    "*.u2sb.com",
    "*.xxwhite.com",
    "*.sm9.top"
  ],

  // 下面是域名白名单 支持正则表达式 删掉这行注释
  "WhiteListDomains": [
    "172.16.5.18",
    "^*xxwhite.com$",
    "^*u2sb.com$",
    "^*sm9.top$"
  ],

  // 必填 删掉这行注释
  "WxSdk": {
    "AppId": "",
    "AppSecret": ""
  }
}
```

测试运行

```sh
chmod +x ./Wx.Share
./Wx.Share
```

配置 systemd

```ini
[Unit]
Description=WxShare
After=network.target remote-fs.target nss-lookup.target

[Service]
Type=forking

User=your_username
Group=your_groupname

PIDFile=/path/to/wx.share.pid
WorkingDirectory=/path/to/directory
ExecStartPre=/usr/bin/rm -f /path/to/wx.share.pid
ExecStart=/path/to/Wx.Share
ExecStartPost=/usr/bin/sleep 0.5
KillSignal=SIGTERM

Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
```

测试启动并添加到开机启动

配置 NGINX

```nginx
server {
    listen 443 http3;
    listen 443 http2;
    server_name your_domain;

    ssl_early_data on;
    proxy_set_header Early-Data $ssl_early_data;
    ssl_protocols TLSv1.2 TLSv1.3;
    add_header Alt-Svc 'h3=":443"; ma=86400; h3-29=":443"; h3-28=":443";';
    ssl_certificate /home/mc/.acme.sh/s.cer;
    ssl_certificate_key /home/mc/.acme.sh/s.key;

    index index.html index.htm;

    location / {
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_pass "http://unix:/path/to/wx.share.sock";

        brotli_types text/plain application/xml application/json application/octet-stream;
        gzip_types text/plain application/xml application/json application/octet-stream;
    }
}
```
