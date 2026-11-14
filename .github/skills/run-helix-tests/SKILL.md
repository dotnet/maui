---
name: run-helix-tests
description: "Compatibility entrypoint for retired MAUI Helix submission. Run unit tests directly with dotnet test instead; existing Helix log readers remain available for historical investigations."
metadata:
  author: dotnet-maui
  version: "2.0"
compatibility: Requires the repository-selected .NET SDK.
---

# Run tests directly

MAUI no longer submits tests to Helix. Do not attempt to install a Helix SDK,
find a queue, obtain a submission token, or use the retired submission scripts.

For unit tests, provision the repository SDK/build tasks using the documented
build prerequisites, then execute the required project:

```powershell
dotnet test --project src/Core/tests/UnitTests/Core.UnitTests.csproj --configuration Release --report-trx --results-directory artifacts/TestResults/Core
```

The complete previously submitted unit suite list and Windows-only lane are in
`eng/pipelines/arcade/stage-unit-tests.yml`. CI runs Debug and Release on both
Windows and macOS with `DOTNET_MODIFIABLE_ASSEMBLIES=debug` for real Hot Reload
metadata-update tests. Do not set Helix payload environment variables for direct
checkout-based execution.

For native tests, follow
`.github/instructions/helix-device-tests.instructions.md` (the retained filename
is a compatibility reference). Native platforms require explicitly selected
devices and real reporter-generated TRX; Apple result collection is not
automatically supplied by the current SDK proxy.

`scripts/Get-HelixJobStatus.ps1` and `scripts/Get-HelixWorkItemLog.ps1` are
read-only readers for historical or external jobs. Their presence does not
authorize or re-enable submission.
