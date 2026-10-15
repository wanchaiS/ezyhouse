# EzyHouse backend

The REST API behind the EzyHouse React app: ASP.NET Core Web API on .NET 10, MySQL 8 through
Entity Framework Core (Code First), tested with NUnit. Every endpoint in [API.md](../API.md) is
built. The design, and the reasons behind it, are in [backend.md](backend.md).

## What you need

- **.NET 10 SDK.** `dotnet --version` should print `10.x`.
- **Docker Desktop**, which runs MySQL 8 for you. Already running your own MySQL 8 on port 3306?
  Create a database `ezyhouse` and a user `ezyhouse` / `admin123` instead, or change
  `ConnectionStrings:EzyHouse` in `src/EzyHouse.Api/appsettings.json`.
- An editor that supports .NET 10: Visual Studio 2026, Rider or VS Code, or just the command line.
  **Visual Studio 2022 can't build .NET 10 projects.**

## Run it

From this `Backend/` folder:

```bash
docker compose up -d --wait                                   # MySQL 8 on localhost:3306
dotnet run --project src/EzyHouse.Api --launch-profile http   # the API on localhost:5219
```

`--wait` returns once MySQL accepts connections. The first time takes up to a minute while the
image downloads and MySQL sets itself up.

On start-up in Development, the API creates or upgrades the tables (EF Core migrations) and, if the
database is empty, adds the demo data below. No `dotnet ef` command is needed.

In Visual Studio or Rider: open `EzyHouse.slnx`, set **EzyHouse.Api** as the start-up project and
choose the **http** profile. MySQL still needs `docker compose up -d --wait` first.

### Try it in Swagger

Open **<http://localhost:5219/swagger>**.

1. `POST /api/auth/login` with `{ "email": "alice@example.com", "password": "Password1!" }`, then
   copy the `token` from the response.
2. Click **Authorize** and paste the token on its own, without "Bearer".
3. Try `GET /api/household`, `GET /api/balances` and `GET /api/rent-periods`.

Without a token every endpoint except register, login and logout returns 401. Login and register
also set an HttpOnly cookie, `ezyhouse_token`, which the API accepts instead of the header;
`POST /api/auth/logout` clears it.

### Demo data

| | |
|---|---|
| Users | **Alice** `alice@example.com` (primary), **Bob** `bob@example.com`, **Cara** `cara@example.com` |
| Password | `Password1!` for all three |
| Household | "Demo House", invite code `DEMH7KQ2` |
| Records | 3 grocery expenses and a settlement; weekly rent from 6 weeks ago; Electricity (quarterly) and Internet (monthly) bills; a couch bought for $600 and sold for $300, plus a TV; a $2,400 bond with one contribution |

Dates are relative to the day the database was seeded, so rent periods and bills are always due.

## Run the React frontend

With the API running, start the app from `../Frontend` (see [its README](../Frontend/README.md)):

```bash
cd ../Frontend && npm install && npm run dev    # http://localhost:5173
```

Vite forwards `/api` to `http://localhost:5219`, so the app and the API share an origin. That
matters because the login cookie is `SameSite=Strict` and isn't sent on cross-origin calls, so the
app must always go through the proxy, never to `http://localhost:5219` directly.

## Run the tests

```bash
dotnet test EzyHouse.slnx
```

114 NUnit tests. They use an in-memory SQLite database, so MySQL doesn't need to be running. In
Visual Studio or Rider, use the Test Explorer.

## Start again with fresh demo data

```bash
docker compose down -v        # -v deletes the database volume
docker compose up -d --wait
```

Then run the API again: it recreates the tables and the demo data.

To start with **no** demo data instead, as a brand-new user would see it, run the API with the
demo data switched off. Demo data is only added at start-up, so later restarts must keep the switch
off too:

```bash
DemoData__Seed=false dotnet run --project src/EzyHouse.Api --launch-profile http
```

## Change the database schema

Edit an entity or its configuration in `src/EzyHouse.Infrastructure/Data/Configurations/`, then add
a migration. The next start-up applies it.

```bash
dotnet tool install --global dotnet-ef --version 9.*          # once
dotnet ef migrations add <Name> --project src/EzyHouse.Infrastructure --startup-project src/EzyHouse.Api
```

## When something goes wrong

| Symptom | Fix |
|---|---|
| `Unable to connect to any of the specified MySQL hosts` | Docker Desktop isn't running, or MySQL isn't up yet. Start Docker Desktop, then `docker compose up -d --wait`. |
| `docker compose up` says port 3306 is in use | Another MySQL is running. Stop it, or use it as described under "What you need". |
| `dotnet run` says port 5219 is in use | The API is already running in another terminal or in your editor. |
| `NETSDK1045: The current .NET SDK does not support targeting .NET 10.0` | Install the .NET 10 SDK. |
| `Access denied for user 'ezyhouse'` | The database volume was created with other credentials. Run `docker compose down -v`, then `docker compose up -d --wait`. |

The database password and the JWT signing key in `appsettings.json` are demo-only values.

## Where to find each marking-guide technique

Paths are relative to `src/` (or `tests/` for the tests). Each technique does real work in the app;
[backend.md](backend.md) section 5 explains why each design was chosen.

| Technique | Where | What it does |
|---|---|---|
| **Polymorphism** (overriding) | `EzyHouse.Domain/Scheduling/RecurringSchedule.cs`, `EzyHouse.Domain/Entities/RentSchedule.cs`, `BillType.cs` | `CreateOccurrence` is overridden: a rent schedule creates a rent period, a bill type creates a bill placeholder |
| | `EzyHouse.Domain/Entities/FurnitureTransaction.cs` | `GetLedgerEntries` is overridden: a purchase means the sharers owe the payer; a sale reverses it |
| | `EzyHouse.Domain/Exceptions/AppException.cs` | each exception overrides its HTTP status code |
| **Polymorphism** (through an interface) | `EzyHouse.Domain/Splitting/EqualSplitStrategy.cs`, `ExactSplitStrategy.cs` | two `ISplitStrategy` implementations, picked by `splitType` |
| **Polymorphism** (overloading) | `EzyHouse.Domain/ValueObjects/PaidShare.cs` | three constructors |
| **Interfaces** (at least two) | `EzyHouse.Domain/Interfaces/` | `ISplitStrategy`, `IBalanceContributor`, `IHouseholdOwned`, `IRepository`, `IClock` |
| | `EzyHouse.Api/Core/ICurrentUserContext.cs`, `EzyHouse.Api/Features/*/I*Service.cs`, `I*Repository.cs` | the current user; one service and one repository interface per feature |
| **Generics** | `EzyHouse.Domain/Scheduling/RecurringSchedule.cs` | `RecurringSchedule<TOccurrence>`, shared by rent and bills |
| | `EzyHouse.Domain/Extensions/QueryableExtensions.cs` | `ForHousehold<T>() where T : IHouseholdOwned` keeps every query inside one household |
| | `EzyHouse.Domain/Splitting/SplitCalculator.cs`, `EzyHouse.Api/Features/Auth/AuthFeature.cs` | `Dictionary<SplitType, ISplitStrategy>`; ASP.NET's `PasswordHasher<User>` hashes passwords |
| **Anonymous method with LINQ and lambdas** | `EzyHouse.Api/Features/Households/HouseholdService.cs` (`UnpaidRentProblem`) | an explicit `delegate (PaidShare share) { … }` used inside a LINQ query |
| | `EzyHouse.Domain/Balances/BalanceCalculator.cs` | LINQ with lambdas: `GroupBy` per pair of people, then net each pair |
| **Delegates** | `EzyHouse.Domain/Scheduling/FrequencyExtensions.cs` | `Dictionary<Frequency, Func<DateOnly, int, DateOnly>>`: one date step per frequency |
| **Enumerators** | `EzyHouse.Domain/Scheduling/RecurringSchedule.cs` | `GetSlots()` uses `yield return` to produce each rent or bill slot |
| **Properties** | `EzyHouse.Domain/Entities/Bond.cs`, `Furniture.cs`, `Bill.cs` | computed `Outstanding`, `Purchase`, `Sale`; private setters on lifecycle fields such as `Status` |
| **Extension methods** | `EzyHouse.Domain/Scheduling/FrequencyExtensions.cs`, `EzyHouse.Domain/Extensions/MoneyExtensions.cs`, `EzyHouse.Api/Core/ClaimsPrincipalExtensions.cs` | `AddFrequency`, `HasAtMostTwoDecimals`, `GetUserId` |
| | `EzyHouse.Api/Core/CoreMappings.cs`, `EzyHouse.Api/Features/*/*Mappings.cs`, `*Feature.cs` | `ToDto()` mappers; `Add<Feature>Feature()` service registration |
| **Enums** | `EzyHouse.Domain/Enums/` | `SplitType`, `Frequency`, `BillStatus`, `FurnitureStatus`, sent as lowercase strings |
| **Error handling** | `EzyHouse.Domain/Exceptions/AppException.cs`, `EzyHouse.Api/Core/AppExceptionHandler.cs` | one exception type per status (400, 401, 403, 404, 409), turned into ProblemDetails in one place |
| **Validation** | `EzyHouse.Api/Core/MoneyAttribute.cs`, `EzyHouse.Api/Features/*/*Dtos.cs` | `[Required]`, `[EmailAddress]`, `[StringLength]` and a custom `[Money]` on request fields |
| | `EzyHouse.Domain/Entities/Furniture.cs`, `Bill.cs`, `Bond.cs` | lifecycle rules: e.g. a sold item can't be sold again (409) |
| **NUnit** | `EzyHouse.Tests/` | 114 tests: `Domain/` (pure rules), `Features/` (each service against a real database, plus the login cookie), `Core/`, `Infrastructure/` |
| **Database + EF + LINQ** (bonus) | `EzyHouse.Infrastructure/Data/EzyHouseDbContext.cs`, `Data/Configurations/`, `Migrations/` | MySQL through Pomelo EF Core, Code First; LINQ to Entities in every `*Repository.cs` |
| **Non-Windows-Forms UI** (bonus) | `../API.md` | the React frontend calls this Web API |
| **High cohesion, low coupling** | `EzyHouse.Api/Features/` | one folder per feature; features never reference each other; dependencies injected through interfaces |
