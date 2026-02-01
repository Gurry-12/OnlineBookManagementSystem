@echo off
echo ========================================
echo   WHISPERING PAGES - RELEASE BUILD
echo ========================================

cd OnlineBookManagementSystem

echo.
echo [1/5] Checking .NET installation...
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ERROR: .NET 9.0 SDK is not installed
    echo Please install from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)
echo ✓ .NET SDK is available

echo.
echo [2/5] Cleaning previous builds...
dotnet clean -c Release
if exist ".\bin\Release\net9.0\publish" rmdir /s /q ".\bin\Release\net9.0\publish"
echo ✓ Cleaned previous builds

echo.
echo [3/5] Restoring packages...
dotnet restore
if errorlevel 1 (
    echo ERROR: Package restore failed
    pause
    exit /b 1
)
echo ✓ Packages restored

echo.
echo [4/5] Building release version...
dotnet build -c Release --no-restore
if errorlevel 1 (
    echo ERROR: Build failed
    pause
    exit /b 1
)
echo ✓ Release build successful

echo.
echo [5/5] Publishing application...
dotnet publish -c Release --no-build --output ".\bin\Release\net9.0\publish"
if errorlevel 1 (
    echo ERROR: Publish failed
    pause
    exit /b 1
)
echo ✓ Application published

echo.
echo ========================================
echo   BUILD COMPLETE!
echo ========================================
echo.
echo Published to: .\bin\Release\net9.0\publish
echo.
echo To run the published application:
echo 1. Navigate to .\bin\Release\net9.0\publish
echo 2. Run: dotnet OnlineBookManagementSystem.dll
echo.
echo Files ready for deployment to any server with .NET 9.0 runtime
echo.
pause