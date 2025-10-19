# Testing the MSIX Build Workflow

This document explains how to test and validate the MSIX build workflow.

## Testing Methods

### Method 1: Manual Workflow Trigger (Recommended for Testing)

1. Navigate to the repository on GitHub
2. Click on the "Actions" tab
3. Select "Build MSIX Package" from the workflows list
4. Click "Run workflow" button
5. Select the branch to test (e.g., `copilot/setup-github-actions`)
6. Click "Run workflow"

The workflow will execute and you can monitor its progress in real-time.

### Method 2: Push to Dev Branch

1. Merge this PR or push these changes to the `dev` branch
2. The workflow will automatically trigger
3. Monitor the workflow execution in the Actions tab

### Method 3: Push to Main Branch

1. Merge the `dev` branch into `main`
2. The workflow will automatically trigger
3. Monitor the workflow execution in the Actions tab

## Expected Results

A successful workflow run should:

1. ✅ Complete all steps without errors
2. ✅ Generate an MSIX bundle (`.msixbundle` file)
3. ✅ Generate an AppInstaller file (`.appinstaller` file)
4. ✅ Generate a certificate file (`.cer` file)
5. ✅ Upload all artifacts to GitHub Actions
6. ✅ Complete in approximately 5-10 minutes

## Validation Checklist

After a successful workflow run:

- [ ] Workflow completed with green checkmark
- [ ] No errors in workflow logs
- [ ] Artifacts are available for download
- [ ] Artifact contains `.msixbundle` file
- [ ] Artifact contains `.appinstaller` file
- [ ] Artifact contains `.cer` file
- [ ] Certificate was properly cleaned up (check cleanup logs)

## Downloading and Testing the Package

1. Go to the completed workflow run
2. Scroll to the "Artifacts" section at the bottom
3. Download the `msix-package-{run_number}` artifact
4. Extract the ZIP file
5. Install the certificate (`.cer` file) to Trusted Root
6. Double-click the `.appinstaller` file to install the app

## Known Build Requirements

The workflow requires:
- Windows runner (uses `windows-latest`)
- .NET 8.0 SDK
- MSBuild
- PowerShell

All of these are pre-installed on GitHub-hosted Windows runners.

## Troubleshooting Build Failures

### Issue: NETSDK1102 Error
**Solution**: Already addressed - `PublishTrimmed` and `PublishReadyToRun` are set to `false`

### Issue: Missing Certificate
**Solution**: Certificate is generated automatically in the workflow

### Issue: Build Timeout
**Solution**: Increase timeout in workflow if needed (default is 360 minutes)

### Issue: Missing Dependencies
**Solution**: Check the "Restore NuGet packages" step logs

## Build Configuration Details

- **Target Framework**: net8.0-windows10.0.26100.0
- **Platform**: x64
- **Configuration**: Release
- **Package Mode**: SideloadOnly
- **Bundle Mode**: Always (creates .msixbundle)
- **Signing**: Self-signed certificate (auto-generated)

## Next Steps After Validation

Once the workflow is validated:
1. Consider updating the AppInstaller URI to point to your actual distribution location
2. For production, replace the self-signed certificate with a trusted code-signing certificate
3. Set up automated releases to distribute the generated packages
