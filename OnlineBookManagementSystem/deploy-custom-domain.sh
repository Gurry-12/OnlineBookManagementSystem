#!/bin/bash

echo "========================================"
echo "   WHISPERING PAGES - CUSTOM DOMAIN DEPLOYMENT"
echo "========================================"

# Check if running as root or with sudo
if [[ $EUID -ne 0 ]]; then
   echo "This script should be run as root or with sudo"
   exit 1
fi

echo
echo "[1/8] Checking prerequisites..."

# Check Docker
if ! command -v docker &> /dev/null; then
    echo "ERROR: Docker is not installed"
    echo "Installing Docker..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    systemctl enable docker
    systemctl start docker
    rm get-docker.sh
fi
echo "✓ Docker is available"

# Check Docker Compose
if ! command -v docker-compose &> /dev/null; then
    echo "Installing Docker Compose..."
    curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose
fi
echo "✓ Docker Compose is available"

echo
echo "[2/8] Setting up environment configuration..."
if [[ ! -f .env ]]; then
    if [[ -f .env.production ]]; then
        cp .env.production .env
        echo "✓ Created .env file from production template"
    else
        echo "ERROR: No .env file found. Please run setup-domain.sh first"
        exit 1
    fi
    echo
    echo "⚠️  IMPORTANT: Please edit .env file with your domain and settings"
    echo "Press Enter to continue after editing .env file..."
    read
else
    echo "✓ .env file already exists"
fi

echo
echo "[3/8] Creating required directories..."
mkdir -p ssl ssl-challenge uploads backups logs/nginx
chmod 755 ssl ssl-challenge uploads backups logs/nginx
echo "✓ Directories created"

echo
echo "[4/8] Setting up firewall..."
# Configure UFW firewall
if command -v ufw &> /dev/null; then
    ufw --force enable
    ufw allow ssh
    ufw allow http
    ufw allow https
    echo "✓ Firewall configured"
else
    echo "⚠️  UFW not available, please configure firewall manually"
fi

echo
echo "[5/8] Building Docker images..."
docker-compose -f docker-compose.production.yml build
if [[ $? -ne 0 ]]; then
    echo "ERROR: Failed to build Docker images"
    exit 1
fi
echo "✓ Docker images built successfully"

echo
echo "[6/8] Starting core services..."
docker-compose -f docker-compose.production.yml up -d app sqlserver redis
if [[ $? -ne 0 ]]; then
    echo "ERROR: Failed to start core services"
    exit 1
fi
echo "✓ Core services started"

# Wait for services to be ready
echo "Waiting for services to initialize..."
sleep 30

echo
echo "[7/8] Generating SSL certificate..."
DOMAIN_NAME=$(grep "DOMAIN_NAME=" .env | cut -d '=' -f2)
echo "Domain: $DOMAIN_NAME"
echo
echo "This will request a Let's Encrypt SSL certificate for your domain."
echo "Make sure your domain DNS points to this server's IP address."
echo

read -p "Continue with SSL certificate generation? (y/n): " continue
if [[ "$continue" =~ ^[Yy]$ ]]; then
    docker-compose -f docker-compose.production.yml run --rm certbot
    if [[ $? -ne 0 ]]; then
        echo "WARNING: SSL certificate generation failed"
        echo "You can continue without HTTPS or fix DNS and try again"
    else
        echo "✓ SSL certificate generated successfully"
    fi
fi

echo
echo "[8/8] Starting all services with Nginx..."
docker-compose -f docker-compose.production.yml up -d
if [[ $? -ne 0 ]]; then
    echo "ERROR: Failed to start all services"
    exit 1
fi
echo "✓ All services started successfully"

# Set up SSL certificate renewal cron job
echo "Setting up SSL certificate auto-renewal..."
(crontab -l 2>/dev/null; echo "0 12 * * * cd $(pwd) && docker-compose -f docker-compose.production.yml run --rm certbot renew --quiet") | crontab -
echo "✓ SSL auto-renewal configured"

echo
echo "========================================"
echo "   DEPLOYMENT COMPLETE!"
echo "========================================"
echo

DOMAIN_NAME=$(grep "DOMAIN_NAME=" .env | cut -d '=' -f2)
echo "Application URL: https://$DOMAIN_NAME"
echo "Fallback URL:   http://$DOMAIN_NAME"
echo "Health Check:   https://$DOMAIN_NAME/health"
echo
echo "Default Login Credentials:"
echo "SuperAdmin: superadmin@gmail.com / SuperP@ssw0rd123!"
echo "Admin:      admin@gmail.com / Admin@123"
echo "User:       user@gmail.com / User@123@@"
echo
echo "Management Commands:"
echo "View logs:      docker-compose -f docker-compose.production.yml logs -f"
echo "Stop services:  docker-compose -f docker-compose.production.yml down"
echo "Restart:        docker-compose -f docker-compose.production.yml restart"
echo
echo "SSL Certificate Renewal:"
echo "docker-compose -f docker-compose.production.yml run --rm certbot renew"
echo
echo "System Status:"
docker-compose -f docker-compose.production.yml ps
echo