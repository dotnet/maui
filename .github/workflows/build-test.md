---
description: >
  Experiment: Test if dotnet build works inside the gh-aw agent container.
  Verifies firewall restrictions (api.nuget.org blocked) and env var exposure.

on:
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number (optional, for context only)'
        required: false
        type: number

permissions:
  contents: read

tools:
  bash: ["dotnet", "pwsh", "env", "gh"]

safe-outputs:
  add-comment:
    max: 1
    target: "*"
  noop:

timeout-minutes: 15
---

# Build Test Experiment

You are testing whether `dotnet build` works inside the gh-aw agent container.

## Instructions

Run the following steps **in order** and report the results of each:

### Step 1: Check environment

```bash
echo "=== .NET SDK ==="
dotnet --version 2>&1 || echo "dotnet not found"

echo "=== Environment variables containing TOKEN ==="
env | grep -i TOKEN | sed 's/=.*/=<REDACTED>/' || echo "No TOKEN vars found"

echo "=== Network test: api.nuget.org ==="
curl -s --connect-timeout 5 https://api.nuget.org/v3/index.json | head -c 200 2>&1 || echo "api.nuget.org BLOCKED"

echo "=== Network test: api.github.com ==="
curl -s --connect-timeout 5 https://api.github.com 2>&1 | head -c 200 || echo "api.github.com BLOCKED"
```

### Step 2: Attempt to build TestCases.HostApp for Android

Try to build the HostApp:

```bash
dotnet build src/Controls/tests/TestCases.HostApp/Maui.Controls.Sample.HostApp.csproj -f net10.0-android -c Debug --no-restore 2>&1 | tail -30
```

If that fails (expected due to no restore), also try with restore:

```bash
dotnet build src/Controls/tests/TestCases.HostApp/Maui.Controls.Sample.HostApp.csproj -f net10.0-android -c Debug 2>&1 | tail -50
```

### Step 3: Report results

Call `add_comment` (if a PR number was provided) or `noop` with a summary of:
1. Whether `dotnet` CLI was available
2. Whether `api.nuget.org` was reachable
3. Whether the build succeeded or failed (and why)
4. What TOKEN-related env vars exist (names only, values redacted)

If no PR number was provided, call `noop` with the full results summary.

