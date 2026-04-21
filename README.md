# Global Logistics Management System (GLMS)
### PROG7311 – Portfolio of Evidence Part 2 | ST10439147

A Blazor Server web application for managing logistics clients, contracts, service requests, and users, with real-time notifications and live currency conversion.

---

## Table of Contents

- [Overview](#overview)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [Running Unit Tests](#running-unit-tests)
- [Features](#features)
- [Design Patterns](#design-patterns)

---

## Overview

GLMS is a Blazor Server application that provides an interface for logistics staff to manage:

- **Clients** – Companies grouped by geographic region (EMEA, APAC, North America, etc.)
- **Contracts** – Agreements linked to clients with lifecycle status management (Draft → Active → Expired | On Hold), service levels, signed PDF agreements, and start/end dates
- **Service Requests** – Requests tied to contracts
- **Users** – Internal accounts with role-based access control
- **Currency Conversion** – Live foreign exchange rates via the OpenExchangeRates API with 10-minute caching

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 10 / Blazor Server |
| ORM | Entity Framework Core 10 (SQL Server) |
| Real-time | SignalR (`NotificationHub`) |
| HTTP resilience | Polly |
| UI | Bootstrap 5 |
| Testing | xUnit, Moq, EF Core InMemory |

---

## Project Structure

```
prog7311-poe-part2-ST10439147/
│
├── ST10439147_PRO7311_POE/          # Main application
│   ├── Components/
│   │   ├── Layout/                  # MainLayout, NavMenu, ReconnectModal
│   │   └── Pages/                   # Razor page components
│   │       ├── Home.razor
│   │       ├── Login.razor
│   │       ├── Clients.razor
│   │       ├── ContractList.razor
│   │       ├── ContractForm.razor
│   │       ├── ContractDetail.razor
│   │       ├── ServiceRequests.razor
│   │       └── Users.razor
│   ├── Controllers/                 # Web API controllers (REST endpoints)
│   │   ├── ClientsController.cs
│   │   ├── ContractController.cs
│   │   ├── CurrencyController.cs
│   │   └── UserController.cs
│   ├── Data/
│   │   ├── AppDBContext.cs          # EF Core DbContext
│   │   └── AppDbContextFactory.cs   # Design-time factory for migrations
│   ├── DTOs/                        # Data Transfer Objects
│   ├── Extensions/
│   │   └── ControllerExtensions.cs
│   ├── Hubs/
│   │   └── NotificationHub.cs       # SignalR hub
│   ├── Migrations/                  # EF Core migration files
│   ├── Models/
│   │   ├── Clients.cs               # Client entity + ClientRegion enum
│   │   ├── Contract.cs              # Contract entity + ContractStatus & ServiceLvl enums
│   │   ├── ServiceRequest.cs
│   │   └── User.cs                  # User entity + UserRole enum
│   ├── Services/
│   │   ├── AccessLevelHandler.cs    # DelegatingHandler – stamps X-Access-Level header
│   │   ├── ClientService.cs / IClientService.cs
│   │   ├── ContractService.cs / IContractService.cs
│   │   ├── FileService.cs / IFileService.cs
│   │   ├── ICurrencyConversionStrategy.cs   # Strategy interface + OpenExchangeStrategy
│   │   ├── NotificationService.cs
│   │   ├── UserService.cs / IUserService.cs
│   │   └── UserState.cs             # Scoped state (logged-in user)
│   ├── wwwroot/
│   │   ├── app.css
│   │   └── uploads/agreements/      # Uploaded signed agreement PDFs
│   ├── appsettings.json
│   └── Program.cs
│
└── ST10439147_PROG7311_POE_UnitTests/   # xUnit test project
    ├── ClientServiceTests.cs
    ├── ContractServiceTests.cs
    ├── CurrencyCalculationTests.cs
    ├── FileValidationTests.cs
    └── Helper/
        └── DbContextHelper.cs
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server LocalDB (included with Visual Studio)
- Visual Studio 2022 (v17.x or later) **or** VS Code with the C# Dev Kit extension

---

## Getting Started

1. **Clone or extract** the repository.

2. **Restore NuGet packages** (Visual Studio does this automatically on build, or run):
   ```bash
   dotnet restore
   ```

3. **Configure the connection string** – see [Configuration](#configuration) below.

4. **Apply database migrations** – see [Database Setup](#database-setup) below.

5. **Run the application** – see [Running the Application](#running-the-application) below.

---

## Configuration

All configuration lives in `ST10439147_PRO7311_POE/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ST10439147_GLMS_DB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "FxProviders": {
    "OpenExchangeRates": {
      "AppId": "<your-open-exchange-rates-app-id>"
    }
  }
}
```

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string. The default targets LocalDB. Change this for a full SQL Server instance. |
| `FxProviders:OpenExchangeRates:AppId` | API key from [openexchangerates.org](https://openexchangerates.org). If left empty, the free `open.er-api.com` endpoint is used as a fallback. |

> **Note:** For a production deployment, store secrets in environment variables or Azure Key Vault rather than `appsettings.json`.

---

## Database Setup

The project uses **EF Core migrations** to create and manage the database schema.

### Via Visual Studio Package Manager Console

```powershell
Update-Database
```

### Via .NET CLI

```bash
cd ST10439147_PRO7311_POE
dotnet ef database update
```

This applies all pending migrations and creates the `ST10439147_GLMS_DB` database (or whichever database name you set in the connection string).

To add a new migration after modifying models:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

---

## Running the Application

### Via Visual Studio

Press **F5** (Debug) or **Ctrl+F5** (Run without Debugging) with `ST10439147_PRO7311_POE` set as the startup project.

### Via .NET CLI

```bash
cd ST10439147_PRO7311_POE
dotnet run
```

The application will be available at `https://localhost:7181` (or the port configured in `Properties/launchSettings.json`).

---

## Running Unit Tests

### Via Visual Studio

Open the **Test Explorer** (`Test → Test Explorer`) and click **Run All**.

### Via .NET CLI

```bash
cd ST10439147_PROG7311_POE_UnitTests
dotnet test
```

The test project uses an **EF Core InMemory** database provider so no SQL Server connection is required to run tests. Test coverage includes:

- `ClientServiceTests` – CRUD operations on the Client service
- `ContractServiceTests` – Contract deletion rules, search/filter, and automatic status correction (expired end date → `Expired`; missing signed agreement → `Draft`)
- `CurrencyCalculationTests` – Exchange rate conversion calculations
- `FileValidationTests` – PDF upload validation logic (extension check, magic-byte check, 20 MB size limit)

---

## Features

**Client Management**
- Create, view, edit, and delete clients
- Assign each client to a geographic region (EMEA, APAC, North America, Latin America, Middle East, Sub-Saharan Africa, North Africa, Central Asia, South Asia, East Asia, Southeast Asia)

**Contract Management**
- Full contract lifecycle: Draft → Active → Expired / On Hold
- Automatic status correction on every read:
  - If a contract's `EndDate` has passed, its status is set to `Expired`
  - If a contract has no uploaded signed agreement, its status is set to `Draft`
  - `Expired` takes priority — a contract past its end date is `Expired` regardless of whether a document is present
  - Corrections are persisted to the database and broadcast via SignalR
- Service levels: Standard, Premium, Express, Economy
- Upload and store a signed agreement PDF per contract (maximum **20 MB**, PDF files only)
- Link contracts to clients with start/end dates

**Service Requests**
- Log and track service requests linked to contracts

**Currency Conversion**
- Live FX rates via OpenExchangeRates (10-minute in-memory cache)
- Convert contract values between currencies

**Real-time Notifications**
- SignalR hub (`/notificationHub`) pushes live notifications to connected clients, including automatic status correction events

---

## File Upload Rules

Signed agreement uploads are validated at two layers:

| Layer | Rule |
|---|---|
| `FileService` (business layer) | File must not be empty; extension must be `.pdf`; first 4 bytes must match the PDF magic number (`%PDF`); file must not exceed **20 MB** |
| ASP.NET / Kestrel (infrastructure) | `[RequestSizeLimit]` and `MultipartBodyLengthLimit` are set to 20 MB on the `UploadAgreement` endpoint, preventing oversized requests from being buffered before reaching application code |

---

## Contract Auto-Status Rules

`ContractService` applies the following rules whenever a contract is read (`GetContract`, `GetAllContracts`, `SearchContracts`). If the resolved status differs from the stored value, the database record is updated and a SignalR notification is broadcast.

| Condition | Resolved Status | Priority |
|---|---|---|
| `EndDate < DateTime.UtcNow` | `Expired` | 1 (highest) |
| `SignedAgreementPath` is null or empty | `Draft` | 2 |
| Neither condition applies | Stored status unchanged | — |

---

## Design Patterns

### Strategy — Currency Conversion

The `ICurrencyConversionStrategy` interface decouples the currency conversion logic from the rest of the application. Any class that implements `Convert`, `GetRate`, and `GetAvailableCurrencies` can be dropped in as the active provider by changing a single line in `Program.cs` — no other code changes required.

```
ICurrencyConversionStrategy         (interface – Services/)
        │
        └── OpenExchangeStrategy    (concrete – calls open.er-api.com or openexchangerates.org)
```

**Interface**
```csharp
// ICurrencyConversionStrategy.cs
public interface ICurrencyConversionStrategy
{
    Task<decimal> Convert(decimal amount, string fromCurrency, string toCurrency);
    Task<decimal> GetRate(string fromCurrency, string toCurrency);
    Task<List<string>> GetAvailableCurrencies();
}
```

**Concrete implementation** (`OpenExchangeStrategy`) fetches rates from the API, caches them for 10 minutes, and converts via USD as the base currency.

**Registration** — swap providers without touching any consumer:
```csharp
// Program.cs
builder.Services.AddScoped<ICurrencyConversionStrategy, OpenExchangeStrategy>();
```

---

### Observer — Real-time Notifications (via SignalR)

`ContractService` acts as the **Subject**: after every state-changing operation (create, update, delete, status change, auto-correction) it pushes a notification through `IHubContext<NotificationHub>`. Blazor pages act as **Observers** by subscribing to the `NotificationService` event and re-rendering when a message arrives — without polling.

```
ContractService  ──pushes──▶  IHubContext<NotificationHub>  ──broadcasts──▶  NotificationHub (/notificationHub)
                                                                                      │
                                                              ◀──listens──  NotificationService (per-session Scoped)
                                                                                      │
                                                              ◀──subscribes──  Blazor pages (OnMessageReceived event)
```

**Subject (ContractService)** — calls `Notify` after every mutation:
```csharp
// ContractService.cs
private async Task Notify(string message, int contractId, ContractStatus status)
{
    var payload = new ContractResponseDto { Message = message, Status = true,
        Contract = new Contract { Id = contractId, Status = status } };
    await _hubContext.Clients.All.SendAsync("ReceiveStatusUpdate", payload);
}
```

**Hub** — thin transport layer, no business logic:
```csharp
// NotificationHub.cs
public class NotificationHub : Hub
{
    public async Task SendStatusUpdate(int contractId, string status)
        => await Clients.All.SendAsync("ReceiveStatusUpdate", contractId, status);
}
```

**Observer (NotificationService)** — one SignalR connection per user session; exposes an event that Blazor pages subscribe to:
```csharp
// NotificationService.cs
public event Action<string>? OnMessageReceived;

_hubConnection.On<ContractResponseDto>("ReceiveStatusUpdate", (data) =>
{
    OnMessageReceived?.Invoke(data.Message);
});
```

---

### Factory — DbContext Creation

`IDbContextFactory<AppDbContext>` is registered in `Program.cs` so that services can create short-lived `DbContext` instances on demand using `_dbFactory.CreateDbContext()`. This is the correct pattern for Blazor Server, where a scoped `DbContext` would be shared across concurrent requests on the same circuit.

`AppDbContextFactory` is the **design-time** counterpart — it implements `IDesignTimeDbContextFactory<AppDbContext>` so the EF Core CLI tools (`dotnet ef migrations add`, `dotnet ef database update`) can construct a context without running the full application.

```
IDbContextFactory<AppDbContext>   ──registered in──▶  Program.cs  (runtime – one DbContext per operation)
AppDbContextFactory               ──used by──▶  EF Core CLI tooling only  (design-time migrations)
```

**Runtime registration:**
```csharp
// Program.cs
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

**Usage in services** — each method opens and disposes its own context:
```csharp
// ContractService.cs
public async Task<List<Contract>> GetAllContracts()
{
    using var ctx = _dbFactory.CreateDbContext();
    return await ctx.Contracts.Include(c => c.Client).ToListAsync();
}
```

**Design-time factory** — resolves the MDF path at CLI invocation time so migrations work on any machine without manual configuration:
```csharp
// AppDbContextFactory.cs
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var mdfPath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "ST10439147_PRO7311_POE.mdf");
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer($"...AttachDbFilename={mdfPath}...")
            .Options;
        return new AppDbContext(options);
    }
}
```

---

### Additional Patterns

| Pattern | Where Used |
|---|---|
| **Repository / Service Layer** | `IClientService`, `IContractService`, `IUserService`, `IFileService` abstract all data access from Blazor pages |
| **Delegation (HTTP Handler)** | `AccessLevelHandler` – a `DelegatingHandler` that automatically stamps the `X-Access-Level` header on every outgoing API request based on the currently logged-in user |

---

*PROG7311 – Portfolio of Evidence Part 2 | Student Number: ST10439147*
