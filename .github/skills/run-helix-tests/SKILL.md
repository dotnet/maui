---
name: run-helix-tests
description: "Submit and monitor .NET MAUI unit tests on Helix infrastructure. Supports running XAML, Resizetizer, Core, Essentials, and other unit test projects on distributed Helix queues."
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires local .dotnet SDK provisioned (run `dotnet cake` first if missing).
---

# Run Helix Tests Skill

Submit and monitor .NET MAUI unit tests on Helix infrastructure from your local machine.

## Tools Required

This skill uses `pwsh` (PowerShell 7+) to run the scripts. The scripts call the local `.dotnet/dotnet` SDK.

## When to Use

- User asks to "run tests on Helix"
- User wants to validate test changes on Helix infrastructure
- User asks to "test on multiple platforms" (Windows + macOS)
- User wants to debug Helix-specific test failures locally
- User asks about "Helix unit tests" (not device tests - those use `helix_xharness.proj`)

## Prerequisites

Before running Helix tests, ensure:

1. **Local SDK provisioned**: The `.dotnet/` folder must exist with the SDK
   ```powershell
   # If missing, run:
   dotnet cake --target=dotnet
   ```

2. **Build tasks compiled** (for first run):
   ```powershell
   # Windows
   .\build.cmd -restore -build -configuration Release -projects .\Microsoft.Maui.BuildTasks.slnf

   # macOS/Linux  
   ./build.sh -restore -build -configuration Release -projects ./Microsoft.Maui.BuildTasks.slnf
   ```

## Scripts

All scripts are in `.github/skills/run-helix-tests/scripts/`

### 1. Submit Unit Tests to Helix
```powershell
# Submit all unit tests (XAML, Core, Essentials, Resizetizer, etc.)
pwsh .github/skills/run-helix-tests/scripts/Submit-HelixTests.ps1

# Submit with specific configuration
pwsh .github/skills/run-helix-tests/scripts/Submit-HelixTests.ps1 -Configuration Debug

# Submit to specific queue only
pwsh .github/skills/run-helix-tests/scripts/Submit-HelixTests.ps1 -Queue "Windows.10.Amd64.Open"
```

### 2. Monitor Helix Job Status
```powershell
# Monitor a submitted job (use job ID from submission output)
pwsh .github/skills/run-helix-tests/scripts/Get-HelixJobStatus.ps1 -JobId <JOB_ID>

# Wait for completion with polling
pwsh .github/skills/run-helix-tests/scripts/Get-HelixJobStatus.ps1 -JobId <JOB_ID> -Wait
```

### 3. Get Helix Work Item Console Logs
```powershell
# Get console output for a specific work item
pwsh .github/skills/run-helix-tests/scripts/Get-HelixWorkItemLog.ps1 -JobId <JOB_ID> -WorkItem <WORK_ITEM_NAME>

# Get failed work items only
pwsh .github/skills/run-helix-tests/scripts/Get-HelixWorkItemLog.ps1 -JobId <JOB_ID> -FailedOnly
```

## Quick Start (Manual)

If you prefer to run directly without scripts:

```powershell
# Windows
.\helix.cmd

# macOS/Linux
./helix.sh
```

These wrapper scripts set the required environment variables and submit to Helix.

## Test Projects Submitted

The following test projects are submitted to Helix (defined in `eng/helix.proj`):

| Project | Description |
|---------|-------------|
| `Controls.Xaml.UnitTests` | XAML parsing and compilation tests |
| `Controls.Core.UnitTests` | Core controls unit tests |
| `Controls.BindingSourceGen.UnitTests` | Binding source generator tests |
| `SourceGen.UnitTests` | General source generator tests |
| `Core.UnitTests` | Core framework tests |
| `Essentials.UnitTests` | Essentials APIs tests |
| `Resizetizer.UnitTests` | Image resizing tests |

## Helix Queues

Tests run on multiple queues in parallel:

| Queue | Platform |
|-------|----------|
| `Windows.10.Amd64.Open` | Windows 10 x64 |
| `osx.15.arm64.maui.open` | macOS 15 ARM64 |

## Workflow

### Submit and Monitor Tests

1. **Submit tests**:
   ```powershell
   $result = pwsh .github/skills/run-helix-tests/scripts/Submit-HelixTests.ps1
   # Note the job IDs from output
   ```

2. **Monitor progress**:
   ```powershell
   pwsh .github/skills/run-helix-tests/scripts/Get-HelixJobStatus.ps1 -JobId $jobId -Wait
   ```

3. **Check failures** (if any):
   ```powershell
   pwsh .github/skills/run-helix-tests/scripts/Get-HelixWorkItemLog.ps1 -JobId $jobId -FailedOnly
   ```

### Debug a Specific Test Failure

1. Get the job ID from the Helix submission output
2. Find the failed work item name
3. Get the console log:
   ```powershell
   pwsh .github/skills/run-helix-tests/scripts/Get-HelixWorkItemLog.ps1 -JobId <JOB_ID> -WorkItem "Microsoft.Maui.Controls.Xaml.UnitTests.dll"
   ```

## Understanding Results

Helix test results are reported to the console and can be viewed at:
- **Helix Portal**: https://helix.dot.net/api/2019-06-17/jobs/{jobId}
- **Work Item Console**: https://helix.dot.net/api/2019-06-17/jobs/{jobId}/workitems/{workItemName}/console

## Common Issues

| Issue | Solution |
|-------|----------|
| "dotnet not found" | Run `dotnet cake --target=dotnet` to provision SDK |
| "Build failed" | Run build.cmd/build.sh first to build required projects |
| "Queue not available" | Check https://helix.dot.net for queue status |
| "Test timeout" | Some tests have long execution times; this is normal |

## Difference from Device Tests

This skill runs **unit tests** on Helix (`eng/helix.proj`).

For **device tests** (iOS, Android, MacCatalyst emulators/simulators), use:
```powershell
# Device tests use helix_xharness.proj instead
./eng/common/msbuild.sh ./eng/helix_xharness.proj /t:Test /p:TargetOS=ios
```

See `.github/instructions/helix-device-tests.instructions.md` for device test guidance.
