# Household Expenses — API Contract (v5)

Frontend ↔ backend contract. Endpoint shapes and top-level schemas only.
⚠ = judgement call, open to veto.

## What changed from v4

- **Everything that costs money is an expense with a tag:** `rent`, `bills`, `groceries` or `furniture`. The tag only decides which tab shows it. Rent periods, bills and furniture items are no longer separate models.
- **One balance per pair of people.** Every expense, whatever its tag, and every settlement count in it. A settlement of any amount pays down the whole balance between two people.
- **No paid ticks.** `PaidShare`, the rent and bill share ticks, bill placeholders with `status`, and bill submit are gone. An expense is a record of what was spent and how it was split.
- **Schedules create expenses in the background.** Rent schedules and bill types now have a stored `nextDueDate`. A background job creates each expense when it falls due. Nothing is created when a list is read.
- **Furniture is a plain expense** tagged `furniture`, added by the primary. There is no owned/sold/disposed status, sale or disposal.
- **Leaving checks the balance only.** A leaving primary names the next primary in the request. The person leaving is taken off every rent schedule and bill type.
- **Removed endpoints:** `/api/rent-periods`, `/api/rent-periods/{id}/shares/{userId}`, `/api/bills`, `/api/bills/{id}/submit`, `/api/bills/{id}/shares/{userId}`, and every `/api/furniture` endpoint.

---

## Conventions

- Base path `/api`. JSON uses camelCase.
- Every endpoint except `/api/auth/*` requires `Authorization: Bearer <token>`, or the `ezyhouse_token` cookie set by register/login (see Auth). If both are sent, the header wins.
- IDs are integers.
- Money is a number with exactly 2 decimal places. More decimals → 400.
- Dates are `YYYY-MM-DD`.
- Enums are lowercase strings (`JsonStringEnumConverter(JsonNamingPolicy.CamelCase)`).
- Errors use ProblemDetails:

  | Code | Meaning |
  |---|---|
  | 400 | Validation failed |
  | 401 | Not logged in, or bad token |
  | 403 | Not in a household, or not allowed (e.g. not primary) |
  | 404 | Not found |
  | 409 | Breaks a business rule |

- URLs never contain a household ID. Everything applies to the caller's household.

## Shared shapes

```ts
UserRef      { id, name }
Share        { user: UserRef, amount }
ShareInput   { userId, amount? }          // amount only when splitType = exact
SplitType    = "equal" | "exact"
Frequency    = "weekly" | "fortnightly" | "monthly" | "quarterly"
ExpenseTag   = "rent" | "bills" | "groceries" | "furniture"
```

### Splitting rules (used everywhere)

- **equal**: the server divides the amount. Leftover cents go to the first share.
- **exact**: share amounts must add up to the total. Otherwise → 400.
- Everyone named in a split must be a current household member. Otherwise → 400.

---

## Auth

```ts
User       { id, name, email, householdId: int | null }
AuthResult { token, user: User }
```

| Method | Path | Body | Returns | Errors |
|---|---|---|---|---|
| POST | `/api/auth/register` | `{ name, email, password }` | `AuthResult` | 400 invalid email or password under 8 characters; 409 email already used |
| POST | `/api/auth/login` | `{ email, password }` | `AuthResult` | 401 |
| POST | `/api/auth/logout` | — | 204, no body | |
| GET | `/api/me` | — | `User` | |

- Register and login also set the cookie `ezyhouse_token` (HttpOnly, SameSite=Strict, Path=/, lasts as long as the token; Secure only over HTTPS). The API accepts it instead of the `Authorization` header, so the browser app never needs to read or store the token itself.
- Logout clears that cookie. It works without being logged in. The token itself stays valid until it expires, so a copy sent in the header still works.

## Household

Each user belongs to at most one household. The creator becomes **primary** and can hand the role to another member.

Handing over primary doesn't move existing debts: expenses already created keep their payer. Rent and bill expenses created from then on are paid by the new primary.

A member who joins isn't added to anything automatically. The primary adds them to each rent schedule and bill type.

```ts
Household  { id, name, inviteCode, primary: UserRef, members: UserRef[] }
LeaveInput { nextPrimaryUserId?: int }   // required when you are primary and other members remain
```

| Method | Path | Body | Returns | Who | Errors |
|---|---|---|---|---|---|
| POST | `/api/household` | `{ name }` | `Household` | anyone | 409 already in a household |
| GET | `/api/household` | — | `Household` | member | 404 not in one |
| POST | `/api/household/join` | `{ inviteCode }` | `Household` | anyone | 404 bad code, 409 already in a household |
| POST | `/api/household/leave` | `LeaveInput` | 204 | member | 400 next primary missing or not another member (checked before 409); 409 (see Leaving) |
| PUT | `/api/household/primary` | `{ userId }` | `Household` | primary | 403, 400 not a member |

### Leaving is blocked (409) while

- you owe anyone, or anyone owes you (you appear in any entry of `debts` from `GET /api/balances`)

The 409's `detail` lists every reason that applies, one per line.

On leaving:
- if you are primary and others remain, `nextPrimaryUserId` becomes primary first
- you are taken off every rent schedule's split and every bill type's members ⚠. A rent schedule with an exact split becomes an equal split among those left. A schedule left with nobody ends that day (`endDate` = today).
- if you were the last member, the household is kept as history and its invite code stops working (joining with it returns 404)

---

## Expenses (Rent, Bills, Groceries and Furniture tabs)

Every cost is an expense with a tag. The tag decides which tab shows it and who may change it; it never changes how the money counts. Every expense counts in **balances**.

```ts
Expense         { id, tag: ExpenseTag, description, amount, date,
                  paidBy: UserRef, createdBy: UserRef, splitType, shares: Share[],
                  canChange: bool,                 // whether the caller may edit or delete it
                  rentScheduleId: int | null,      // rent created by a schedule
                  periodEnd: date | null,          // rent created by a schedule: the last day it covers
                  billTypeId: int | null }         // bills created by a bill type
ExpenseInput    { description, amount, date, paidByUserId, splitType, shares: ShareInput[] }
NewExpenseInput { tag: ExpenseTag, ...ExpenseInput }
```

| Method | Path | Body | Returns | Who |
|---|---|---|---|---|
| GET | `/api/expenses?tag=` | — | `Expense[]`, newest first; every tag when `tag` is omitted; 400 unknown tag | member |
| GET | `/api/expenses/{id}` | — | `Expense` | member |
| POST | `/api/expenses` | `NewExpenseInput` | `Expense` | `groceries`: any member. `rent`, `bills`, `furniture`: primary (403) |
| PUT | `/api/expenses/{id}` | `ExpenseInput` | `Expense` | `groceries`: whoever added it. Others: the current primary (403) |
| DELETE | `/api/expenses/{id}` | — | 204 | same as PUT |

Rules:
- `amount` may be 0. A $0 expense counts for nothing. Bills start at $0 until the primary enters the amount ⚠.
- An expense's tag never changes.
- `paidBy` must be a current member.
- Deleting an expense created by a schedule is permanent: the schedule never creates that date again.

## Schedules

Rent schedules and bill types create expenses on their dates. They hold no money of their own.

A background job runs when the API starts and then every hour. For every schedule whose `nextDueDate` is today or earlier, it creates that date's expense, moves `nextDueDate` one step on, and repeats until `nextDueDate` is in the future. So due dates passed while the API was off are still created, each with its own date. A unique index on (schedule, date) stops the same expense being created twice.

- **Creating a schedule:** `nextDueDate` becomes the first date of the schedule on or after today. Earlier dates are never created ⚠. If that date is today, the expense is created straight away.
- **Editing a schedule:** expenses already created don't change. `nextDueDate` is recalculated from the new settings: never earlier than today, and never on or before a date the schedule has already created (even if that expense was deleted).
- **Ending:** once the next date would fall after `endDate`, `nextDueDate` is `null`.
- **Deleting a schedule** keeps its expenses, which stay in balances and on their tab. They lose the link to the schedule.
- **Who pays a created expense:** whoever is primary on that date.
- **Nobody to split with:** a rent schedule needs at least one share and a bill type at least one member (400). The job skips a schedule with nobody in it rather than failing.

### Rent schedules (Rent tab)

The primary manages schedules. Each created expense is tagged `rent`, described "Rent", dated the period's first day, with `periodEnd` its last day, split as the schedule says.

```ts
RentSchedule      { id, amount, frequency, startDate, endDate: date | null, splitType, shares: Share[], nextDueDate: date | null }
RentScheduleInput { amount, frequency, startDate, endDate?, splitType, shares: ShareInput[] }
```

| Method | Path | Body | Returns | Who |
|---|---|---|---|---|
| GET | `/api/rent-schedules` | — | `RentSchedule[]` | member |
| POST | `/api/rent-schedules` | `RentScheduleInput` | `RentSchedule` | primary |
| PUT | `/api/rent-schedules/{id}` | `RentScheduleInput` | `RentSchedule` | primary |
| DELETE | `/api/rent-schedules/{id}` | — | 204 | primary |

### Bill types (Bills tab)

The primary sets up bill types (e.g. Electricity, Gas, Internet) with a frequency and members. Each created expense is tagged `bills`, described with the bill type's name, dated the due date, **$0**, and split equally among the members. When the real bill arrives, the primary edits the expense to its amount and split.

```ts
BillType      { id, name, frequency, startDate, endDate: date | null, members: UserRef[], nextDueDate: date | null }
BillTypeInput { name, frequency, startDate, endDate?, memberIds: int[] }
```

| Method | Path | Body | Returns | Who |
|---|---|---|---|---|
| GET | `/api/bill-types` | — | `BillType[]` | member |
| POST | `/api/bill-types` | `BillTypeInput` | `BillType` | primary |
| PUT | `/api/bill-types/{id}` | `BillTypeInput` | `BillType` | primary |
| DELETE | `/api/bill-types/{id}` | — | 204 | primary |

---

## Balances & settle-up

Splitwise style: one balance per pair of people, worked out on every read from every expense and every settlement. You can settle any positive amount between two people; it pays down the whole balance between them, never a particular expense. There is no debt simplification.

```ts
Balances        { members: { user: UserRef, net }[], debts: { from: UserRef, to: UserRef, amount }[] }
                // net > 0: the household owes you; net < 0: you owe
Settlement      { id, from: UserRef, to: UserRef, amount, date, createdBy: UserRef }
SettlementInput { fromUserId, toUserId, amount }   // date = today
```

| Method | Path | Body | Returns | Who |
|---|---|---|---|---|
| GET | `/api/balances` | — | `Balances` | member |
| GET | `/api/settlements` | — | `Settlement[]` | member |
| POST | `/api/settlements` | `SettlementInput` | `Settlement` | only the `from` or `to` person (403) |

Settlements can't be undone. To fix a mistake, record a settlement the other way.

## Bond (Bond tab)

Record only. The primary manages it. Bond is not an expense and never affects balances or leaving.

```ts
Contribution      { user: UserRef, amount, date }
Bond              { id, total, paidDate, contributions: Contribution[], outstanding }  // outstanding = total − Σ contributions
BondInput         { total, paidDate, contributions: { userId, amount, date }[] }
```

| Method | Path | Body | Returns | Who |
|---|---|---|---|---|
| GET | `/api/bonds` | — | `Bond[]` | member |
| GET | `/api/bonds/{id}` | — | `Bond` | member |
| POST | `/api/bonds` | `BondInput` | `Bond` | primary |
| PUT | `/api/bonds/{id}` | `BondInput` | `Bond` | primary |
| DELETE | `/api/bonds/{id}` | — | 204 | primary |

Rule: contributions can't add up to more than `total` (400). The primary can always edit a bond; it never locks.

---

## Dashboard

There is no dedicated endpoint. The frontend builds Home from existing endpoints:

| Section | Source | Shows |
|---|---|---|
| Settle up | `GET /api/balances` | One amount per person you owe or who owes you, with the payment button |
| Bills to enter (primary) | `GET /api/expenses?tag=bills` | Bills still at $0 |
| Recent | `GET /api/expenses`, `GET /api/settlements` | The latest expenses and payments |
| What's in use | `GET /api/rent-schedules`, `GET /api/bill-types`, `GET /api/expenses` | Which tags to offer to start |

## Left out on purpose

- Payments: per-expense paid status or ticks, undoing a settlement (record the reverse instead).
- Splits and money: percentage or weighted splits, multiple currencies.
- Users and households: profile edits, removing members.
- Features: receipts or file uploads, pagination, furniture sale or disposal, back-filling a schedule's past dates, bond refunds.

## Open questions

None open.
