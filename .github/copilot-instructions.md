# Copilot Instructions for SqlFormatter

## Project Overview

SqlFormatter is a **Blazor Server application** built with **.NET 10.0** that provides SQL formatting functionality. This is a small, early-stage project with a clean architecture using ASP.NET Core Blazor's Interactive Server rendering mode.

**Key Technologies:**
- Framework: ASP.NET Core Blazor Server (.NET 10.0)
- UI: Bootstrap 5 (included in wwwroot/lib/bootstrap)
- Testing: xUnit with coverlet for code coverage
- Render Mode: Interactive Server Components (SignalR-based real-time communication)
- Language: C# with nullable reference types and implicit usings enabled

## Build and Validation Commands

### CRITICAL: Command Order Matters

The GitHub Actions CI pipeline runs commands in this EXACT order. Always follow this sequence to match CI behavior:

```bash
# 1. ALWAYS restore dependencies first
dotnet restore

# 2. Build in Release configuration (matches CI)
dotnet build --no-restore --configuration Release

# 3. Run tests (requires build first)
dotnet test --no-build --configuration Release --verbosity normal
```

### Verified Command Details

**Restore (REQUIRED before build):**
```bash
dotnet restore
```
- Takes ~1-2 seconds on clean environment
- MUST be run before building
- Restores NuGet packages for both SqlFormatter and SqlFormatter.Tests projects

**Build:**
```bash
# Release build (matches CI - RECOMMENDED)
dotnet build --no-restore --configuration Release

# Debug build (for local development)
dotnet build --configuration Debug
```
- Build time: ~3-6 seconds
- Outputs to `bin/Release/net10.0/` or `bin/Debug/net10.0/`
- The `--no-restore` flag assumes restore was already run
- Release configuration is what CI uses

**Test:**
```bash
# After Release build (matches CI)
dotnet test --no-build --configuration Release --verbosity normal

# Or build and test together in Debug
dotnet test --configuration Debug --verbosity normal
```
- Test execution time: ~1-2 seconds
- Currently 2 tests in SqlFormatter.Tests project
- Uses xUnit framework
- The `--no-build` flag requires matching configuration to existing build

**Run Application:**
```bash
# From repository root
dotnet run --project SqlFormatter/SqlFormatter.csproj

# With specific profile
dotnet run --project SqlFormatter/SqlFormatter.csproj --launch-profile https
dotnet run --project SqlFormatter/SqlFormatter.csproj --launch-profile http
```
- HTTPS profile: https://localhost:7169 (primary), http://localhost:5114 (fallback)
- HTTP profile: http://localhost:5114
- Uses Development environment by default (set via ASPNETCORE_ENVIRONMENT)

### .NET SDK Version

- Required: .NET 10.0 SDK (version 10.0.101 verified working)
- Target Framework: net10.0

## Project Structure

```
SqlFormatter/                          # Repository root
├── .github/
│   └── workflows/
│       └── dotnet.yml                 # CI/CD pipeline
├── SqlFormatter/                      # Main Blazor Server project
│   ├── SqlFormatter.csproj            # Project file (SDK: Microsoft.NET.Sdk.Web)
│   ├── Program.cs                     # Application entry point
│   ├── appsettings.json               # Application configuration
│   ├── appsettings.Development.json   # Development overrides
│   ├── Components/
│   │   ├── App.razor                  # Root HTML document
│   │   ├── Routes.razor               # Router configuration
│   │   ├── _Imports.razor             # Global using directives
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor       # Main layout wrapper
│   │   │   ├── NavMenu.razor          # Navigation menu
│   │   │   └── ReconnectModal.razor   # SignalR reconnection modal
│   │   └── Pages/
│   │       ├── Home.razor             # "/" route
│   │       ├── Counter.razor          # Demo page
│   │       ├── Weather.razor          # Demo page
│   │       ├── Error.razor            # Error page ("/Error")
│   │       └── NotFound.razor         # 404 page ("/not-found")
│   ├── Properties/
│   │   └── launchSettings.json        # Launch profiles (http/https)
│   └── wwwroot/                       # Static assets
│       ├── app.css                    # Application styles
│       ├── favicon.png                # Site icon
│       └── lib/bootstrap/             # Bootstrap library
├── SqlFormatter.Tests/                # xUnit test project
│   ├── SqlFormatter.Tests.csproj      # Test project file
│   └── UnitTest1.cs                   # Basic tests
└── SqlFormatter.slnx                  # Solution file (XML format)
```

## GitHub Actions CI Pipeline

**File:** `.github/workflows/dotnet.yml`

**Triggers:**
- Push to `main` or `claude` branches
- Pull requests to `main` branch

**Pipeline Steps:**
1. Checkout code (actions/checkout@v4)
2. Setup .NET 10.0.x (actions/setup-dotnet@v4)
3. `dotnet restore`
4. `dotnet build --no-restore --configuration Release`
5. `dotnet test --no-build --configuration Release --verbosity normal`

**Environment:** Ubuntu-latest

**To replicate CI locally, run the exact same commands in order.**

## Key Architecture Notes

### Blazor Server Render Mode
- Uses `InteractiveServer` render mode throughout (set in App.razor)
- Requires SignalR WebSocket connection for interactivity
- Configured via `.AddInteractiveServerComponents()` in Program.cs

### HTTP Request Pipeline (Program.cs)
The middleware pipeline is configured in this order:
1. Exception handler (non-dev: `/Error` with scoped errors)
2. HSTS (non-development only)
3. Status code pages (redirects to `/not-found` with status code scopes)
4. HTTPS redirection
5. Antiforgery protection
6. Static asset mapping (with cache busting via @Assets directive)
7. Razor Components with Interactive Server mode

### Special Configuration
- `BlazorDisableThrowNavigationException` is set to `true` in SqlFormatter.csproj
- Nullable reference types enabled globally
- Implicit usings enabled
- Program class has `public partial class Program { }` at the end to enable testing

### Testing
- Test project references main project via `<ProjectReference>`
- Tests verify basic setup and assembly references
- Uses xUnit attributes: `[Fact]` for test methods
- Coverage collector: coverlet.collector v6.0.4

## Common Pitfalls and Solutions

1. **Build fails after test**: If you build in Debug but test with `--configuration Release`, it will fail. Configurations must match when using `--no-build`.

2. **Missing restore**: Always run `dotnet restore` before building, especially in fresh environments or after modifying .csproj files.

3. **Port conflicts**: Default ports are 7169 (HTTPS) and 5114 (HTTP). If these are in use, modify Properties/launchSettings.json.

## Files to Modify for Common Tasks

- **Add new page**: Create `.razor` file in `SqlFormatter/Components/Pages/`
- **Modify layout**: Edit `SqlFormatter/Components/Layout/MainLayout.razor`
- **Add service**: Register in `Program.cs` via `builder.Services.Add...()`
- **Styling**: Modify `SqlFormatter/wwwroot/app.css` or component-scoped `.razor.css`
- **Configuration**: Edit `appsettings.json` or `appsettings.Development.json`
- **Add test**: Add test class/methods to `SqlFormatter.Tests/` directory

## Trust These Instructions

This document was created through systematic exploration and testing of the codebase. All commands have been verified to work. Only search the codebase if:
- You need specific implementation details not covered here
- You find these instructions are outdated or incorrect
- You need to understand specific business logic

For structural questions (where to add files, how to build, what the architecture is), trust this document and proceed confidently.
