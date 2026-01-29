# Implementation Plan: Clean Code Refactoring

## Overview

This implementation plan systematically refactors the Online Book Management System to apply Clean Code principles, SOLID principles, and proper Separation of Concerns. The approach prioritizes resolving build errors first, then incrementally improves code quality while maintaining system functionality.

## Tasks

- [x] 1. Resolve Build Errors and Namespace Issues
  - [x] 1.1 Fix ViewModel namespace references in controllers
    - Update all controller using statements to reference new ViewModel namespaces
    - Fix compilation errors related to ViewModel type resolution
    - _Requirements: 1.1, 1.2_
  
  - [x] 1.2 Fix ViewModel namespace references in views
    - Update all @model directives in views to use correct ViewModel namespaces
    - Fix @using statements in views for ViewModel access
    - _Requirements: 1.1, 1.3_
  
  - [x] 1.3 Write property test for namespace resolution

    - **Property 1: Namespace Resolution Consistency**
    - **Validates: Requirements 1.2, 1.3**
  

- [x] 2. Checkpoint - Ensure solution builds successfully
  - Ensure all tests pass, ask the user if questions arise.

- [x] 3. Refactor Service Layer for Single Responsibility Principle
  - [x] 3.1 Split BookService into focused services
    - Create IBookQueryService and BookQueryService for read operations
    - Create IBookCommandService and BookCommandService for write operations
    - Create IBookValidationService and BookValidationService for validation
    - _Requirements: 2.1, 2.4_
  
  - [x] 3.2 Split UserService into focused services
    - Create IUserQueryService and UserQueryService for user queries
    - Create IUserCommandService and UserCommandService for user commands
    - Extract authentication logic to IUserAuthenticationService
    - _Requirements: 2.1, 2.5_
  
  - [x] 3.3 Refactor OrderService for single responsibility
    - Separate order creation, processing, and query operations
    - Extract payment processing logic to dedicated service
    - _Requirements: 2.1, 2.6_

- [x] 4. Implement Proper Dependency Injection
  - [x] 4.1 Update service registrations with explicit lifetimes
    - Review all service registrations in ServiceCollectionExtensions
    - Specify appropriate lifetimes (Scoped, Singleton, Transient) for each service
    - _Requirements: 3.2_
  
  - [x] 4.2 Replace concrete dependencies with abstractions
    - Identify services depending on concrete implementations
    - Create interfaces for concrete dependencies
    - Update constructor parameters to use interfaces
    - _Requirements: 3.3_
  
  - [x] 4.3 Eliminate static dependencies and service locator patterns
    - Identify and remove static service calls
    - Replace service locator usage with dependency injection
    - _Requirements: 3.4_
  
  - [ ]* 4.4 Write property tests for dependency injection compliance
    - **Property 5: Service Lifetime Registration**
    - **Property 6: Dependency on Abstractions**
    - **Property 7: No Static Dependencies**
    - **Validates: Requirements 3.2, 3.3, 3.4**

- [x] 5. Eliminate Code Duplication
  - [x] 5.1 Centralize validation logic
    - Create dedicated validator classes for each domain entity
    - Extract repeated validation logic from services and controllers
    - Implement IValidator<T> pattern for consistent validation
    - _Requirements: 4.2_
  
  - [x] 5.2 Implement consistent mapping strategy
    - Choose mapping approach (AutoMapper, manual mapping services, or extension methods)
    - Create mapping profiles or services for entity-to-DTO conversions
    - Replace scattered mapping logic with centralized approach
    - _Requirements: 4.3_
  
  - [x]* 5.3 Write property tests for code duplication elimination
    - **Property 9: Centralized Validation Logic**
    - **Property 10: Consistent Mapping Strategy**
    - **Validates: Requirements 4.2, 4.3**

- [-] 7. Refactor Repository Interfaces (Interface Segregation)
  - [x] 7.1 Split large repository interfaces
    - Create separate read and write repository interfaces
    - Split IBookRepository into IBookReadRepository and IBookWriteRepository
    - Create query-specific repository interfaces (IBookQueryRepository)
    - _Requirements: 5.1, 5.4_
  
  - [x] 7.2 Update repository implementations
    - Implement new segregated repository interfaces
    - Update existing repository classes to implement focused interfaces
    - _Requirements: 5.2_
  
  - [ ] 7.3 Update service dependencies to use segregated interfaces
    - Update service constructors to depend on specific repository interfaces
    - Ensure services only depend on repository methods they actually use
    - _Requirements: 5.2_
  
  - [ ]* 7.4 Write property tests for interface segregation
    - **Property 11: Interface Method Usage**
    - **Property 12: Repository Interface Organization**
    - **Validates: Requirements 5.2, 5.4**

- [ ] 8. Improve Method Complexity and Structure
  - [ ] 8.1 Refactor large methods in services
    - Identify methods exceeding 20 lines or high cyclomatic complexity
    - Break large methods into smaller, focused methods
    - Extract complex logic into separate methods with descriptive names
    - _Requirements: 6.1, 6.2_
  
  - [ ] 8.2 Refactor methods with many parameters
    - Identify methods with more than 5 parameters
    - Create parameter objects or use builder patterns where appropriate
    - _Requirements: 6.5_
  
  - [ ]* 8.3 Write property tests
   for method complexity
    - **Property 14: Method Length Constraint**
    - **Property 15: Cyclomatic Complexity Constraint**
    - **Property 16: Parameter Count Constraint**
    - **Validates: Requirements 6.1, 6.2, 6.5**

- [ ] 9. Establish Consistent Naming Conventions
  - [ ] 9.1 Review and fix naming convention violations
    - Ensure all public members use PascalCase
    - Ensure all private members use camelCase
    - Verify all interfaces start with 'I'
    - _Requirements: 7.1, 7.2, 7.5_
  
  - [ ]* 9.2 Write property tests for naming conventions
    - **Property 17: C# Naming Conventions**
    - **Property 18: Interface Naming Convention**
    - **Validates: Requirements 7.1, 7.2, 7.5**

- [ ] 10. Enhance Layer Separation and Architecture
  - [ ] 10.1 Audit and fix layer dependencies
    - Ensure Core layer doesn't reference Infrastructure or Presentation
    - Verify Infrastructure layer only depends on Core abstractions
    - Check Presentation layer only depends on Core application interfaces
    - _Requirements: 8.1, 8.2, 8.3_
  
  - [ ] 10.2 Clean up domain entities
    - Remove infrastructure-specific attributes from domain entities
    - Move data annotations to separate configuration classes
    - Ensure domain entities are pure business objects
    - _Requirements: 8.4_
  
  - [ ] 10.3 Move business logic to appropriate layers
    - Identify business logic in controllers and repositories
    - Move complex business logic to Core layer services or use cases
    - _Requirements: 8.5_
  
  - [ ]* 10.4 Write property tests for architectural compliance
    - **Property 19: Core Layer Independence**
    - **Property 20: Infrastructure Layer Dependencies**
    - **Property 21: Presentation Layer Dependencies**
    - **Property 22: Domain Entity Purity**
    - **Property 23: Business Logic Placement**
    - **Validates: Requirements 8.1, 8.2, 8.3, 8.4, 8.5**

- [ ] 11. Implement Comprehensive Error Handling
  - [ ] 11.1 Create domain exception hierarchy
    - Implement base DomainException class
    - Create specific exception types (ValidationException, BusinessRuleException)
    - _Requirements: 9.2_
  
  - [ ] 11.2 Implement global exception handling middleware
    - Create GlobalExceptionMiddleware for centralized error handling
    - Handle different exception types appropriately
    - Ensure sensitive information is not exposed in error messages
    - _Requirements: 9.1, 9.5_
  
  - [ ] 11.3 Implement structured validation results
    - Create ValidationResult and ValidationError classes
    - Update validation methods to return structured results
    - _Requirements: 9.3_
  
  - [ ] 11.4 Add comprehensive logging
    - Add logging to all exception handling blocks
    - Use appropriate log levels for different error types
    - _Requirements: 9.4_
  
  - [ ]* 11.5 Write property tests for error handling
    - **Property 24: Exception Handling at Boundaries**
    - **Property 25: Domain Exception Usage**
    - **Property 26: Structured Validation Results**
    - **Property 27: Error Logging Implementation**
    - **Property 28: Secure Error Messages**
    - **Validates: Requirements 9.1, 9.2, 9.3, 9.4, 9.5**

- [ ] 12. Improve Testability
  - [ ] 12.1 Ensure all dependencies are abstracted
    - Verify all service dependencies are interfaces
    - Create interfaces for any remaining concrete dependencies
    - _Requirements: 10.1, 10.3_
  
  - [ ] 12.2 Isolate business logic from infrastructure
    - Ensure business logic classes don't directly depend on infrastructure
    - Abstract external dependencies behind interfaces
    - _Requirements: 10.2_
  
  - [ ] 12.3 Clean up domain entity dependencies
    - Remove any database context or external service dependencies from domain entities
    - Ensure domain entities are completely isolated
    - _Requirements: 10.5_
  
  - [ ]* 12.4 Write property tests for testability
    - **Property 29: Testable Component Dependencies**
    - **Property 30: Business Logic Isolation**
    - **Property 31: Domain Entity Independence**
    - **Validates: Requirements 10.1, 10.2, 10.3, 10.4, 10.5**

- [ ] 13. Setup Property-Based Testing Framework
  - [ ] 13.1 Install and configure FsCheck for .NET
    - Add FsCheck NuGet package to test projects
    - Configure test runners for property-based tests
    - _Requirements: All property tests_
  
  - [ ] 13.2 Create property test base classes and utilities
    - Create helper methods for analyzing code structure
    - Implement utilities for checking naming conventions and architectural constraints
    - _Requirements: All property tests_

- [ ] 14. Final Integration and Validation
  - [ ] 14.1 Run comprehensive test suite
    - Execute all unit tests to ensure functionality is preserved
    - Run all property-based tests to verify architectural compliance
    - _Requirements: All requirements_
  
  - [ ] 14.2 Perform final code quality review
    - Verify all build errors are resolved
    - Confirm SOLID principles are applied throughout
    - Validate clean code practices are consistently implemented
    - _Requirements: All requirements_

- [ ] 15. Final checkpoint - Ensure all tests pass and system is fully functional
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Checkpoints ensure incremental validation and system stability
- Property tests validate universal correctness properties and architectural constraints
- Unit tests validate specific examples and integration points
- The refactoring maintains system functionality while systematically improving code quality