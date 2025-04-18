# IdentityServiceApi
A secure and modular RESTful API built with ASP.NET Core Web API for identity management, user authentication, and role-based access control. Designed for scalability, maintainability, and modern DevOps practices.

## Overview
This API allows clients to communicate securely with a server via HTTPS using standard web protocols. It’s built using a modular architecture, breaking the application into:
* Interfaces – Contracts for services.
* Services – Handle business logic.
* Controllers – Manage HTTP requests and routing.
* Models & DTOs – Represent entities, request/response payloads, and transfer data between layers.


## System Design
* Modular Architecture: Separation of concerns using layers (interfaces, services, controllers, and models).
* Entity Framework Core: Communicates with MSSQL Server to manage database operations using code-first migrations.
* Dependency Injection: Services and components are injected for testability and flexibility.
* Swagger UI: API documentation and interactive testing.

## Authentication & Authorization
* ASP.NET Identity for user and role management.
* JWT (JSON Web Tokens) for stateless, secure authentication.
* Role-based Access Control (RBAC) to restrict and control access to resources.
* Password hashing, validation, and history tracking for secure password management.

## Testing
xUnit for unit and integration testing.
* Unit Tests: Validate service logic in isolation.
* Integration Tests: Ensure controllers and middleware work end-to-end.

## Documentation
Swagger UI is enabled for this API.
Interactive documentation is available at /swagger when the application is running.

## Docker Support
Fully containerized with Docker for cross-platform development and deployment.
Includes a Dockerfile to define build steps and runtime environment.
Easily deployable to cloud platforms or local environments.
