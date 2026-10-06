# Technical Code Specifications: ezyhouse

**Product Name:** ezyhouse  
**System Type:** Sharehouse Expense & Rent Management Web Application  
**Target Environment:** .NET 9.0 (C# Web API), MySQL, React 18+  
**Target Assessment:** UTS 31927/32998 Assignment 2 (Spring 2026)  

---

## 1. System Architecture & Tech Stack

ezyhouse follows a decoupled Client-Server architecture designed to fulfill all technical requirements and bonus point criteria of the assignment specification.

+-----------------------------------------------------------------------+
|                           React Frontend                              |
| (Responsive Single Page Application built with React 18 & Axios)      |
+-----------------------------------------------------------------------+
|
REST API (JSON / HTTP)
v
+-----------------------------------------------------------------------+
|                       C# ASP.NET Core Web API                         |
| (Controllers, Business Logic Layer, Generic Repositories, LINQ)       |
+-----------------------------------------------------------------------+
|
Entity Framework Core
v
+-----------------------------------------------------------------------+
|                            MySQL Database                             |
|        (Relational Database Managed via EF Core Migrations)           |
+-----------------------------------------------------------------------+

### Stack Components

- **Frontend:** React 18, React Router, Axios, Recharts (for interactive charts), Tailwind CSS / Bootstrap.
- **Backend:** C# ASP.NET Core Web API (.NET 9.0 framework in Visual Studio 2022).
- **Database & ORM:** MySQL Server using `Pomelo.EntityFrameworkCore.MySql` with Entity Framework Core code-first migrations.
- **Testing:** NUnit test suite verifying financial algorithms and frequency conversion engines.

---



## 2. Assessment Technical Requirements Mapping


| Assignment Requirement                      | Technical Implementation in ezyhouse                                                                                                                |
| ------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| **At least 4 Unique Responsive Screens**    | 5 full responsive screens: Dashboard, Expense & Rent Split Calculator, Recurring Cycle Manager, Bond Ledger, and PayID/Export Hub.                  |
| **At least 6 UI Element Categories**        | DataGrid tables, Interactive Bar/Pie Charts, Range Sliders, Modals, Category Dropdowns, Action Buttons, and Context Menus.                          |
| **Polymorphism (Inheritance / Overriding)** | Abstract base class `Expense` with derived classes `RentExpense`, `UtilityBill`, and `AdHocExpense` overriding the `CalculateSplitShares()` method. |
| **At least 2 Interfaces**                   | `ISplitStrategy` (calculates proportioning formulas) and `INotifiable` (handles bill overdue and imbalance alerts).                                 |
| **Generics / Generic Collections**          | Generic Repository Pattern (`IRepository<T>`, `Repository<T>`) and strong typing with `List<T>`, `Dictionary<TKey, TValue>`.                        |
| **Anonymous Methods & LINQ Lambdas**        | LINQ queries utilizing Lambda expressions for debt minimization algorithms, category aggregations, and timeline sorting.                            |
| **Entity Framework & Database**             | Entity Framework Core context (`EzyHouseDbContext`) mapped to a MySQL database schema[cite: 1, 2].                                                  |
| **NUnit Test Cases**                        | Unit tests for fortnightly-to-monthly rent conversions, split proportioning logic, and credit balance calculations[cite: 1, 2].                     |
| **Bonus Marks (UI Library)**                | Uses a React UI framework communicating with ASP.NET Core instead of default Windows Forms (+2 Bonus Marks).                                        |


---



## 3. Database Schema & Entity Framework Models



### 3.1 ER Diagram Structure

- **Flatmate**: `Id`, `Name`, `Email`, `PayID`, `RoomSizeSqm`, `MoveInDate`, `IsActive`
- **Expense (Base Entity)**: `Id`, `Title`, `Amount`, `PayerId` (FK -> Flatmate), `DatePaid`, `ExpenseType` (Discriminator column), `Notes`
- **RentExpense (Derived)**: `BillingPeriod` (Weekly / Fortnightly / Monthly), `StartDate`, `EndDate`, `CalendarMonthlyEquivalent`
- **UtilityBill (Derived)**: `UtilityType` (Electricity / Gas / Water / Internet), `MeterReadingPrevious`, `MeterReadingCurrent`, `IsQuarterly`
- **AdHocExpense (Derived)**: `Category` (Groceries / Cleaning / Repairs), `ReceiptImageUrls`
- **ExpenseSplit**: `Id`, `ExpenseId` (FK), `FlatmateId` (FK), `AmountOwed`, `IsPaid`
- **BondRecord**: `Id`, `FlatmateId` (FK), `BondAmountPaid`, `DepositDate`, `RefundStatus`, `DeductionNotes`

---



## 4. Page Specifications & Implementation Details



### Page 1: Household Dashboard (`/dashboard`)

- **Purpose:** Provides a real-time overview of current household financial balances, recent expense streams, and upcoming bill due dates.
- **UI Elements Used:**
  - **Interactive Pie Chart:** Financial spending breakdown by category[cite: 2].
  - **DataGrid Table:** List of recent transactions[cite: 2].
  - **Status Cards:** Total household balance, personal balance owing/owed.
  - **Context Menu:** Quick options to mark debts as settled or view transaction details[cite: 2].
- **Backend Implementation:**
  - Endpoint: `GET /api/dashboard/summary`
  - **LINQ Implementation:** Lambda expressions aggregate total spending, group costs by category, and calculate balance matrices (`.GroupBy()`, `.Sum()`, `.Select()`)[cite: 2].



### Page 2: Expense & Rent Split Calculator (`/calculator`)

- **Purpose:** Allows users to record new expenses and calculate split shares using equal, room-size weighted, or custom percentage distributions[cite: 2].
- **UI Elements Used:**
  - **Category Dropdown:** Selection of expense types (Rent, Utility, Ad-Hoc)[cite: 2].
  - **Range Sliders:** Interactive room-size proportioning adjustments (`Sqm` slider per flatmate)[cite: 2].
  - **Action Modals:** Modal confirmation before logging high-value expenses[cite: 2].
  - **Input Fields:** Numerical currency inputs with live validation[cite: 2].
- **Backend Implementation:**
  - Endpoint: `POST /api/expenses/calculate-and-save`
  - **Polymorphism & Interfaces:** Uses `ISplitStrategy` (e.g., `RoomSizeSplitStrategy : ISplitStrategy`) and instantiates derived `Expense` subclasses (`RentExpense`, `UtilityBill`, `AdHocExpense`) which override `CalculateSplitShares()`[cite: 2].



### Page 3: Recurring Cycle Manager (`/cycles`)

- **Purpose:** Manages Australian recurring billing schedules (weekly rent, fortnightly rent, quarterly utility bills) and previews bill-smoothing options[cite: 2].
- **UI Elements Used:**
  - **Billing Frequency Selectors:** Dropdown choices (Weekly, Fortnightly, Calendar Monthly, Quarterly)[cite: 2].
  - **Interactive Line Chart:** Bill-smoothing trajectory over a 12-month timeline[cite: 2].
  - **Action Buttons:** "Smooth Bills" and "Adjust Cycle" triggers[cite: 2].
- **Backend Implementation:**
  - Endpoint: `GET /api/recurring-cycles`
  - **Algorithm Details:** Converts non-monthly schedules to standard monthly equivalents using Australian standards:
    $$
    \text{Monthly Rent} = \left(\text{Fortnightly Rent} \times \frac{26}{12}\right)
    $$
    $$
    \text{Monthly Rent} = \left(\text{Weekly Rent} \times \frac{52}{12}\right)
    $$



### Page 4: Bond & Deposit Ledger (`/bond-ledger`)

- **Purpose:** Tracks security deposits paid to rental authorities/agents, active lease periods, and tenant room assignment changes[cite: 2].
- **UI Elements Used:**
  - **Data Table:** Flatmate bond deposit status and balance log[cite: 2].
  - **Status Badges:** Visual indicators (`Held`, `Refunded`, `Partially Deducted`)[cite: 2].
  - **Action Modals:** Form to record bond deductions with supporting evidence descriptions[cite: 2].
- **Backend Implementation:**
  - Endpoints: `GET /api/bond`, `POST /api/bond/deduct`
  - **Generics Implementation:** Calls `IRepository<BondRecord>.GetByIdAsync()` and performs state transitions wrapped in Entity Framework database transactions[cite: 1, 2].



### Page 5: PayID & Export Hub (`/export-hub`)

- **Purpose:** Generates Australian PayID payment reference strings, outputs debt simplification instructions, and exports CSV reports for house meetings[cite: 2].
- **UI Elements Used:**
  - **Action Buttons:** "Export CSV" and "Simplify Debts"[cite: 2].
  - **Copy-to-Clipboard Text Inputs:** Formatted PayID transaction strings.
  - **Interactive Matrix / Flow Graph:** Visualizing "who pays whom" to settle all balances in minimum transactions[cite: 2].
- **Backend Implementation:**
  - Endpoints: `GET /api/export/csv`, `GET /api/debts/simplify`
  - **Debt Simplification Algorithm:** Uses LINQ Lambda expressions to balance total creditors and debtors, computing minimum required transactions using a greedy debt minimization algorithm[cite: 2].

---



## 5. C# Code Contracts & Design Patterns



### 5.1 Polymorphism Architecture

```csharp
public abstract class Expense
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int PayerId { get; set; }
    public DateTime DatePaid { get; set; }

    // Abstract method overridden by subclasses
    public abstract Dictionary<int, decimal> CalculateSplitShares(
        List<Flatmate> flatmates, 
        ISplitStrategy strategy
    );
}

public class RentExpense : Expense
{
    public string BillingPeriod { get; set; } = "Fortnightly"; // Weekly, Fortnightly, Monthly

    public override Dictionary<int, decimal> CalculateSplitShares(
        List<Flatmate> flatmates, 
        ISplitStrategy strategy
    )
    {
        // Executes rent-specific frequency normalization before applying split strategy
        decimal normalizedAmount = BillingPeriod switch
        {
            "Weekly" => (Amount * 52m) / 12m,
            "Fortnightly" => (Amount * 26m) / 12m,
            _ => Amount
        };
        return strategy.ExecuteSplit(normalizedAmount, flatmates);
    }
}

public interface ISplitStrategy
{
    Dictionary<int, decimal> ExecuteSplit(decimal totalAmount, List<Flatmate> flatmates);
}

public interface INotifiable
{
    Task SendOverdueAlertAsync(int flatmateId, decimal amountOwed, string billTitle);
}

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
}
```

---



## 6. NUnit Test Strategy

The test suite in ezyhouse.Tests verifies backend financial logic using NUnit

Rent Conversion Tests: Ensures fortnightly rent amounts convert to calendar monthly figures accurately without rounding errors

Split Strategy Tests: Validates that RoomSizeSplitStrategy allocates exact percentages totaling 100%

Debt Simplification Tests: Verifies that a 4-person debt matrix with multiple cross-payments simplifies into minimum net payments

---

