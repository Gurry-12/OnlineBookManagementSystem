@echo off
echo ========================================
echo   WHISPERING PAGES - DEVELOPMENT SERVER
echo ========================================

cd OnlineBookManagementSystem

echo.
echo [1/3] Checking .NET installation...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 9.0 SDK is not installed
    echo Please install from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo ✓ .NET SDK is available

echo.
echo [2/3] Restoring packages and building...
dotnet restore
dotnet build
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo ✓ Build successful

echo.
echo [3/3] Starting development server...
echo.
echo ========================================
echo   SERVER STARTING...
echo ========================================
echo.
echo Application will be available at:
echo - HTTP:  http://localhost:5000
echo - HTTPS: https://localhost:5001
echo.
echo Default Login Credentials:
echo SuperAdmin: superadmin@gmail.com / SuperP@ssw0rd123!
echo Admin:      admin@gmail.com / Admin@123
echo User:       user@gmail.com / User@123@@
echo.
echo Press Ctrl+C to stop the server
echo.

dotnet run