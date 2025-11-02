ARG NODE_VERSION=22-alpine
ARG NGINX_VERSION=alpine3.22

# Stage 1: build
FROM node:${NODE_VERSION} AS builder

WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci
RUN npm install -g @angular/cli

COPY . .

RUN npm run build --configuration=production

# Stage 2: serve
FROM nginxinc/nginx-unprivileged:${NGINX_VERSION} AS runner

USER nginx

COPY nginx.conf /etc/nginx/conf.d/default.conf

COPY --from=builder /app/dist/cargo-web/browser /usr/share/nginx/html

EXPOSE 8081

CMD ["nginx", "-g", "daemon off;"]
