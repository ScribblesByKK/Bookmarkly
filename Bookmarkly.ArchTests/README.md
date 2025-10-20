# Bookmarkly Architecture Tests

This project contains ArchUnit tests that enforce architectural dependencies between projects in the Bookmarkly solution.

## Purpose

These tests ensure that the project dependencies follow the specified architecture rules:

1. **ViewModels** can reference only:
   - ViewContracts
   - Library
   - Entities

2. **ViewContracts** can reference only:
   - Entities

3. **Library** can reference only:
   - Entities

4. **Views** can reference only:
   - ViewContracts
   - Entities

## Technology

The tests use [ArchUnitNET](https://github.com/TNG/ArchUnitNET), a C# architecture test library that allows you to specify and assert architecture rules in C# for automated testing.

## Running the Tests

From the solution root directory:

```bash
dotnet test Bookmarkly.ArchTests
```

Or from the test project directory:

```bash
cd Bookmarkly.ArchTests
dotnet test
```

## How It Works

The tests load all project assemblies and analyze their dependencies at runtime. For each project namespace, the tests verify that types only depend on types from allowed namespaces. If a type references a type from a disallowed namespace, the test will fail with a detailed error message indicating which type violated the rule.

## Example Test Failure

If you add an incorrect reference (e.g., Library referencing ViewModels), the test will fail with a message like:

```
Library type 'Bookmarkly.Library.SomeClass' should not depend on 'Bookmarkly.ViewModels.SomeViewModel' from namespace 'Bookmarkly.ViewModels'
```

This helps catch architectural violations early in the development process.

## Adding New Rules

To add new architecture rules, modify the `ArchitectureDependencyTests.cs` file and add new test methods following the same pattern as the existing tests.
