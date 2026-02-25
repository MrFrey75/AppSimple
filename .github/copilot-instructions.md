# AppSimple - Copilot Instructions

## Project Structure

```
AppSimple/
├── docs/                          # Additional documentation, design decisions, architectural diagrams
├── src/
│   ├── AppSimple.Core/             # Core business logic, domain models, interfaces, extensions, convereters, and other shared code. Enums, constants, and common utilities should also go here. 
│   ├── AppSimple.DataLib/          # Data access library (Dapper + SQLite), implements Core interfaces
│   ├── AppSimple.WebApi/           # Future ASP.NET Core Web API project
│   ├── AppSimple.AdminCli/         # Future Admin CLI project for seeding admin user and testing services
│   ├── AppSimple.WebApp/           # Future ASP.NET Core MVC web app project for user GUI
│   └── AppSimple.sln
├── .github/
│   └── copilot-instructions.md     # Instructions for code generation and conventions
└── README.md
```

## Key Conventions

### Documentation

- Use XML documentation comments (`///`) for all public members and classes.
- Provide clear and concise summaries of what the class/method/property does, its parameters, and return values.
- For complex methods, include examples in the documentation to illustrate usage.

### Additional Docs

- Add `docs/` folder for more detailed documentation, design decisions, and architectural diagrams.

### Naming & Modern C# Features
* **PascalCase**: Use for class names, method names, and properties.
* **camelCase**: Use for local variables and method parameters.
* **File-Scoped Namespaces**: Use `namespace AppSimple.Name;` to reduce indentation and vertical clutter.
* **NO Primary Constructors**: Avoid using primary constructors for now to keep things simple and explicit.
* **Required Members**: Use the `required` keyword for properties that must be initialized during object creation.

### Models
* **Location**: Place domain models in the `Models` folder within `AppSimple.Core`.
* **POCOs**: Use simple classes with properties and minimal logic.
* **BaseEntity Inheritance**: All models must inherit from `BaseEntity`.
    * **Uid**: Generated as a new GUID when creating a new entity (utilize `Guid.CreateVersion7()` for database performance).
    * **CreatedAt / UpdatedAt**: Set automatically in the service layer.
    * **IsSystem**: Set to true for system entities that should not be modified by users.

#### BaseEntity 
- `CreatedAt` and `UpdatedAt` should be set automatically in the service layer when creating or updating entities
- `Uid` should be generated as a new GUID when creating a new entity
- `IsSystem` should be set to true for system entities that should not be modified by users

### Services
* **Interfaces**: Place in the `Services` folder using the `IServiceName` convention.
* **Implementations**: Place in `Services/Impl`.
* **Logic**: Implement business logic in service classes, keeping UI/Controllers thin.

### Nullable Reference Types & Safety
* **Nullable Enabled**: Project-wide `<Nullable>enable</Nullable>` is mandatory.
* **Explicit Handling**: Always handle null cases or use nullable annotations (`?`).
* **Implicit Usings**: Enabled project-wide to remove boilerplate `using` statements.

### Technical Integrations (Free & Open Source)
* **Dapper**: High-performance Micro-ORM for SQLite mapping.
* **CommunityToolkit.Mvvm**: Standard library for MVVM support in WPF (ObservableProperty, RelayCommand).
* **SQLite**: Primary development and local database engine.
* **Microsoft.Extensions.DependencyInjection**: Standard container for managing service lifetimes.
* **Serilog**: Structured logging for better diagnostics.
* **FluentValidation**: For robust input validation in services and API layers.

### Advanced .NET 10 Patterns
* **C# 14 Field Keyword**: Use `field` in properties to replace manual backing fields.
* **Guid v7**: Always use `Guid.CreateVersion7()` for entity `Uid` generation.
* **DI Registration**: Provide static `Add[ProjectName]Services` methods for easy DI configuration.
* **Frozen Collections**: Use `System.Collections.Frozen` for immutable lookup tables.
* **Extension Blocks**: Use `extension` blocks for adding properties/static members to existing types.


