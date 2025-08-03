# Lived Experience Research - Dashboard
## Project Structure
5 projects are defined - 

### LERD_Backend
This is the main entry point for the system. It exposes RESTful API endpoints using ASP.NET Core Web API.
<br><br>Responsibilities:
- HTTP request handling
- Routing and Controllers
- Authentication and API response formatting
- Swagger/OpenAPI support

### LERD.Appliction
This project contains the core business logic interfaces and service implementations. It acts as the intermediary between the controllers (in Backend) and infrastructure (data source).
<br><br>Responsibilities:
- Defining service interfaces
- Implementing application services
- Coordinating between domain models and data providers
- Performing data filtering, validation and transformations

### LERD.Domain
Defines the core domain models (entities) that represent the business concepts in the system.
<br><br>Responsibilities:
- Declaring POCO classes like `SurveyData`, `User`, `Service`
- Establishing strong typing for data
- Providing a stable core for use across all other layers

### LERD.Infrastructure
Contains implementations for external operations such as reading survey data from files, calling Python scripts, or accessing external APIs or storage.
<br><br>Responsibilities:
- Implementing data access (e.g., reading JSON files from storage)
- Executing Python scripts for Qualtrics data extraction
- Handling filesystem, I/O or network operations
- Supporting future database access, caching or logging

### LERD.Shared
Provides shared types, DTOs, constants and helper methods used across all other projects.
<br><br>Responsibilities:
- Data Transfer Objects
- Utility functions (e.g., data conversion, config parsing)
- Constants and configuration keys
- Shared enums or error codes

## Project References
Backend: depends on all projects

Application: depends on Domain, Infrastructure, Shared

Infrastructure: depends on Domain, Shared

Domain: depends on Shared