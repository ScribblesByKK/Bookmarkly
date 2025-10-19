# GitHub Actions Workflows

## Build MSIX Package

This workflow automatically builds and generates MSIX apppackage bundles for the Bookmarkly application.

### Trigger Conditions

The workflow runs automatically on:
- Push to `main` branch
- Push to `dev` branch
- Manual trigger via GitHub Actions UI (`workflow_dispatch`)

### What It Does

1. **Environment Setup**
   - Uses Windows runner (required for Windows App SDK)
   - Installs .NET 8.0 SDK
   - Sets up MSBuild

2. **Package Signing**
   - Generates a self-signed certificate with a strong random password
   - Temporarily installs the certificate for signing
   - Cleans up the certificate after build completion

3. **Build Process**
   - Restores NuGet packages
   - Builds the application for x64 Release configuration
   - Creates an MSIX bundle (`.msixbundle`)
   - Generates an AppInstaller file (`.appinstaller`) for easy sideloading
   - Disables trimming and ReadyToRun optimization (required for MSIX packaging)

4. **Artifact Upload**
   - Uploads the generated package files as GitHub Actions artifacts
   - Includes: MSIX bundle, AppInstaller file, and certificate
   - Artifacts are retained for 90 days

### Configuration

Key settings in the workflow:
- **Platform**: x64
- **Configuration**: Release
- **Build Mode**: SideloadOnly (for testing/distribution outside the Store)
- **Bundle**: Always (generates .msixbundle file)

### Manual Trigger

To manually trigger the workflow:
1. Go to Actions tab in GitHub
2. Select "Build MSIX Package" workflow
3. Click "Run workflow"
4. Select the branch to build from
5. Click "Run workflow" button

### Generated Files

After a successful build, the following files are available as artifacts:
- `*.msixbundle` - The packaged application bundle
- `*.appinstaller` - Installation manifest for sideloading
- `*.cer` - Certificate file (needed to trust the package on target machines)

### Installing the Package

To install the generated package on a Windows machine:

1. Download the artifact from the GitHub Actions run
2. Extract the zip file
3. Install the certificate:
   - Right-click the `.cer` file
   - Click "Install Certificate"
   - Select "Local Machine"
   - Place the certificate in "Trusted Root Certification Authorities"
4. Install the app:
   - Double-click the `.appinstaller` file, or
   - Right-click the `.msixbundle` and select "Install"

### Troubleshooting

If the build fails:
- Check the workflow logs in the Actions tab
- Common issues:
  - Missing dependencies (check NuGet restore step)
  - Certificate generation issues (check certificate step)
  - Build errors (check Build MSIX package step)

### Notes

- The self-signed certificate is generated fresh for each build
- For production releases, consider using a real code-signing certificate
- The AppInstaller URI is set to GitHub releases; update if using a different distribution method
