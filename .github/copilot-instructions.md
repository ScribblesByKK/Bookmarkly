# GitHub Copilot Instructions for Bookmarkly

## Repository Overview

Bookmarkly is a bookmark management application for Windows built with:
- **Language**: C# 12+ (.NET 10)
- **UI Framework**: Windows App SDK (WinUI 3)
- **Target Platform**: Windows 10/11 (minimum version 10.0.19041.0)
- **Architecture**: MVVM pattern with clean architecture principles
- **Packaging**: MSIX for Windows Store distribution

### Project Structure

```
Bookmarkly/
├── Bookmarkly.App/              # Main WinUI 3 application (platform-specific)
├── Bookmarkly.Views/            # UI view components (WinUI 3)
├── Bookmarkly.ViewModels/       # View models (MVVM pattern)
├── Bookmarkly.Abstractions/     # View abstractions
├── Bookmarkly.Entities.Abstractions/  # Domain entities
├── Bookmarkly.Library.Abstractions/   # Business logic abstractions
├── Instapaper/                  # Instapaper integration implementation
│   ├── Instapaper.Entities/     # Instapaper-specific entities
│   └── Instapaper.Library/      # Instapaper API client
├── Megakat/                     # Utility library (git submodule)
├── Tests/
│   └── ArchitectureTests/       # Architecture tests using ArchUnit
└── build/                       # Build scripts and version management
```

## Development Environment Setup

### Prerequisites

- **Required for full build**:
  - Windows 10/11
  - .NET 10 SDK or later
  - Visual Studio 2022 17.9+ (for MSBuild)
  - Git (for submodule management)

- **Linux/macOS limitations**:
  - Can build platform-agnostic projects (ViewModels, Entities, Libraries)
  - Cannot build WinUI 3 projects (App, Views) - requires Windows
  - Architecture tests can run on any platform

### Initial Setup

1. **Clone with submodules**:
   ```bash
   git clone --recurse-submodules https://github.com/Kumara-Krishnan/Bookmarkly.git
   cd Bookmarkly
   ```

2. **Or initialize submodules after clone**:
   ```bash
   git submodule update --init --recursive
   ```

3. **Restore dependencies** (Windows only for full solution):
   ```bash
   dotnet restore Bookmarkly.slnx
   ```

### Git Submodules

The `Megakat` submodule contains shared utility projects. Important commands:

- **Update to latest**: `git submodule update --remote Megakat`
- **After pulling**: `git submodule update --init --recursive`
- **Check status**: `git submodule status`

**CI/CD Note**: GitHub Actions workflows require a PAT with `repo` scope stored as `GH_PAT` secret to access the private Megakat submodule. The workflow falls back to `GITHUB_TOKEN` if `GH_PAT` is not available.

## Building and Testing

### Building

**Full solution (Windows only)**:
```bash
dotnet build Bookmarkly.slnx --configuration Release
```

**Platform-agnostic projects only**:
```bash
dotnet build Bookmarkly.ViewModels/Bookmarkly.ViewModels.csproj
dotnet build Tests/ArchitectureTests/ArchitectureTests.csproj
```

**MSIX package (Windows only)**:
See `.github/workflows/msix-build-release.yml` for the complete MSBuild command with signing.

### Testing

**Run architecture tests**:
```bash
cd /home/runner/work/Bookmarkly/Bookmarkly
dotnet test Tests/ArchitectureTests/ArchitectureTests.csproj --configuration Release
```

**Run tests with coverage** (as in CI):
```bash
dotnet test Tests/ArchitectureTests/ArchitectureTests.csproj \
  --configuration Release \
  --verbosity normal \
  /p:CollectCoverage=true \
  /p:CoverletOutputFormat=cobertura \
  /p:CoverletOutput=./coverage/coverage.cobertura.xml
```

## Code Style and Conventions

The project uses `.editorconfig` for consistent code style. Key conventions:

### C# Conventions
- **Indentation**: 4 spaces, no tabs
- **Line endings**: CRLF (Windows)
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`)
- **Implicit usings**: Enabled
- **Naming**:
  - Interfaces: `IPascalCase` (prefix with `I`)
  - Classes/structs/enums: `PascalCase`
  - Methods/properties: `PascalCase`
  - Local variables: `camelCase`
  - Use `var` for built-in types and when type is apparent
- **Braces**: Always use braces for code blocks (Allman style - new line)
- **Organize usings**: System directives first, alphabetically sorted
- **Expression bodies**: Use for properties and lambdas, avoid for methods

### XAML Conventions
- **Indentation**: 4 spaces
- **Attribute alignment**: Follow existing patterns in the codebase

### Comments
- Add XML documentation for public APIs
- Use `//` for inline comments sparingly - prefer self-documenting code
- Avoid redundant comments that restate the code

## Architecture Patterns

### Project Dependencies

Follow clean architecture dependency rules:
- **Abstractions** → No dependencies (pure interfaces/contracts)
- **Entities** → May depend on Entities.Abstractions only
- **Libraries** → May depend on Abstractions and Entities
- **ViewModels** → May depend on Abstractions, Entities, Libraries
- **Views** → May depend on ViewModels and Abstractions
- **App** → May depend on all layers

**Implementation plugins** (e.g., Instapaper) should depend only on Abstractions and implement the contracts.

### Package Management

This project uses **Central Package Management**:
- Versions are defined in `Directory.Packages.Props` at the root
- Project files reference packages WITHOUT versions: `<PackageReference Include="PackageName" />`
- To add a new package:
  1. Add to `Directory.Packages.Props` with version
  2. Reference in project file without version
  3. **Always run security checks** before adding new dependencies

### Common Packages
- `Microsoft.WindowsAppSDK`: Windows App SDK runtime
- `Microsoft.Windows.CsWinRT`: C#/WinRT projections
- `TngTech.ArchUnitNET`: Architecture testing
- `coverlet.msbuild`: Code coverage

## Version Management

The project uses automated semantic versioning: `major.yyww.build.revision`

- **Format**: `1.2547.3.0` = major 1, week 47 of 2025, 3rd build, revision 0
- **Managed by**: `build/update-version.ps1` (runs in CI)
- **Storage**: `build/version.json` and `Bookmarkly.App/Package.appxmanifest`
- **Auto-increments**: Build number within same week
- **Manual updates**: Edit `build/version.json` to change major/revision

See `build/VERSIONING.md` for complete details.

## CI/CD Workflows

### Test Workflow (`.github/workflows/test.yml`)

**Triggers**: Push to `main`, `ci`; PRs to `main`, `ci`, `dev`

**Jobs**:
1. **test**: Runs architecture tests with coverage on Windows
2. **coverage**: Generates coverage reports and updates badge (main/ci branches only)

**Artifacts**:
- `dotnet-build-binlog`: MSBuild binary logs (7 days)
- `coverage-report`: Cobertura coverage XML (1 day)

### Build & Release Workflow (`.github/workflows/msix-build-release.yml`)

**Triggers**: Push to `main`, `ci` branches only

**Jobs**:
1. **bump-version**: Updates version, commits with `[skip ci]`
2. **build**: Creates MSIX package with temporary signing certificate
3. **package**: Zips build output
4. **release**: Creates GitHub release with artifacts (main/ci only)

**Artifacts**:
- `msbuild-binlog`: MSBuild binary logs (7 days)
- `build-output`: MSIX packages (1 day)
- `Bookmarkly_{version}`: Release zip

## Workflow Changes

When making changes to GitHub Actions workflows (`.github/workflows/*.yml`):

1. **Always sync main into ci branch first**
   ```bash
   git checkout ci
   git merge main
   git push
   ```

2. **Target PRs to the ci branch instead of main**
   - Create your feature branch from `ci`
   - Make workflow changes
   - Open PR targeting `ci` branch (not `main`)

3. **Test workflow changes in the ci branch**
   - The workflows are configured to run on the `ci` branch
   - This allows testing workflow changes before merging to `main`
   - Verify the workflow runs successfully in the ci branch

4. **After testing, merge ci to main**
   - Once workflow changes are validated in `ci`, merge `ci` to `main`
   - This ensures only tested workflow changes reach the main branch

### Rationale

This approach provides:
- **Safety**: Test workflow changes without affecting main branch
- **Isolation**: Keep experimental workflow changes separate
- **Validation**: Ensure workflows work before merging to main
- **CI/CD Best Practices**: Simulate production-like testing environment

## Common Development Tasks

### Adding a New Feature

1. Create a feature branch: `git checkout -b feature/your-feature-name`
2. Implement following MVVM pattern:
   - Add interface to appropriate Abstractions project
   - Add entity to Entities project (if needed)
   - Implement logic in Library project
   - Create ViewModel in ViewModels project
   - Add View in Views project (XAML + code-behind)
3. Write or update architecture tests
4. Test locally on Windows
5. Create PR to `main` (or `ci` for workflow changes)

### Adding a New Package Dependency

1. **Check security first**: Use GitHub's security tools or check advisory databases
2. Add to `Directory.Packages.Props`:
   ```xml
   <PackageVersion Include="PackageName" Version="1.2.3" />
   ```
3. Reference in project file(s) WITHOUT version:
   ```xml
   <PackageReference Include="PackageName" />
   ```
4. Run `dotnet restore`
5. Document any setup requirements in code comments or README

### Troubleshooting Build Issues

- **Submodule issues**: Run `git submodule update --init --recursive`
- **Package version errors**: Ensure version is in `Directory.Packages.Props` and project reference has no version attribute
- **Windows SDK errors**: Check `Directory.Build.Props` for target framework versions
- **MSIX packaging errors**: Check build logs (`.binlog` files) with MSBuild Structured Log Viewer

## Testing Guidelines

### Architecture Tests

Located in `Tests/ArchitectureTests/`:
- Use TngTech.ArchUnitNET framework
- Verify architectural boundaries and dependencies
- Test naming conventions
- Validate layer separation

### Writing New Tests

- Use MSTest framework (`[TestClass]`, `[TestMethod]`)
- Follow AAA pattern: Arrange, Act, Assert
- Use descriptive test names: `MethodName_Scenario_ExpectedBehavior`
- Keep tests isolated and independent
- Mock external dependencies

### Test Coverage

- Target: Maintain or improve current coverage
- Coverage reports generated in CI for main/ci branches
- View coverage details in PR comments
- Badge: ![Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/Kumara-Krishnan/bookmarkly-coverage-badge/raw/coverage.json)

## Documentation

When making code changes:
- Update README.md if adding user-facing features
- Update CONTRIBUTING.md if changing development workflow
- Update inline XML documentation for public APIs
- Keep copilot-instructions.md synchronized with project changes

## Security Practices

- **Never commit secrets** (API keys, passwords, certificates)
- Use GitHub Secrets for CI/CD sensitive values
- Temporary certificates in CI are created and deleted within workflow
- Review dependency vulnerabilities before adding packages
- Keep dependencies up to date

## Tips for Copilot Coding Agent

### Ideal Tasks
✅ Bug fixes in existing code
✅ Adding tests for existing functionality
✅ Refactoring within existing patterns
✅ Updating documentation
✅ Adding new features following MVVM pattern
✅ Dependency updates

### Challenging Tasks
⚠️ Major architectural changes (requires human review)
⚠️ Windows-specific UI components (requires testing on Windows)
⚠️ MSIX packaging changes (requires Windows build environment)
⚠️ Certificate and signing modifications

### Before Starting
1. Understand the MVVM pattern and project structure
2. Check existing similar implementations as examples
3. Review `.editorconfig` for code style
4. Plan minimal changes that follow existing patterns
5. Consider testability and architectural boundaries

### During Development
- Make incremental changes, test frequently
- Follow existing naming and code organization
- Respect architectural boundaries (Abstractions → Entities → Libraries → ViewModels → Views)
- Add or update tests when modifying behavior
- Use XML documentation for public APIs

### Before Completing
- Build and test on appropriate platform (Windows for UI, any for libraries)
- Run architecture tests to verify dependencies
- Update documentation if needed
- Review changes against `.editorconfig` style
- Check that changes are minimal and focused
