# EzyHouse backend

The REST API behind the EzyHouse React app: ASP.NET Core Web API on .NET 10, MySQL 8 through
Entity Framework Core (Code First), tested with NUnit. Every endpoint in [API.md](../API.md) (v5) is
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
database is empty, adds the demo data below. No `dotnet ef` command is needed. Once running, a
background job creates rent and bill expenses as they fall due: at start-up, then every hour.

> **Schema rebuilt for API.md v5.** The tables were redesigned (one `Expenses` table for rent, bills,
> groceries and furniture) and the migration history starts again. A database created by an earlier
> version can't be upgraded: if you ran the API before, drop the old `ezyhouse` database first, with
> `docker compose down -v` (see [Start again](#start-again-with-fresh-demo-data)) or
> `DROP DATABASE ezyhouse;` in your own MySQL.

In Visual Studio or Rider: open `EzyHouse.slnx`, set **EzyHouse.Api** as the start-up project and
choose the **http** profile. MySQL still needs `docker compose up -d --wait` first.

### Try it in Swagger

Open **<http://localhost:5219/swagger>**.

1. `POST /api/auth/login` with `{ "email": "alice@example.com", "password": "Password1!" }`, then
   copy the `token` from the response.
2. Click **Authorize** and paste the token on its own, without "Bearer".
3. Try `GET /api/household`, `GET /api/balances` and `GET /api/expenses?tag=rent`.

Without a token every endpoint except register, login and logout returns 401. Login and register
also set an HttpOnly cookie, `ezyhouse_token`, which the API accepts instead of the header;
`POST /api/auth/logout` clears it.

### Demo data

| | |
|---|---|
| Users | **Alice** `alice@example.com` (primary), **Bob** `bob@example.com`, **Cara** `cara@example.com` |
| Password | `Password1!` for all three |
| Household | "Demo House", invite code `DEMH7KQ2` |
| Records | 3 grocery expenses and a settlement; a weekly $750 rent schedule from 6 weeks ago with its 7 rent expenses; Electricity (quarterly) and Internet (monthly) bill types whose past bills have amounts and whose latest bill is still $0; a couch ($600) and a TV ($450) as furniture expenses; a $2,400 bond with one contribution |

Dates are relative to the day the database was seeded. From then on the background job adds each new
week's rent and each new bill.

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

124 NUnit tests. They use an in-memory SQLite database, so MySQL doesn't need to be running. In
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
| **Polymorphism** (overriding) | `EzyHouse.Domain/Entities/Expense.cs` | `CanBeChangedBy` is abstract on `Expense` and overridden per tag: groceries by whoever added them; rent, bills and furniture by the current primary |
| | `EzyHouse.Domain/Scheduling/RecurringSchedule.cs`, `EzyHouse.Domain/Entities/RentSchedule.cs`, `BillType.cs` | `CreateOccurrence` is overridden: a rent schedule creates a rent expense for the period, a bill type creates a $0 bill expense |
| | `EzyHouse.Domain/Exceptions/AppException.cs` | each exception overrides its HTTP status code and title |
| **Polymorphism** (through an interface) | `EzyHouse.Domain/Splitting/EqualSplitStrategy.cs`, `ExactSplitStrategy.cs` | two `ISplitStrategy` implementations, picked by `splitType` |
| **Polymorphism** (overloading) | `EzyHouse.Domain/ValueObjects/Share.cs`, `EzyHouse.Infrastructure/Data/Configurations/ConfigurationExtensions.cs` | two `Share` constructors; `HasMoneyPrecision` for `decimal` and `decimal?` properties |
| **Interfaces** (at least two) | `EzyHouse.Domain/Interfaces/` | `ISplitStrategy`, `IBalanceContributor`, `IHouseholdOwned`, `IRepository`, `IClock` |
| | `EzyHouse.Api/Core/ICurrentUserContext.cs`, `EzyHouse.Api/Features/*/I*Service.cs`, `I*Repository.cs` | the current user; one service and one repository interface per feature |
| **Generics** | `EzyHouse.Domain/Scheduling/RecurringSchedule.cs` | `RecurringSchedule<TOccurrence> where TOccurrence : Expense`, shared by rent and bills |
| | `EzyHouse.Domain/Extensions/QueryableExtensions.cs` | `ForHousehold<T>() where T : IHouseholdOwned` keeps every query inside one household |
| | `EzyHouse.Domain/Splitting/SplitCalculator.cs`, `EzyHouse.Api/Features/Auth/AuthFeature.cs` | `Dictionary<SplitType, ISplitStrategy>`; ASP.NET's `PasswordHasher<User>` hashes passwords |
| **Anonymous method with LINQ and lambdas** | `EzyHouse.Api/Features/Households/HouseholdService.cs` (`LeaveAsync`) | an explicit `delegate (Debt debt) { … }` used in LINQ's `Any` to find the leaver's debts |
| | `EzyHouse.Domain/Balances/BalanceCalculator.cs` | LINQ with lambdas: `GroupBy` per pair of people, then net each pair |
| **Delegates** | `EzyHouse.Domain/Scheduling/FrequencyExtensions.cs` | `Dictionary<Frequency, Func<DateOnly, int, DateOnly>>`: one date step per frequency |
| **Enumerators** | `EzyHouse.Domain/Scheduling/RecurringSchedule.cs` | `GetSlots()` uses `yield return` to produce each rent or bill date, without end when there is no end date |
| **Properties** | `EzyHouse.Domain/Entities/Bond.cs`, `Expense.cs`, `RentSchedule.cs`, `BillType.cs` | computed `Outstanding` and `HasAnyoneToShare`; private setters on `Bond.Total` and on `Expense.Tag`, which can't change |
| **Extension methods** | `EzyHouse.Domain/Scheduling/FrequencyExtensions.cs`, `EzyHouse.Domain/Extensions/MoneyExtensions.cs`, `EzyHouse.Api/Core/ClaimsPrincipalExtensions.cs` | `AddFrequency`, `HasAtMostTwoDecimals`, `GetUserId` |
| | `EzyHouse.Api/Core/CoreMappings.cs`, `EzyHouse.Api/Features/*/*Mappings.cs`, `*Feature.cs` | `ToDto()` mappers; `Add<Feature>Feature()` service registration |
| **Enums** | `EzyHouse.Domain/Enums/` | `SplitType`, `Frequency`, `ExpenseTag` (also the database discriminator), sent as lowercase strings |
| **Error handling** | `EzyHouse.Domain/Exceptions/AppException.cs`, `EzyHouse.Api/Core/AppExceptionHandler.cs` | one exception type per status (400, 401, 403, 404, 409), turned into ProblemDetails in one place |
| **Validation** | `EzyHouse.Api/Core/MoneyAttribute.cs`, `EzyHouse.Api/Features/*/*Dtos.cs` | `[Required]`, `[EmailAddress]`, `[StringLength]` and a custom `[Money]` on request fields |
| | `EzyHouse.Domain/Entities/Bond.cs`, `EzyHouse.Api/Features/Rent/RentService.cs`, `Bills/BillService.cs` | rules checked in code rather than by an attribute: contributions can't exceed the bond total; a rent schedule or bill type needs at least one person (400) |
| **NUnit** | `EzyHouse.Tests/` | 124 tests: `Domain/` (pure rules), `Features/` (each service against a real database, plus the login cookie), `Core/`, `Infrastructure/` |
| **Database + EF + LINQ** (bonus) | `EzyHouse.Infrastructure/Data/EzyHouseDbContext.cs`, `Data/Configurations/`, `Migrations/` | MySQL through Pomelo EF Core, Code First; LINQ to Entities in every `*Repository.cs` |
| **Non-Windows-Forms UI** (bonus) | `../API.md` | the React frontend calls this Web API |
| **High cohesion, low coupling** | `EzyHouse.Api/Features/` | one folder per feature; features never reference each other; dependencies injected through interfaces |
