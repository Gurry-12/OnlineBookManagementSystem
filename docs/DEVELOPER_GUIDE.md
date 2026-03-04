# Developer & Deployment Guide - Whispering Pages

This guide covers the technical setup, development workflow, and deployment procedures for developers.

## Prerequisites
- **.NET 9.0 SDK**
- **Entity Framework Core Tools** (`dotnet tool install --global dotnet-ef`)
- **SQLite** (for local development)
- **Visual Studio 2022** or **VS Code**

## Development Setup
1. **Clone the Repo:**
   ```bash
   git clone https://github.com/Gurry-12/OnlineBookManagementSystem.git
   ```
2. **Restore Dependencies:**
   ```bash
   dotnet restore
   ```
3. **Database Setup:**
   The project uses SQLite. Migrations are included in the repository.
   ```bash
   dotnet ef database update
   ```
4. **Run the Application:**
   ```bash
   dotnet watch run
   ```

## Project Structure
- `Core/`: Domain Entities, Interfaces, Logic.
- `Infrastructure/`: DB Context, Service Implementations, Migrations.
- `Presentation/`: MVC Controllers, Razor Views, static assets.
- `Shared/`: Common constants and extensions.

## Database Seeding
To seed the database with test data:
1. Ensure `EnableDatabaseSeeding` is set to `true` in `appsettings.json`.
2. Delete the existing `whisperingpages.db` file if you want a fresh start.
3. Run the application; seeding happens automatically on startup.

## Deployment
### Publishing
To create a release build:
```bash
dotnet publish -c Release -o ./publish
```

### Environment Configuration
- Set `ASPNETCORE_ENVIRONMENT` to `Production`.
- Update connection strings in `appsettings.json`.
- Configure your SMTP settings for email notifications.
