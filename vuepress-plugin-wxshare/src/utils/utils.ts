export const checkIsMobile = function (ua: string) {
  return /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(
    ua
  );
};

export const checkIsWeChat = function (ua: string) {
  return /MicroMessenger/i.test(ua);
};
