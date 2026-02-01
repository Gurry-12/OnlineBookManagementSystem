# 🚀 Online Book Management System - Deployment Guide

## 📋 Pre-Deployment Checklist

### ✅ **Required Configurations**

1. **Database Setup**
   ```bash
   # For Production (SQL Server)
   Update-Database -Context BookManagementContext
   
   # For Development (SQLite) - Already configured
   dotnet ef database update
   ```

2. **Email Service Configuration**
   ```json
   // appsettings.Production.json
   "Email": {
     "EnableEmailService": true,
     "SmtpHost": "smtp.gmail.com",
     "SmtpPort": 587,
     "EnableSsl": true,
     "Username": "your-email@domain.com",
     "Password": "your-app-password"
   }
   ```

3. **Environment Variables**
   ```bash
   export EMAIL_USERNAME="your-email@domain.com"
   export EMAIL_PASSWORD="your-app-password"
   export JWT_KEY="your-super-secure-jwt-key-256-bits"
   ```

---

## 🐳 **Docker Deployment**

### **Option 1: Docker Compose (Recommended)**

```bash
# 1. Clone repository
git clone <repository-url>
cd OnlineBookManagementSystem

# 2. Build and run
docker-compose up -d

# 3. Access application
# http://localhost:8080
```

### **Option 2: Manual Docker Build**

```bash
# 1. Build image
docker build -t book-management-system .

# 2. Run container
docker run -d \
  --name book-management \
  -p 8080:8080 \
  -e EMAIL_USERNAME="your-email@domain.com" \
  -e EMAIL_PASSWORD="your-app-password" \
  book-management-system
```

---

## ☁️ **Cloud Deployment**

### **Azure App Service**

1. **Create App Service**
   ```bash
   az webapp create \
     --resource-group myResourceGroup \
     --plan myAppServicePlan \
     --name myBookManagementApp \
     --runtime "DOTNET|8.0"
   ```

2. **Configure Connection String**
   ```bash
   az webapp config connection-string set \
     --resource-group myResourceGroup \
     --name myBookManagementApp \
     --settings DefaultConnection="Server=tcp:myserver.database.windows.net,1433;Database=BookManagementDB;User ID=myuser;Password=mypassword;Encrypt=true;"
   ```

3. **Deploy**
   ```bash
   dotnet publish -c Release
   az webapp deployment source config-zip \
     --resource-group myResourceGroup \
     --name myBookManagementApp \
     --src publish.zip
   ```

### **AWS Elastic Beanstalk**

1. **Install EB CLI**
   ```bash
   pip install awsebcli
   ```

2. **Initialize and Deploy**
   ```bash
   eb init
   eb create production
   eb deploy
   ```

---

## 🔧 **Production Configuration**

### **1. Security Settings**

```json
// appsettings.Production.json
{
  "Jwt": {
    "Key": "${JWT_KEY}",
    "ExpiryMinutes": 30,
    "RefreshTokenExpiryDays": 7
  },
  "RateLimiting": {
    "PermitLimit": 50,
    "Window": "00:01:00"
  }
}
```

### **2. Database Migration**

```bash
# Generate migration
dotnet ef migrations add InitialCreate

# Update database
dotnet ef database update
```

### **3. SSL Certificate**

```bash
# Let's Encrypt (Linux)
sudo certbot --nginx -d yourdomain.com

# Or configure in appsettings.json
"Kestrel": {
  "Certificates": {
    "Default": {
      "Path": "certificate.pfx",
      "Password": "certificate-password"
    }
  }
}
```

---

## 📊 **Monitoring & Health Checks**

### **Health Check Endpoints**

- **Application Health**: `/health`
- **System Status**: `/api/Health/status` (Admin only)
- **Database Health**: Included in system status

### **Logging Configuration**

```json
// appsettings.Production.json
"Serilog": {
  "MinimumLevel": "Warning",
  "WriteTo": [
    {
      "Name": "File",
      "Args": {
        "path": "/var/log/bookmanagement/app-.log",
        "rollingInterval": "Day",
        "retainedFileCountLimit": 30
      }
    }
  ]
}
```

---

## 🔐 **Security Checklist**

### ✅ **Pre-Production Security**

- [ ] Change default admin passwords
- [ ] Configure HTTPS/SSL
- [ ] Set secure JWT keys
- [ ] Enable rate limiting
- [ ] Configure CORS properly
- [ ] Set secure cookie policies
- [ ] Enable CSRF protection
- [ ] Configure security headers

### **Default Accounts (CHANGE PASSWORDS!)**

```json
{
  "SuperAdmin": {
    "Email": "superadmin@gmail.com",
    "Password": "SuperP@ssw0rd123!"
  },
  "Admin": {
    "Email": "admin@gmail.com", 
    "Password": "Admin@123"
  }
}
```

---

## 🚀 **Performance Optimization**

### **1. Caching Strategy**

```json
"Caching": {
  "DefaultExpirationMinutes": 60,
  "SlidingExpirationMinutes": 30
}
```

### **2. Database Optimization**

```bash
# Enable WAL mode for SQLite
PRAGMA journal_mode=WAL;

# For SQL Server - Enable connection pooling
"ConnectionStrings": {
  "DefaultConnection": "Server=...;Pooling=true;Max Pool Size=100;"
}
```

### **3. Image Optimization**

- Images automatically resized to 400x600px
- Consider CDN for production
- Implement lazy loading

---

## 📈 **Scaling Considerations**

### **Horizontal Scaling**

1. **Load Balancer Configuration**
2. **Session State Management** (Redis)
3. **Database Connection Pooling**
4. **File Storage** (Azure Blob/AWS S3)

### **Vertical Scaling**

- **Memory**: Minimum 2GB RAM
- **CPU**: 2+ cores recommended
- **Storage**: SSD recommended for database

---

## 🔍 **Troubleshooting**

### **Common Issues**

1. **Email Not Sending**
   - Check SMTP configuration
   - Verify firewall settings
   - Test with telnet

2. **Database Connection Issues**
   - Verify connection string
   - Check network connectivity
   - Ensure database exists

3. **JWT Token Issues**
   - Verify JWT key configuration
   - Check token expiration
   - Validate issuer/audience

### **Logs Location**

- **Application Logs**: `/logs/app-{date}.txt`
- **System Logs**: Check system event logs
- **Database Logs**: Check database server logs

---

## 📞 **Support & Maintenance**

### **Regular Maintenance Tasks**

1. **Daily**
   - Monitor system health
   - Check error logs
   - Verify backup completion

2. **Weekly**
   - Review performance metrics
   - Clean old logs
   - Update security patches

3. **Monthly**
   - Database maintenance
   - Security audit
   - Performance optimization review

### **Backup Strategy**

```bash
# Database backup (automated)
# Configured in SystemSettingsService.BackupDatabaseAsync()

# File backup
tar -czf backup-$(date +%Y%m%d).tar.gz /app/wwwroot/images /app/logs
```

---

## 🎯 **Success Metrics**

### **Key Performance Indicators**

- **Uptime**: Target 99.9%
- **Response Time**: < 2 seconds
- **Error Rate**: < 0.1%
- **User Satisfaction**: Monitor via feedback

### **Monitoring Tools**

- Built-in health checks
- Application Insights (Azure)
- CloudWatch (AWS)
- Custom dashboards

---

**🎉 Your Online Book Management System is now ready for production deployment!**

For additional support, refer to the application documentation or contact the development team.