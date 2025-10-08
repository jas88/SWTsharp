# NuGet API Key Setup

This guide explains how to set up your NuGet API key as a GitHub secret for automated package publishing.

## Step 1: Get Your NuGet API Key

1. Go to [NuGet.org](https://www.nuget.org/)
2. Sign in to your account
3. Click on your username in the top-right corner
4. Select **API Keys** from the dropdown menu
5. Click **Create** to generate a new API key

### Recommended API Key Settings:

- **Key Name**: `SWTSharp-GitHub-Actions`
- **Package Owner**: Your NuGet username
- **Glob Pattern**: `SWTSharp`
- **Select Scopes**: Check `Push` and `Push new packages and package versions`
- **Expiration**: 365 days (set a reminder to refresh annually)

6. Click **Create** and copy the generated API key
   - ⚠️ **Important**: Save this key immediately - you won't be able to see it again!

## Step 2: Add API Key to GitHub Secrets

1. Go to your GitHub repository: https://github.com/jas88/SWTsharp
2. Click **Settings** (top menu)
3. In the left sidebar, click **Secrets and variables** → **Actions**
4. Click **New repository secret**
5. Configure the secret:
   - **Name**: `NUGET_API_KEY`
   - **Value**: Paste your NuGet API key
6. Click **Add secret**

## Step 3: Verify Setup

The GitHub Actions workflow (`.github/workflows/ci.yml`) is already configured to use this secret.

When you push a tag (e.g., `v0.2.0`), the workflow will:
1. Run tests on all platforms
2. Create NuGet package
3. Publish to NuGet.org automatically
4. Create a GitHub release with artifacts

## Creating a Release

```bash
# Tag the release (use semantic versioning)
git tag v0.2.0
git push origin v0.2.0

# Or create via GitHub UI:
# Releases → Draft a new release → Choose a tag → Create tag: v0.2.0
```

## Troubleshooting

### API Key Invalid or Expired
- Go to NuGet.org → API Keys
- Regenerate or create a new key
- Update the `NUGET_API_KEY` secret in GitHub

### Permission Denied
- Verify the API key has `Push` scope enabled
- Check the glob pattern includes `SWTSharp`

### Package Already Exists
- The workflow uses `--skip-duplicate` to handle this gracefully
- Increment the version in `src/SWTSharp/SWTSharp.csproj` before tagging

## Security Best Practices

1. ✅ Use separate API keys for different purposes
2. ✅ Set appropriate expiration dates (1 year recommended)
3. ✅ Use restrictive glob patterns (only `SWTSharp`)
4. ✅ Limit scopes to only what's needed (`Push` only)
5. ✅ Rotate keys annually
6. ✅ Never commit API keys to source control
7. ✅ Use GitHub secrets for all sensitive data

## Current Status

- ✅ CI/CD workflow configured
- ✅ GitHub Actions set up for multi-platform testing
- ✅ NuGet publishing step added to release workflow
- ⏳ **Action Required**: Add `NUGET_API_KEY` to GitHub secrets

Once the secret is added, all future releases will automatically publish to NuGet.org!
