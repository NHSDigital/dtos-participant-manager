FROM node:22-alpine AS base

WORKDIR /app

ENV NODE_ENV=development

# Install dependencies
COPY package.json package-lock.json* ./

RUN \
  if [ -f package-lock.json ]; then npm ci; \
  else echo "Error: package-lock.json not found. Please run npm install locally first." && exit 1; \
  fi

COPY . .

# Start Next.js in development mode
CMD npm run dev
