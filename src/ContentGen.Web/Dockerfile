# syntax=docker/dockerfile:1
# Build context is this folder: docker build -t contentgen-web src/ContentGen.Web

# --- Build stage ---
FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
# API base URL is baked into the bundle at build time (Vite). Override with
# --build-arg VITE_API_BASE_URL=... ; defaults to the local API port.
ARG VITE_API_BASE_URL=http://localhost:5234
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
RUN npm run build

# --- Serve stage ---
FROM nginx:alpine AS runtime
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
# Use 127.0.0.1 (not "localhost"): BusyBox wget resolves localhost to ::1 first,
# but nginx listens on IPv4 only, which would make the probe fail.
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget -q -O /dev/null http://127.0.0.1/ || exit 1
