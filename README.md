<p align="center">
  <img src="frontend/static/logo.svg" width="64" height="64" alt="Трамплин">
</p>

<h1 align="center">Трамплин</h1>

<p align="center">
  Карьерная платформа для студентов, выпускников и работодателей в сфере IT
</p>

<p align="center">
  <a href="https://github.com/reyni-overflow/trampline/blob/master/LICENSE"><img src="https://img.shields.io/badge/license-AGPL--3.0-blue" alt="License"></a>
  <img src="https://img.shields.io/badge/.NET-10.0-blueviolet" alt=".NET 10">
  <img src="https://img.shields.io/badge/Svelte-5-orange" alt="Svelte 5">
  <img src="https://img.shields.io/badge/PostgreSQL-17-336791" alt="PostgreSQL">
  <img src="https://img.shields.io/badge/Docker-ready-2496ED" alt="Docker">
</p>

---

## О проекте

**Трамплин** - интерактивная карьерная экосистема, где студенты и выпускники ищут стажировки, вакансии, менторские программы и карьерные мероприятия, а работодатели находят талантливых начинающих специалистов. Платформа объединяет соискателей, работодателей и кураторов вузов в едином пространстве с картой возможностей, системой откликов, нетворкингом и модерацией контента.

Проект разработан в рамках **Международной олимпиады [IT-Планета 2026](https://world-it-planet.org)**, конкурс **«Прикладное программирование if...else»**, второй этап.

### Ключевые возможности

- **Карта возможностей** - вакансии, стажировки и мероприятия на интерактивной карте Leaflet с кластеризацией и геокодингом через DaData
- **Три роли** - соискатель, работодатель, куратор платформы с разграничением доступа
- **Полный цикл откликов** - создание вакансий, отклики с сопроводительным письмом, управление статусами (на рассмотрении, просмотрен, приглашён, в резерве, принят, отклонён, отозван)
- **Верификация компаний** - автоматическая проверка ИНН через DaData API + ручное подтверждение куратором
- **Нетворкинг** - профессиональные контакты между соискателями, рекомендации на вакансии
- **Менторские программы** - отдельная сущность с подачей заявок и управлением участниками
- **Карьерные мероприятия** - хакатоны, митапы, воркшопы с календарным видом и откликами
- **Real-time уведомления** - SignalR WebSocket + Web Push (VAPID)
- **Полнотекстовый поиск** - PostgreSQL `tsvector`/`websearch_to_tsquery` с GIN-индексами
- **Двухфакторная аутентификация** - TOTP (RFC 6238) с QR-кодом
- **Локализация** - 4 языка (русский, английский, китайский, пиратский)
- **Темизация** - тёмная/светлая тема + 9 акцентных цветов
- **PWA** - манифест, service worker, offline-страница

---

## Технологический стек

### Frontend

| Технология | Версия | Назначение |
|---|---|---|
| [Svelte](https://svelte.dev) | 5.54 | UI-фреймворк (режим Runes) |
| [SvelteKit](https://svelte.dev/docs/kit) | 2.55 | Мета-фреймворк, маршрутизация |
| [TypeScript](https://www.typescriptlang.org) | 5.9 | Типизация |
| [Vite](https://vitejs.dev) | 8.0 | Сборщик |
| [Leaflet](https://leafletjs.com) | 1.9 | Интерактивные карты |
| [SignalR](https://learn.microsoft.com/aspnet/signalr) | 10.0 | Real-time WebSocket клиент |
| [DOMPurify](https://github.com/cure53/DOMPurify) | 3.3 | Санитизация HTML |

### Backend

| Технология | Версия | Назначение |
|---|---|---|
| [ASP.NET Core](https://dotnet.microsoft.com) | 10.0 | Web API |
| [Entity Framework Core](https://learn.microsoft.com/ef) | 10.0 | ORM |
| [PostgreSQL](https://www.postgresql.org) | 17 | Основная БД |
| [Valkey](https://valkey.io) | 9 | Кэш, сессии, blacklist токенов |
| [MinIO](https://min.io) | latest | S3-совместимое хранилище файлов |
| [FluentValidation](https://docs.fluentvalidation.net) | 12.1 | Валидация запросов |
| [DaData](https://dadata.ru) | 25.7 | Верификация ИНН, геокодинг |

### Инфраструктура

| Технология | Версия | Назначение |
|---|---|---|
| [Docker](https://www.docker.com) + Compose | - | Контейнеризация |
| [Traefik](https://traefik.io) | 3.6 | Reverse proxy, TLS, rate limiting |
| [Nginx](https://nginx.org) | 1.29 | Сервинг SPA (в контейнере frontend) |

---

## Архитектура

```
┌─────────────────────────────────────────────────────────┐
│                     Traefik (reverse proxy)             │
│        trampline.localhost → frontend                   │
│    api.trampline.localhost → backend                    │
│    api.trampline.localhost/files/* → MinIO              │
└────────────┬──────────────────────┬──────────────────────┘
             │                      │
    ┌────────▼────────┐   ┌────────▼────────────┐
    │    Frontend     │   │    Backend          │
    │   Svelte 5 SPA  │   │  ASP.NET 10 API     │
    │   nginx:1.29    │   │  142 endpoints      │
    └─────────────────┘   └────┬─────┬──────┬───┘
                               │     │      │
                   ┌───────────▼┐ ┌──▼───┐ ┌▼───────┐
                   │ PostgreSQL │ │Valkey│ │ MinIO  │
                   │    17      │ │  9   │ │  S3    │
                   └────────────┘ └──────┘ └────────┘
```

### Backend - Clean Architecture

```
Trampline.Web                     > Контроллеры, middleware, SignalR hub
Trampline.Application             > Бизнес-логика, сервисы, валидаторы
Trampline.Core                    > Доменные модели, интерфейсы репозиториев
Trampline.Contracts               > DTO (запросы/ответы)
Trampline.Shared                  > Result-монада, общие утилиты
Trampline.Infrastructure.Postgres > EF Core, реализация репозиториев
Trampline.Infrastructure.Redis    > Refresh-токены в Valkey/Redis
```

### Статистика кодовой базы

| Метрика | Количество |
|---|---|
| API-эндпоинтов | 142 |
| Svelte-компонентов | 44 |
| Страниц (routes) | 46 |
| Строк C# (backend/src) | ~6 700 |
| Строк Svelte + TS (frontend/src) | ~48 600 |
| Ключей локализации | ~960 × 4 языка |

---

## Быстрый старт

### Требования

- [Docker](https://docs.docker.com/get-docker/) и Docker Compose
- (опционально) [Node.js 24+](https://nodejs.org) и [pnpm](https://pnpm.io) - для локальной разработки frontend
- (опционально) [.NET 10 SDK](https://dotnet.microsoft.com/download) - для локальной разработки backend

### Запуск через Docker

```bash
# 1. Клонировать репозиторий
git clone https://github.com/reyni-overflow/trampline.git
cd trampline

# 2. Создать .env из примера
cp .env.example .env
# Отредактировать .env - задать POSTGRES_PASSWORD, REDIS_PASSWORD, JWT_KEY и т.д.

# 3. Запустить все сервисы
docker compose up -d

# 4. Проверить здоровье
curl http://localhost:7103/health
# → {"status":"healthy","timestamp":"...","checks":{"postgres":"healthy","redis":"healthy"}}
```

После запуска:

| Сервис | URL |
|---|---|
| Frontend | [https://trampline.localhost](https://trampline.localhost) или http://localhost:3000 |
| Backend API | [https://api.trampline.localhost](https://api.trampline.localhost) или http://localhost:7103 |
| API-документация (Scalar) | http://localhost:7103/scalar |
| Traefik Dashboard | [https://traefik.trampline.localhost](https://traefik.trampline.localhost) |
| MinIO Console | http://localhost:9001 |

> Для работы доменов `*.trampline.localhost` добавьте в `/etc/hosts` (Linux/Mac) или `C:\Windows\System32\drivers\etc\hosts` (Windows):
> ```
> 127.0.0.1 trampline.localhost api.trampline.localhost traefik.trampline.localhost
> ```

### Учётная запись администратора

При первом запуске создаётся суперадминистратор:

| Поле | Значение |
|---|---|
| Email | `admin@gmail.com` |
| Пароль | `Admin123!` |
| Роль | Admin (SuperAdmin) |

> После входа рекомендуется сменить пароль.

---

## Разработка

### Frontend

```bash
cd frontend
pnpm install
pnpm dev       # Запуск dev-сервера (http://localhost:5173)
pnpm build     # Сборка для продакшена
pnpm check     # Проверка типов (svelte-check)
pnpm lint      # ESLint + Prettier
pnpm test      # Unit-тесты (Vitest)
pnpm test:e2e  # E2E-тесты (Playwright)
```

### Backend

```bash
cd backend
dotnet build                            # Сборка всех проектов
dotnet test                             # Запуск тестов
dotnet run --project src/Trampline.Web  # Запуск API
dotnet format                           # Автоформатирование
```

---

## Переменные окружения

<details>
<summary>Полный список переменных (.env.example)</summary>

### База данных

| Переменная | Описание | Пример |
|---|---|---|
| `POSTGRES_PASSWORD` | Пароль PostgreSQL | `your_secure_password` |
| `REDIS_PASSWORD` | Пароль Valkey/Redis | `your_redis_password` |

### Аутентификация

| Переменная | Описание | Пример |
|---|---|---|
| `JWT_KEY` | Секрет JWT (минимум 32 символа) | `your-secret-key-at-least-32-chars-long` |
| `JWT_ISSUER` | Издатель JWT | `trampline` |
| `JWT_AUDIENCE` | Аудитория JWT | `trampline-users` |

### Внешние сервисы

| Переменная | Описание | Пример |
|---|---|---|
| `DADATA_TOKEN` | Токен DaData API | `your_dadata_token` |
| `DADATA_SECRET` | Секрет DaData API | `your_dadata_secret` |
| `IPINFO_TOKEN` | Токен IPInfo.io | `your_ipinfo_token` |

### Email (SMTP)

| Переменная | Описание | Пример |
|---|---|---|
| `SMTP_HOST` | SMTP-сервер | `smtp.gmail.com` |
| `SMTP_PORT` | Порт | `587` |
| `SMTP_USERNAME` | Логин | `user@gmail.com` |
| `SMTP_PASSWORD` | Пароль / app password | `xxxx xxxx xxxx xxxx` |
| `SMTP_FROM_EMAIL` | Адрес отправителя | `noreply@trampline.ru` |

### Web Push (VAPID)

| Переменная | Описание |
|---|---|
| `VAPID_PUBLIC_KEY` | Публичный ключ VAPID |
| `VAPID_PRIVATE_KEY` | Приватный ключ VAPID |
| `VAPID_SUBJECT` | Контакт (mailto:) |

### Хранилище файлов

| Переменная | Описание | Пример |
|---|---|---|
| `STORAGE_PROVIDER` | Провайдер хранения | `local` или `minio` |
| `MINIO_ENDPOINT` | Адрес MinIO | `minio:9000` |
| `MINIO_ACCESS_KEY` | Ключ доступа | `minioadmin` |
| `MINIO_SECRET_KEY` | Секретный ключ | `minioadmin` |
| `MINIO_BUCKET` | Название бакета | `trampline` |

### Frontend (build-time)

| Переменная | Описание | Пример |
|---|---|---|
| `PUBLIC_API_URL` | URL бэкенда | `/api` |
| `PUBLIC_CONTACT_EMAIL` | Email поддержки | `support@trampline.org` |
| `PUBLIC_LINK_TELEGRAM` | Ссылка на Telegram | `https://t.me/trampline` |
| `PUBLIC_LINK_VK` | Ссылка на VK | `https://vk.com/trampline` |
| `PUBLIC_LINK_DZEN` | Ссылка на Дзен | `https://dzen.ru/trampline` |
| `PUBLIC_LINK_MAX` | Ссылка на Max | `https://max.ru/trampline` |

</details>

---

## API

Backend предоставляет RESTful API с 142 эндпоинтами, организованными по 13 контроллерам:

| Контроллер | Префикс | Описание |
|---|---|---|
| `AuthController` | `/auth` | Регистрация, вход, JWT, сессии, TOTP, пароли, приватность |
| `JobController` | `/job` | CRUD вакансий, отклики, теги, медиа |
| `EventController` | `/event` | CRUD мероприятий, отклики, медиа |
| `MentorshipController` | `/mentorship` | CRUD менторских программ, отклики, медиа |
| `EmployeeController` | `/employee` | Профили компаний, верификация, медиа |
| `WorkerController` | `/worker` | Профили соискателей, резюме, отклики |
| `AdminController` | `/admin` | Управление пользователями, модерация, верификация, теги, кураторы |
| `ContactController` | `/contact` | Контакты, заявки, рекомендации |
| `FavoriteController` | `/favorite` | Избранное (вакансии, компании, мероприятия) |
| `NotificationController` | `/notification` | Уведомления, Web Push подписки |
| `ReviewController` | `/review` | Отзывы с модерацией |
| `MapController` | `/map` | Маркеры для карты |

Интерактивная документация доступна по адресу `/scalar` (Scalar UI) или `/swagger` (Swagger UI) после запуска backend.

---

## Безопасность

- **Аутентификация** - JWT в HttpOnly cookies, ротация refresh-токенов, blacklist через Redis
- **Хэширование паролей** - PBKDF2-SHA256, 100 000 итераций, 16-байтная соль
- **CSRF-защита** - обязательный заголовок `X-Requested-With` + валидация Origin
- **Rate limiting** - 7 политик по типам эндпоинтов (auth: 10/мин, API: 60/мин, upload: 10/мин)
- **Загрузка файлов** - валидация magic bytes, MIME-типов, сканирование на вредоносное содержимое
- **Санитизация HTML** - DOMPurify (frontend) + HtmlSanitizer (backend)
- **Security headers** - HSTS, X-Frame-Options, CSP, Referrer-Policy через Traefik
- **2FA** - TOTP с шифрованием секретов (AES-CBC)
- **Аудит** - логирование всех действий администраторов

---

## Структура проекта

```
trampline/
├── backend/
│   ├── src/
│   │   ├── Trampline.Web/           # API, контроллеры, middleware, SignalR
│   │   ├── Trampline.Application/   # Сервисы, валидаторы
│   │   ├── Trampline.Core/          # Доменные модели, интерфейсы
│   │   ├── Trampline.Contracts/     # DTO
│   │   ├── Trampline.Shared/        # Result, утилиты
│   │   ├── Trampline.Infrastructure.Postgres/  # EF Core
│   │   └── Trampline.Infrastructure.Redis/     # Redis/Valkey
│   └── tests/
│       └── Trampline.Tests/         # xUnit тесты
├── frontend/
│   ├── src/
│   │   ├── lib/
│   │   │   ├── api/                 # HTTP-клиент, API-модули
│   │   │   ├── components/          # 44 UI-компонента
│   │   │   ├── i18n/                # Локализация (4 языка)
│   │   │   ├── stores/              # Svelte stores (auth, theme, toast, favorites)
│   │   │   └── utils/               # Утилиты (форматирование, валидация)
│   │   ├── routes/                  # 46 страниц SvelteKit
│   │   └── styles/                  # CSS (темы, акценты, типографика, анимации)
│   └── static/                      # Шрифты, иконки, PWA-ресурсы
├── traefik/
│   ├── dynamic.yml                  # Маршрутизация, middleware, TLS
│   └── certs/generate.sh            # Генерация self-signed сертификатов
├── docker-compose.yml
├── .env.example
└── LICENSE                          # AGPL-3.0
```

---

## Лицензия

Этот проект распространяется под лицензией [GNU Affero General Public License v3.0](LICENSE).

Copyright 2026 [reyni-overflow](https://github.com/reyni-overflow), [programistro](https://github.com/programistro)
