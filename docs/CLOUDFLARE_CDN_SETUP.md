# Cloudflare Pages CDN Deployment

This document describes how to configure Cloudflare Pages deployment for the Bookmarkly app release landing page.

## Overview

The build workflow automatically deploys a landing page to Cloudflare Pages CDN after each release. The landing page includes:

- App icon and branding
- Current version and commit SHA
- Download buttons for MSIX installer and ZIP archive
- Release notes history

## Prerequisites

1. A Cloudflare account
2. A Cloudflare Pages project (created in the Cloudflare dashboard)
3. Cloudflare API credentials with appropriate permissions

## Setup Steps

### 1. Create a Cloudflare Pages Project

1. Log in to your [Cloudflare Dashboard](https://dash.cloudflare.com)
2. Navigate to **Workers & Pages** > **Pages**
3. Click **Create a project** > **Direct Upload**
4. Name your project (e.g., `bookmarkly-releases`)
5. Upload a placeholder file to create the project
6. Note down the project name for later use

### 2. Get Your Cloudflare Account ID

1. In the Cloudflare Dashboard, go to any domain or the main dashboard
2. On the right sidebar, find **Account ID** under the API section
3. Copy this value - you'll need it for the GitHub secret

### 3. Create a Cloudflare API Token

1. Go to your [Cloudflare API Tokens page](https://dash.cloudflare.com/profile/api-tokens)
2. Click **Create Token**
3. Use the **Custom token** template
4. Configure the token with the following permissions:
   - **Account** > **Cloudflare Pages** > **Edit**
5. Under **Account Resources**, select your account
6. Click **Continue to summary** and then **Create Token**
7. Copy the token immediately (you won't be able to see it again)

### 4. Configure GitHub Repository Secrets

Add the following secrets to your GitHub repository:

1. Go to your repository on GitHub
2. Navigate to **Settings** > **Secrets and variables** > **Actions**
3. Click **New repository secret** and add each of the following:

| Secret Name | Description | Example Value |
|-------------|-------------|---------------|
| `CLOUDFLARE_API_TOKEN` | The API token created in step 3 | `abcdef1234567890...` |
| `CLOUDFLARE_ACCOUNT_ID` | Your Cloudflare account ID from step 2 | `1234567890abcdef...` |
| `CLOUDFLARE_PROJECT_NAME` | The name of your Cloudflare Pages project | `bookmarkly-releases` |

### 5. Verify the Setup

After configuring the secrets:

1. Trigger a new build by pushing to the `main` or `ci` branch
2. Check the GitHub Actions workflow run
3. Look for the "Deploy to Cloudflare Pages" job
4. Verify the deployment URL in the job output

## Deployment URL

Once deployed, your landing page will be available at:

- Production: `https://<project-name>.pages.dev`
- Preview (for non-main branches): `https://<commit-hash>.<project-name>.pages.dev`

## Troubleshooting

### Common Issues

**"Authentication error" during deployment**
- Verify that `CLOUDFLARE_API_TOKEN` is correct and has not expired
- Ensure the token has **Cloudflare Pages - Edit** permission
- Check that the account ID matches the account where the Pages project exists

**"Project not found" error**
- Verify that `CLOUDFLARE_PROJECT_NAME` exactly matches the project name in Cloudflare
- Ensure the API token has access to the correct account

**"Permission denied" error**
- The API token may not have sufficient permissions
- Recreate the token with **Account > Cloudflare Pages > Edit** permission

### Checking Deployment Status

1. Go to your Cloudflare Dashboard
2. Navigate to **Workers & Pages** > **Pages**
3. Click on your project to see deployment history and logs

## Security Considerations

- **Never** commit API tokens or account IDs directly in code
- Use GitHub repository secrets for all sensitive values
- Rotate API tokens periodically
- Use the minimum required permissions for API tokens

## File Structure

The deployment creates the following structure on Cloudflare Pages:

```
/
├── index.html          # Landing page
├── assets/
│   └── app-icon.png    # Application icon
└── downloads/
    └── Bookmarkly_<version>.zip  # Release archive
```

## Custom Domain (Optional)

To use a custom domain for your landing page:

1. In Cloudflare Dashboard, go to your Pages project
2. Click **Custom domains** > **Set up a custom domain**
3. Follow the prompts to add and verify your domain

## Additional Resources

- [Cloudflare Pages Documentation](https://developers.cloudflare.com/pages/)
- [Cloudflare Pages GitHub Action](https://github.com/cloudflare/pages-action)
- [Cloudflare API Token Guide](https://developers.cloudflare.com/fundamentals/api/get-started/create-token/)
