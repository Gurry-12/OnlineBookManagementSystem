@echo off
echo ========================================
echo   WHISPERING PAGES - CUSTOM DOMAIN DEPLOYMENT
echo ========================================

cd OnlineBookManagementSystem

echo.
echo [1/8] Checking prerequisites...
docker --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: Docker is not installed or not in PATH
    echo Please install Docker Desktop from https://www.docker.com/products/docker-desktop
    pause
    exit /b 1
)
echo ✓ Docker is available

echo.
echo [2/8] Setting up environment configuration...
if not exist .env (
    copy .env.production .env
    echo ✓ Created .env file from production template
    echo.
    echo ⚠️  IMPORTANT: Please edit .env file with your domain and settings:
    echo    - DOMAIN_NAME=yourdomain.com
    echo    - SSL_EMAIL=admin@yourdomain.com
    echo    - Database passwords
    echo    - SMTP settings
    echo.
    echo Press any key to continue after editing .env file...
    pause
) else (
    echo ✓ .env file already exists
)

echo.
echo [3/8] Creating required directories...
if not exist "ssl" mkdir ssl
if not exist "ssl-challenge" mkdir ssl-challenge
if not exist "uploads" mkdir uploads
if not exist "backups" mkdir backups
if not exist "logs\nginx" mkdir logs\nginx
echo ✓ Directories created

echo.
echo [4/8] Setting up Nginx configuration...
powershell -Command "(Get-Content nginx\sites-available\whisperingpages.conf) -replace '\${DOMAIN_NAME}', (Get-Content .env | Where-Object {$_ -match 'DOMAIN_NAME='} | ForEach-Object {$_.Split('=')[1]}) | Set-Content nginx\sites-available\whisperingpages.conf"
echo ✓ Nginx configuration updated

echo.
echo [5/8] Building Docker images...
docker-compose -f docker-compose.production.yml build
if errorlevel 1 (
    echo ERROR: Failed to build Docker images
    pause
    exit /b 1
)
echo ✓ Docker images built successfully

echo.
echo [6/8] Starting services (without SSL first)...
docker-compose -f docker-compose.production.yml up -d app sqlserver redis
if errorlevel 1 (
    echo ERROR: Failed to start initial services
    pause
    exit /b 1
)
echo ✓ Core services started

echo.
echo [7/8] Generating SSL certificate...
echo This will request a Let's Encrypt SSL certificate for your domain.
echo Make sure your domain DNS points to this server's IP address.
echo.
set /p continue="Continue with SSL certificate generation? (y/n): "
if /i "%continue%"=="y" (
    docker-compose -f docker-compose.production.yml run --rm certbot
    if errorlevel 1 (
        echo WARNING: SSL certificate generation failed
        echo You can continue without HTTPS or fix DNS and try again
    ) else (
        echo ✓ SSL certificate generated successfully
    )
)

echo.
echo [8/8] Starting all services with Nginx...
docker-compose -f docker-compose.production.yml up -d
if errorlevel 1 (
    echo ERROR: Failed to start all services
    pause
    exit /b 1
)
echo ✓ All services started successfully

echo.
echo ========================================
echo   DEPLOYMENT COMPLETE!
echo ========================================
echo.
for /f "tokens=2 delims==" %%a in ('findstr "DOMAIN_NAME=" .env') do set DOMAIN=%%a
echo Application URL: https://%DOMAIN%
echo Fallback URL:   http://%DOMAIN%
echo Health Check:   https://%DOMAIN%/health
echo.
echo Default Login Credentials:
echo SuperAdmin: superadmin@gmail.com / SuperP@ssw0rd123!
echo Admin:      admin@gmail.com / Admin@123
echo User:       user@gmail.com / User@123@@
echo.
echo Management Commands:
echo View logs:      docker-compose -f docker-compose.production.yml logs -f
echo Stop services:  docker-compose -f docker-compose.production.yml down
echo Restart:        docker-compose -f docker-compose.production.yml restart
echo.
echo SSL Certificate Renewal:
echo docker-compose -f docker-compose.production.yml run --rm certbot renew
echo.
pause