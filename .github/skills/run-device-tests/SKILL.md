---
name: run-device-tests
description: "Build and run native MAUI device tests with plain dotnet test, explicit devices, native xUnit selection and genuine MTP TRX."
---

# Run device tests

Use `scripts/Run-DeviceTests.ps1` for Controls, Core, Essentials, Graphics or
BlazorWebView. macOS supports iOS, Mac Catalyst and Android; Windows supports
Android and unpackaged Windows. The repository SDK/workloads, built build tasks
and native SDK tools are prerequisites. XHarness is not used.

```powershell
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform ios -iOSVersion 26 -TestFilter Category=Shell
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Core -Platform android -DeviceUdid emulator-5554
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Core -Platform maccatalyst -IncludeClasses Microsoft.Maui.DeviceTests.WindowHandlerTests -IncludeMethods Runs -Rebuild
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Core -Platform windows -BuildOnly
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Core -Platform windows -WindowsPackaging packaged
```

## Parameters

Existing parameters remain supported: `Project`, `Platform`, `iOSVersion`,
`Configuration` (Release), `Rebuild`, `TestFilter`, `IncludeClasses`,
`IncludeMethods`, `BuildOnly`, `OutputDirectory` (artifacts/log), `Timeout`
(01:00:00), `DeviceUdid`, and `SkipXcodeVersionCheck`.
`WindowsPackaging` selects `unpackaged` (the existing default) or `packaged`.
Optional `RepoRoot` and `DotnetPath` select an explicit checkout/SDK.
Use `ProjectPath` instead of `Project` for another native test application inside
the selected checkout, including the device-test sample. Optional
`TargetFramework` preserves an explicitly selected platform TFM; it must match
`Platform`. Windows Cake callers use these parameters rather than duplicating
package activation or report validation.

Without `DeviceUdid`, the runner provisions a unique iOS simulator or Android
API 33 emulator using SDK tools, records its identity and cleans up only that
resource. Supplying an exact UDID/serial borrows that device without deleting or
stopping it. iOS major-version requests select an installed matching runtime;
unavailable runtimes fail rather than silently changing the requested OS.
Android SDK licenses must already be accepted.
Android cleanup derives its executable from the configured SDK and rejects a
persisted executable path that differs from that SDK's adb.
The owned iOS simulator uses an installed iPhone profile compatible with the
selected runtime. Cleanup revalidates its individual UUID and recorded unique
name; aggregate selectors and foreign simulator state cannot reach deletion.

`TestFilter` accepts `Category=X` or `SkipCategories=X,Y,Z`.
`IncludeClasses` accepts comma/semicolon-separated class FQNs; `IncludeMethods`
accepts simple method names with class scope, or class-qualified methods.
Selection becomes validated `MauiTestFilter`, `MauiTestClasses` and
`MauiTestMethods` MSBuild metadata. Commas/semicolons are escaped for MSBuild.
There is no source injection, category-index loop, or post-hoc acceptance of a
broad suite: genuine TRX identities must demonstrate exact target isolation,
including every requested class/method. Theory rows retain their real identities.

`Rebuild` rebuilds the full project-reference graph before `dotnet test --no-build`,
so Gate A/B attempts cannot accidentally reuse the other revision's dependencies.
Every invocation has a unique child result directory, even when reusing the same
`OutputDirectory`.

## Reporting and failure contract

Android and Windows use host `--report-trx --results-directory`.
The Android SDK proxy rewrites method identities to display names. The local
runner also pulls the app's original `files/TestResults/testResults.trx` from
external storage using the project's literal `ApplicationId` and exact device
serial. That native report alone supplies counts and selector verification;
the host proxy report is retained for diagnostics and must agree on outcomes.
A pre-run snapshot rejects unchanged results on borrowed Android devices.
The current Apple proxy does not register host `--report-trx`.
iOS writes native `Documents/TestResults/testResults.trx`; the local runner
extracts that genuine file through `simctl get_app_container` and rejects an
unchanged result left by a previous run on a borrowed simulator. Catalyst uses
an explicit `MauiTestResultsDirectory` host path.

The runner emits the verifier's existing `Passed:`, `Failed:`, `Skipped:` and
`Total:` summary only after validating real TRX. Empty/all-skipped/missing
targets, malformed reports, broad execution, crashes, timeouts, run-level
errors and infrastructure exit failures cannot become success. In particular,
a nonzero launcher exit is **not** ignored because a passing TRX exists.
Stdout is diagnostic only and never synthesized into TRX or trusted counts.

GH tokens are stripped at every build/test child-process start; PR-controlled
console text is defanged before printing. Logs and native reports remain in
the unique result directory. Scoped Windows timeouts retain the verifier's
`WINDOWS_DEVICE_TEST_TARGET_TIMEOUT:` marker and missing reports retain
`WINDOWS_DEVICE_TEST_NO_RESULTS:`.

Windows packaged execution builds a signed MSIX with the original application
ID and a job-unique signing publisher, installs it and its x64 SDK dependencies
with `Add-AppxPackage`, then uses evaluated `RunCommand`/`RunArguments` to launch
the standard MTP server through the small `windows-launcher` shim. The shim uses
`IApplicationActivationManager.ActivateApplication`, forwards the complete MTP
argument vector, verifies the activated PID's actual package identity and returns
its real exit code. Native WinUI startup recovers activation arguments through
`AppInstance.GetCurrent().GetActivatedEventArgs().Data` as
`ILaunchActivatedEventArgs`, parsing with `CommandLineToArgvW` when needed.
The app verifies both expected package name and publisher before running tests.
There is no unpackaged or `Invoke-CommandInDesktopPackage` fallback.

The state file records owner, package identity, certificate, activated PID and
start time. Cleanup kills only that PID after matching start time and package
identity and verifying the actual package registration's name and publisher
against the owned session. It uninstalls only the job-owned main package and removes its signing
certificate/private key. Shared SDK dependencies are never uninstalled. Cleanup
verifies each actual certificate's subject and the private certificate's
session-specific friendly name before deleting either certificate-store entry.
Cleanup failure cannot emit a passing Gate summary. A logged-in Windows desktop and
permission to install a signed MSIX remain agent prerequisites. The non-exportable
private key stays in `CurrentUser\My`; only its public certificate is trusted in
`CurrentUser\TrustedPeople`. No private PFX/password is exported or published.

## Validation

```powershell
Invoke-Pester -Path .github/skills/run-device-tests/scripts/Run-DeviceTests.Tests.ps1,.github/skills/run-device-tests/scripts/WindowsTestPackage.Tests.ps1 -Output Detailed
```

The suite tests selectors, TRX identities/theory rows, process failure/timeout,
credential stripping, real subprocess invocation, A/B rebuild and stale-result
rejection using a fake SDK. It does not claim native device execution.
`TestUtils.UnitTests` additionally exercises Windows quoting, real child-process
exit/failure/timeout/cancellation, and package/PID ownership. The native
`CommandLineToArgvW` roundtrip test runs only on Windows. Cross-host compilation
does not establish real installed-MSIX activation or successful Windows CI.
