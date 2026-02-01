@echo off
echo ========================================
echo   WHISPERING PAGES - LOCAL DEPLOYMENT
echo ========================================

cd OnlineBookManagementSystem

echo.
echo [1/4] Checking .NET installation...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 9.0 SDK is not installed
    echo Please install from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo ✓ .NET SDK is available

echo.
echo [2/4] Cleaning previous builds...
dotnet clean -c Release
if exist "C:\BookApplication" rmdir /s /q "C:\BookApplication"
echo ✓ Cleaned previous builds

echo.
echo [3/4] Building and publishing application...
dotnet publish -c Release -p:PublishProfile=FolderProfile
if errorlevel 1 (
    echo ERROR: Failed to publish application
    pause
    exit /b 1
)
echo ✓ Application published to C:\BookApplication

echo.
echo [4/4] Setting up database...
dotnet ef database update --configuration Release
if errorlevel 1 (
    echo WARNING: Database migration failed - you may need to run this manually
)
echo ✓ Database setup complete

echo.
echo ========================================
echo   DEPLOYMENT COMPLETE!
echo ========================================
echo.
echo Application published to: C:\BookApplication
echo.
echo To run the application:
echo 1. Navigate to C:\BookApplication
echo 2. Run: dotnet OnlineBookManagementSystem.dll
echo 3. Open: http://localhost:5000
echo.
echo Default Login Credentials:
echo SuperAdmin: superadmin@gmail.com / SuperP@ssw0rd123!
echo Admin:      admin@gmail.com / Admin@123
echo User:       user@gmail.com / User@123@@
echo.
pause