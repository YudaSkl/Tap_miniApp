# Runbook: локально и деплой

## Требования

- Node.js 20+
- Docker (для PostgreSQL локально)
- Unity 2022.3 LTS (или новее с WebGL)

## 1. PostgreSQL локально

Из корня репозитория:

```bash
docker compose up -d
```

Применить миграции:

```bash
docker compose exec -T postgres psql -U tapmini -d tapmini -f - < backend/src/db/migrate.sql
```

Windows (PowerShell), из корня репозитория:

```powershell
Get-Content backend\src\db\migrate.sql -Raw | docker compose exec -T postgres psql -U tapmini -d tapmini
```

Или с хоста (если порт 5432 открыт):

```bash
psql "postgres://tapmini:tapmini@localhost:5432/tapmini" -f backend/src/db/migrate.sql
```

## 2. Backend

Создайте `backend/.env` (не коммитить):

```env
BOT_TOKEN=PLACEHOLDER_BOT_TOKEN
DATABASE_URL=postgres://tapmini:tapmini@localhost:5432/tapmini
WEBHOOK_SECRET=PLACEHOLDER_RANDOM_SECRET
PORT=3000
ALLOWED_ORIGINS=*
```

Установка и запуск:

```bash
cd backend
npm install
npm run dev
```

Проверка:

```bash
curl http://localhost:3000/health
curl "http://localhost:3000/api/leaderboard?limit=5"
```

## 3. Telegram и туннель

Без HTTPS Telegram Mini App не откроет вашу страницу в прод-режиме; для разработки используйте туннель (ngrok, cloudflared и т.д.):

```bash
cloudflared tunnel --url http://localhost:3000
```

- URL API пропишите в `window.TAP_API_BASE` или в `web/bridge.js` вместо `PLACEHOLDER_API_BASE`.
- Для статики игры — второй туннель на простой HTTP-сервер с билдом WebGL, либо хостите билд на том же домене (прокси).

**Webhook бота** (после того как API доступен по HTTPS):

```text
https://<ваш-api-хост>/telegram/webhook?secret=<WEBHOOK_SECRET>
```

Установка через `https://api.telegram.org/bot<BOT_TOKEN>/setWebhook?url=...` (URL-кодируйте query).

## 4. Unity WebGL

1. Откройте папку `unity` в Unity Hub.
2. **File → Build Settings → WebGL → Player Settings:** шаблон **TapMiniApp** (`Assets/WebGLTemplates/TapMiniApp`).
3. Соберите WebGL; скопируйте в выходную папку билда файлы из репозитория: `web/bridge.js`, `web/style.css` (рядом с `index.html`, как в шаблоне).
4. В `bridge.js` укажите реальный API base или задайте в шаблоне до загрузки bridge:

```html
<script>window.TAP_API_BASE = 'https://your-api.example.com';</script>
<script src="bridge.js"></script>
```

5. В BotFather укажите HTTPS URL на `index.html` мини-приложения.

## 5. Yandex Cloud

См. [infra/deploy-notes.md](../infra/deploy-notes.md). Кратко: бакет + статический сайт для билда, Cloud Function для `backend`, Managed PostgreSQL, переменные окружения, webhook.

### Сборка артефакта API

```bash
cd backend
npm install
npm run build
```

Точка входа для функции: собранный файл **`dist/handler.js`**, экспортируемый обработчик **`handler`** (ESM: `export const handler`). Если среда выполнения ожидает CommonJS, настройте сборку под `module: CommonJS` и `exports.handler` или обёртку по документации Yandex Cloud Functions.

Переменные окружения функции: `BOT_TOKEN`, `DATABASE_URL`, `WEBHOOK_SECRET`, опционально `ALLOWED_ORIGINS`, `PORT` (локально).

## Docker Compose

Файл в корне: `docker-compose.yml` — только сервис `postgres` для разработки.
