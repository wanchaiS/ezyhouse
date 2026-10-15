# EzyHouse frontend

React 19 + TypeScript single-page app, built with Vite. It talks to the API in [`../Backend`](../Backend/README.md).

## Run it

Start the backend first (it listens on `http://localhost:5219`), then from this `Frontend/` folder:

```bash
npm install
npm run dev        # http://localhost:5173
```

Vite forwards every `/api` request to the backend, so the app and the API share an origin and the
login cookie just works. To point at another backend or use another port:

```bash
API_TARGET=http://localhost:5300 npm run dev -- --port 5311
```

Or run the whole app (database, API and this app) in Docker from the repo root, with no Node or
.NET installed: `docker compose up -d --build --wait`, then open <http://localhost:8080>. See
[the backend README](../Backend/README.md#run-the-whole-app-in-docker).

## Scripts

| | |
|---|---|
| `npm run dev` | dev server with hot reload |
| `npm run build` | type-check, then build into `dist/` |
| `npm run lint` | oxlint |

## Where things live

- `src/App.tsx`: routes and the sign-in / household guards.
- `src/features/<slice>/`: one folder per screen. Slices never import each other.
- `src/shared/`: only what two or more slices use: `api/` (Axios client, query keys, types, shared
  calls), `components/` (each with its own `.module.css`), `format/` (money and dates),
  `styles/tokens.css` (the design tokens).
- `Dockerfile`, `nginx.conf`: the production build, served by nginx, which forwards `/api` to the
  API container on the same origin (the login cookie needs that, as the Vite proxy does in
  development).
