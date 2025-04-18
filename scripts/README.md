# Identity Service API - Setup & Instructions
This guide provides the necessary steps to set up and run the Identity Service API in a development environment, including running the API, applying migrations, and configuring necessary environment variables.

## Prerequisites
Before you begin, ensure you have the following installed:
* Docker
* .NET SDK 6.0
* A SQL Server or Docker container with a SQL Server instance running

## Setup Instructions
### 1. Setting Up the API Project
To run the API in a local development environment, you'll need to configure your appsettings.json file with the appropriate connection strings and other settings.

Create an appsettings.Development.json or appsettings.json file in the root of the API project (if one doesn't exist) and include the following sample configuration: 

<pre> {
  "ConnectionStrings": {
    "ApplicationDatabase": "Server=localhost;Database={replace with database name};User={replace with username};Password={replace with password};Encrypt=true;TrustServerCertificate=true;",
    "HealthChecksDatabase": "Server=localhost;Database={replace with database name};User={replace with username};Password={replace with password};Encrypt=true;TrustServerCertificate=true;"
  },
  "JwtSettings": {
    "ValidIssuer": "https://localhost:52870",
    "ValidAudience": "https://localhost:3000",
    "SecretKey": "{replace with your 32 bit secret key}"
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
}  </pre>

* ConnectionStrings: Update the ApplicationDatabase and HealthChecksDatabase connection strings to match your local SQL Server or Docker-based instance.
* JwtSettings: The SecretKey must be a 32-byte string. A sample key could be:
"your32ByteSecretKeyHere1234567890123456"
You can use a tool to generate a 32-byte key for better security.

### 2. Running the Development Environment
Once the appsettings.json file is configured, you're ready to run the development environment. The following scripts will help you get started.

#### Running the Development Environment
1. Run API Development with Docker <br />
&ensp;You need Docker running to launch the API with all necessary dependencies.
* Open a terminal window.
* Run the following script: <br />
&ensp;run-api-dev.bat <br />

This will:
* Start the Docker container for the development environment (if using Docker).
* Run the application locally via Docker. <br />
#### Note: Docker must be installed and running on your machine to use this script.

#### Running the API Tests
To run unit and integration tests for the API, use the run-api-tests.bat script.
1. Run API Tests
* Open a terminal window.
* Run the following script: <br />
&ensp;run-api-tests.bat <br />

This will:
* Execute all tests for the API using the configured testing framework.

### 3. Applying Migrations
To ensure the database schema is up to date, you need to run migrations. The migrations are applied using the dotnet-ef CLI.
#### Apply Migrations Script
1. Create and Apply Migrations The batch script apply-migrations.bat handles this process. This script will generate migrations and apply them to both the ApplicationDbContext and HealthChecksDbContext.
* Open a terminal window.
* Run the following script: <br />
&ensp;apply-migrations.bat <br />

The script will:
* Generate and apply migrations for both the ApplicationDbContext and HealthChecksDbContext.

## Troubleshooting
* Docker not running: If you're unable to start the Docker container, make sure Docker Desktop is installed and running. You can verify by running docker ps to check if any containers are running.
* Migrations not applying: If the migrations are not applying correctly, ensure your dotnet-ef tool is installed and that you're in the correct project directory when running the migration commands.





  
