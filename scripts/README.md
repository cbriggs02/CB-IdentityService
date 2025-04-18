# Identity Service API - Setup & Instructions
This guide provides the necessary steps to set up and run the Identity Service API in a development environment, including running the API, applying migrations, and configuring necessary environment variables.

# Prerequisites
Before you begin, ensure you have the following installed:
* Docker
* .NET SDK 6.0
* A SQL Server or Docker container with a SQL Server instance running

# Setup Instructions

## 1. Configuring the API Project

To run the API locally, create a configuration file with the correct settings.

### Step 1: Create `appsettings.Development.json` or `appsettings.json`

Place it in the **root of the API project**. Use the following sample template:

```json
{
  "ConnectionStrings": {
    "ApplicationDatabase": "Server=localhost;Database={replace with database name};User={replace with username};Password={replace with password};Encrypt=true;TrustServerCertificate=true;",
    "HealthChecksDatabase": "Server=localhost;Database={replace with database name};User={replace with username};Password={replace with password};Encrypt=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "ValidIssuer": "https://localhost:52870",
    "ValidAudience": "https://localhost:3000",
    "SecretKey": "{replace with your 32-byte secret key}"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "API Health Check",
        "Uri": "https://localhost:52870/health"
      },
      {
        "Name": "Database Health Check",
        "Uri": "https://localhost:52870/health/database"
      }
    ],
    "UI": {
      "Uri": "https://localhost:52870/health-ui"
    },
    "Storage": {
      "ConnectionString": "Server=localhost;Database={replace with database name};User={replace with username};Password={replace with password};Encrypt=true;TrustServerCertificate=true;"
    },
    "Webhooks": []
  }
}
```

### Notes:

- **Connection Strings**: Replace the values with your local or Docker SQL Server credentials.
- **Secret Key**: Must be a **32-byte string** (e.g., `"your32ByteSecretKeyHere1234567890123456"`).
- 🔐 You can use a password generator tool to securely create a random 32-byte key.

---

## 2. Running the Development Environment

Before starting, ensure **Docker is installed and the Docker Engine is running**.

### Running the API with Docker

1. Open a terminal window.
2. Run:

```bash
run-api-dev.bat
```

This will:

- Start required Docker containers for the API.
- Launch the application in a local development environment.

> **Tip**: Ensure Docker Desktop is running before executing the script.

---

## 3. Running API Tests

Run unit and integration tests using the included script:

1. Open a terminal.
2. Run:

```bash
run-api-tests.bat
```

This will:

- Execute all configured tests using the test runner.

---

## 4. Applying Entity Framework Migrations

Use the provided script to generate and apply migrations for the databases.

### Running Migration Script

1. Open a terminal window.
2. Run:

```bash
apply-migrations.bat
```

This script will:

- Automatically create migrations (if needed).
- Apply migrations to both:
  - `ApplicationDbContext`
  - `HealthChecksDbContext`

> **Note**: If `dotnet-ef` is not installed the first time, the script will install it. You may need to run the script again after installation.

## Troubleshooting
* Docker not running: If you're unable to start the Docker container, make sure Docker Desktop is installed and running. You can verify by running docker ps to check if any containers are running.
* Migrations not applying: If the migrations are not applying correctly, ensure your dotnet-ef tool is installed and that you're in the correct project directory when running the migration commands.

---

##  Author

Christian Briglio – 2025



  
