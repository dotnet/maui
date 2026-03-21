# .NET MAUI PR Artifact Scripts

Scripts to test .NET MAUI pull request builds in your own projects before they're merged.

## Scripts

- `get-maui-pr.sh` - Bash script for Unix-like systems (Linux, macOS)
- `get-maui-pr.ps1` - PowerShell script for cross-platform use (Windows, Linux, macOS)

## Quick Start

> **⚠️ WARNING:** Always review the PR code before running these scripts. Only test PRs you trust.

**Bash:**

```bash
curl -fsSL https://raw.githubusercontent.com/dotnet/maui/main/eng/scripts/get-maui-pr.sh | bash -s -- <PR_NUMBER>
```

**PowerShell:**

```powershell
iex "& { $(irm https://raw.githubusercontent.com/dotnet/maui/main/eng/scripts/get-maui-pr.ps1) } <PR_NUMBER>"
```

## NuGet Hive Path

Downloaded packages are stored in a hive directory:

- **Unix (Linux/macOS)**: `~/.maui/hives/pr-<PR_NUMBER>/packages`
- **Windows**: `%USERPROFILE%\.maui\hives\pr-<PR_NUMBER>\packages`

## Requirements

- .NET SDK installed
- A .NET MAUI project (`.csproj` with `<UseMaui>true</UseMaui>`)
- Internet connection
- **Bash only**: `curl`, `jq`, and `unzip`

## Parameters

### Bash Script (`get-maui-pr.sh`)

| Argument | Description | Required |
|----------|-------------|----------|
| 1st | PR number to test | Yes |
| 2nd | Path to .csproj file | No (auto-detects) |

### PowerShell Script (`get-maui-pr.ps1`)

| Parameter | Description | Default |
|-----------|-------------|---------|
| `-PrNumber` | PR number to test | Required |
| `-ProjectPath` | Path to .csproj file | Auto-detect |

## Usage Examples

### Bash Script Examples

```bash
# Test PR in current directory
./get-maui-pr.sh 33002

# Test PR with specific project
./get-maui-pr.sh 33002 ./MyApp/MyApp.csproj
```

### PowerShell Script Examples

```powershell
# Test PR in current directory
.\get-maui-pr.ps1 33002

# Test PR with specific project
.\get-maui-pr.ps1 -PrNumber 33002 -ProjectPath ./MyApp/MyApp.csproj
```

## Repository Override

You can point the scripts at a fork by setting the `MAUI_REPO` environment variable to `owner/name` before invoking the script (defaults to `dotnet/maui`).

```bash
export MAUI_REPO=myfork/maui
./get-maui-pr.sh 1234
```

```powershell
$env:MAUI_REPO = 'myfork/maui'
./get-maui-pr.ps1 1234
```

## Reverting Changes

**TIP:** Use a separate Git branch for testing!

```bash
git checkout -b test-pr-33002
# ... test the PR ...
git checkout main  # Easy revert!
```

Or manually revert:

1. Edit your `.csproj` - change package version back to stable (see [NuGet](https://www.nuget.org/packages/Microsoft.Maui.Controls))
2. Remove the `maui-pr-build` source from `NuGet.config`
3. Run `dotnet restore --force`

## Troubleshooting

### Common Issues

1. **"No build found for PR"**: The PR may not have a completed build yet. Check the PR on GitHub for build status.
2. **"No .NET MAUI project found"**: Ensure you're in a directory with a `.csproj` file that has `<UseMaui>true</UseMaui>`.
3. **"Failed to download artifacts"**: Check your internet connection. Artifacts may have expired for older PRs.

### Getting Help

Run the PowerShell script with the help flag to see detailed usage information:

```powershell
Get-Help .\get-maui-pr.ps1 -Detailed
```

## More Information

- [Testing PR Builds Wiki](https://github.com/dotnet/maui/wiki/Testing-PR-Builds)
- [.NET MAUI Nightly Builds](https://github.com/dotnet/maui/wiki/Nightly-Builds)
- [Contributing Guide](https://github.com/dotnet/maui/blob/main/CONTRIBUTING.md)
