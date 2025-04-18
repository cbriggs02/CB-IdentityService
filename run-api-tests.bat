@echo off

ECHO.
ECHO.
ECHO Building...
dotnet build IdentityServiceApi.sln --configuration Debug

ECHO.
ECHO.
ECHO Running Unit Tests...
dotnet test ./IdentityServiceApi.Tests\IdentityServiceApi.Tests.csproj --no-build --configuration Debug --filter "TestCategory=UnitTest"

ECHO.
ECHO.
ECHO Press any key to continue...
pause > nul

ECHO.
ECHO.
ECHO Running Integration Tests...
dotnet test ./IdentityServiceApi.Tests\IdentityServiceApi.Tests.csproj --no-build --configuration Debug --filter "TestCategory=IntegrationTest"

ECHO.
ECHO.
echo All tests completed.

ECHO.
ECHO.
ECHO Press any key to exit...
pause > nul
@echo on