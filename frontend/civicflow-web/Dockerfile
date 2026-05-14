# syntax=docker/dockerfile:1.7
FROM node:20-alpine AS build
WORKDIR /src
COPY frontend/civicflow-web/package.json frontend/civicflow-web/package-lock.json* ./
RUN npm install --no-audit --no-fund --prefer-offline
COPY frontend/civicflow-web/ ./
RUN npx ng build --output-path=/dist

FROM nginx:1.27-alpine AS runtime
COPY --from=build /dist/browser /usr/share/nginx/html
COPY frontend/civicflow-web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
HEALTHCHECK --interval=30s --timeout=5s --retries=5 \
    CMD wget --quiet --tries=1 --spider http://127.0.0.1:80/ || exit 1
