# MAUI Build and Publishing Scripts

This directory contains scripts for building and publishing the MAUI framework components.

## push_nuget_org.ps1

This PowerShell script is used to push NuGet packages to NuGet.org or a custom NuGet feed.

### Features

- Supports multiple API keys to handle NuGet.org rate limiting and quota issues
- Automatically retries failed pushes with backoff strategy
- Includes detailed logging and error handling
- Generates a summary report of successful, failed, and skipped packages
- Supports include/exclude filters for package selection
- Supports dry runs with the `PUSH_PACKAGES` environment variable

### Parameters

| Parameter | Description | Required |
|-----------|-------------|----------|
| ApiKey | Primary NuGet API key | Yes |
| NuGetSearchPath | Path to search for NuGet packages | Yes |
| NuGetIncludeFilters | Semicolon-delimited list of include patterns | No |
| NuGetExcludeFilters | Semicolon-delimited list of exclude patterns | No |
| FeedUrl | URL of the NuGet feed (defaults to NuGet.org) | No |
| NuGetOrgApiKey2 | Second NuGet.org API key for handling quota limits | No |
| NuGetOrgApiKey3 | Third NuGet.org API key for handling quota limits | No |

### Usage

```powershell
pwsh ./push_nuget_org.ps1 -ApiKey "your-api-key" -NuGetSearchPath "./artifacts/bin" -NuGetIncludeFilters "Microsoft.Maui.*" -NuGetExcludeFilters "*.symbols.nupkg"
```

To perform a dry run (no actual packages are pushed):

```powershell
$env:PUSH_PACKAGES="False"
pwsh ./push_nuget_org.ps1 -ApiKey "your-api-key" -NuGetSearchPath "./artifacts/bin"
```

### Logging

The script creates a timestamped log file in the same directory as the script file.
