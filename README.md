# Nursery Management System API

An ASP.NET Core API for nursery operations. It handles parent registration and approval, child and staff attendance, care plans, scheduling, and monthly invoices.

## Stack

- .NET 10 and ASP.NET Core Web API
- Entity Framework Core with PostgreSQL or SQL Server
- CQRS with MediatR, ASP.NET Core Identity, and JWT bearer authentication

## Run locally

Install the .NET 10 SDK and create an empty PostgreSQL database. Run the following in PowerShell from the repository root, replacing the connection string values with your local settings:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Host=localhost;Port=5432;Database=nursery_management;Username=postgres;Password=<your-local-password>'
$env:Jwt__SecretKey = [guid]::NewGuid().ToString('N') + [guid]::NewGuid().ToString('N')
dotnet build NurseryManagementSystem.slnx
dotnet run --project NurseryManagementSystem.API --launch-profile http
```

The API listens on `http://localhost:5293`. On startup it creates the schema in an empty database and seeds the application roles. To create an initial SuperAdmin, set `AdminSeed__UserName` and `AdminSeed__Password` in your local environment before starting the API.

## API reference

- `GET /health` checks whether the API is responding.
- `GET /swagger/v1/swagger.json` serves the OpenAPI specification.
- [Client integration guide](CLIENT_INTEGRATION.md) explains authentication and response contracts.
- [Parent registration API](PARENT_REGISTRATION_API.md) documents the approval flow.

## Project structure

| Project | Responsibility |
| --- | --- |
| `NurseryManagementSystem.API` | HTTP endpoints, request validation responses, and OpenAPI |
| `NurseryManagementSystem.Application` | MediatR commands, queries, and application contracts |
| `NurseryManagementSystem.Domain` | Entities, roles, and business models |
| `NurseryManagementSystem.Infrastructure` | EF Core persistence, Identity, and JWT services |

## Deployment

- [MonsterASP.NET setup and GitHub Actions](MONSTERASP_DEPLOYMENT.md)
- [Render and Docker setup](RENDER_DEPLOYMENT.md)
