@echo off
ECHO Checking if dotnet-ef is installed...

:: Check if dotnet-ef is installed by running `dotnet ef --version`
dotnet ef --version >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    ECHO dotnet-ef not found. Installing dotnet-ef...
    dotnet tool install --global dotnet-ef
    IF %ERRORLEVEL% NEQ 0 (
        ECHO Failed to install dotnet-ef. Please make sure you have the .NET SDK installed.
        pause
        EXIT /B 1
    )
    ECHO dotnet-ef installed successfully.
    
    :: Restart the script to apply dotnet-ef changes
    ECHO Restarting the script to apply the installed dotnet-ef tool...
    call "%~f0"
    EXIT /B
) ELSE (
    ECHO dotnet-ef is already installed.
)

ECHO.
ECHO Navigating to the API project folder...
:: Check if the folder exists
IF EXIST "%~dp0..\IdentityServiceApi\IdentityServiceApi.csproj" (
    cd /d "%~dp0..\IdentityServiceApi"
    ECHO Successfully navigated to IdentityServiceApi folder.
) ELSE (
    ECHO ERROR: IdentityServiceApi folder not found. Please check the path.
    pause
    EXIT /B 1
)

ECHO Creating migration for ApplicationDbContext...
dotnet ef migrations add InitialCreate_ApplicationDbContext --context ApplicationDbContext

ECHO.
ECHO.
ECHO Applying migrations for ApplicationDbContext...
dotnet ef database update --context ApplicationDbContext

ECHO.
ECHO.
ECHO Creating migration for HealthChecksDbContext...
dotnet ef migrations add InitialCreate_HealthChecksDbContext --context HealthChecksDbContext

ECHO.
ECHO.
ECHO Applying migrations for HealthChecksDbContext...
dotnet ef database update --context HealthChecksDbContext

ECHO.
ECHO.
ECHO All migrations applied!
pause
@echo on