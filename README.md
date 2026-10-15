# EzyHouse

A shared-house money app: rent, bills, groceries and furniture are split between housemates, and
everyone settles up from one balance per person. A React app in front of an ASP.NET Core Web API
(.NET 10) and MySQL 8.

| Folder | What it is | Details |
|---|---|---|
| `Frontend/` | React 19 + TypeScript app, built with Vite | [Frontend/README.md](Frontend/README.md) |
| `Backend/` | the Web API, EF Core (Code First) and NUnit tests | [Backend/README.md](Backend/README.md), [backend.md](Backend/backend.md) |
| `API.md` | the contract between the two | |
| `docker-compose.yml` | runs the database, the API and the app together | below |

## Local deployment playbook

Everything runs in Docker, so the only thing to install is Docker Desktop. No .NET SDK or Node.

### 1. Install the prerequisites

- **Docker Desktop**, started. `docker compose version` should print v2 or later.
- **Git.**
- Free ports **8080** (the app), **5080** (the API) and **3306** (MySQL). Busy? See step 4.

### 2. Get the code

```bash
git clone https://github.com/wanchaiS/ezyhouse.git
cd ezyhouse
```

### 3. Clear out an older setup (only if you ran EzyHouse before 16 Oct)

MySQL used to start from `Backend/docker-compose.yml`, and the database was redesigned since. If
that older container exists, remove it once, with its database:

```bash
docker rm -f backend-mysql-1
docker volume rm backend_ezyhouse-mysql
```

Both commands only complain if there's nothing to remove.

### 4. Start everything

```bash
docker compose up -d --build --wait
```

The first run downloads the base images and builds the API and the app: a few minutes. `--wait`
returns once every service is up. Then check:

```bash
docker compose ps        # db and api "healthy", web "running"
```

If a port is taken, choose others for this run without editing any file:

```bash
WEB_PORT=9000 API_PORT=5081 DB_PORT=3307 docker compose up -d --build --wait
```

### 5. Open the app and log in

| | |
|---|---|
| App | <http://localhost:8080> |
| API (Swagger) | <http://localhost:5080/swagger> |
| MySQL | `localhost:3306`, database `ezyhouse`, user `ezyhouse` / `admin123` |

On first start the API creates the tables and adds a demo household:

| Email | Password | Role in "Demo House" |
|---|---|---|
| `alice@example.com` | `Password1!` | primary |
| `bob@example.com` | `Password1!` | housemate |
| `cara@example.com` | `Password1!` | housemate |

To try it as a new household instead, create an account on the login screen, then create a
household and share its invite code. Logging in as two people at once needs two browser profiles
(or one normal and one private window), because each browser keeps one login cookie.

### 6. Run it day to day

All from the repo root.

| To | Run |
|---|---|
| stop (keeps the data) | `docker compose down` |
| start again | `docker compose up -d --wait` |
| pick up new code after `git pull` | `docker compose up -d --build --wait` |
| follow the API's log | `docker compose logs -f api` |
| open a MySQL prompt | `docker compose exec db mysql -uezyhouse -padmin123 ezyhouse` |
| start again with fresh demo data | `docker compose down -v`, then `docker compose up -d --wait` |
| start with an empty database | `docker compose down -v`, then `DEMO_DATA=false docker compose up -d --wait` |

`-v` deletes the database. Demo data is only added to an empty database at start-up, so for an
empty start keep `DEMO_DATA=false` on each start until you've created an account.

Rent and bills are created by the API in the background when they fall due (at start-up, then
hourly), in Sydney time.

### 7. When something goes wrong

| Symptom | Fix |
|---|---|
| `Cannot connect to the Docker daemon` | Start Docker Desktop and wait until it says it's running. |
| `port is already allocated` | Something else uses 8080, 5080 or 3306: often an older `backend-mysql-1` (step 3) or a local MySQL. Stop it, or choose other ports (step 4). |
| `up --wait` fails, or `docker compose ps` shows `api` exited | Read why with `docker compose logs api`. If it's about the database, start it fresh: `docker compose down -v`, then start again. |
| `Access denied for user 'ezyhouse'` | The database volume was made with other credentials: `docker compose down -v`, then start again. |
| A change in the code doesn't show | Rebuild: `docker compose up -d --build --wait`. |

## Develop with hot reload

For working on the code, run MySQL in Docker and the API and the app yourself (needs the .NET 10
SDK and Node 22):

```bash
docker compose up -d --wait db                                          # MySQL only
dotnet run --project Backend/src/EzyHouse.Api --launch-profile http     # API on localhost:5219
cd Frontend && npm install && npm run dev                               # app on localhost:5173
```

Tests: `dotnet test Backend/EzyHouse.slnx`. More in [Backend/README.md](Backend/README.md) and
[Frontend/README.md](Frontend/README.md).

The passwords and the JWT signing key in this repo are for local demos only.
