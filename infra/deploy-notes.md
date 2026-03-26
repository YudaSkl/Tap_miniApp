# Yandex Cloud — кратко

1. **Object Storage:** бакет, статический сайт, индекс `index.html`. Залить содержимое WebGL-билда + `bridge.js` + `style.css`. CORS — см. `object-storage-cors.json` (при необходимости добавьте origin вашего CDN).
2. **HTTPS:** поддомен + Certificate Manager + CDN перед бакетом (или свой вариант с валидным сертификатом для Mini App).
3. **Cloud Function:** Node.js 20, артефакт из `backend/` (`npm run build`, задеплоить `dist/` + `node_modules` или zip по документации Yandex). Точка входа: обработчик из собранного `index.js` (см. `backend`).
4. **Managed PostgreSQL:** создать кластер, применить `backend/src/db/migrate.sql`, передать `DATABASE_URL` в переменные функции.
5. **Переменные окружения функции:** `BOT_TOKEN`, `DATABASE_URL`, `WEBHOOK_SECRET`, опционально `ALLOWED_ORIGINS`.
6. **Webhook бота:** `https://<function-url>/telegram/webhook?secret=<WEBHOOK_SECRET>`.

CLI: `yc` для загрузки в бакет и создания функции — см. официальные гайды Yandex Cloud.

## Чеклист перед выкладкой

1. Собран WebGL с шаблоном `TapMiniApp`, в корень билда скопирован `web/bridge.js`, в `bridge.js` или `index.html` задан реальный `TAP_API_BASE`.
2. В консоли функции заданы `BOT_TOKEN`, `DATABASE_URL`, `WEBHOOK_SECRET`, при необходимости `ALLOWED_ORIGINS`.
3. На бакете/ CDN включён HTTPS; URL добавлен в BotFather как Mini App.
4. Выполнен `setWebhook` на `https://<host>/telegram/webhook?secret=<WEBHOOK_SECRET>`.
5. Прогнаны руками: открытие из Telegram, тапы, «Sync», «Топ», тестовый платёж Stars и строка в `payments`.
