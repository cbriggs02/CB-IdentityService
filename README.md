# IdentityServiceApi

A secure, modular RESTful API built with ASP.NET Core Web API for identity management, authentication, and role-based access control. The project is designed with scalability, maintainability, and modern DevOps practices in mind, making it suitable for both development and production environments.

---

## Technology Stack

- **Framework:** ASP.NET Core Web API (.NET 6 LTS)
- **ORM:** Entity Framework Core 6.x (SQL Server)
- **Authentication:** ASP.NET Identity with JWT
- **API Documentation:** Swagger UI
- **Testing:** xUnit for unit and integration tests
- **Containerization:** Docker support included

---

## Overview

This API enables secure communication between clients and the server over HTTPS using standard web protocols. It follows a clean, modular architecture that separates responsibilities across well-defined layers:

* **Interfaces** – Define contracts for application services
* **Services** – Contain business logic and core functionality
* **Controllers** – Handle HTTP requests and API routing
* **Models & DTOs** – Represent domain entities and data transfer objects

---

## Clients / Consumers

This API is consumed by the following frontend application(s):

* [Admin Portal (Angular)](https://github.com/cbriggs02/CB-IdentityAdminPortal)

---

## System Design

* **Modular Architecture** – Clear separation of concerns across layers
* **Entity Framework Core** – Code-first approach with SQL Server
* **Dependency Injection** – Built-in DI for flexibility and testability
* **Swagger UI** – Interactive API documentation and testing

---

## Authentication & Authorization

* **ASP.NET Identity** for user and role management
* **JWT (JSON Web Tokens)** for stateless authentication
* **Role-Based Access Control (RBAC)** for secure resource access
* **Password Security** including hashing, validation, and history tracking

---

## Testing

* **xUnit** for unit and integration testing

  * *Unit Tests*: Validate business logic in isolation
  * *Integration Tests*: Verify full request pipeline (controllers, middleware, etc.)

For more details, see:
`/IdentityServiceApi.Tests/README.md`

---

## Documentation

* Swagger UI is enabled
* Available at:

  ```
  /index.html
  ```

---

## Docker Support

* Fully containerized for consistent environments
* Includes a Dockerfile for build and runtime configuration
* Supports local development and cloud deployment

---

## Configuration (appsettings.json)

The application requires configuration values to run. These can be provided via:

* `appsettings.json`
* Environment variables (recommended for Docker/production)

---

### Required Settings

#### Connection Strings

* `ApplicationDatabase` – SQL Server connection used by EF Core

#### JWT Settings

* `ValidIssuer` – API base URL
* `ValidAudience` – Client application URL
* `SecretKey` – Used to sign JWT tokens (keep secure)

---

### appsettings Template

Copy and modify the following template:

```
{
  "ConnectionStrings": {
    "ApplicationDatabase": "Server=host.docker.internal;Database=[database];User=[database_user];Password=[database_password];Encrypt=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "ValidIssuer": "https://localhost:52870",
    "ValidAudience": "https://localhost:4200",
    "SecretKey": "YOUR_SUPER_SECRET_KEY_HERE"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Webhooks": []
}
```

---

### Docker Notes

* Use `host.docker.internal` to access your local database (Windows/macOS)
* Ensure SQL Server is accessible from the container

---

## Running the Application

### Local (without Docker)

```
dotnet restore
dotnet build
dotnet run
```

---

### With Docker

```
docker-compose up --build
```

Then access:

* API: `https://localhost:52870`
* Swagger: `https://localhost:52870/index.html`

---

## 👤 Author

Christian Briglio – 2025
