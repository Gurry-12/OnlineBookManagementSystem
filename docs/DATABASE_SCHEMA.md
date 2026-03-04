# Database Schema - Whispering Pages

This document details the database structure and relationships for the Book Management System.

## Entity Relationship Diagram (Conceptual)
- **Users** (1) <---> (N) **Orders**
- **Users** (1) <---> (N) **BookReviews**
- **Users** (1) <---> (N) **UserFavorites**
- **Categories** (1) <---> (N) **Books**
- **Books** (1) <---> (N) **OrderDetails**
- **Books** (1) <---> (N) **BookReviews**
- **Orders** (1) <---> (N) **OrderDetails**

## Primary Tables

### Users (Identity-based)
- `Id`: Primary Key
- `Name`: User's full name
- `IsPendingApproval`: Boolean for role-based access
- `CreatedAt`/`UpdatedAt`: Metadata

### Books
- `Id`: Primary Key
- `Title`: string (Indexed)
- `Author`: string
- `Price`: Money (Value Object: Amount, Currency)
- `StockQuantity`: integer
- `CategoryId`: Foreign Key
- `ISBN`: string (Unique)

### Orders
- `Id`: Primary Key
- `UserId`: Foreign Key
- `OrderDate`: DateTime
- `TotalAmount`: Money
- `Status`: Enum (Pending, Processing, Completed, Cancelled)

### Categories
- `Id`: Primary Key
- `Name`: string
- `Description`: string

### ActivityLogs
- `Id`: Primary Key
- `UserId`: Foreign Key (Nullable)
- `Action`: string
- `Message`: string
- `Timestamp`: DateTime
- `IpAddress`: string

## Migrations History
The system uses **EF Core Migrations**. Key migrations include:
- `InitialCreate`: Core tables and identity.
- `AddActivityLogging`: Table for user tracking.
- `FixOrderDetailMoneyColumns`: Value object mapping refinement.
