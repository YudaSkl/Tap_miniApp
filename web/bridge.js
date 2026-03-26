/**
 * Связка Telegram WebApp ↔ Unity WebGL ↔ backend API.
 * Задайте window.TAP_API_BASE до загрузки скрипта или замените PLACEHOLDER_API_BASE.
 */
(function () {
  const DEFAULT_API_BASE = 'PLACEHOLDER_API_BASE';

  function apiBase() {
    return (typeof window.TAP_API_BASE === 'string' && window.TAP_API_BASE) || DEFAULT_API_BASE;
  }

  function getTg() {
    return window.Telegram && window.Telegram.WebApp;
  }

  window.tgBridge = {
    requestUser: function () {
      const tg = getTg();
      if (!tg || !tg.initDataUnsafe || !tg.initDataUnsafe.user) return;
      const u = tg.initDataUnsafe.user;
      const payload = JSON.stringify({
        id: u.id,
        username: u.username || '',
        first_name: u.first_name || ''
      });
      if (window.unityInstance) {
        window.unityInstance.SendMessage('GameBootstrap', 'OnTelegramUser', payload);
      }
    },

    postProgress: function (taps, coins) {
      const tg = getTg();
      if (!tg || !tg.initData) return;
      const base = apiBase();
      if (!base || base.indexOf('PLACEHOLDER') !== -1) return;
      fetch(base.replace(/\/$/, '') + '/api/progress', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ initData: tg.initData, taps: taps, coins: coins })
      }).catch(function () {});
    },

    openDonate: function (stars) {
      const tg = getTg();
      if (!tg || !tg.initData) return;
      const base = apiBase();
      if (!base || base.indexOf('PLACEHOLDER') !== -1) return;
      fetch(base.replace(/\/$/, '') + '/api/payments/invoice', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          initData: tg.initData,
          amountStars: stars,
          itemLabel: 'Support'
        })
      })
        .then(function (r) {
          return r.json();
        })
        .then(function (data) {
          if (data && data.invoiceUrl && tg.openInvoice) {
            tg.openInvoice(data.invoiceUrl, function () {});
          }
        })
        .catch(function () {});
    }
  };

  document.addEventListener('DOMContentLoaded', function () {
    const tg = getTg();
    if (tg) {
      tg.ready();
      tg.expand();
    }
  });
})();
