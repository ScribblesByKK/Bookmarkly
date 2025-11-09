# Version Management System

This repository uses an automated version management system for CI builds.

## Version Format

The version follows the format: `major.yyww.build.revision`

Where:
- **major**: The major version number (manually set by developers, default: 1)
- **yyww**: Year and week number component
  - **yy**: Last 2 digits of the year (for years < 2100)
  - **yyy**: Last 3 digits of the year (for years >= 2100)
  - **ww**: ISO 8601 week number (01-53)
- **build**: Auto-incremented build number (starts at 1, increments on each CI run)
- **revision**: Revision number (manually set by developers, default: 0)

## Examples

- Job 1 on January 1, 2025: `1.2501.1.0`
- Job 2 on January 1, 2025: `1.2501.2.0`
- Job 3 on January 2, 2025: `1.2501.3.0`
- Job 4 on January 9, 2025 (week 2): `1.2502.4.0`
- Job 1 on January 1, 2100: `1.10001.1.0` (note the yyyww format)
- Job 5 on June 15, 2150 (week 25): `1.15025.5.0`

## How It Works

1. The `build/version.json` file stores the persistent values:
   - `major`: The major version (developer-controlled)
   - `build`: The current build number (auto-incremented)
   - `revision`: The revision number (developer-controlled)

2. During CI execution (on push to main):
   - The `build/update-version.ps1` script runs
   - It calculates the `yyww` or `yyyww` component based on current date
   - It increments the `build` number
   - It updates both `build/version.json` and `Package.appxmanifest`
   - The changes are committed back to the repository with `[skip ci]` to prevent infinite loops

3. The updated version is used throughout the build and release process

## Manual Version Changes

To manually change the major or revision version:

1. Edit `build/version.json`:
   ```json
   {
     "major": 2,
     "build": 0,
     "revision": 0
   }
   ```

2. Commit and push to main:
   ```bash
   git add build/version.json
   git commit -m "chore: bump major version to 2"
   git push
   ```

3. The next CI run will start with the new major version

## Preventing Infinite CI Loops

**Question: Is it possible in CI action to make a commit to main? Will it not trigger a new CI action?**

**Answer:** Yes, it is possible, and there are two main approaches to prevent infinite loops:

### Approach 1: Using `[skip ci]` in commit message (Implemented)

GitHub Actions recognizes certain keywords in commit messages that prevent triggering workflows:
- `[skip ci]`
- `[ci skip]`
- `[no ci]`
- `[skip actions]`
- `[actions skip]`

Our implementation uses `[skip ci]` in the commit message when committing version changes:
```bash
git commit -m "chore: update version to X.YYWW.B.R [skip ci]"
```

This prevents the version commit from triggering another CI run, thus avoiding an infinite loop.

### Approach 2: Conditional execution (Additional safety)

The workflow only commits version changes on `push` events to the `main` branch, not on `pull_request` events. This provides an additional layer of protection:

```yaml
- name: Commit version changes
  if: github.event_name == 'push' && github.ref == 'refs/heads/main'
  run: |
    # commit and push
```

### How it works in practice:

1. Developer pushes code to main
2. CI workflow is triggered
3. Version is updated and committed with `[skip ci]`
4. The version commit does NOT trigger another CI run
5. Build continues with the updated version

This approach is safe, widely used, and recommended by GitHub.

## Files Involved

- `build/version.json`: Stores major, build, and revision values
- `build/update-version.ps1`: PowerShell script that calculates and updates version
- `.github/workflows/msix-build-release.yml`: CI workflow that executes versioning
- `Bookmarkly.App/Package.appxmanifest`: MSIX manifest with version number
