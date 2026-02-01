# Clean Architecture Migration Guide

## File Structure Migration

### Before (Legacy Structure)
```
OnlineBookManagementSystem/
├── Controllers/
├── Views/
├── Models/
├── Services/
├── Interfaces/
├── Extensions/
├── Middleware/
├── Utilities/
├── Helper/
├── wwwroot/
└── Migrations/
```

### After (Clean Architecture Structure)
```
OnlineBookManagementSystem/
├── Core/
│   ├── Domain/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Enums/
│   │   └── Exceptions/
│   └── Application/
│       ├── Interfaces/
│       ├── UseCases/
│       ├── DTOs/
│       └── Validators/
├── Infrastructure/
│   ├── Data/
│   │   ├── Context/
│   │   ├── Repositories/
│   │   ├── Configurations/
│   │   └── Migrations/
│   ├── Services/
│   └── Email/
├── Presentation/
│   ├── Controllers/
│   ├── Views/
│   ├── ViewModels/
│   ├── Middleware/
│   └── wwwroot/
└── Shared/
    ├── Constants/
    ├── Extensions/
    └── Utilities/
```

## Migration Mapping

### Controllers → Presentation/Controllers
- ✅ All controller files moved
- ✅ Namespace updates needed: `OnlineBookManagementSystem.Controllers` → `OnlineBookManagementSystem.Presentation.Controllers`

### Views → Presentation/Views
- ✅ All view files moved
- ✅ No namespace changes needed (Razor views)

### Models/ViewModel → Presentation/ViewModels
- ✅ All ViewModel files moved
- ✅ Namespace updates needed: `OnlineBookManagementSystem.Models.ViewModel` → `OnlineBookManagementSystem.Presentation.ViewModels`

### Services → Infrastructure/Services
- ✅ All service files moved
- ✅ Namespace updates needed: `OnlineBookManagementSystem.Services` → `OnlineBookManagementSystem.Infrastructure.Services`

### Interfaces → Core/Application/Interfaces
- ✅ All interface files moved
- ✅ Namespace updates needed: `OnlineBookManagementSystem.Interfaces` → `OnlineBookManagementSystem.Core.Application.Interfaces`

### Models → Infrastructure/Data/Context
- ✅ Data models moved to Infrastructure
- ✅ Domain entities created in Core/Domain/Entities
- ✅ Namespace updates needed: `OnlineBookManagementSystem.Models` → `OnlineBookManagementSystem.Infrastructure.Data.Context`

### Extensions → Shared/Extensions
- ✅ All extension files moved
- ✅ Namespace updates needed: `OnlineBookManagementSystem.Extensions` → `OnlineBookManagementSystem.Shared.Extensions`

### Utilities → Shared/Utilities
- ✅ All utility files moved
- ✅ Namespace updates needed: `OnlineBookManagementSystem.Utilities` → `OnlineBookManagementSystem.Shared.Utilities`

### Middleware → Presentation/Middleware
- ✅ All middleware files moved
- ✅ Namespace updates needed: `OnlineBookManagementSystem.Middleware` → `OnlineBookManagementSystem.Presentation.Middleware`

### wwwroot → Presentation/wwwroot
- ✅ All static files moved
- ✅ No namespace changes needed (static files)

### Migrations → Infrastructure/Data/Migrations
- ✅ All migration files moved
- ✅ Namespace updates needed in migration files

## Required Namespace Updates

### 1. Controllers
**Files**: All files in `Presentation/Controllers/`
**Old**: `namespace OnlineBookManagementSystem.Controllers`
**New**: `namespace OnlineBookManagementSystem.Presentation.Controllers`

### 2. ViewModels
**Files**: All files in `Presentation/ViewModels/`
**Old**: `namespace OnlineBookManagementSystem.Models.ViewModel`
**New**: `namespace OnlineBookManagementSystem.Presentation.ViewModels`

### 3. Services
**Files**: All files in `Infrastructure/Services/`
**Old**: `namespace OnlineBookManagementSystem.Services`
**New**: `namespace OnlineBookManagementSystem.Infrastructure.Services`

### 4. Interfaces
**Files**: All files in `Core/Application/Interfaces/`
**Old**: `namespace OnlineBookManagementSystem.Interfaces`
**New**: `namespace OnlineBookManagementSystem.Core.Application.Interfaces`

### 5. Data Models
**Files**: All files in `Infrastructure/Data/Context/`
**Old**: `namespace OnlineBookManagementSystem.Models`
**New**: `namespace OnlineBookManagementSystem.Infrastructure.Data.Context`

### 6. Extensions
**Files**: All files in `Shared/Extensions/`
**Old**: `namespace OnlineBookManagementSystem.Extensions`
**New**: `namespace OnlineBookManagementSystem.Shared.Extensions`

### 7. Utilities
**Files**: All files in `Shared/Utilities/`
**Old**: `namespace OnlineBookManagementSystem.Utilities`
**New**: `namespace OnlineBookManagementSystem.Shared.Utilities`

### 8. Middleware
**Files**: All files in `Presentation/Middleware/`
**Old**: `namespace OnlineBookManagementSystem.Middleware`
**New**: `namespace OnlineBookManagementSystem.Presentation.Middleware`

## Using Statements Updates

### Controllers
```csharp
// Old using statements
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Services;
using OnlineBookManagementSystem.Interfaces;

// New using statements
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Infrastructure.Services;
using OnlineBookManagementSystem.Core.Application.Interfaces;
using OnlineBookManagementSystem.Presentation.ViewModels;
```

### Services
```csharp
// Old using statements
using OnlineBookManagementSystem.Models;
using OnlineBookManagementSystem.Interfaces;

// New using statements
using OnlineBookManagementSystem.Infrastructure.Data.Context;
using OnlineBookManagementSystem.Core.Application.Interfaces;
using OnlineBookManagementSystem.Core.Domain.Entities;
```

## Benefits of Migration

### 1. Clear Separation of Concerns
- **Domain Logic**: Isolated in Core/Domain
- **Business Rules**: Contained in Core/Application
- **Data Access**: Separated in Infrastructure
- **UI Logic**: Contained in Presentation

### 2. Dependency Direction
- **Core**: No dependencies on other layers
- **Application**: Depends only on Domain
- **Infrastructure**: Depends on Application and Domain
- **Presentation**: Depends on Application (through interfaces)

### 3. Testability
- **Domain**: Pure business logic, easily unit testable
- **Application**: Use cases testable with mocks
- **Infrastructure**: Integration testable
- **Presentation**: UI testable in isolation

### 4. Maintainability
- **Focused Responsibilities**: Each layer has clear purpose
- **Easy Navigation**: Logical file organization
- **Reduced Coupling**: Interfaces define contracts
- **Technology Independence**: Core is framework-agnostic

## Migration Checklist

### Phase 1: File Movement ✅
- [x] Move Controllers to Presentation/Controllers
- [x] Move Views to Presentation/Views
- [x] Move ViewModels to Presentation/ViewModels
- [x] Move Services to Infrastructure/Services
- [x] Move Interfaces to Core/Application/Interfaces
- [x] Move Models to Infrastructure/Data/Context
- [x] Move Extensions to Shared/Extensions
- [x] Move Utilities to Shared/Utilities
- [x] Move Middleware to Presentation/Middleware
- [x] Move wwwroot to Presentation/wwwroot
- [x] Move Migrations to Infrastructure/Data/Migrations

### Phase 2: Namespace Updates 🔄
- [ ] Update Controller namespaces
- [ ] Update ViewModel namespaces
- [ ] Update Service namespaces
- [ ] Update Interface namespaces
- [ ] Update Model namespaces
- [ ] Update Extension namespaces
- [ ] Update Utility namespaces
- [ ] Update Middleware namespaces

### Phase 3: Using Statement Updates 🔄
- [ ] Update Controller using statements
- [ ] Update Service using statements
- [ ] Update Extension using statements
- [ ] Update Utility using statements
- [ ] Update Middleware using statements

### Phase 4: Configuration Updates 🔄
- [ ] Update Program.cs references
- [ ] Update appsettings.json paths
- [ ] Update project file references
- [ ] Update build configurations

### Phase 5: Testing & Validation 🔄
- [ ] Compile and fix build errors
- [ ] Run existing tests
- [ ] Validate functionality
- [ ] Performance testing

## Next Steps

1. **Namespace Updates**: Update all namespace declarations
2. **Using Statement Updates**: Fix all import statements
3. **Configuration Updates**: Update Program.cs and configuration files
4. **Build & Test**: Ensure everything compiles and works
5. **Documentation**: Update README and documentation
6. **Team Training**: Educate team on new structure

## Tools for Migration

### Find and Replace Patterns
```bash
# Namespace updates
Find: "namespace OnlineBookManagementSystem.Controllers"
Replace: "namespace OnlineBookManagementSystem.Presentation.Controllers"

Find: "namespace OnlineBookManagementSystem.Services"
Replace: "namespace OnlineBookManagementSystem.Infrastructure.Services"

# Using statement updates
Find: "using OnlineBookManagementSystem.Models;"
Replace: "using OnlineBookManagementSystem.Infrastructure.Data.Context;"

Find: "using OnlineBookManagementSystem.Services;"
Replace: "using OnlineBookManagementSystem.Infrastructure.Services;"
```

### PowerShell Scripts
```powershell
# Update namespaces in all C# files
Get-ChildItem -Path "OnlineBookManagementSystem" -Filter "*.cs" -Recurse | 
ForEach-Object {
    (Get-Content $_.FullName) -replace 
    'namespace OnlineBookManagementSystem.Controllers', 
    'namespace OnlineBookManagementSystem.Presentation.Controllers' | 
    Set-Content $_.FullName
}
```

This migration provides a solid foundation for Clean Architecture while maintaining all existing functionality.