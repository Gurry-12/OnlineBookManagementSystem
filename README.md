# 📚 Book Management System

An ASP.NET Core MVC-based Book Management System that supports secure JWT-based authentication, role-based access control (RBAC), and dynamic client-side rendering. Users can browse, manage, and interact with books in various ways depending on their role (Admin/User).

## 🔧 Technologies Used

- **ASP.NET Core MVC (.NET 8)**
- **Entity Framework Core (Code-First)**
- **SQL Server**
- **JavaScript + jQuery**
- **HTML5 + CSS3**
- **JWT (JSON Web Tokens)**
- **Bootstrap (UI layout)**

---

## ✨ Features

### 🔐 Authentication & Authorization

- Login system using JWT with secure token handling on the client side.
- Role-based access: Admins and Users have different views and permissions.

### 📖 Book Management

- Admins can **add, update, delete**, and **view all books**.
- Users can **view book details** in a dynamic grid layout.
- Books are dynamically loaded from the database using API endpoints.

### 🛒 Shopping Cart

- Users can add books to their cart.
- View and manage cart items.
- Quantity tracking per item.

### ❤️ Favorites

- Mark/unmark favorite books.
- Favorites view similar to cart, using same card/grid layout.

### 📂 Dashboard & Layout

- Admin and User dashboards have distinct sidebar navigation and top bar.
- Responsive layout with search bar and category filtering.

---

## 📁 Project Structure (Overview)

```
OnlineBookManagementSystem/
│
├── Controllers/
│   ├── AuthController.cs
│   ├── BooksController.cs
│   ├── CartController.cs
│
├── Models/
│   ├── Book.cs
│   ├── User.cs
│   ├── ShoppingCart.cs
│
├── Views/
│   ├── Auth/
│   │   ├── Login.cshtml
│   │   └── Register.cshtml
│   ├── Books/
│   │   ├── DisplayBookDetails.cshtml
│   │   ├── Admin/
│   │   │   ├── AdminIndex.cshtml
│   │   │   ├── CreateBookData.cshtml
│   │   │   └── UserList.cshtml
│   │   ├── User/
│   │   │   ├── Favorite.cshtml
│   │   │   ├── Favorite.cshtml.cs
│   │   │   └── UserIndex.cshtml
│   ├── Cart/
│   │   └── Index.cshtml
│
├── wwwroot/
│   ├── css/
│   ├── js/
│   ├── images/
│
├── appsettings.json
├── Program.cs
```

---

## 🚀 Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server
- Visual Studio / VS Code

### Steps to Run

1. **Clone the Repository**

   ```bash
   git clone https://github.com/Gurry-12/OnlineBookManagementSystem.git
   cd OnlineBookManagementSystem
   ```

2. **Configure Database Connection**\
   Update the `appsettings.json` with your SQL Server connection string.

3. **Apply Migrations and Create Database**\
   Run the following commands in the terminal:

   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

4. **Run the App**

   ```bash
   dotnet run
   ```

---

## 👤 Roles & Permissions

| Feature          | Admin | User |
| ---------------- | ----- | ---- |
| View Books       | ✅     | ✅    |
| Add/Edit Books   | ✅     | ❌    |
| Delete Books     | ✅     | ❌    |
| Add to Cart      | ❌     | ✅    |
| Mark as Favorite | ❌     | ✅    |

## 💃 License

This project is licensed under the MIT License.

---

## 💬 Contact

For feedback or support, contact [work.gurpreetsw@gmail.com](mailto\:work.gurpreetsw@gmail.com).


# Advanced Architecture Refactor

This branch (`feature/advanced-architecture`) implements a strict separation of concerns based on Clean Architecture and SOLID principles.

## Architecture Overview

### Controllers
Controllers are now split by role in `Controllers/SuperAdmin`, `Controllers/Admin`, and `Controllers/User`.
Each controller exposes only the endpoints relevant to that role and uses `[Authorize(Policy = "...")]` to enforce access.

**Key Changes:**
- **No Direct EF Access:** Controllers never touch `DbContext`. They rely solely on Services.
- **DTOs:** Request and response bodies use DTOs (`Models/DTOs`), not EF entities.

### Services
Services are similarly split into `Services/SuperAdmin`, `Services/Admin`, and `Services/User`.
Shared logic (like image processing or generic mapping) is extracted to `Services/Common/BookManager.cs`.

### Repositories
Data access is encapsulated in `Interfaces/Repositories` and `Services/Repositories`.
Services depend on `IBookRepository` and `IOrderRepository`.

### Frontend
A new helper `wwwroot/js/authFetch.js` is available to automatically inject the JWT Bearer token from `localStorage` ('jwtToken') into fetch requests.

## How to Run

1. **Build:** `dotnet build`
2. **Run:** `dotnet run`
3. **Login:** Log in as SuperAdmin/Admin. The Access Token is stored in `localStorage` and injected via `authFetch`.

## Testing

### Unit Tests
A new test project `OnlineBookManagementSystem.Tests` includes unit tests for the controllers.
Run: `dotnet test`

### Manual Verification
- Navigate to SuperAdmin Dashboard.
- Click 'Test authFetch'.
- Verify successful API call to `api/superadmin/books`.
