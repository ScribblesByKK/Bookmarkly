# Contributing to Bookmarkly

Thank you for your interest in contributing to Bookmarkly! This guide will help you get started.

## Setting Up Your Development Environment

### Prerequisites

- .NET 10 SDK or later
- Windows 10/11 (required for building the full application)
- Git

### Initial Setup

1. **Clone the repository with submodules:**

   ```bash
   git clone --recurse-submodules https://github.com/Kumara-Krishnan/Bookmarkly.git
   cd Bookmarkly
   ```

   If you've already cloned the repository without submodules, initialize them:

   ```bash
   git submodule update --init --recursive
   ```

2. **Restore dependencies:**

   ```bash
   dotnet restore
   ```

### Working with Submodules

This project uses the [Megakat](https://github.com/Kumara-Krishnan/Megakat) repository as a submodule for utility projects.

#### Updating Submodules

To update the submodule to the latest commit:

```bash
git submodule update --remote Megakat
```

#### After Pulling Changes

If the submodule reference has been updated in the main repository:

```bash
git pull
git submodule update --init --recursive
```

### Building the Project

The full application requires Windows to build:

```bash
dotnet build
```

For more details, see the [build workflow](.github/workflows/msix-build-release.yml).

### Running Tests

Tests can be run on any platform:

```bash
dotnet test Tests/ArchitectureTests/ArchitectureTests.csproj
```

### Making Changes

1. Create a new branch for your changes:
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. Make your changes and commit them with clear, descriptive commit messages.

3. Push your changes and create a pull request.

### Code Style

This project uses `.editorconfig` to maintain consistent code style. Please ensure your IDE respects these settings.

## GitHub Actions Setup

For repository maintainers: The GitHub Actions workflows require access to the private Megakat submodule. 

### Required Secrets

To enable the workflows to check out the Megakat submodule, you need to configure a Personal Access Token (PAT):

1. Create a GitHub Personal Access Token with `repo` scope
2. Add it as a repository secret named `GH_PAT`

The workflows will use `GH_PAT` if available, otherwise they'll fall back to the default `GITHUB_TOKEN` (which may not have access to private submodules).

## Questions?

If you have questions or need help, please open an issue on GitHub.
