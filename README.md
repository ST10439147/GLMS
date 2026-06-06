# Global Logistics Management System (GLMS)
### PROG7311 – Portfolio of Evidence Part 3 | ST10439147

A full-stack logistics management platform built with a Blazor Server frontend and a dedicated ASP.NET Core REST API backend, containerised with Docker Compose.

---

## Table of Contents

- [Overview](#overview)
- [Login Credentials](#login-details)
- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [Running with Docker](#running-with-docker)
- [Running Unit Tests](#running-unit-tests)
- [API Reference](#api-reference)
- [Authentication](#authentication)
- [Features](#features)
- [Design Patterns](#design-patterns)

---

## Overview

GLMS provides logistics staff with an interface to manage:

- **Clients** – Companies grouped by geographic region (EMEA, APAC, North America, etc.)
- **Contracts** – Agreements linked to clients with lifecycle status management (Draft → Active → Expired | On Hold), service levels, signed PDF agreements, and start/end dates
- **Service Requests** – Requests tied to contracts
- **Users** – Internal accounts with role-based access control (Admin, Manager, Viewer)
- **Currency Conversion** – Live foreign exchange rates via the OpenExchangeRates API with 10-minute caching

---

## Login Details
- UserName : admin
- Password: Admin123@

---

## Architecture

Part 3 introduces a clean frontend/backend split and Docker containerisation:

```
┌──────────────────────────────────────────────────────────────────────┐
│  Browser                                                             │
│      │  Blazor Server (interactive server-side rendering)           │
│      └──▶  glms-frontend-web  :5001  (ST10439147_PRO7311_POE)       │
│                │  HTTP + JWT                                         │
│                ▼                                                     │
│           glms-backend-api  :5000  (ST10439147_PROG7311_Back)        │
│                │  EF Core                                            │
│                ▼                                                     │
│           sql-server-db  :1433  (SQL Server 2022)                    │
└──────────────────────────────────────────────────────────────────────┘
```

| Container | Image / Project | Port |
|---|---|---|
| `sql-server-db` | `mcr.microsoft.com/mssql/server:2022-latest` | 1433 |
| `glms-backend-api` | `ST10439147_PROG7311_Back` (Dockerfile) | 5000 → 8080 |
| `glms-frontend-web` | `ST10439147_PRO7311_POE` (Dockerfile) | 5001 → 8080 |

The frontend holds **no business logic or data layer** — it calls the backend REST API for all operations. The `AccessLevelHandler` automatically stamps the `X-Access-Level` header on every outgoing request based on the logged-in user's role.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Frontend | ASP.NET Core 10 / Blazor Server (interactive server render mode) |
| Backend | ASP.NET Core 10 Web API |
| ORM | Entity Framework Core 10 (SQL Server) |
| Authentication | JWT Bearer tokens (HmacSha256, 8-hour expiry) |
| Real-time | SignalR (`NotificationHub`) |
| HTTP resilience | Polly |
| API docs | Swagger / OpenAPI (always-on, available at `/swagger`) |
| UI | Bootstrap 5 |
| Containerisation | Docker / Docker Compose |
| Testing | xUnit, Moq, EF Core InMemory |

---

## Project Structure

```
prog7311-poe-part2-ST10439147/
│
├── ST10439147_PRO7311_POE/              # Blazor Server frontend
│   ├── Components/
│   │   ├── Layout/                      # MainLayout, NavMenu, ReconnectModal
│   │   └── Pages/                       # Razor page components
│   │       ├── Home.razor
│   │       ├── Login.razor
│   │       ├── Clients.razor
│   │       ├── ContractList.razor
│   │       ├── ContractForm.razor
│   │       ├── ContractDetail.razor
│   │       ├── ServiceRequests.razor
│   │       └── Users.razor
│   ├── DTOs/                            # Shared DTOs (LoginDTO, ContractDTO, etc.)
│   ├── Models/                          # Shared model types (Client, Contract, User, etc.)
│   ├── Services/
│   │   ├── AccessLevelHandler.cs        # DelegatingHandler – stamps X-Access-Level header
│   │   └── UserState.cs                 # Scoped state: logged-in user + JWT token
│   ├── Dockerfile
│   ├── appsettings.json
│   └── Program.cs
│
├── ST10439147_PROG7311_Back/            # ASP.NET Core Web API backend
│   ├── Controllers/
│   │   ├── AuthController.cs            # POST /api/auth/login  →  returns JWT
│   │   ├── ClientsController.cs
│   │   ├── ContractController.cs
│   │   ├── CurrencyController.cs
│   │   └── UserController.cs
│   ├── Data/
│   │   ├── AppDBContext.cs
│   │   └── AppDbContextFactory.cs       # Design-time factory for EF migrations
│   ├── DTOs/
│   ├── Extensions/
│   ├── Hubs/
│   │   └── NotificationHub.cs           # SignalR hub
│   ├── Migrations/
│   ├── Models/
│   ├── Services/
│   │   ├── ClientService.cs / IClientService.cs
│   │   ├── ContractService.cs / IContractService.cs
│   │   ├── FileService.cs / IFileService.cs
│   │   ├── ICurrencyConversionStrategy.cs   # Strategy interface + OpenExchangeStrategy
│   │   ├── NotificationService.cs
│   │   └── UserService.cs / IUserService.cs
│   ├── Dockerfile
│   ├── appsettings.json
│   └── Program.cs
│
├── ST10439147_PROG7311_POE_UnitTests/   # xUnit test project
│   ├── ClientServiceTests.cs
│   ├── ContractServiceTests.cs
│   ├── CurrencyCalculationTests.cs
│   ├── FileValidationTests.cs
│   └── Helper/
│       └── DbContextHelper.cs
│
└── docker-compose.yml
```

---

## Prerequisites

### Local development
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server or SQL Server LocalDB (included with Visual Studio)
- Visual Studio 2022 (v17.x or later) **or** VS Code with the C# Dev Kit extension

### Docker deployment
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (includes Docker Compose)

---

## Getting Started

1. **Clone or extract** the repository.

2. **Restore NuGet packages** (Visual Studio does this automatically on build, or run):
   ```bash
   dotnet restore
   ```

3. **Configure secrets** – see [Configuration](#configuration) below.

4. **Choose how to run:**
   - For Docker (recommended): see [Running with Docker](#running-with-docker)
   - For local development: see [Database Setup](#database-setup) and [Running the Application](#running-the-application)

---

## Configuration

### Backend (`ST10439147_PROG7311_Back/appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ST10439147_GLMS_DB;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "Key": "<your-256-bit-secret-key>",
    "Issuer": "glms-backend-api",
    "Audience": "glms-frontend-web"
  },
  "FxProviders": {
    "OpenExchangeRates": {
      "AppId": "<your-open-exchange-rates-app-id>"
    }
  }
}
```

### Frontend (`ST10439147_PRO7311_POE/appsettings.json`)

```json
{
  "ApiBaseUrl": "https://localhost:7055"
}
```

| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string. The default targets LocalDB. |
| `Jwt:Key` | HS256 signing key (minimum 32 characters). Must match between backend config and Docker environment. |
| `Jwt:Issuer` / `Jwt:Audience` | Token issuer and audience. Must match between `appsettings.json` and JWT validation config. |
| `FxProviders:OpenExchangeRates:AppId` | API key from [openexchangerates.org](https://openexchangerates.org). If left empty, the free `open.er-api.com` endpoint is used as a fallback. |
| `ApiBaseUrl` | Base URL the frontend uses to reach the backend. Override with an environment variable in Docker. |

> **Note:** For production deployments, store secrets in environment variables or Azure Key Vault rather than `appsettings.json`.

---

## Database Setup

The project uses **EF Core migrations** to create and manage the database schema. Migrations are applied automatically on startup in production (Docker). For local development:

### Via Visual Studio Package Manager Console

```powershell
# Set default project to ST10439147_PROG7311_Back
Update-Database
```

### Via .NET CLI

```bash
cd ST10439147_PROG7311_Back
dotnet ef database update
```

To add a new migration after modifying models:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

---

## Running the Application

### Via Visual Studio

Set **multiple startup projects** (right-click solution → Properties):
1. `ST10439147_PROG7311_Back` — starts the API on `https://localhost:7055`
2. `ST10439147_PRO7311_POE` — starts the frontend on `https://localhost:7181`

Then press **F5** or **Ctrl+F5**.

### Via .NET CLI

Open two terminals:

**Terminal 1 – Backend API**
```bash
cd ST10439147_PROG7311_Back
dotnet run
# API available at https://localhost:7055
# Swagger UI at https://localhost:7055/swagger
```

**Terminal 2 – Frontend**
```bash
cd ST10439147_PRO7311_POE
dotnet run
# App available at https://localhost:7181
```

---

## Running with Docker

Docker Compose starts all three services (SQL Server, backend API, frontend) with a single command.

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| Frontend | http://localhost:5001 |
| Backend API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |
| SQL Server | localhost:1433 |

**Stopping and removing containers:**
```bash
docker compose down
```

**Removing all data (including the database volume):**
```bash
docker compose down -v
```

> The backend container waits for SQL Server to pass its health check before starting, then runs `dotnet ef database migrate` automatically on first boot.

---

## Running Unit Tests

### Via Visual Studio

Open **Test Explorer** (`Test → Test Explorer`) and click **Run All**.

### Via .NET CLI

```bash
cd ST10439147_PROG7311_POE_UnitTests
dotnet test
```

The test project uses an **EF Core InMemory** database provider — no SQL Server connection is required. Coverage includes:

- `ClientServiceTests` – CRUD operations on the Client service
- `ContractServiceTests` – Contract deletion rules, search/filter, and automatic status correction (expired end date → `Expired`; missing signed agreement → `Draft`)
- `CurrencyCalculationTests` – Exchange rate conversion calculations
- `FileValidationTests` – PDF upload validation logic (extension check, magic-byte check, 20 MB size limit)

---

## API Reference

The backend exposes a fully documented REST API. Browse the interactive Swagger UI at `/swagger` (always enabled).

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Authenticate and receive a JWT token |
| `GET` | `/api/clients` | List all clients |
| `POST` | `/api/clients` | Create a client |
| `PUT` | `/api/clients/{id}` | Update a client |
| `PATCH` | `/api/clients/{id}` | Partially update a client |
| `DELETE` | `/api/clients/{id}` | Delete a client |
| `GET` | `/api/contract` | List all contracts |
| `GET` | `/api/contract/{id}` | Get a contract by ID |
| `POST` | `/api/contract` | Create a contract |
| `PUT` | `/api/contract/{id}` | Update a contract |
| `PATCH` | `/api/contract/{id}` | Partially update a contract |
| `DELETE` | `/api/contract/{id}` | Delete a contract |
| `GET` | `/api/contract/search` | Search contracts by date range and/or status |
| `POST` | `/api/contract/{id}/upload-agreement` | Upload a signed agreement PDF |
| `POST` | `/api/contract/service-request` | Create a service request |
| `PATCH` | `/api/contract/service-request/{id}/status` | Update service request status |
| `DELETE` | `/api/contract/service-request/{id}` | Delete a service request |
| `GET` | `/api/currency/rates` | Get available currencies and rates |
| `GET` | `/api/currency/convert` | Convert an amount between currencies |
| `GET` | `/api/user` | List all users |
| `GET` | `/api/user/{id}` | Get a user by ID |
| `POST` | `/api/user` | Create or update a user |
| `PATCH` | `/api/user/{id}` | Partially update a user |
| `DELETE` | `/api/user/{id}` | Delete a user |

All endpoints (except `/api/auth/login`) require the `X-Access-Level` header:

| Value | Role |
|---|---|
| `1` | Admin |
| `2` | Manager |
| `3` | Viewer |

---

## Authentication

GLMS uses **JWT Bearer authentication**.

1. The frontend calls `POST /api/auth/login` with username and password.
2. On success, the backend returns a signed JWT (HS256, 8-hour expiry) and the user profile.
3. The token is stored in the `UserState` scoped service (`UserState.JwtToken`).
4. The `AccessLevelHandler` DelegatingHandler automatically attaches the `X-Access-Level` header to every subsequent API request based on the logged-in user's role.

```
Login page  →  POST /api/auth/login  →  JWT token
                                              │
                                     UserState.JwtToken
                                              │
                               AccessLevelHandler stamps
                               X-Access-Level on every
                               outgoing HttpClient request
```

---

## Features

**Client Management**
- Create, view, edit, and delete clients
- Assign each client to a geographic region (EMEA, APAC, North America, Latin America, Middle East, Sub-Saharan Africa, North Africa, Central Asia, South Asia, East Asia, Southeast Asia)

**Contract Management**
- Full contract lifecycle: Draft → Active → Expired / On Hold
- Automatic status correction on every read:
  - If a contract's `EndDate` has passed → `Expired` (highest priority)
  - If a contract has no uploaded signed agreement → `Draft`
  - Corrections are persisted to the database and broadcast via SignalR
- Service levels: Standard, Premium, Express, Economy
- Upload and store a signed agreement PDF per contract (maximum **20 MB**, PDF files only)
- Link contracts to clients with start/end dates

**Service Requests**
- Log and track service requests linked to contracts
- Service requests can only be created against `Active` contracts

**Currency Conversion**
- Live FX rates via OpenExchangeRates (10-minute in-memory cache)
- Convert contract values between currencies

**Real-time Notifications**
- SignalR hub (`/notificationHub`) pushes live notifications to connected clients on all create, update, delete, and auto-correction events

---

## File Upload Rules

Signed agreement uploads are validated at two layers:

| Layer | Rule |
|---|---|
| `FileService` (business layer) | File must not be empty; extension must be `.pdf`; first 4 bytes must match the PDF magic number (`%PDF`); file must not exceed **20 MB** |
| ASP.NET / Kestrel (infrastructure) | `[RequestSizeLimit]` and `MultipartBodyLengthLimit` are set to 20 MB on the upload endpoint |

---

## Contract Auto-Status Rules

`ContractService` applies the following rules whenever a contract is read. If the resolved status differs from the stored value, the record is updated and a SignalR notification is broadcast.

| Condition | Resolved Status | Priority |
|---|---|---|
| `EndDate < DateTime.UtcNow` | `Expired` | 1 (highest) |
| `SignedAgreementPath` is null or empty | `Draft` | 2 |
| Neither condition applies | Stored status unchanged | — |

---

## Design Patterns

### Strategy — Currency Conversion

`ICurrencyConversionStrategy` decouples conversion logic from the rest of the application. Swap providers by changing a single registration in `Program.cs`.

```
ICurrencyConversionStrategy         (interface – Services/)
        │
        └── OpenExchangeStrategy    (concrete – calls open.er-api.com or openexchangerates.org)
```

```csharp
// Program.cs (backend)
builder.Services.AddScoped<ICurrencyConversionStrategy, OpenExchangeStrategy>();
```

---

### Observer — Real-time Notifications (via SignalR)

`ContractService` and `UserService` act as **Subjects**: after every state-changing operation they push a notification through `IHubContext<NotificationHub>`. Blazor pages act as **Observers** by subscribing to the `NotificationService` event and re-rendering without polling.

```
ContractService  ──pushes──▶  IHubContext<NotificationHub>  ──broadcasts──▶  /notificationHub
                                                                                      │
                                                              ◀──listens──  NotificationService (scoped)
                                                                                      │
                                                              ◀──subscribes──  Blazor pages
```

---

### Factory — DbContext Creation

`IDbContextFactory<AppDbContext>` is registered in `Program.cs` so that services create short-lived `DbContext` instances per operation — the correct pattern for Blazor Server where a scoped context would be shared across concurrent requests on the same circuit.

`AppDbContextFactory` is the **design-time** counterpart used exclusively by EF Core CLI tooling (`dotnet ef migrations add`, etc.).

```csharp
// Program.cs (backend)
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(connectionString));
```

---

### Delegation (HTTP Handler) — Access Level Propagation

`AccessLevelHandler` is a `DelegatingHandler` registered on the frontend's `HttpClient`. It reads the logged-in user's access level from `UserState` and stamps the `X-Access-Level` header on every outgoing API request — no individual call site needs to do this manually.

---

### Additional Patterns

| Pattern | Where Used |
|---|---|
| **Repository / Service Layer** | `IClientService`, `IContractService`, `IUserService`, `IFileService` abstract all data access from controllers |
| **JWT Authentication** | `AuthController` issues signed tokens; all subsequent API calls are authenticated via the `Bearer` scheme |

---

*PROG7311 – Portfolio of Evidence Part 3 | Student Number: ST10439147*
