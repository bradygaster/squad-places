# Multi-stage build for knock-knock Squad SDK sample

FROM node:20-alpine AS builder

WORKDIR /build

# Copy package files
COPY packages/squad-sdk/package.json packages/squad-sdk/
COPY samples/knock-knock/package.json samples/knock-knock/

# Copy SDK source and built dist
COPY packages/squad-sdk/dist packages/squad-sdk/dist
COPY packages/squad-sdk/node_modules packages/squad-sdk/node_modules

# Copy the sample source
COPY samples/knock-knock/tsconfig.json samples/knock-knock/
COPY samples/knock-knock/index.ts samples/knock-knock/

# Install sample dependencies (resolves to local SDK)
WORKDIR /build/samples/knock-knock
RUN npm install

# Production stage
FROM node:20-alpine

WORKDIR /app

# Copy everything needed
COPY --from=builder /build/samples/knock-knock /app
COPY --from=builder /build/packages/squad-sdk /packages/squad-sdk

CMD ["npx", "tsx", "index.ts"]
