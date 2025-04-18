@echo off

ECHO =======================================================================================================================
ECHO  		WELCOME TO THE IDENTITY SERVICE WEB API DEV ENV BUILT USING THE .NET CORE FRAMEWORK
ECHO =======================================================================================================================
ECHO.
ECHO.
ECHO.
ECHO =======================================================================================================================
ECHO  		This script will build the API project and allow you to interact with it via the Swagger UI.
ECHO.
ECHO  		WARNING: The Swagger UI will be accessible on port 52870 but may take a moment to be ready.
ECHO.
ECHO  		WARNING: You must have DOCKER downloaded and the DOCKER ENGINE RUNNING on your DESKTOP.
ECHO. 
ECHO  		Swagger UI will only be accessible after the Docker container is fully running.
ECHO =======================================================================================================================
ECHO.
ECHO.
ECHO.
ECHO Press any key to continue...
pause >nul

ECHO.
ECHO.
ECHO Building Docker images...
docker-compose build

ECHO.
ECHO.
ECHO Opening Swagger UI (may not be ready yet)...
start https://localhost:52870/index.html

ECHO.
ECHO.
ECHO ========================================================================================================================
ECHO  		The Docker containers are about to start. To stop the process, press Ctrl+C in the command prompt.
ECHO  		Alternatively, you can close the command prompt window to exit the batch job.
ECHO  		The Swagger UI will be accessible once the containers are running.
ECHO ========================================================================================================================

ECHO.
ECHO.
ECHO Starting containers in detached mode...
docker-compose up -d

ECHO.
ECHO.
ECHO ========================================================================================================================
ECHO  		The Docker containers are running in the background.
ECHO  		To view logs, you can use the command: docker-compose logs
ECHO  		To stop the containers, use: docker-compose down
ECHO ========================================================================================================================
ECHO.
ECHO.
ECHO Press any key to exit the batch script...
pause >nul

ECHO Stopping Docker containers...
docker-compose down
@echo on