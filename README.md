<div align="center">

# 📚 Whispering Pages
### *Online Book Management System*

<img src="https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt=".NET 9.0">
<img src="https://img.shields.io/badge/ASP.NET_Core-MVC-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" alt="ASP.NET Core MVC">
<img src="https://img.shields.io/badge/Entity_Framework-Core-512BD4?style=for-the-badge&logo=microsoft&logoColor=white" alt="EF Core">
<img src="https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white" alt="SQLite">
<img src="https://img.shields.io/badge/JWT-Authentication-000000?style=for-the-badge&logo=jsonwebtokens&logoColor=white" alt="JWT">

---

*A comprehensive enterprise-grade Book Management System featuring secure JWT authentication, advanced role-based access control, book reviews, order management, and a unified design system. Built with modern web technologies and clean architecture principles.*

**✨ Where every book tells a story, and every story finds its reader ✨**

</div>

## �️ Tech Stack

<div align="center">

### Backend Technologies
| Technology | Version | Purpose |
|------------|---------|---------|
| ![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet) | 9.0 | Core Framework |
| ![Entity Framework](https://img.shields.io/badge/EF_Core-9.0.3-512BD4?style=flat-square&logo=microsoft) | 9.0.3 | ORM & Database |
| ![SQLite](https://img.shields.io/badge/SQLite-Primary-003B57?style=flat-square&logo=sqlite) | Latest | Database Engine |
| ![JWT](https://img.shields.io/badge/JWT-Authentication-000000?style=flat-square&logo=jsonwebtokens) | Latest | Security & Auth |
| ![Serilog](https://img.shields.io/badge/Serilog-Logging-FF6B35?style=flat-square) | Latest | Structured Logging |

### Frontend Technologies
| Technology | Purpose | Features |
|------------|---------|----------|
| ![HTML5](https://img.shields.io/badge/HTML5-E34F26?style=flat-square&logo=html5&logoColor=white) | Markup | Semantic Structure |
| ![CSS3](https://img.shields.io/badge/CSS3-1572B6?style=flat-square&logo=css3&logoColor=white) | Styling | Unified Design System |
| ![JavaScript](https://img.shields.io/badge/JavaScript-F7DF1E?style=flat-square&logo=javascript&logoColor=black) | Interactivity | Vanilla JS + jQuery |
| ![Bootstrap](https://img.shields.io/badge/Bootstrap-7952B3?style=flat-square&logo=bootstrap&logoColor=white) | UI Framework | Custom Components |

### Enterprise Features
| Feature | Technology | Benefit |
|---------|------------|---------|
| 📧 Email Service | MailKit 4.14.1 | Transactional Emails |
| 📊 API Docs | Swagger/OpenAPI | Interactive Documentation |
| 🏥 Health Checks | ASP.NET Core | System Monitoring |
| 🚦 Rate Limiting | Built-in | 100 req/min Protection |
| 🗄️ Caching | In-Memory + Redis | Performance Optimization |
| 🖼️ Image Processing | SixLabors.ImageSharp | Media Handling |

</div>

## ✨ Key Features

<div align="center">

### 🔐 Authentication & Security
```
🛡️ JWT-based Authentication    🔄 Refresh Token Support    🔒 Role-based Access Control
🚫 Account Lockout Protection  🔀 SuperAdmin Role Switching  💾 Secure Session Management
```

### � Book Management
```
📖 Complete CRUD Operations    🔍 Advanced Search & Filters   📂 Category Management
📊 Stock Tracking & Alerts     ⭐ Featured Books System      🗑️ Soft Delete Support
🔢 ISBN Validation             🚫 Duplicate Prevention       📈 Performance Optimization
```

### ⭐ Review & Rating System
```
⭐ 1-5 Star Rating System      📝 User Review Submission     🔍 Review Moderation Workflow
👤 One Review Per User/Book    ⚡ Rating Cache Optimization   📊 Bulk Admin Operations
💬 Rejection Reason Feedback   ✅ Approval/Rejection System   📈 Review Analytics
```

### 🛒 E-Commerce Features
```
🛒 Persistent Shopping Cart    📦 Stock Validation           💳 Checkout Process
📋 Order Status Tracking       💰 Payment Method Selection   📊 Order History & Details
🔍 Admin Order Management      📈 Sales Analytics            📧 Order Notifications
```

### 👤 User Experience
```
❤️ Favorites/Wishlist         👤 Profile Management         📊 Personal Dashboard
📋 Activity Logging           📱 Responsive Design          🎨 Role-specific Theming
🔔 Notification System        ⌨️ Keyboard Navigation        ♿ Accessibility Features
```

### 🎛️ Administrative Power
```
👥 Multi-level Admin System   📊 Comprehensive Dashboards   📋 User Management
📈 Activity Logs & Audit      ⚙️ System Settings           🔍 Advanced Search & Filters
📊 Statistics & Analytics     🔄 Bulk Operations            📧 Email Management
```

</div>

## 📁 Project Architecture

<div align="center">

### 🏗️ Clean Architecture Structure

```
📦 OnlineBookManagementSystem/
├── 🎯 Core/                           # Business Logic & Domain
│   ├── 📋 Application/                # Use Cases & DTOs
│   │   ├── DTOs/                      # Data Transfer Objects
│   │   ├── Interfaces/                # Service Contracts
│   │   ├── Mappings/                  # Object Mapping
│   │   └── UseCases/                  # Business Use Cases
│   └── 🏛️ Domain/                     # Domain Entities & Rules
│       ├── Entities/                  # Core Business Entities
│       ├── Enums/                     # Domain Enumerations
│       ├── Exceptions/                # Domain Exceptions
│       └── ValueObjects/              # Value Objects
│
├── 🔧 Infrastructure/                 # External Concerns
│   ├── 🗄️ Data/                       # Database & Repositories
│   │   ├── Configurations/            # EF Configurations
│   │   ├── Context/                   # Database Context
│   │   └── Repositories/              # Data Access Layer
│   └── 🛠️ Services/                   # External Services
│       ├── Domain/                    # Domain Services
│       ├── Helpers/                   # Utility Services
│       ├── Infrastructure/            # Infrastructure Services
│       └── System/                    # System Services
│
├── 🎨 Presentation/                   # User Interface Layer
│   ├── 🎮 Controllers/                # MVC Controllers
│   │   ├── Admin/                     # Admin Controllers
│   │   ├── Api/                       # API Controllers
│   │   └── User/                      # User Controllers
│   ├── 📊 ViewModels/                 # View Models & DTOs
│   ├── 🖼️ Views/                      # Razor Views
│   │   ├── Admin/                     # Admin Views
│   │   ├── Auth/                      # Authentication Views
│   │   ├── Public/                    # Public Views
│   │   ├── Shared/                    # Shared Components
│   │   └── User/                      # User Views
│   ├── 🌐 wwwroot/                    # Static Assets
│   │   ├── css/                       # Stylesheets
│   │   ├── js/                        # JavaScript Files
│   │   └── images/                    # Image Assets
│   ├── 🔧 Middleware/                 # Custom Middleware
│   ├── 🗺️ Mappers/                    # View Model Mappers
│   └── 🎯 Handlers/                   # Request Handlers
│
├── 🔗 Shared/                         # Shared Components
│   ├── Extensions/                    # Extension Methods
│   └── Utilities/                     # Utility Classes
│
├── 🧪 Tests/                          # Test Suite
│   ├── Unit/                          # Unit Tests
│   ├── Properties/                    # Property-based Tests
│   └── Standalone/                    # Integration Tests
│
├── 🗄️ Database/                       # Database Files
├── 📝 Logs/                           # Application Logs
├── 🐳 Docker/                         # Container Configuration
└── 📋 Configuration Files             # App Settings & Config
```

### 🎯 Key Architectural Patterns

| Pattern | Implementation | Benefit |
|---------|----------------|---------|
| **Clean Architecture** | Layered separation of concerns | Maintainable & Testable |
| **Repository Pattern** | Data access abstraction | Database independence |
| **Unit of Work** | Transaction management | Data consistency |
| **CQRS** | Command/Query separation | Performance optimization |
| **Dependency Injection** | IoC container | Loose coupling |
| **Middleware Pipeline** | Request/Response processing | Cross-cutting concerns |

</div>

## 🚀 Quick Start Guide

<div align="center">

### 📋 Prerequisites

| Requirement | Version | Download |
|-------------|---------|----------|
| ![.NET](https://img.shields.io/badge/.NET_SDK-9.0+-512BD4?style=flat-square&logo=dotnet) | 9.0+ | [Download](https://dotnet.microsoft.com/download) |
| ![SQLite](https://img.shields.io/badge/SQLite-Included-003B57?style=flat-square&logo=sqlite) | Latest | Included with .NET |
| ![IDE](https://img.shields.io/badge/IDE-VS_2022_|_VS_Code_|_Rider-blue?style=flat-square) | Latest | Your choice |
| ![Git](https://img.shields.io/badge/Git-Version_Control-F05032?style=flat-square&logo=git) | Latest | [Download](https://git-scm.com/) |

</div>

### ⚡ Installation Steps

<details>
<summary><b>🔽 Step 1: Clone Repository</b></summary>

```bash
# Clone the repository
git clone https://github.com/Gurry-12/OnlineBookManagementSystem.git

# Navigate to project directory
cd OnlineBookManagementSystem/OnlineBookManagementSystem
```
</details>

<details>
<summary><b>🔽 Step 2: Configure Settings</b></summary>

Update `appsettings.json` with your configuration:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=./db/whisperingpages.db"
  },
  "Jwt": {
    "Key": "Your-Super-Secret-256-bit-Key-Here-Make-It-Strong!",
    "Issuer": "WhisperingPages",
    "Audience": "WhisperingPagesUsers",
    "ExpiryMinutes": 60
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```
</details>

<details>
<summary><b>🔽 Step 3: Build & Run</b></summary>

```bash
# Restore NuGet packages
dotnet restore

# Apply database migrations (creates SQLite database)
dotnet ef database update

# Build the application
dotnet build

# Run the application
dotnet run
```
</details>

<details>
<summary><b>🔽 Step 4: Access Application</b></summary>

| Service | URL | Description |
|---------|-----|-------------|
| 🌐 **Main App** | `https://localhost:5001` | Primary application |
| 📚 **API Docs** | `https://localhost:5001/api-docs` | Swagger UI |
| 🏥 **Health Check** | `https://localhost:5001/health` | System status |
| 📊 **Metrics** | `https://localhost:5001/metrics` | Performance data |

</details>

### 👤 Default Test Accounts

<div align="center">

| Role | 👤 Email | 🔑 Password | 🎯 Access Level |
|------|----------|-------------|-----------------|
| **🔴 SuperAdmin** | `superadmin@gmail.com` | `SuperP@ssw0rd123!` | 🌟 Full system access + role switching |
| **🟡 Admin** | `admin@gmail.com` | `Admin@123` | 📚 Book & user management |
| **🔵 User** | `user@gmail.com` | `User@123@@` | 🛒 Shopping, reviews, favorites |
| **🟢 Public** | `public@whisperingpages.com` | `Public123!` | 👁️ Browse books only |

</div>

### 🐳 Docker Deployment

<details>
<summary><b>🔽 Docker Compose (Recommended)</b></summary>

```bash
# Development environment
docker-compose up -d

# Production environment
docker-compose -f docker-compose.production.yml up -d
```
</details>

<details>
<summary><b>🔽 Manual Docker Build</b></summary>

```bash
# Build the image
docker build -t whispering-pages .

# Run the container
docker run -p 5000:80 -p 5001:443 whispering-pages
```
</details>

## 🎭 User Roles & Permissions Matrix

<div align="center">

### 🔐 Role-Based Access Control

| 🎯 **Feature** | 🔴 **SuperAdmin** | 🟡 **Admin** | 🔵 **User** | 🟢 **Public** |
|----------------|:-----------------:|:------------:|:------------:|:--------------:|
| **🔐 Authentication** |
| Login/Register | ✅ | ✅ | ✅ | ✅ |
| Role Switching | ✅ | ❌ | ❌ | ❌ |
| **📚 Book Management** |
| View Books | ✅ | ✅ | ✅ | ✅ |
| Create/Edit Books | ✅ | ✅ | ❌ | ❌ |
| Delete Books | ✅ | ✅ | ❌ | ❌ |
| Manage Categories | ✅ | ✅ | ❌ | ❌ |
| **🛒 Shopping & Orders** |
| Add to Cart | ✅ | ❌ | ✅ | ❌ |
| Place Orders | ✅ | ❌ | ✅ | ❌ |
| View Order History | ✅ | ✅ | ✅ | ❌ |
| Manage All Orders | ✅ | ✅ | ❌ | ❌ |
| **⭐ Reviews & Ratings** |
| Submit Reviews | ✅ | ❌ | ✅ | ❌ |
| Moderate Reviews | ✅ | ✅ | ❌ | ❌ |
| **👥 User Management** |
| View All Users | ✅ | ✅ | ❌ | ❌ |
| Manage User Roles | ✅ | ❌ | ❌ | ❌ |
| **⚙️ System Administration** |
| Activity Logs | ✅ | ✅ | ❌ | ❌ |
| System Settings | ✅ | ❌ | ❌ | ❌ |
| **❤️ Personal Features** |
| Favorites/Wishlist | ✅ | ❌ | ✅ | ❌ |
| Profile Management | ✅ | ✅ | ✅ | ❌ |

### 🎯 Role Descriptions

<table>
<tr>
<td align="center">

**🔴 SuperAdmin**
```
🌟 Complete system access
🔄 Role switching capabilities
🛠️ System configuration
📊 Full analytics access
```

</td>
<td align="center">

**🟡 Admin**
```
📚 Book management
👥 User administration
📋 Order processing
⭐ Review moderation
```

</td>
</tr>
<tr>
<td align="center">

**🔵 User**
```
🛒 Shopping experience
⭐ Reviews & ratings
❤️ Favorites management
📊 Personal dashboard
```

</td>
<td align="center">

**🟢 Public**
```
👁️ Browse books
🔍 Search functionality
📖 View book details
ℹ️ Basic information
```

</td>
</tr>
</table>

</div>

## 🎨 Design System & UI/UX

<div align="center">

### 🌈 Unified Design Philosophy

```
🎯 Role-Specific Theming    🎨 Consistent Components    📱 Mobile-First Design
♿ Accessibility Features   ⚡ Smooth Animations       🔔 Smart Notifications
```

### 🎭 Role-Based Color Themes

<table>
<tr>
<td align="center">

**🔵 User Theme**
```css
Primary: #007bff (Blue)
Secondary: #6c757d
Success: #28a745
Accent: #17a2b8
```

</td>
<td align="center">

**🟡 Admin Theme**
```css
Primary: #ffc107 (Amber)
Secondary: #6c757d
Success: #28a745
Accent: #fd7e14
```

</td>
</tr>
<tr>
<td align="center">

**🔴 SuperAdmin Theme**
```css
Primary: #dc3545 (Red)
Secondary: #6c757d
Success: #28a745
Accent: #e83e8c
```

</td>
<td align="center">

**🟢 Public Theme**
```css
Primary: #28a745 (Green)
Secondary: #6c757d
Success: #20c997
Accent: #6f42c1
```

</td>
</tr>
</table>

### 🧩 Component Library

| Component | Features | Usage |
|-----------|----------|-------|
| **🔘 Buttons** | Role-themed, Loading states, Icon support | Primary actions |
| **📝 Forms** | Validation, Error states, Auto-complete | Data input |
| **🃏 Cards** | Shadows, Hover effects, Responsive | Content display |
| **🔔 Notifications** | Toast messages, Auto-dismiss, Positioning | User feedback |
| **📊 Tables** | Sorting, Filtering, Pagination | Data presentation |
| **🎛️ Navigation** | Responsive sidebar, Breadcrumbs, Search | Site navigation |

### 📱 Responsive Breakpoints

```css
📱 Mobile:    < 768px   (Stack layout, Touch-friendly)
📟 Tablet:    768-1024px (Hybrid layout, Optimized spacing)
💻 Desktop:   > 1024px   (Full layout, Rich interactions)
🖥️ Large:     > 1440px   (Wide layout, Enhanced features)
```

</div>

## ⚙️ Configuration & Environment

<div align="center">

### 🔧 Environment Variables

<table>
<tr>
<td>

**🔐 JWT Configuration**
```bash
JWT_KEY=Your-Super-Secret-256-bit-Key
JWT_ISSUER=WhisperingPages
JWT_AUDIENCE=WhisperingPagesUsers
JWT_EXPIRY_MINUTES=60
```

</td>
<td>

**🗄️ Database Configuration**
```bash
CONNECTION_STRING=Data Source=./db/whisperingpages.db
DB_PROVIDER=SQLite
ENABLE_SENSITIVE_DATA_LOGGING=false
```

</td>
</tr>
<tr>
<td>

**📧 Email Configuration**
```bash
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
SMTP_USE_SSL=true
```

</td>
<td>

**🚦 Performance Settings**
```bash
RATE_LIMIT_REQUESTS=100
RATE_LIMIT_WINDOW=00:01:00
CACHE_EXPIRY_MINUTES=30
MAX_REQUEST_SIZE=10MB
```

</td>
</tr>
</table>

### 🎛️ Feature Flags (appsettings.json)

```json
{
  "Features": {
    "EnableSwagger": true,
    "EnableHealthChecks": true,
    "EnableDatabaseSeeding": true,
    "EnableMetrics": true,
    "EnableRateLimiting": true,
    "EnableCaching": true,
    "EnableEmailService": true,
    "EnableImageProcessing": true
  },
  "Performance": {
    "RateLimiting": {
      "PermitLimit": 100,
      "Window": "00:01:00",
      "QueueLimit": 10
    },
    "Caching": {
      "DefaultExpiryMinutes": 30,
      "SlidingExpiryMinutes": 10
    }
  },
  "Security": {
    "RequireHttps": true,
    "EnableCors": true,
    "AllowedOrigins": ["https://localhost:3000"],
    "MaxLoginAttempts": 5,
    "LockoutDurationMinutes": 15
  }
}
```

</div>

## 📊 API Documentation & Endpoints

<div align="center">

### 🔐 Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| `POST` | `/Auth/LoginData` | 🔑 User login with JWT response | ❌ |
| `POST` | `/Auth/RegisterData` | 👤 User registration | ❌ |
| `POST` | `/Auth/RefreshToken` | 🔄 Refresh JWT token | ✅ |
| `POST` | `/Auth/Logout` | 🚪 User logout | ✅ |
| `POST` | `/Auth/ForgotPassword` | 🔐 Password reset request | ❌ |
| `POST` | `/Auth/ResetPassword` | 🔑 Password reset confirmation | ❌ |

### 📚 Book Management API

| Method | Endpoint | Description | Role Required |
|--------|----------|-------------|---------------|
| `GET` | `/Books/GetBooks` | 📖 Get books with filtering | Public |
| `GET` | `/Books/Details/{id}` | 📋 Get book details | Public |
| `POST` | `/Books/CreateBook` | ➕ Create new book | Admin+ |
| `PUT` | `/Books/UpdateBook/{id}` | ✏️ Update book | Admin+ |
| `DELETE` | `/Books/DeleteBook/{id}` | 🗑️ Delete book | Admin+ |
| `GET` | `/Books/Search` | 🔍 Advanced book search | Public |

### 🛒 Shopping & Orders API

| Method | Endpoint | Description | Role Required |
|--------|----------|-------------|---------------|
| `POST` | `/Cart/AddOrUpdateCart` | 🛒 Add/update cart item | User |
| `GET` | `/Cart/GetCart` | 📋 Get user's cart | User |
| `DELETE` | `/Cart/RemoveItem/{id}` | ❌ Remove cart item | User |
| `POST` | `/Order/CreateOrder` | 📦 Create order from cart | User |
| `GET` | `/Order/UserOrders` | 📊 Get user's order history | User |
| `GET` | `/Order/Details/{id}` | 📋 Get order details | User |

### ⭐ Review System API

| Method | Endpoint | Description | Role Required |
|--------|----------|-------------|---------------|
| `POST` | `/Review/Submit` | ⭐ Submit book review | User |
| `GET` | `/Review/GetReviews/{bookId}` | 📝 Get book reviews | Public |
| `POST` | `/Review/ApproveReview/{id}` | ✅ Approve review | Admin+ |
| `POST` | `/Review/RejectReview/{id}` | ❌ Reject review | Admin+ |
| `DELETE` | `/Review/DeleteReview/{id}` | 🗑️ Delete review | Admin+ |

### 👥 User Management API

| Method | Endpoint | Description | Role Required |
|--------|----------|-------------|---------------|
| `GET` | `/User/Profile` | 👤 Get user profile | User |
| `PUT` | `/User/UpdateProfile` | ✏️ Update user profile | User |
| `GET` | `/User/Favorites` | ❤️ Get user favorites | User |
| `POST` | `/User/AddFavorite/{bookId}` | ➕ Add to favorites | User |
| `DELETE` | `/User/RemoveFavorite/{bookId}` | ❌ Remove from favorites | User |

### 📊 Admin Analytics API

| Method | Endpoint | Description | Role Required |
|--------|----------|-------------|---------------|
| `GET` | `/Admin/Dashboard` | 📊 Admin dashboard data | Admin+ |
| `GET` | `/Admin/Users` | 👥 Get all users | Admin+ |
| `GET` | `/Admin/Orders` | 📦 Get all orders | Admin+ |
| `GET` | `/Admin/Analytics` | 📈 System analytics | Admin+ |
| `GET` | `/Admin/ActivityLogs` | 📋 Activity logs | Admin+ |

**🔗 Full interactive API documentation available at `/api-docs` when running the application.**

</div>

## 🧪 Testing & Quality Assurance

<div align="center">

### 🔬 Test Suite Overview

| Test Type | Coverage | Framework | Purpose |
|-----------|----------|-----------|---------|
| **🧪 Unit Tests** | 85%+ | xUnit | Service layer & business logic |
| **🔗 Integration Tests** | 70%+ | ASP.NET Core TestHost | API endpoints & database |
| **🎭 E2E Tests** | 60%+ | Playwright | Full user workflows |
| **🎲 Property Tests** | Custom | FsCheck | Input validation & edge cases |

### 🚀 Running Tests

<details>
<summary><b>🔽 Basic Test Commands</b></summary>

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test project
dotnet test OnlineBookManagementSystem.Tests/

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```
</details>

<details>
<summary><b>🔽 Advanced Test Options</b></summary>

```bash
# Run specific test category
dotnet test --filter Category=Integration

# Run tests matching pattern
dotnet test --filter "FullyQualifiedName~BookService"

# Run tests with custom settings
dotnet test --settings test.runsettings

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage" --results-directory ./TestResults
```
</details>

### 📊 Test Categories & Coverage

```
🧪 Unit Tests (85% Coverage)
├── 📚 Book Services
├── 🛒 Cart Operations  
├── 👤 User Management
├── ⭐ Review System
└── 🔐 Authentication

🔗 Integration Tests (70% Coverage)
├── 🌐 API Endpoints
├── 🗄️ Database Operations
├── 🔐 Authentication Flow
└── 📧 Email Services

🎭 E2E Tests (60% Coverage)
├── 👤 User Registration/Login
├── 🛒 Shopping Cart Flow
├── 📚 Book Management
└── ⭐ Review Submission
```

</div>

## 🚀 Deployment & Production

<div align="center">

### ✅ Production Checklist

<table>
<tr>
<td>

**🔐 Security**
- [ ] Update JWT secret key (256-bit minimum)
- [ ] Configure HTTPS certificates
- [ ] Set up CORS policies
- [ ] Enable rate limiting
- [ ] Configure security headers

</td>
<td>

**🗄️ Database**
- [ ] Configure production database
- [ ] Set up backup strategy
- [ ] Enable connection pooling
- [ ] Configure migrations
- [ ] Set up monitoring

</td>
</tr>
<tr>
<td>

**📧 Services**
- [ ] Configure email service (SMTP)
- [ ] Set up logging aggregation
- [ ] Configure health checks
- [ ] Set up metrics collection
- [ ] Configure caching

</td>
<td>

**🚀 Performance**
- [ ] Enable compression
- [ ] Configure CDN
- [ ] Set up load balancing
- [ ] Optimize images
- [ ] Configure caching headers

</td>
</tr>
</table>

### 🐳 Docker Production Deployment

<details>
<summary><b>🔽 Multi-stage Dockerfile</b></summary>

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["OnlineBookManagementSystem.csproj", "."]
RUN dotnet restore
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Security: Create non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

EXPOSE 80 443
ENTRYPOINT ["dotnet", "OnlineBookManagementSystem.dll"]
```
</details>

<details>
<summary><b>🔽 Production Docker Compose</b></summary>

```yaml
version: '3.8'
services:
  app:
    build: .
    ports:
      - "80:80"
      - "443:443"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:443;http://+:80
    volumes:
      - ./certs:/https:ro
      - ./db:/app/db
      - ./logs:/app/logs
    restart: unless-stopped
    
  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx/nginx.conf:/etc/nginx/nginx.conf:ro
      - ./certs:/etc/nginx/certs:ro
    depends_on:
      - app
    restart: unless-stopped
```
</details>

### 🌐 Cloud Deployment Options

| Platform | Configuration | Benefits |
|----------|---------------|----------|
| **☁️ Azure App Service** | `azure-pipelines.yml` | Integrated CI/CD, Auto-scaling |
| **🚀 AWS Elastic Beanstalk** | `Dockerrun.aws.json` | Easy deployment, Load balancing |
| **🔥 Google Cloud Run** | `cloudbuild.yaml` | Serverless, Pay-per-use |
| **🌊 DigitalOcean Apps** | `app.yaml` | Simple setup, Affordable |

</div>

## ⚡ Performance & Security Features

<div align="center">

### 🚀 Performance Optimizations

<table>
<tr>
<td align="center">

**🗄️ Caching Strategy**
```
📦 In-Memory Cache
🔄 Redis Support
⏱️ Sliding Expiration
🎯 Smart Cache Keys
```

</td>
<td align="center">

**🗃️ Database Optimization**
```
📊 Optimized Queries
🔗 Lazy Loading
📈 Connection Pooling
🎯 Indexed Searches
```

</td>
</tr>
<tr>
<td align="center">

**🚦 Rate Limiting**
```
⏱️ 100 requests/minute
👤 Per-user tracking
🛡️ DDoS protection
📊 Usage analytics
```

</td>
<td align="center">

**🖼️ Media Processing**
```
📸 Image optimization
🔄 Format conversion
📏 Automatic resizing
💾 Efficient storage
```

</td>
</tr>
</table>

### 🔒 Security Implementations

| Feature | Implementation | Benefit |
|---------|----------------|---------|
| **🔐 JWT Authentication** | Bearer tokens + Refresh tokens | Stateless, Scalable |
| **🛡️ Role-based Authorization** | Policy-based access control | Granular permissions |
| **🚫 Rate Limiting** | IP + User-based throttling | Abuse prevention |
| **✅ Input Validation** | FluentValidation rules | Data integrity |
| **🔒 HTTPS Enforcement** | Redirect + HSTS headers | Encrypted communication |
| **🛡️ CSRF Protection** | Anti-forgery tokens | Request authenticity |
| **📋 Activity Logging** | Comprehensive audit trail | Security monitoring |
| **🔑 Secure Passwords** | Hashing + Salt + Policies | Account protection |

### 📊 Monitoring & Logging

```
📈 Performance Metrics
├── 🕐 Response Times
├── 💾 Memory Usage
├── 🔄 Request Throughput
└── ❌ Error Rates

📋 Structured Logging
├── 🎯 Serilog Integration
├── 📊 Log Levels (Debug → Fatal)
├── 🔍 Contextual Information
└── 📁 File + Console Output

🏥 Health Checks
├── 🗄️ Database Connectivity
├── 📧 Email Service Status
├── 💾 Memory Usage
└── 🌐 External Dependencies
```

</div>

## 📝 Recent Updates & Roadmap

<div align="center">

### 🎉 Version 2.0 - Major Feature Release

<table>
<tr>
<td>

**✅ Completed Features**
- 🔄 SuperAdmin Role Switching
- ⭐ Complete Review System
- 🎨 UI Consistency Overhaul
- 🧩 Pure CSS Components
- 🔐 Enhanced Security
- 📦 Order Management
- 📧 Email Integration
- ⚡ Performance Improvements

</td>
<td>

**🔄 In Progress**
- 🌙 Dark Mode Support
- � Advanced Analytics
- � Mobile App (React Native)
- � Elasticsearch Integration
- 🎯 Recommendation Engine
- 📈 Real-time Notifications
- 🌐 Multi-language Support
- 🔄 GraphQL API

</td>
</tr>
</table>

### 🗺️ Upcoming Features Roadmap

```
🎯 Q1 2026
├── 🌙 Dark Mode Theme System
├── 📊 Advanced Sales Analytics
├── 📱 Progressive Web App (PWA)
└── 🔍 Enhanced Search (Elasticsearch)

🚀 Q2 2026
├── 💳 Payment Integration (Stripe/PayPal)
├── 📦 Inventory Management System
├── 🤖 AI-Powered Recommendations
└── 📧 Advanced Email Templates

🌟 Q3 2026
├── 📱 Mobile App (React Native)
├── 🌐 Multi-language Support (i18n)
├── 🔄 Real-time Notifications
└── 📈 Business Intelligence Dashboard

🎊 Q4 2026
├── 🤝 Third-party Integrations
├── 🔄 GraphQL API
├── 🌍 Multi-tenant Architecture
└── 🚀 Microservices Migration
```

### 📊 Version History

| Version | Release Date | Key Features |
|---------|--------------|--------------|
| **🎉 v2.0** | Jan 2026 | Role switching, Reviews, UI overhaul |
| **🚀 v1.5** | Dec 2025 | Order management, Email service |
| **⭐ v1.0** | Nov 2025 | Initial release, Core features |

</div>

## 🤝 Contributing & Community

<div align="center">

### 🌟 We Welcome Contributors!

```
🐛 Bug Reports    ✨ Feature Requests    📝 Documentation    🧪 Testing    💻 Code
```

### 🚀 Development Setup

<details>
<summary><b>🔽 Getting Started</b></summary>

```bash
# 1. Fork the repository
git clone https://github.com/YOUR-USERNAME/OnlineBookManagementSystem.git

# 2. Create a feature branch
git checkout -b feature/amazing-feature

# 3. Set up development environment
cd OnlineBookManagementSystem/OnlineBookManagementSystem
dotnet restore
dotnet ef database update

# 4. Make your changes and test
dotnet test
dotnet run

# 5. Commit and push
git add .
git commit -m "Add amazing feature"
git push origin feature/amazing-feature

# 6. Open a Pull Request
```
</details>

### 📋 Contribution Guidelines

| Type | Guidelines | Examples |
|------|------------|----------|
| **🐛 Bug Fixes** | Include reproduction steps, fix root cause | UI bugs, Logic errors |
| **✨ Features** | Discuss in issues first, add tests | New endpoints, UI components |
| **📝 Documentation** | Clear, concise, with examples | README updates, Code comments |
| **🧪 Tests** | Maintain 80%+ coverage | Unit tests, Integration tests |
| **🎨 UI/UX** | Follow design system, responsive | Styling, Accessibility |

### 🏆 Code Standards

```
✅ Follow C# coding conventions
✅ Add XML documentation for public APIs
✅ Include unit tests for new features
✅ Update README for significant changes
✅ Use meaningful commit messages
✅ Ensure all tests pass before PR
```

### 👥 Community Guidelines

<table>
<tr>
<td align="center">

**🤝 Be Respectful**
```
💬 Constructive feedback
🌍 Inclusive language
🤝 Collaborative spirit
📚 Help others learn
```

</td>
<td align="center">

**📋 Issue Reporting**
```
🐛 Clear bug descriptions
📸 Screenshots if applicable
🔄 Steps to reproduce
💻 Environment details
```

</td>
</tr>
<tr>
<td align="center">

**🔄 Pull Requests**
```
📝 Descriptive titles
📋 Detailed descriptions
🧪 Include tests
📚 Update documentation
```

</td>
<td align="center">

**💬 Discussions**
```
💡 Share ideas
❓ Ask questions
🎯 Stay on topic
🤝 Be helpful
```

</td>
</tr>
</table>

</div>

## 📄 License & Legal

<div align="center">

### 📜 MIT License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

```
Copyright (c) 2026 Whispering Pages Team

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

</div>

## 💬 Support & Contact

<div align="center">

### 🤝 Get Help & Connect

<table>
<tr>
<td align="center">

**📧 Email Support**
[work.gurpreetsw@gmail.com](mailto:work.gurpreetsw@gmail.com)
*For technical support and inquiries*

</td>
<td align="center">

**🐛 Bug Reports**
[GitHub Issues](https://github.com/Gurry-12/OnlineBookManagementSystem/issues)
*Report bugs or request features*

</td>
</tr>
<tr>
<td align="center">

**📚 Documentation**
[/Markdowns Folder](./Markdowns)
*Detailed guides and documentation*

</td>
<td align="center">

**💬 Discussions**
[GitHub Discussions](https://github.com/Gurry-12/OnlineBookManagementSystem/discussions)
*Community discussions and Q&A*

</td>
</tr>
</table>

### 🔗 Quick Links

[![📧 Email](https://img.shields.io/badge/Email-work.gurpreetsw%40gmail.com-red?style=for-the-badge&logo=gmail)](mailto:work.gurpreetsw@gmail.com)
[![🐛 Issues](https://img.shields.io/badge/Issues-GitHub-green?style=for-the-badge&logo=github)](https://github.com/Gurry-12/OnlineBookManagementSystem/issues)
[![📚 Docs](https://img.shields.io/badge/Documentation-Markdowns-blue?style=for-the-badge&logo=markdown)](./Markdowns)
[![⭐ Star](https://img.shields.io/badge/Star-Repository-yellow?style=for-the-badge&logo=github)](https://github.com/Gurry-12/OnlineBookManagementSystem)

</div>

## 🙏 Acknowledgments

<div align="center">

### 🌟 Special Thanks

<table>
<tr>
<td align="center">

**🏗️ Framework & Tools**
- ASP.NET Core Team
- Entity Framework Core
- Serilog Contributors
- FluentValidation Team

</td>
<td align="center">

**🎨 UI & Design**
- Bootstrap Team
- Font Awesome
- Unsplash (Images)
- CSS Grid Community

</td>
</tr>
<tr>
<td align="center">

**🤝 Community**
- Stack Overflow Community
- GitHub Open Source
- .NET Developer Community
- Code Review Contributors

</td>
<td align="center">

**📚 Learning Resources**
- Microsoft Documentation
- Clean Architecture Guides
- DDD Community
- Testing Best Practices

</td>
</tr>
</table>

### 💝 Contributors

Thanks to all the amazing people who have contributed to this project!

[![Contributors](https://contrib.rocks/image?repo=Gurry-12/OnlineBookManagementSystem)](https://github.com/Gurry-12/OnlineBookManagementSystem/graphs/contributors)

</div>

---

<div align="center">

### ✨ **Whispering Pages** ✨
*Where every book tells a story, and every story finds its reader.*

**Made with ❤️ by the Whispering Pages Team**

[![⭐ Star this repo](https://img.shields.io/github/stars/Gurry-12/OnlineBookManagementSystem?style=social)](https://github.com/Gurry-12/OnlineBookManagementSystem)
[![🍴 Fork this repo](https://img.shields.io/github/forks/Gurry-12/OnlineBookManagementSystem?style=social)](https://github.com/Gurry-12/OnlineBookManagementSystem/fork)
[![👁️ Watch this repo](https://img.shields.io/github/watchers/Gurry-12/OnlineBookManagementSystem?style=social)](https://github.com/Gurry-12/OnlineBookManagementSystem)

</div>

