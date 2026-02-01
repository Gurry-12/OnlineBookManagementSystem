@echo off
echo ========================================
echo   DOMAIN SETUP WIZARD
echo ========================================

echo.
echo This wizard will help you configure your custom domain deployment.
echo.

:domain_input
set /p domain="Enter your domain name (e.g., whisperingpages.com): "
if "%domain%"=="" (
    echo Please enter a valid domain name.
    goto domain_input
)

:email_input
set /p email="Enter your email for SSL certificate (e.g., admin@%domain%): "
if "%email%"=="" (
    echo Please enter a valid email address.
    goto email_input
)

:password_input
set /p dbpassword="Enter a strong database password: "
if "%dbpassword%"=="" (
    echo Please enter a database password.
    goto password_input
)

echo.
echo ========================================
echo   CONFIGURATION SUMMARY
echo ========================================
echo Domain:    %domain%
echo SSL Email: %email%
echo Database:  Strong password set
echo.

set /p confirm="Is this correct? (y/n): "
if /i not "%confirm%"=="y" goto domain_input

echo.
echo [1/3] Creating environment file...
cd OnlineBookManagementSystem

(
echo # PRODUCTION ENVIRONMENT VARIABLES - AUTO GENERATED
echo # Generated on %date% at %time%
echo.
echo # ===== DOMAIN CONFIGURATION =====
echo DOMAIN_NAME=%domain%
echo ENABLE_HTTPS=true
echo SSL_EMAIL=%email%
echo.
echo # ===== DATABASE CONFIGURATION =====
echo SA_PASSWORD=%dbpassword%
echo DATABASE_CONNECTION_STRING=Server=sqlserver;Database=WhisperingPages;User Id=sa;Password=%dbpassword%;TrustServerCertificate=true;
echo.
echo # ===== JWT CONFIGURATION =====
echo JWT_SECRET_KEY=WhisperingPages_Production_JWT_Key_2024_Enterprise_%domain%_512bit_SecureKey
echo JWT_ISSUER=WhisperingPages
echo JWT_AUDIENCE=WhisperingPagesUsers
echo.
echo # ===== EMAIL CONFIGURATION =====
echo SMTP_HOST=smtp.gmail.com
echo SMTP_PORT=587
echo SMTP_USERNAME=noreply@%domain%
echo SMTP_PASSWORD=your-app-specific-password
echo FROM_EMAIL=noreply@%domain%
echo FROM_NAME=Whispering Pages
echo.
echo # ===== REDIS CONFIGURATION =====
echo REDIS_CONNECTION_STRING=redis:6379
echo.
echo # ===== APPLICATION SETTINGS =====
echo ASPNETCORE_ENVIRONMENT=Production
echo ASPNETCORE_URLS=http://+:8080;https://+:8443
echo.
echo # ===== SECURITY SETTINGS =====
echo ENABLE_RATE_LIMITING=true
echo ENABLE_HTTPS_REDIRECT=true
echo ENABLE_HSTS=true
echo.
echo # ===== PERFORMANCE SETTINGS =====
echo ENABLE_RESPONSE_COMPRESSION=true
echo ENABLE_RESPONSE_CACHING=true
echo ENABLE_DISTRIBUTED_CACHE=true
echo.
echo # ===== MONITORING =====
echo ENABLE_HEALTH_CHECKS=true
echo ENABLE_DETAILED_ERRORS=false
) > .env

echo ✓ Environment file created

echo.
echo [2/3] Updating Nginx configuration...
powershell -Command "(Get-Content nginx\sites-available\whisperingpages.conf) -replace '\${DOMAIN_NAME}', '%domain%' | Set-Content nginx\sites-available\whisperingpages.conf"
echo ✓ Nginx configuration updated

echo.
echo [3/3] Creating DNS setup instructions...
(
echo ========================================
echo   DNS CONFIGURATION REQUIRED
echo ========================================
echo.
echo Before deploying, you need to configure your DNS:
echo.
echo 1. Log into your domain registrar ^(GoDaddy, Namecheap, etc.^)
echo 2. Go to DNS Management for %domain%
echo 3. Add these A records:
echo.
echo    Type: A
echo    Name: @
echo    Value: YOUR_SERVER_IP_ADDRESS
echo    TTL: 300
echo.
echo    Type: A  
echo    Name: www
echo    Value: YOUR_SERVER_IP_ADDRESS
echo    TTL: 300
echo.
echo 4. Wait for DNS propagation ^(5-30 minutes^)
echo 5. Test with: nslookup %domain%
echo.
echo ========================================
echo   EMAIL CONFIGURATION REQUIRED
echo ========================================
echo.
echo To enable email notifications:
echo 1. Set up Gmail App Password or SMTP service
echo 2. Update SMTP_PASSWORD in .env file
echo 3. Configure SPF/DKIM records for %domain%
echo.
echo ========================================
echo   NEXT STEPS
echo ========================================
echo.
echo 1. Configure DNS as shown above
echo 2. Update SMTP_PASSWORD in .env file
echo 3. Run: deploy-custom-domain.cmd
echo 4. Your site will be available at: https://%domain%
echo.
) > DNS_SETUP_INSTRUCTIONS.txt

echo ✓ DNS instructions created

echo.
echo ========================================
echo   SETUP COMPLETE!
echo ========================================
echo.
echo Configuration files created:
echo ✓ .env - Environment variables
echo ✓ nginx/sites-available/whisperingpages.conf - Web server config
echo ✓ DNS_SETUP_INSTRUCTIONS.txt - DNS setup guide
echo.
echo NEXT STEPS:
echo 1. Read DNS_SETUP_INSTRUCTIONS.txt
echo 2. Configure your domain's DNS settings
echo 3. Update SMTP password in .env file
echo 4. Run: deploy-custom-domain.cmd
echo.
echo Your domain: https://%domain%
echo.
pause