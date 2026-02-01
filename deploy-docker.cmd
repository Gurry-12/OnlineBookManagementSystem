@echo off
echo ========================================
echo   WHISPERING PAGES - DOCKER DEPLOYMENT
echo ========================================

cd OnlineBookManagementSystem

echo.
echo [1/5] Checking Docker installation...
docker --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker is not installed or not in PATH
    echo Please install Docker Desktop from https://www.docker.com/products/docker-desktop
    pause
    exit /b 1
)
echo ✓ Docker is available

echo.
echo [2/5] Creating environment file...
if not exist .env (
    copy .env.example .env
    echo ✓ Created .env file from template
    echo WARNING: Please edit .env file with your actual values before continuing
    echo Press any key to continue after editing .env file...
    pause
) else (
    echo ✓ .env file already exists
)

echo.
echo [3/5] Building Docker images...
docker-compose build
if errorlevel 1 (
    echo ERROR: Failed to build Docker images
    pause
    exit /b 1
)
echo ✓ Docker images built successfully

echo.
echo [4/5] Starting services...
docker-compose up -d
if errorlevel 1 (
    echo ERROR: Failed to start services
    pause
    exit /b 1
)
echo ✓ Services started successfully

echo.
echo [5/5] Checking service health...
timeout /t 10 /nobreak >nul
docker-compose ps

echo.
echo ========================================
echo   DEPLOYMENT COMPLETE!
echo ========================================
echo.
echo Application URL: http://localhost:8080
echo Health Check:   http://localhost:8080/health
echo.
echo Default Login Credentials:
echo SuperAdmin: superadmin@gmail.com / SuperP@ssw0rd123!
echo Admin:      admin@gmail.com / Admin@123
echo User:       user@gmail.com / User@123@@
echo.
echo To stop services: docker-compose down
echo To view logs:     docker-compose logs -f
echo.
pause