# Unity — Tap Mini App

## Версия редактора

Рекомендуется **Unity 2022.3 LTS** (см. `ProjectSettings/ProjectVersion.txt`). При открытии в другой версии Unity может пересобрать настройки — это нормально.

## Сцена

`Assets/Scenes/Main.unity`: **Main Camera**, **EventSystem** и **App** с **GameBootstrap**. UI (Canvas, кнопки, тексты) не создаётся в рантайме — после клонирования или смены сцены выполните **Tools → Tap miniApp → Compose Main Scene** (или **Compose Scene** для активной сцены).

## WebGL

1. **Edit → Project Settings → Player → WebGL → Resolution and Presentation** — шаблон **TapMiniApp** (`Assets/WebGLTemplates/TapMiniApp`).
2. Соберите билд.
3. Скопируйте в **корень** билда из репозитория: `web/bridge.js` (рядом с `index.html`). Стили уже подключаются из шаблона (`style.css` в шаблоне копируется в билд автоматически).
4. Замените `PLACEHOLDER_API_BASE` в `bridge.js` или задайте `window.TAP_API_BASE` в `index.html` до загрузки `bridge.js`.
5. В инспекторе **App → GameBootstrap** укажите тот же API URL в поле **Api Base** (для кнопки «Топ» / лидерборда).

## Плагин

`Assets/Plugins/WebGL/tg-bridge.jslib` — только платформа WebGL.
