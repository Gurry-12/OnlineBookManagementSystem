#!/bin/bash

echo "========================================"
echo "   DOMAIN SETUP WIZARD (Linux)"
echo "========================================"
echo

echo "This wizard will help you configure your custom domain deployment."
echo

# Domain input
while true; do
    read -p "Enter your domain name (e.g., whisperingpages.com): " domain
    if [[ -n "$domain" ]]; then
        break
    fi
    echo "Please enter a valid domain name."
done

# Email input
while true; do
    read -p "Enter your email for SSL certificate (e.g., admin@$domain): " email
    if [[ -n "$email" ]]; then
        break
    fi
    echo "Please enter a valid email address."
done

# Password input
while true; do
    read -s -p "Enter a strong database password: " dbpassword
    echo
    if [[ -n "$dbpassword" ]]; then
        break
    fi
    echo "Please enter a database password."
done

echo
echo "========================================"
echo "   CONFIGURATION SUMMARY"
echo "========================================"
echo "Domain:    $domain"
echo "SSL Email: $email"
echo "Database:  Strong password set"
echo

read -p "Is this correct? (y/n): " confirm
if [[ ! "$confirm" =~ ^[Yy]$ ]]; then
    echo "Setup cancelled."
    exit 1
fi

echo
echo "[1/3] Creating environment file..."

cat > .env << EOF
# PRODUCTION ENVIRONMENT VARIABLES - AUTO GENERATED
# Generated on $(date)

# ===== DOMAIN CONFIGURATION =====
DOMAIN_NAME=$domain
ENABLE_HTTPS=true
SSL_EMAIL=$email

# ===== DATABASE CONFIGURATION =====
SA_PASSWORD=$dbpassword
DATABASE_CONNECTION_STRING=Server=sqlserver;Database=WhisperingPages;User Id=sa;Password=$dbpassword;TrustServerCertificate=true;

# ===== JWT CONFIGURATION =====
JWT_SECRET_KEY=WhisperingPages_Production_JWT_Key_2024_Enterprise_${domain}_512bit_SecureKey
JWT_ISSUER=WhisperingPages
JWT_AUDIENCE=WhisperingPagesUsers

# ===== EMAIL CONFIGURATION =====
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=noreply@$domain
SMTP_PASSWORD=your-app-specific-password
FROM_EMAIL=noreply@$domain
FROM_NAME=Whispering Pages

# ===== REDIS CONFIGURATION =====
REDIS_CONNECTION_STRING=redis:6379

# ===== APPLICATION SETTINGS =====
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080;https://+:8443

# ===== SECURITY SETTINGS =====
ENABLE_RATE_LIMITING=true
ENABLE_HTTPS_REDIRECT=true
ENABLE_HSTS=true

# ===== PERFORMANCE SETTINGS =====
ENABLE_RESPONSE_COMPRESSION=true
ENABLE_RESPONSE_CACHING=true
ENABLE_DISTRIBUTED_CACHE=true

# ===== MONITORING =====
ENABLE_HEALTH_CHECKS=true
ENABLE_DETAILED_ERRORS=false
EOF

echo "✓ Environment file created"

echo
echo "[2/3] Updating Nginx configuration..."
sed -i "s/\${DOMAIN_NAME}/$domain/g" nginx/sites-available/whisperingpages.conf
echo "✓ Nginx configuration updated"

echo
echo "[3/3] Creating DNS setup instructions..."

cat > DNS_SETUP_INSTRUCTIONS.txt << EOF
========================================
   DNS CONFIGURATION REQUIRED
========================================

Before deploying, you need to configure your DNS:

1. Log into your domain registrar (GoDaddy, Namecheap, etc.)
2. Go to DNS Management for $domain
3. Add these A records:

   Type: A
   Name: @
   Value: YOUR_SERVER_IP_ADDRESS
   TTL: 300

   Type: A  
   Name: www
   Value: YOUR_SERVER_IP_ADDRESS
   TTL: 300

4. Wait for DNS propagation (5-30 minutes)
5. Test with: nslookup $domain

========================================
   EMAIL CONFIGURATION REQUIRED
========================================

To enable email notifications:
1. Set up Gmail App Password or SMTP service
2. Update SMTP_PASSWORD in .env file
3. Configure SPF/DKIM records for $domain

========================================
   NEXT STEPS
========================================

1. Configure DNS as shown above
2. Update SMTP_PASSWORD in .env file
3. Run: ./deploy-custom-domain.sh
4. Your site will be available at: https://$domain
EOF

echo "✓ DNS instructions created"

echo
echo "========================================"
echo "   SETUP COMPLETE!"
echo "========================================"
echo
echo "Configuration files created:"
echo "✓ .env - Environment variables"
echo "✓ nginx/sites-available/whisperingpages.conf - Web server config"
echo "✓ DNS_SETUP_INSTRUCTIONS.txt - DNS setup guide"
echo
echo "NEXT STEPS:"
echo "1. Read DNS_SETUP_INSTRUCTIONS.txt"
echo "2. Configure your domain's DNS settings"
echo "3. Update SMTP password in .env file"
echo "4. Run: ./deploy-custom-domain.sh"
echo
echo "Your domain: https://$domain"
echo