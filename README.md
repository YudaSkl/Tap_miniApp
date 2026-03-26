# Tap Mini App (Unity WebGL + Telegram + Yandex Cloud)

MVP: tap-игра в Unity WebGL, оболочка Telegram Mini App, API на Node.js (Fastify), PostgreSQL, донат через Telegram Stars.

## Структура

| Путь | Назначение |
|------|------------|
| `unity/` | Проект Unity (WebGL) |
| `web/` | `bridge.js` и стили — копировать в корень WebGL-билда вместе с шаблоном |
| `backend/` | TypeScript API (локально или Yandex Cloud Function) |
| `infra/` | Примеры CORS и заметки по деплою |
| `docs/RUNBOOK.md` | Локальный запуск, Docker Postgres, деплой |

## PLACEHOLDER (заменить перед продом)

- `BOT_TOKEN` — токен бота от BotFather
- `DATABASE_URL` — строка подключения PostgreSQL (`postgres://user:pass@host:5432/db`)
- `WEBHOOK_SECRET` — случайная строка для `?secret=` на webhook
- `PLACEHOLDER_API_BASE` в `web/bridge.js` (или задайте `window.TAP_API_BASE` до загрузки bridge)
- `PLACEHOLDER_GAME_HOST` — HTTPS-домен статики (Object Storage / CDN)
- В шаблоне WebGL `unity/Assets/WebGLTemplates/TapMiniApp/index.html` — при необходимости поправить пути к `bridge.js`

## Быстрый старт

1. **База:** см. `docs/RUNBOOK.md` — Docker Compose, миграции.
2. **Backend:** `cd backend && npm install && npm run dev`
3. **Unity:** открыть `unity` в Unity Hub, сцена `Assets/Scenes/Main.unity`, билд WebGL с шаблоном `TapMiniApp`.
4. **Telegram:** Mini App URL на ваш HTTPS; webhook `https://<api>/telegram/webhook?secret=<WEBHOOK_SECRET>`.

Подробности — [docs/RUNBOOK.md](docs/RUNBOOK.md).
