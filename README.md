# Bookmarkly

[![Tests](https://github.com/Kumara-Krishnan/Bookmarkly/actions/workflows/test.yml/badge.svg?branch=main)](https://github.com/Kumara-Krishnan/Bookmarkly/actions/workflows/test.yml)
[![Build and Release](https://github.com/Kumara-Krishnan/Bookmarkly/actions/workflows/msix-build-release.yml/badge.svg)](https://github.com/Kumara-Krishnan/Bookmarkly/releases/latest)
[![Code Coverage](https://img.shields.io/endpoint?url=https://gist.githubusercontent.com/Kumara-Krishnan/bookmarkly-coverage-badge/raw/coverage.json)](https://github.com/Kumara-Krishnan/Bookmarkly/actions/workflows/test.yml)

A bookmark management application for Windows.

## Features

- Bookmark management
- Windows App SDK integration
- MSIX packaging

## Development

This project uses .NET 10 and Windows App SDK.

### Getting Started

1. Clone the repository with submodules:
   ```bash
   git clone --recurse-submodules https://github.com/Kumara-Krishnan/Bookmarkly.git
   ```

   Or if you've already cloned the repository, initialize the submodules:
   ```bash
   git submodule update --init --recursive
   ```

2. The `Megakat` submodule contains utility projects used by Bookmarkly.

For more detailed setup and contribution guidelines, see [CONTRIBUTING.md](CONTRIBUTING.md).

### Building

The project requires Windows for building the full application. See the [build workflow](.github/workflows/msix-build-release.yml) for details.

### Deployment

Releases are automatically deployed to Cloudflare Pages CDN with a landing page. See [Cloudflare CDN Setup](docs/CLOUDFLARE_CDN_SETUP.md) for configuration instructions.

### Testing

Tests can be run on any platform:

```bash
dotnet test Tests/ArchitectureTests/ArchitectureTests.csproj
```

## License

See [LICENSE](LICENSE) file for details.
