mergeInto(LibraryManager.library, {
  Tg_RequestUser: function () {
    if (window.tgBridge) window.tgBridge.requestUser();
  },
  Tg_PostProgress: function (taps, coins) {
    if (window.tgBridge) window.tgBridge.postProgress(taps, coins);
  },
  Tg_OpenDonate: function (stars) {
    if (window.tgBridge) window.tgBridge.openDonate(stars);
  }
});
