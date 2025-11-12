# Bookmarkly

[![Tests](https://github.com/Kumara-Krishnan/Bookmarkly/actions/workflows/test.yml/badge.svg?branch=main)](https://github.com/Kumara-Krishnan/Bookmarkly/actions/workflows/test.yml)
[![Build and Release](https://github.com/Kumara-Krishnan/Bookmarkly/actions/workflows/msix-build-release.yml/badge.svg)](https://github.com/Kumara-Krishnan/Bookmarkly/releases/latest)

A bookmark management application for Windows.

## Features

- Bookmark management
- Windows App SDK integration
- MSIX packaging

## Development

This project uses .NET 10 and Windows App SDK.

### Building

The project requires Windows for building the full application. See the [build workflow](.github/workflows/msix-build-release.yml) for details.

### Testing

Tests can be run on any platform:

```bash
dotnet test Tests/ArchitectureTests/ArchitectureTests.csproj
```

## License

See [LICENSE](LICENSE) file for details.
