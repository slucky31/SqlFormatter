# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SqlFormatter is a Blazor Server application built with .NET 10.0 that provides SQL formatting functionality. The application uses interactive server-side rendering with Blazor's InteractiveServer render mode.

## Technology Stack

- **Framework**: ASP.NET Core Blazor Server (.NET 10.0)
- **UI Framework**: Bootstrap (included in wwwroot/lib/bootstrap)
- **Render Mode**: Interactive Server Components
- **Language Features**: C# with nullable reference types and implicit usings enabled

## Development Commands

### Build and Run
```bash
# Build the entire solution (from repository root)
dotnet build

# Run the application (from repository root)
dotnet run --project SqlFormatter/SqlFormatter.csproj

# Or navigate to the project directory first
cd SqlFormatter
dotnet run

# Run with specific profile
dotnet run --project SqlFormatter/SqlFormatter.csproj --launch-profile https
dotnet run --project SqlFormatter/SqlFormatter.csproj --launch-profile http
```

The application will be available at:
- HTTPS: https://localhost:7169
- HTTP: http://localhost:5114

### Clean and Restore
```bash
# Clean build artifacts
dotnet clean

# Restore NuGet packages
dotnet restore
```

### Testing
```bash
# Run all tests (from repository root)
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests in a specific project
dotnet test SqlFormatter.Tests/SqlFormatter.Tests.csproj
```

Test project location: `SqlFormatter.Tests/` (subdirectory at repository root level)
- Uses xUnit testing framework
- Includes test coverage collection via coverlet.collector

## Project Structure

Repository structure:
```
SqlFormatter/                    # Repository root
├── SqlFormatter.slnx           # Solution file
├── SqlFormatter/               # Main Blazor Server application
│   ├── SqlFormatter.csproj
│   ├── Program.cs
│   ├── Components/
│   ├── wwwroot/
│   └── Properties/
└── SqlFormatter.Tests/         # xUnit test project
    ├── SqlFormatter.Tests.csproj
    └── UnitTest1.cs
```

### Core Application Files
- **Program.cs**: Application entry point and service configuration. Sets up Razor Components with Interactive Server mode, configures the HTTP pipeline with HTTPS redirection, antiforgery tokens, and status code pages. Includes a partial `Program` class declaration at the end to enable testing.
- **SqlFormatter.csproj**: Project file targeting .NET 10.0 with nullable reference types enabled and `BlazorDisableThrowNavigationException` set to true.

### Components Architecture
- **Components/App.razor**: Root HTML document that hosts the Blazor application. Includes Bootstrap CSS, app styles, and scoped component styles. Uses `InteractiveServer` render mode.
- **Components/Routes.razor**: Router configuration using `Router` component with the `Program` assembly. Defines route handling with `MainLayout` as the default layout and `NotFound` page for unmatched routes.
- **Components/_Imports.razor**: Global using statements and namespace imports for all Razor components.

### Layout System
Located in `Components/Layout/`:
- **MainLayout.razor**: Main application layout wrapper
- **NavMenu.razor**: Navigation menu component
- **ReconnectModal.razor**: Modal displayed when SignalR connection is lost (includes .razor.css and .razor.js files for scoped styling and JavaScript interop)

### Pages
Located in `Components/Pages/`:
- **Home.razor**: Landing page at route "/"
- **Counter.razor**: Counter demo page
- **Weather.razor**: Weather forecast demo page
- **Error.razor**: Error handling page (used in non-development environments via exception handler)
- **NotFound.razor**: 404 page for unmatched routes

### Static Assets
- **wwwroot/**: Contains static files including Bootstrap library, app.css, and favicon.png
- Asset loading uses the `@Assets` directive for cache busting

## Key Configuration

### Launch Settings
Defined in `Properties/launchSettings.json`:
- Two profiles available: `http` and `https`
- Development environment variables set via `ASPNETCORE_ENVIRONMENT`

### Application Settings
- **appsettings.json**: Base application configuration
- **appsettings.Development.json**: Development-specific overrides

## Architecture Notes

### Blazor Server Render Mode
The application uses `InteractiveServer` render mode throughout:
- Set in `App.razor` on `<Routes>` and `<HeadOutlet>` components
- Configured in `Program.cs` via `.AddInteractiveServerComponents()` and `.AddInteractiveServerRenderMode()`
- Provides real-time interactivity through SignalR WebSocket connections

### HTTP Pipeline Configuration
The middleware pipeline in `Program.cs` includes:
1. Exception handling (non-development: redirects to /Error with error scopes)
2. HSTS (non-development only)
3. Status code pages (redirects to /not-found with status code scopes)
4. HTTPS redirection
5. Antiforgery protection
6. Static asset mapping with cache busting
7. Razor Components with Interactive Server mode

### Error Handling
- Production errors route to `/Error` page with scoped error handling
- Status code errors (like 404) route to `/not-found` with scoped status code handling
- Blazor-specific: `BlazorDisableThrowNavigationException` is enabled in project properties
