# EMS Mini Project 2

Production-style Employee Management System built with:

- Backend: .NET 8 Web API
- ORM: Entity Framework Core (Code First)
- Database: SQL Server 2022
- Authentication: JWT + Role-Based Authorization
- Password Hashing: BCrypt.Net-Next
- API Docs: Swagger / OpenAPI
- Frontend: HTML, CSS, Bootstrap 5, Vanilla JavaScript, jQuery
- Testing: NUnit, Moq, EF Core InMemory

## Project Structure

```text
EMS-MiniProject2/
|-- EMS.API/
|-- EMS.Tests/
`-- frontend/
```

## NuGet Packages

### EMS.API

- `BCrypt.Net-Next`
- `Microsoft.AspNetCore.Authentication.JwtBearer`
- `Microsoft.AspNetCore.OpenApi`
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.SqlServer`
- `Swashbuckle.AspNetCore`

### EMS.Tests

- `coverlet.collector`
- `Microsoft.EntityFrameworkCore.InMemory`
- `Microsoft.NET.Test.Sdk`
- `Moq`
- `NUnit`
- `NUnit.Analyzers`
- `NUnit3TestAdapter`

## Setup Instructions

1. Make sure SQL Server 2022 is running and update `EMS.API/appsettings.json` if your server name or credentials differ.
2. Restore dependencies:

   ```bash
   dotnet restore EMS-MiniProject2.slnx
   ```

3. Run the API:

   ```bash
   ./scripts/run-api.sh
   ```

4. Open Swagger:

   - `https://localhost:7057/swagger`
   - `http://localhost:5160/swagger`

5. Serve the frontend from the `frontend` folder using any static server, for example:

   ```bash
   ./scripts/run-frontend.sh
   ```

6. Open:

   - `http://localhost:5500`

7. Run tests:

   ```bash
   dotnet test EMS-MiniProject2.slnx
   ```

## Seed Data

- Admin user: `admin / admin123`
- Viewer user: `viewer / viewer123`
- 15 employees across Engineering, HR, Finance, Sales, Marketing, Support, and Operations

## API Endpoints

### Authentication

- `POST /api/auth/register`
- `POST /api/auth/login`

### Employees

- `GET /api/employees`
- `GET /api/employees/{id}`
- `POST /api/employees`
- `PUT /api/employees/{id}`
- `DELETE /api/employees/{id}`
- `GET /api/employees/dashboard`

## Notes

- JWT is held only in frontend memory and is cleared on refresh.
- Viewer users get read-only UI and API authorization.
- The API seeds data automatically on first run.
- The frontend defaults to `http://localhost:5160/api` to avoid local HTTPS certificate trust issues during development.
- If your default `dotnet` command points to another installation, `./scripts/run-api.sh` prefers the Homebrew .NET 8 host at `/opt/homebrew/opt/dotnet@8/libexec/dotnet` when available.
