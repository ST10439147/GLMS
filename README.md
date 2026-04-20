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
- [User Roles & Access Levels](#user-roles--access-levels)

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
- `ContractServiceTests` – Contract creation, retrieval, and status logic
- `CurrencyCalculationTests` – Exchange rate conversion calculations
- `FileValidationTests` – PDF upload validation logic

---

## Features

**Client Management**
- Create, view, edit, and delete clients
- Assign each client to a geographic region (EMEA, APAC, North America, Latin America, Middle East, Sub-Saharan Africa, North Africa, Central Asia, South Asia, East Asia, Southeast Asia)

**Contract Management**
- Full contract lifecycle: Draft → Active → Expired / On Hold
- Service levels: Standard, Premium, Express, Economy
- Upload and store a signed agreement PDF per contract
- Link contracts to clients with start/end dates

**Service Requests**
- Log and track service requests linked to contracts

**Currency Conversion**
- Live FX rates via OpenExchangeRates (10-minute in-memory cache)
- Convert contract values between currencies

**Real-time Notifications**
- SignalR hub (`/notificationHub`) pushes live notifications to connected clients

---

## Design Patterns

| Pattern | Where Used |
|---|---|
| **Strategy** | `ICurrencyConversionStrategy` – swap FX providers by changing one line in `Program.cs` |
| **Repository / Service Layer** | `IClientService`, `IContractService`, `IUserService`, `IFileService` abstract data access from Blazor pages |
| **Delegation (HTTP Handler)** | `AccessLevelHandler` – a `DelegatingHandler` that automatically stamps the `X-Access-Level` header on every outgoing API request based on the currently logged-in user |
| **Factory** | `AppDbContextFactory` – design-time EF Core context factory required for migrations in a hosted Blazor Server app |


---

*PROG7311 – Portfolio of Evidence Part 2 | Student Number: ST10439147*
