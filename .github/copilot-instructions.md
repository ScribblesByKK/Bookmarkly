# GitHub Copilot Instructions for Bookmarkly

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

## Rationale

This approach provides:
- **Safety**: Test workflow changes without affecting main branch
- **Isolation**: Keep experimental workflow changes separate
- **Validation**: Ensure workflows work before merging to main
- **CI/CD Best Practices**: Simulate production-like testing environment
