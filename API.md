# Household Expenses — API Contract (v4)

Frontend ↔ backend contract. Endpoint shapes and top-level schemas only.
⚠ = judgement call, open to veto.

## What changed from v3

- **Bills are their own model and tab.** They work like rent: the primary submits the amount, then members tick their share as paid. Bills are no longer expenses.
- **Expenses are groceries and other day-to-day shared costs only.** `category` and `?category=` were removed because only one category was left ⚠.
- **`RentShare` was renamed `PaidShare`.** Rent and bills now use the same shape.
- **`Frequency` gained `quarterly`** (rent and bills) ⚠.
- **Leaving the household is also blocked by unpaid bill shares.**
- **Leaving now checks debts, not your net balance.** It is blocked while you appear in any entry of `debts` from `GET /api/balances`. A net of $0 could hide two debts that cancel out (you owe Alice $10, Bob owes you $10), and once you leave nobody could settle them.
- **Added a Dashboard section** explaining which endpoints build the dashboard. There is no new endpoint.

---

## Conventions

- Base path `/api`. JSON uses camelCase.
- Every endpoint except `/api/auth/*` requires `Authorization: Bearer <token>`.
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
PaidShare    { user: UserRef, amount, paid: bool, paidDate: date | null }
SplitType    = "equal" | "exact"
Frequency    = "weekly" | "fortnightly" | "monthly" | "quarterly"
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
| POST | `/api/auth/register` | `{ name, email, password }` | `AuthResult` | 409 email already used |
| POST | `/api/auth/login` | `{ email, password }` | `AuthResult` | 401 |
| GET | `/api/me` | — | `User` | |

## Household

Each user belongs to at most one household. The creator becomes **primary** and can hand the role to another member.

Handing over primary doesn't move existing debts. Unpaid rent and bill shares stay owed to whoever was primary when they were created. Only new rent periods and bills go to the new primary.

A member who joins isn't added to anything automatically. The primary adds them to each rent schedule and bill type.

```ts
Household { id, name, inviteCode, primary: UserRef, members: UserRef[] }
```

| Method | Path | Body | Returns | Who | Errors |
|---|---|---|---|---|---|
| POST | `/api/household` | `{ name }` | `Household` | anyone | 409 already in a household |
| GET | `/api/household` | — | `Household` | member | 404 not in one |
| POST | `/api/household/join` | `{ inviteCode }` | `Household` | anyone | 404 bad code, 409 already in a household |
| POST | `/api/household/leave` | — | 204 | member | 409 (see Leaving) |
| PUT | `/api/household/primary` | `{ userId }` | `Household` | primary | 403, 400 not a member |

### Leaving is blocked (409) while

- you owe anyone, or anyone owes you, in groceries and furniture (you appear in any entry of `debts` from `GET /api/balances`)
- any rent share owed by you or to you is unpaid
- you are in the split of a rent schedule that hasn't ended
- any bill share owed by you or to you is unpaid
- you are primary and other members remain ⚠ (hand over primary first)

On leaving:
- you are removed from every bill type's members ⚠
- you give up your stake in furniture, and the items stay with the household

---

## Expenses (Groceries tab)

Day-to-day shared costs. They count in **balances**.

```ts
Expense      { id, description, amount, date, paidBy: UserRef, createdBy: UserRef, splitType, shares: Share[] }
ExpenseInput { description, amount, date, paidByUserId, splitType, shares: ShareInput[] }
```

| Method | Path | Body | Returns | Who |
|---|---|---|---|---|
| GET | `/api/expenses` | — | `Expense[]` | member |
| GET | `/api/expenses/{id}` | — | `Expense` | member |
| POST | `/api/expenses` | `ExpenseInput` | `Expense` | member |
| PUT | `/api/expenses/{id}` | `ExpenseInput` | `Expense` | creator only (403) |
| DELETE | `/api/expenses/{id}` | — | 204 | creator only (403) |

## Balances & settle-up

Splitwise style. You can settle any positive amount between two people. There are no per-expense paid flags and no debt simplification.
Balances include **expenses and furniture only**. Rent, bills and bond are excluded.

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

---

## Rent (Rent tab)

The primary manages schedules. Each schedule generates periods, and members tick their share as paid. Rent is **not** in balances.

```ts
RentSchedule      { id, amount, frequency, startDate, endDate: date | null, splitType, shares: Share[], nextDueDate: date | null }
RentScheduleInput { amount, frequency, startDate, endDate?, splitType, shares: ShareInput[] }
RentPeriod        { id, scheduleId, startDate, endDate, amount, payee: UserRef, shares: PaidShare[] }
```

| Method | Path | Body | Returns | Who |
|---|---|---|---|---|
| GET | `/api/rent-schedules` | — | `RentSchedule[]` | member |
| POST | `/api/rent-schedules` | `RentScheduleInput` | `RentSchedule` | primary |
| PUT | `/api/rent-schedules/{id}` | `RentScheduleInput` | `RentSchedule` | primary |
| DELETE | `/api/rent-schedules/{id}` | — | 204 | primary |
| GET | `/api/rent-periods` | — | `RentPeriod[]` | member |
| PUT | `/api/rent-periods/{id}/shares/{userId}` | `{ paid }` | `RentPeriod` | that user, or primary (403) |

Rules:
- There is no background timer. Periods up to today are created when the list is read. A unique index on `(scheduleId, startDate)` prevents duplicates.
- Edits only affect periods created later. Deleting a schedule deletes its periods.
- The payee is whoever is primary when the period is created. The payee's own share is ticked automatically ⚠.
- Ticks can be undone (`paid: false`).
- New members are added to a schedule manually by the primary.

## Bills (Bills tab)

The primary sets up bill types (e.g. Electricity, Gas, Internet) with a frequency. When a bill is due, a **placeholder** appears with no amount. The primary enters the amount and submits it, which means "I paid the provider". The server then works out what each member owes the primary, and members tick their share as paid. Bills are **not** in balances. Everyone can see all bills, past and pending.

```ts
BillType      { id, name, frequency, startDate, endDate: date | null, members: UserRef[], nextDueDate: date | null }
BillTypeInput { name, frequency, startDate, endDate?, memberIds: int[] }
BillStatus    = "pending" | "submitted"
Bill          { id, billType: { id, name }, dueDate, status: BillStatus,
                amount: decimal | null, payee: UserRef | null, submittedDate: date | null,
                shares: PaidShare[] }          // empty while pending
SubmitBillInput { amount, splitType, shares?: ShareInput[] }
                // equal → split across the bill type's members (shares omitted)
                // exact → shares required, must sum to amount
```

| Method | Path | Body | Returns | Who |
|---|---|---|---|---|
| GET | `/api/bill-types` | — | `BillType[]` | member |
| POST | `/api/bill-types` | `BillTypeInput` | `BillType` | primary |
| PUT | `/api/bill-types/{id}` | `BillTypeInput` | `BillType` | primary |
| DELETE | `/api/bill-types/{id}` | — | 204 | primary |
| GET | `/api/bills?status=` | — | `Bill[]` | member |
| POST | `/api/bills/{id}/submit` | `SubmitBillInput` | `Bill` | primary; 409 if already submitted |
| PUT | `/api/bills/{id}/shares/{userId}` | `{ paid }` | `Bill` | that user, or primary (403); 409 if pending |

Rules:
- Placeholders are created when the list is read, the same way as rent periods. A unique index on `(billTypeId, dueDate)` prevents duplicates.
- Edits to a bill type only affect placeholders created later.
- The payee is the primary who submitted. Their own share is ticked automatically ⚠.
- A submitted amount can't be changed ⚠ (409).
- Deleting a bill type removes its pending placeholders and **keeps submitted bills** as history ⚠.
- New members are added to bill types manually by the primary.

---

## Furniture (Furniture tab)

Shared purchases that can later be sold or disposed of. All money moves count in **balances**.

```ts
FurnitureStatus = "owned" | "sold" | "disposed"
Purchase  { date, amount, paidBy: UserRef,     splitType, shares: Share[] }
Sale      { date, amount, receivedBy: UserRef, splitType, shares: Share[] }
Disposal  { date, amount, paidBy: UserRef,     splitType, shares: Share[] }   // amount may be 0
Furniture { id, name, status: FurnitureStatus, createdBy: UserRef,
            purchase: Purchase, sale: Sale | null, disposal: Disposal | null }

FurnitureInput { name, purchase: { date, amount, paidByUserId, splitType, shares: ShareInput[] } }
SaleInput      { date, amount, receivedByUserId, splitType, shares: ShareInput[] }
DisposalInput  { date, amount, paidByUserId, splitType, shares: ShareInput[] }
```

| Method | Path | Body | Returns | Who |
|---|---|---|---|---|
| GET | `/api/furniture?status=` | — | `Furniture[]` | member |
| GET | `/api/furniture/{id}` | — | `Furniture` | member |
| POST | `/api/furniture` | `FurnitureInput` | `Furniture` | member |
| PUT | `/api/furniture/{id}` | `FurnitureInput` | `Furniture` | creator or primary |
| DELETE | `/api/furniture/{id}` | — | 204 | creator or primary |
| POST | `/api/furniture/{id}/sell` | `SaleInput` | `Furniture` | creator or primary |
| POST | `/api/furniture/{id}/dispose` | `DisposalInput` | `Furniture` | creator or primary |

Rules:
- Status only moves forward: `owned → sold` or `owned → disposed`. Editing, selling or disposing an item that isn't `owned` → 409.
- On a sale, the person who receives the cash owes the others their shares.

  Example: Alice, Bob and Cara buy a couch for $600 and later sell it for $300. Cara receives the $300, so she owes Alice and Bob $100 each.

## Bond (Bond tab)

Record only. The primary manages it. Bond never affects balances.

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

Rule: contributions can't add up to more than `total` (400).

---

## Dashboard

There is no dedicated endpoint. The frontend builds the dashboard from existing endpoints:

| Section | Source | Shows |
|---|---|---|
| Groceries | `GET /api/balances` | Your net balance, who owes whom, settle-up button |
| Rent | `GET /api/rent-periods` | Current period, what you owe or are owed |
| Bills | `GET /api/bills` | Placeholders waiting for an amount, unpaid shares you owe the primary |
| Furniture | `GET /api/furniture?status=owned` | One line, e.g. "4 items owned" |

## Left out on purpose

- Payments: per-expense paid status, undoing a settlement (record the reverse instead).
- Splits and money: percentage or weighted splits, multiple currencies.
- Users and households: profile edits, removing members.
- Features: receipts or file uploads, pagination, furniture buyouts, one-off rent changes, bond refunds, editing a submitted bill amount.

## Open questions

- Bond "lock": should a bond become read-only once it's entered?
