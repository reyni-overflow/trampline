FROM node:24-alpine AS build

WORKDIR /app

RUN corepack enable && corepack prepare pnpm@latest --activate

COPY package.json pnpm-lock.yaml .npmrc ./
RUN pnpm install --frozen-lockfile 2>/dev/null || pnpm install

COPY . .

ARG PUBLIC_API_URL=/api
ARG PUBLIC_CONTACT_EMAIL=support@trampline.org
ARG PUBLIC_LINK_TELEGRAM=https://t.me/trampline
ARG PUBLIC_LINK_VK=https://vk.com/trampline
ARG PUBLIC_LINK_DZEN=https://dzen.ru/trampline
ARG PUBLIC_LINK_MAX=https://max.ru/trampline
ARG PUBLIC_BOT_TELEGRAM=https://t.me/trampline_bot
ARG PUBLIC_BOT_VK=https://vk.com/trampline_bot
ARG PUBLIC_BOT_MAX=https://max.ru/trampline_bot
ARG PUBLIC_MOBILE_APP_LINK=https://rutube.ru/video/c6cc4d620b1d4338901770a44b3e82f4/

ENV PUBLIC_API_URL=$PUBLIC_API_URL \
    PUBLIC_CONTACT_EMAIL=$PUBLIC_CONTACT_EMAIL \
    PUBLIC_LINK_TELEGRAM=$PUBLIC_LINK_TELEGRAM \
    PUBLIC_LINK_VK=$PUBLIC_LINK_VK \
    PUBLIC_LINK_DZEN=$PUBLIC_LINK_DZEN \
    PUBLIC_LINK_MAX=$PUBLIC_LINK_MAX \
    PUBLIC_BOT_TELEGRAM=$PUBLIC_BOT_TELEGRAM \
    PUBLIC_BOT_VK=$PUBLIC_BOT_VK \
    PUBLIC_BOT_MAX=$PUBLIC_BOT_MAX \
    PUBLIC_MOBILE_APP_LINK=$PUBLIC_MOBILE_APP_LINK

RUN pnpm exec svelte-kit sync && pnpm build

FROM nginxinc/nginx-unprivileged:1.29-alpine AS production

COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget -qO- http://localhost:8080/ || exit 1
