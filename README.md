# 📚 Whispering Pages - Online Book Management System

A comprehensive ASP.NET Core MVC-based Book Management System featuring secure JWT authentication, advanced role-based access control, book reviews, order management, and a unified design system. The platform supports multiple user roles with distinct experiences and comprehensive administrative capabilities.

## 🔧 Technologies Used

### Backend
- **ASP.NET Core MVC (.NET 9.0)**
- **Entity Framework Core 9.0.3 (Code-First)**
- **SQLite** (primary) with **SQL Server** support
- **ASP.NET Core Identity** with custom User model
- **JWT Bearer Authentication** with refresh tokens
- **Serilog** for structured logging
- **FluentValidation 11.3.0** for input validation
- **AutoMapper 12.0.1** for object mapping

### Frontend
- **HTML5 + CSS3** with unified design system
- **JavaScript** (vanilla) + **jQuery**
- **Bootstrap** with custom components
- **AJAX** for dynamic content loading
- **Responsive design** (mobile-first approach)

### Enterprise Features
- **MailKit 4.14.1** for email services
- **Swagger/OpenAPI** for API documentation
- **Health Checks** for monitoring
- **Rate Limiting** (100 requests/minute)
- **In-memory caching** with Redis support
- **SixLabors.ImageSharp 3.1.12** for image processing

---

## ✨ Features

### 🔐 Authentication & Authorization
- **JWT-based authentication** with secure token handling and refresh tokens
- **Role-based access control** with 5 distinct roles: SuperAdmin, Admin, User, Guest, Public
- **Account security**: Lockout after 5 failed attempts (15-minute lockout)
- **SuperAdmin role switching**: Switch between different role views for testing
- **Session management** with secure token storage

### 📖 Book Management
- **Complete CRUD operations** for books (Admin/SuperAdmin)
- **Advanced search and filtering** by title, author, category, price range
- **Category management** with hierarchical organization
- **Stock tracking** with low stock alerts (configurable threshold)
- **Featured books** system for promotions
- **Soft delete** support for data retention
- **ISBN validation** and duplicate prevention

### ⭐ Review & Rating System
- **User reviews** with 1-5 star ratings
- **Review moderation** workflow (Pending → Approved/Rejected)
- **One review per user per book** constraint
- **Rating cache** for performance optimization
- **Admin review management** with bulk operations
- **Rejection reasons** for moderation feedback

### 🛒 Shopping Cart & Orders
- **Persistent shopping cart** with quantity management
- **Stock validation** during cart operations
- **Order creation** from cart with checkout process
- **Order status tracking** (Pending, Processing, Completed, Cancelled)
- **Payment method selection** and status tracking
- **Order history** with detailed views
- **Admin order management** with search and filtering

### ❤️ User Features
- **Favorites/Wishlist** functionality
- **User profiles** with personal information management
- **Order history** with detailed tracking
- **Activity logging** for user actions
- **Responsive dashboard** with personalized content

### 🎛️ Administrative Features
- **Multi-level admin system** (Admin, SuperAdmin)
- **User management** with role assignment
- **Activity logs** with filtering and search
- **System settings** management
- **Comprehensive dashboards** with statistics
- **Audit trail** for all administrative actions

### 🎨 UI/UX Excellence
- **Unified design system** with role-specific theming
- **Pure CSS components** (no Bootstrap dropdown dependencies)
- **Responsive design** (mobile, tablet, desktop)
- **Accessibility features** (ARIA labels, keyboard navigation)
- **Smooth animations** and transitions
- **Consistent notification system**

---

## 📁 Project Structure

```
OnlineBookManagementSystem/
│
├── Controllers/
│   ├── AuthController.cs              # Authentication & JWT management
│   ├── BooksController.cs             # Book CRUD operations
│   ├── CartController.cs              # Shopping cart functionality
│   ├── OrderController.cs             # Order management
│   ├── ReviewController.cs            # Review & rating system
│   ├── UserController.cs              # User dashboard & profile
│   ├── AdminController.cs             # Admin management
│   ├── SuperAdminController.cs        # System administration
│   ├── CategoryController.cs          # Category management
│   └── BaseController.cs              # Common functionality
│
├── Models/
│   ├── Book.cs                        # Book entity with reviews
│   ├── User.cs                        # Identity user extension
│   ├── Order.cs & OrderDetail.cs      # Order management
│   ├── ShoppingCart.cs                # Cart items
│   ├── BookReview.cs                  # Review system
│   ├── Category.cs                    # Book categories
│   ├── ActivityLog.cs                 # Audit trail
│   ├── RefreshToken.cs                # JWT refresh tokens
│   ├── UserFavorite.cs                # Wishlist functionality
│   ├── SystemSettings.cs              # Configuration
│   ├── BookManagementContext.cs       # EF DbContext
│   ├── Validators/                    # FluentValidation rules
│   └── ViewModel/                     # View models & DTOs
│
├── Services/
│   ├── AuthService.cs                 # Authentication logic
│   ├── BookServices.cs                # Book business logic
│   ├── CartService.cs                 # Cart operations
│   ├── OrderService.cs                # Order processing
│   ├── ReviewService.cs               # Review management
│   ├── UsersService.cs                # User management
│   ├── ActivityLogger.cs              # Audit logging
│   ├── MailKitEmailSender.cs          # Email service
│   └── CacheService.cs                # Caching logic
│
├── Views/
│   ├── Shared/
│   │   ├── _LayoutAuth.cshtml         # Authentication layout
│   │   ├── _LayoutPublic.cshtml       # Public browsing layout
│   │   ├── _LayoutUser.cshtml         # User dashboard layout
│   │   ├── _LayoutAdmin.cshtml        # Admin management layout
│   │   └── _LayoutSuperAdmin.cshtml   # SuperAdmin layout
│   ├── Auth/                          # Login, register, password reset
│   ├── Books/                         # Book display & management
│   ├── Cart/                          # Shopping cart & checkout
│   ├── Order/                         # Order management
│   ├── User/                          # User dashboard & profile
│   ├── Admin/                         # Admin management pages
│   └── SuperAdmin/                    # System administration
│
├── wwwroot/
│   ├── css/
│   │   └── unified-design-system.css  # Unified styling system
│   ├── js/
│   │   └── unified-interactions.js    # Shared JavaScript
│   └── images/                        # Static assets
│
├── Extensions/
│   ├── ServiceCollectionExtensions.cs # Dependency injection setup
│   └── DatabaseSeedingExtensions.cs   # Initial data seeding
│
├── Middleware/
│   ├── RequestLoggingMiddleware.cs    # HTTP request logging
│   ├── RoleSwitchingMiddleware.cs     # SuperAdmin role switching
│   └── ExceptionHandlingMiddleware.cs # Global exception handling
│
├── Interfaces/                        # Service interfaces
├── Helper/                           # Utility classes
├── Migrations/                       # EF Core migrations
├── logs/                            # Application logs
├── db/                              # SQLite database
├── Program.cs                       # Application startup
├── appsettings.json                 # Configuration
└── Dockerfile & docker-compose.yml  # Container support
```

---

## 🚀 Getting Started

### Prerequisites
- **.NET 9.0 SDK** or later
- **SQLite** (included with .NET)
- **Visual Studio 2022** / **VS Code** / **JetBrains Rider**
- **Git** for version control

### Quick Start

1. **Clone the Repository**
   ```bash
   git clone https://github.com/Gurry-12/OnlineBookManagementSystem.git
   cd OnlineBookManagementSystem/OnlineBookManagementSystem
   ```

2. **Configure Application Settings**
   
   The application uses SQLite by default. Update `appsettings.json` if needed:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=./db/whisperingpages.db"
     },
     "Jwt": {
       "Key": "Your-256-bit-secret-key-here",
       "Issuer": "WhisperingPages",
       "ExpiryMinutes": 60
     }
   }
   ```

3. **Install Dependencies & Run**
   ```bash
   # Restore NuGet packages
   dotnet restore
   
   # Apply database migrations (creates SQLite database)
   dotnet ef database update
   
   # Run the application
   dotnet run
   ```

4. **Access the Application**
   - **URL**: `https://localhost:5001` or `http://localhost:5000`
   - **API Documentation**: `https://localhost:5001/api-docs` (Swagger UI)
   - **Health Check**: `https://localhost:5001/health`

### Default User Accounts

The application seeds with default accounts for testing:

| Role | Email | Password | Access Level |
|------|-------|----------|--------------|
| **SuperAdmin** | superadmin@gmail.com | SuperP@ssw0rd123! | Full system access + role switching |
| **Admin** | admin@gmail.com | Admin@123 | Book & user management |
| **User** | user@gmail.com | User@123@@ | Shopping, reviews, favorites |
| **Public** | public@whisperingpages.com | Public123! | Browse books only |

### Docker Deployment

```bash
# Build and run with Docker Compose
docker-compose up -d

# Or build manually
docker build -t whispering-pages .
docker run -p 5000:80 whispering-pages
```

---

## � Uoser Roles & Permissions

| Feature | SuperAdmin | Admin | User | Public |
|---------|------------|-------|------|--------|
| **Authentication** |
| Login/Register | ✅ | ✅ | ✅ | ✅ |
| Role Switching | ✅ | ❌ | ❌ | ❌ |
| **Book Management** |
| View Books | ✅ | ✅ | ✅ | ✅ |
| Create/Edit Books | ✅ | ✅ | ❌ | ❌ |
| Delete Books | ✅ | ✅ | ❌ | ❌ |
| Manage Categories | ✅ | ✅ | ❌ | ❌ |
| **Shopping & Orders** |
| Add to Cart | ✅ | ❌ | ✅ | ❌ |
| Place Orders | ✅ | ❌ | ✅ | ❌ |
| View Order History | ✅ | ✅ | ✅ | ❌ |
| Manage All Orders | ✅ | ✅ | ❌ | ❌ |
| **Reviews & Ratings** |
| Submit Reviews | ✅ | ❌ | ✅ | ❌ |
| Moderate Reviews | ✅ | ✅ | ❌ | ❌ |
| **User Management** |
| View All Users | ✅ | ✅ | ❌ | ❌ |
| Manage User Roles | ✅ | ❌ | ❌ | ❌ |
| **System Administration** |
| Activity Logs | ✅ | ✅ | ❌ | ❌ |
| System Settings | ✅ | ❌ | ❌ | ❌ |
| **Personal Features** |
| Favorites/Wishlist | ✅ | ❌ | ✅ | ❌ |
| Profile Management | ✅ | ✅ | ✅ | ❌ |

### Role Descriptions

- **SuperAdmin**: Complete system access with role switching capabilities for testing
- **Admin**: Book and user management, order processing, review moderation
- **User**: Shopping, reviews, favorites, personal order management
- **Public**: Browse books and basic information (no account required)

## 🎨 Design System

### Unified Styling
The application features a comprehensive design system with:
- **Role-specific color themes** (Blue for User, Amber for Admin, Red for SuperAdmin)
- **Consistent component library** (buttons, forms, cards, modals)
- **Responsive design** with mobile-first approach
- **Accessibility features** (ARIA labels, keyboard navigation)
- **Pure CSS components** (no external framework dependencies)

### Key UI Features
- **Smooth animations** and transitions
- **Loading states** for better user feedback
- **Toast notifications** for actions
- **Sidebar navigation** with responsive behavior
- **Search and filtering** with real-time results

## 🔧 Configuration

### Environment Variables
```bash
# JWT Configuration
JWT_KEY=Your-256-bit-secret-key-here
JWT_ISSUER=WhisperingPages
JWT_AUDIENCE=WhisperingPagesUsers

# Database
CONNECTION_STRING=Data Source=./db/whisperingpages.db

# Email (Optional)
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=your-app-password
```

### Feature Flags (appsettings.json)
```json
{
  "Features": {
    "EnableSwagger": true,
    "EnableHealthChecks": true,
    "EnableDatabaseSeeding": true,
    "EnableMetrics": true
  },
  "RateLimiting": {
    "PermitLimit": 100,
    "Window": "00:01:00"
  }
}
```

## 📊 API Documentation

### Authentication Endpoints
- `POST /Auth/LoginData` - User login with JWT response
- `POST /Auth/RegisterData` - User registration
- `POST /Auth/RefreshToken` - Refresh JWT token
- `POST /Auth/Logout` - User logout

### Book Management API
- `GET /Books/GetBooks` - Get books with filtering
- `POST /Books/CreateBook` - Create new book (Admin+)
- `PUT /Books/UpdateBook` - Update book (Admin+)
- `DELETE /Books/DeleteBook/{id}` - Delete book (Admin+)

### Shopping & Orders API
- `POST /Cart/AddOrUpdateCart` - Add/update cart item
- `GET /Cart/GetCart` - Get user's cart
- `POST /Order/CreateOrder` - Create order from cart
- `GET /Order/UserOrders` - Get user's order history

### Review System API
- `POST /Review/Submit` - Submit book review
- `GET /Review/GetReviews/{bookId}` - Get book reviews
- `POST /Review/ApproveReview/{id}` - Approve review (Admin+)

**Full API documentation available at `/api-docs` when running the application.**

## 🧪 Testing

### Running Tests
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test category
dotnet test --filter Category=Integration
```

### Test Categories
- **Unit Tests**: Service layer and business logic
- **Integration Tests**: API endpoints and database operations
- **E2E Tests**: Full user workflows with Playwright
- **Property-Based Tests**: Input validation with FsCheck

## 🚀 Deployment

### Production Checklist
- [ ] Update JWT secret key (256-bit minimum)
- [ ] Configure production database connection
- [ ] Set up HTTPS certificates
- [ ] Configure email service (SMTP)
- [ ] Set up logging aggregation
- [ ] Configure health check monitoring
- [ ] Set up backup strategy for database
- [ ] Configure rate limiting for production load

### Docker Production
```dockerfile
# Multi-stage build for production
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OnlineBookManagementSystem.dll"]
```

## 📈 Performance Features

- **Caching**: In-memory cache with Redis support
- **Database**: Optimized queries with EF Core
- **Rate Limiting**: 100 requests/minute per user
- **Lazy Loading**: Efficient data loading strategies
- **Image Optimization**: SixLabors.ImageSharp for image processing
- **Logging**: Structured logging with Serilog

## 🔒 Security Features

- **JWT Authentication** with refresh tokens
- **Role-based authorization** with policies
- **Rate limiting** to prevent abuse
- **Input validation** with FluentValidation
- **HTTPS enforcement** in production
- **CSRF protection** with anti-forgery tokens
- **Activity logging** for audit trails
- **Secure password policies**

## 📝 Recent Updates

### Version 2.0 - Major Feature Release
- ✅ **SuperAdmin Role Switching**: Switch between role views for testing
- ✅ **Book Review System**: Complete review and rating functionality
- ✅ **UI Consistency Overhaul**: Unified design system across all roles
- ✅ **Pure CSS Components**: Removed Bootstrap dependencies
- ✅ **Enhanced Security**: Refresh tokens, activity logging, rate limiting
- ✅ **Order Management**: Complete order lifecycle with status tracking
- ✅ **Email Integration**: MailKit for transactional emails
- ✅ **Performance Improvements**: Caching, optimized queries, image processing

### Upcoming Features
- 🔄 **Dark Mode Support**: Theme switching capability
- 🔄 **Advanced Analytics**: Sales reports and user behavior insights
- 🔄 **Inventory Management**: Stock alerts and automated reordering
- 🔄 **Payment Integration**: Stripe/PayPal payment processing
- 🔄 **Mobile App**: React Native companion app

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details.

### Development Setup
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Add tests for new functionality
5. Ensure all tests pass (`dotnet test`)
6. Commit your changes (`git commit -m 'Add amazing feature'`)
7. Push to the branch (`git push origin feature/amazing-feature`)
8. Open a Pull Request

### Code Standards
- Follow C# coding conventions
- Add XML documentation for public APIs
- Include unit tests for new features
- Update README for significant changes

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

## 💬 Support & Contact

- **Email**: [work.gurpreetsw@gmail.com](mailto:work.gurpreetsw@gmail.com)
- **GitHub Issues**: [Report bugs or request features](https://github.com/Gurry-12/OnlineBookManagementSystem/issues)
- **Documentation**: Check the `/Markdowns` folder for detailed guides

## 🙏 Acknowledgments

- **ASP.NET Core Team** for the excellent framework
- **Entity Framework Core** for robust data access
- **Serilog** for structured logging
- **Bootstrap** for responsive UI components
- **Community Contributors** for feedback and improvements

---

**Whispering Pages** - *Where every book tells a story, and every story finds its reader.*

