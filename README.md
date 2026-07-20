# Identity Web Service

[![Board Status](https://dev.azure.com/chrisbriglio/921b1b04-8655-415f-8bcf-4b5b235431d1/c1ed23dd-9fde-4a79-880d-0555945b5d49/_apis/work/boardbadge/694f5a15-99d9-486b-a2b0-b56bd56925c1)](https://dev.azure.com/chrisbriglio/921b1b04-8655-415f-8bcf-4b5b235431d1/_boards/board/t/c1ed23dd-9fde-4a79-880d-0555945b5d49/Backlog%20items/)

Identity Web Service is a secure, modular backend platform built with ASP.NET Core. The solution currently consists of two APIs:

- **Identity Service Gateway API** – The public entry point that routes client requests to backend services and provides a single access point to the platform.
- **Identity Service API** – Responsible for authentication, authorization, user management, and role-based access control.

The project is designed with scalability, maintainability, and modern DevOps practices in mind, making it suitable for both development and production environments.

---

## Technology Stack

- **Framework:** ASP.NET Core Web API (.NET 10.x)
- **ORM:** Entity Framework Core 10.x (SQL Server)
- **Authentication:** ASP.NET Identity with JWT
- **API Documentation:** Swagger UI
- **Testing:** xUnit for unit and integration tests
- **Containerization:** Docker support included

---

## CI/CD & DevOps

This project incorporates modern CI/CD and DevOps practices to ensure code quality, security, and deployment readiness:

- GitHub Actions – Continuous Integration

  - Automatically restores, builds, and runs unit tests on every push and pull request
  - Ensures code changes are validated before being merged

- Docker – Continuous Delivery

  - Builds a Docker image automatically on merges to the main branch
  - Enables consistent, environment-independent deployment

- CodeQL – Security Analysis

  - Performs automated static code analysis to identify potential vulnerabilities and code quality issues
  - Runs on pull requests, pushes, and scheduled scans

- Azure DevOps Boards – Project Management

  - Used for task tracking, backlog management, and agile workflow organization

---

## Overview

Identity Web Service provides secure communication between clients and backend services over HTTPS using standard web protocols.

The solution currently consists of:

- **Identity Service Gateway API** – Routes incoming client requests to the appropriate backend services and acts as the single entry point into the platform.
- **Identity Service API** – Handles authentication, authorization, identity management, and role-based access control.

The backend services follow a **feature-based architecture**, where each feature encapsulates the components required to implement a specific area of functionality. This improves modularity, maintainability, and scalability by keeping related code together.

A typical feature may include:

- **Controllers** – Handle HTTP requests and API routing.
- **Services** – Implement business logic.
- **Models & DTOs** – Represent domain entities and data transfer objects.
- **Interfaces** – Define contracts where abstraction or dependency injection is required.

Shared utilities, infrastructure, and cross-cutting concerns are located in common project areas outside individual features.

---

## System Design

* **API Gateway** – Centralized routing for backend services
* **Feature-Based Architecture** – Organizes code by business capability
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
`/tests/README.md`

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
