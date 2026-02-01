@echo off
echo ========================================
echo   CLOUD DEPLOYMENT OPTIONS
echo ========================================

echo.
echo Choose your cloud deployment platform:
echo.
echo 1. DigitalOcean Droplet
echo 2. AWS EC2
echo 3. Azure Virtual Machine
echo 4. Google Cloud Compute Engine
echo 5. Linode
echo 6. Vultr
echo 7. Hetzner Cloud
echo 8. Generic VPS/Server
echo.

set /p choice="Enter your choice (1-8): "

if "%choice%"=="1" goto digitalocean
if "%choice%"=="2" goto aws
if "%choice%"=="3" goto azure
if "%choice%"=="4" goto gcp
if "%choice%"=="5" goto linode
if "%choice%"=="6" goto vultr
if "%choice%"=="7" goto hetzner
if "%choice%"=="8" goto generic
echo Invalid choice. Please try again.
pause
goto :eof

:digitalocean
echo.
echo ========================================
echo   DIGITALOCEAN DEPLOYMENT
echo ========================================
echo.
echo 1. Create a new Droplet:
echo    - Ubuntu 22.04 LTS
echo    - Minimum: 2GB RAM, 1 vCPU, 50GB SSD
echo    - Recommended: 4GB RAM, 2 vCPU, 80GB SSD
echo.
echo 2. Connect to your droplet:
echo    ssh root@YOUR_DROPLET_IP
echo.
echo 3. Install Docker:
echo    curl -fsSL https://get.docker.com -o get-docker.sh
echo    sh get-docker.sh
echo    systemctl enable docker
echo    systemctl start docker
echo.
echo 4. Install Docker Compose:
echo    curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
echo    chmod +x /usr/local/bin/docker-compose
echo.
echo 5. Upload your project files to the server
echo 6. Run: setup-domain.cmd
echo 7. Run: deploy-custom-domain.cmd
echo.
goto end

:aws
echo.
echo ========================================
echo   AWS EC2 DEPLOYMENT
echo ========================================
echo.
echo 1. Launch EC2 Instance:
echo    - AMI: Ubuntu Server 22.04 LTS
echo    - Instance Type: t3.small or larger
echo    - Security Group: Allow HTTP (80), HTTPS (443), SSH (22)
echo    - Storage: 20GB GP3 minimum
echo.
echo 2. Connect via SSH:
echo    ssh -i your-key.pem ubuntu@YOUR_EC2_IP
echo.
echo 3. Install Docker:
echo    sudo apt update
echo    sudo apt install docker.io docker-compose -y
echo    sudo systemctl enable docker
echo    sudo usermod -aG docker ubuntu
echo.
echo 4. Upload project files
echo 5. Run: setup-domain.cmd
echo 6. Run: deploy-custom-domain.cmd
echo.
goto end

:azure
echo.
echo ========================================
echo   AZURE VM DEPLOYMENT
echo ========================================
echo.
echo 1. Create Virtual Machine:
echo    - Image: Ubuntu Server 22.04 LTS
echo    - Size: Standard_B2s or larger
echo    - Networking: Allow HTTP, HTTPS, SSH
echo.
echo 2. Connect via SSH:
echo    ssh azureuser@YOUR_VM_IP
echo.
echo 3. Install Docker:
echo    sudo apt update
echo    sudo apt install docker.io docker-compose -y
echo    sudo systemctl enable docker
echo    sudo usermod -aG docker azureuser
echo.
echo 4. Upload project files
echo 5. Run: setup-domain.cmd
echo 6. Run: deploy-custom-domain.cmd
echo.
goto end

:gcp
echo.
echo ========================================
echo   GOOGLE CLOUD DEPLOYMENT
echo ========================================
echo.
echo 1. Create Compute Engine Instance:
echo    - Machine Type: e2-small or larger
echo    - Boot Disk: Ubuntu 22.04 LTS, 20GB
echo    - Firewall: Allow HTTP and HTTPS traffic
echo.
echo 2. Connect via SSH (from GCP Console)
echo.
echo 3. Install Docker:
echo    sudo apt update
echo    sudo apt install docker.io docker-compose -y
echo    sudo systemctl enable docker
echo    sudo usermod -aG docker $USER
echo.
echo 4. Upload project files
echo 5. Run: setup-domain.cmd
echo 6. Run: deploy-custom-domain.cmd
echo.
goto end

:linode
echo.
echo ========================================
echo   LINODE DEPLOYMENT
echo ========================================
echo.
echo 1. Create Linode:
echo    - Distribution: Ubuntu 22.04 LTS
echo    - Plan: Nanode 1GB or larger
echo    - Region: Choose closest to your users
echo.
echo 2. Connect via SSH:
echo    ssh root@YOUR_LINODE_IP
echo.
echo 3. Install Docker:
echo    apt update
echo    apt install docker.io docker-compose -y
echo    systemctl enable docker
echo.
echo 4. Upload project files
echo 5. Run: setup-domain.cmd
echo 6. Run: deploy-custom-domain.cmd
echo.
goto end

:vultr
echo.
echo ========================================
echo   VULTR DEPLOYMENT
echo ========================================
echo.
echo 1. Deploy Server:
echo    - OS: Ubuntu 22.04 x64
echo    - Plan: Regular Performance 1GB or larger
echo    - Location: Choose optimal location
echo.
echo 2. Connect via SSH:
echo    ssh root@YOUR_SERVER_IP
echo.
echo 3. Install Docker:
echo    apt update
echo    apt install docker.io docker-compose -y
echo    systemctl enable docker
echo.
echo 4. Upload project files
echo 5. Run: setup-domain.cmd
echo 6. Run: deploy-custom-domain.cmd
echo.
goto end

:hetzner
echo.
echo ========================================
echo   HETZNER CLOUD DEPLOYMENT
echo ========================================
echo.
echo 1. Create Server:
echo    - Image: Ubuntu 22.04
echo    - Type: CX11 or larger
echo    - Location: Choose optimal datacenter
echo.
echo 2. Connect via SSH:
echo    ssh root@YOUR_SERVER_IP
echo.
echo 3. Install Docker:
echo    apt update
echo    apt install docker.io docker-compose -y
echo    systemctl enable docker
echo.
echo 4. Upload project files
echo 5. Run: setup-domain.cmd
echo 6. Run: deploy-custom-domain.cmd
echo.
goto end

:generic
echo.
echo ========================================
echo   GENERIC VPS/SERVER DEPLOYMENT
echo ========================================
echo.
echo Requirements:
echo - Ubuntu 22.04 LTS (or similar Linux distribution)
echo - Minimum 2GB RAM, 1 vCPU, 20GB storage
echo - Root or sudo access
echo - Public IP address
echo - Ports 80, 443, 22 open
echo.
echo Steps:
echo 1. Connect to your server via SSH
echo 2. Install Docker and Docker Compose
echo 3. Upload project files
echo 4. Run: setup-domain.cmd
echo 5. Run: deploy-custom-domain.cmd
echo.
goto end

:end
echo.
echo ========================================
echo   GENERAL DEPLOYMENT STEPS
echo ========================================
echo.
echo After setting up your server:
echo.
echo 1. Upload project files to server:
echo    scp -r OnlineBookManagementSystem user@server:/home/user/
echo.
echo 2. On the server, run:
echo    cd OnlineBookManagementSystem
echo    chmod +x *.sh
echo    ./setup-domain.sh
echo.
echo 3. Configure DNS for your domain
echo.
echo 4. Deploy:
echo    ./deploy-custom-domain.sh
echo.
echo 5. Your site will be available at your custom domain!
echo.
echo Need help? Check DEPLOYMENT_GUIDE.md for detailed instructions.
echo.
pause