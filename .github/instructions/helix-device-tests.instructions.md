---
applyTo: ".github/skills/run-device-tests/scripts/*.ps1,eng/devices/*.ps1,eng/pipelines/**/device-tests*.yml,eng/pipelines/**/stage-device-tests.yml,src/**/DeviceTests/**"
description: "Direct dotnet test device execution, matrix preservation, ownership and result contracts"
---

# Direct device tests

Device tests execute in the pipeline job using `dotnet test`, not a remote Helix
submission or XHarness. Keep vendored `eng/common` and historical/external Helix
log readers unchanged.

## Coverage

All five suites are required: Controls, Core, Graphics, Essentials and
MauiBlazorWebView. The explicit minimum matrix is in
`eng/pipelines/arcade/stage-device-tests.yml`:

- Release Android API 33 on Linux x64: CoreCLR and Mono.
- Release iOS simulator on macOS 15 and 26 arm64 (public), macOS 15 arm64
  (internal): CoreCLR and Mono.
- Release Catalyst on the same Apple OS/architecture rows: CoreCLR.
- Release Windows 11 x64: packaged and unpackaged, each suite. Packaged rows use
  installed MSIX activation, not an unpackaged execution of the built binary.
- iOS Controls has CollectionView, Shell, HybridWebView, and General shards.
  General must exclude all three individually executed categories.

The other pipeline environment retains its CoreCLR-only API matrix, per-suite
API exclusions and Debug/Release configuration in `eng/pipelines/device-tests.yml`.
Do not silently expand exclusions or replace a missing runtime with a newer one.

## Agent contracts

Former submission agents are not assumed to be device-capable execution agents.
Public and internal pipelines must retain separate pools; never schedule PR code
on internal pools or expose submission/publication credentials to tests.

- Android: SDK command-line tools, accepted SDK licenses, JDK, emulator hardware
  acceleration, sufficient disk and `/dev/kvm` access on Linux. Install the exact
  matrix image via `sdkmanager`; create a unique AVD via `avdmanager`.
- Apple: matrix OS and arm64 architecture, compatible Xcode/workload, requested
  simulator runtime, logged-in graphical user for Catalyst. A missing OS/runtime
  is an agent provisioning failure, not permission to drop coverage. The pinned
  SDK may require an Xcode unavailable on an older OS; report this incompatibility
  explicitly rather than moving its row to a newer OS.
- Windows: interactive Windows 11 x64 agents, Windows App SDK, signing support and
  permission to install/remove a job-owned signing certificate. Baseline pool/image defaults are retained; runtime
  checks reject a Server build agent or missing interactive desktop instead of
  silently claiming Windows 11 coverage. No new custom agent capability is assumed.
  Packaged execution must retain package identity; an MSIX build followed by
  unpackaged binary execution does not cover the packaged scenario.

The remaining environment questions are specific: which existing public/internal
pool image supplies an interactive Windows 11 x64 client, and which baseline Apple
image supplies macOS 15 arm64 with the Xcode required by the pinned workload?
`applePoolsPublic`/`applePoolsInternal` retain the old Aces image by default, with
explicit OS/architecture checks per row; do not silently move the macOS 15 row
to macOS 26. The other pipeline retains its original MAUI execution pool.

Windows packaged jobs prepare a job-unique certificate/publisher and manifest
copy while preserving the original application ID (including AppInfo assertions).
The non-exportable private signing key stays in `CurrentUser\My`, selected only
by public thumbprint; trust only the public certificate in the current user's
`TrustedPeople` store. Never export private PFX/password material to results or
binlogs, alter machine-wide trust, or replace a pre-existing user's package.
`WindowsTestPackage.ps1` installs the signed MSIX and x64 dependencies through
`Add-AppxPackage`. Standard evaluated `RunCommand`/`RunArguments` launch the
`windows-launcher` DLL with its owned state file; the SDK appends its original MTP
arguments, including server/pipe options. The launcher quotes the complete vector,
activates the registered AUMID using `IApplicationActivationManager`, checks the
actual PID package identity, records PID/start time and propagates its real exit.

Native WinUI startup verifies package name and publisher, then recovers launch
arguments from `AppInstance.GetCurrent().GetActivatedEventArgs().Data` as
`ILaunchActivatedEventArgs` and parses them with `CommandLineToArgvW` when normal
process argv does not contain the MTP server/pipe options. Never substitute
`Invoke-CommandInDesktopPackage` or an unpackaged process for package activation.
Always-cleanup checks both recorded PID start time and current process package
identity before termination. Only the exact owned main package and certificate
are removed; shared SDK dependency packages remain untouched. Preserve real
failure/timeout/cancellation exits and require genuine TRX as for other platforms.

The launcher and actual Windows activation API hook can be cross-host compiled,
and ownership/process behavior is unit/Pester tested. Actual installed-MSIX,
WinUI/pipe and native argument-roundtrip runtime verification still requires a
Windows execution agent; do not claim that cross-host checks prove Windows CI.

## Invocation and results

Use the existing common templates, not a bespoke test scheduling framework.
`Start-TestDevice.ps1` provisions only the current job's resource and records its
identity in the unique result directory. Select its exact Android serial/iOS
UDID with `--device`; never use a global booted/default device. Cleanup must not
erase, stop or delete someone else's device.

Native category selection uses the validated build property
`-p:MauiTestFilter=Category=Shell`, or
`-p:MauiTestFilter=SkipCategories=CollectionView,Shell,HybridWebView`.
The native proxy does not expose xUnit's `--filter-trait` options. Do not use
legacy VSTest `--filter` or assume process environment reaches the native app.
Escape commas as `%2C` when passing this value through an MSBuild command-line
property; the common template performs this escaping.

Android uses `--report-trx --results-directory <unique-host-directory>`.
The current Apple proxy does **not** register/publish host `--report-trx`.
Apple hosts generate real TRX through MTP. Catalyst uses the explicit
`MauiTestResultsDirectory` build property for its absolute host destination.
iOS writes `Documents/TestResults/testResults.trx` in its native app container.
After `dotnet test`, use `xcrun simctl get_app_container <owned-udid> <bundle-id>
data` and copy that result directory before deleting the simulator. The freshly
created, job-owned simulator prevents stale app results from another execution.
Do not pass the Catalyst host path to the simulator app or assume
`--results-directory` extracts its container. Never synthesize TRX from stdout
or infer success from app launch.

Every job publishes genuine TRX, build binlogs and device logs under unique
job/attempt paths with `condition: always()`. Missing, malformed, empty,
failed or incomplete results fail the job independently of the launcher exit
code. Preserve failed process exit codes too. Timeouts/cancellation must run
bounded cleanup and always-publication tasks.

## Validation

```powershell
Invoke-Pester -Path ./eng/pipelines/ci.Tests.ps1,./eng/scripts/Assert-TestResults.Tests.ps1 -Output Detailed
```

Structural checks do not establish native execution or pool availability.
Validate representative real device jobs on each OS, runtime and packaging mode
before claiming equivalence or successful CI.
