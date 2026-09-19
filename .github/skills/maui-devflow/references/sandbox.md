# In-tree Sandbox with DevFlow

- [Prerequisites and isolation](#prerequisites-and-isolation)
- [Select a target and broker](#select-a-target-and-broker)
- [Launch and retain the connection](#launch-and-retain-the-connection)
- [Inspect with bounded output](#inspect-with-bounded-output)
- [Compatibility and fallback](#compatibility-and-fallback)
- [Maintaining this integration](#maintaining-this-integration)

## Prerequisites and isolation

Work at the current MAUI worktree root, not another checkout. Use its `global.json`, installed platform SDKs, and `.config/dotnet-tools.json`. The CLI already includes DevFlow:

```powershell
dotnet tool run maui -- devflow --help
```

If that fails because the local tool is missing, restore the existing manifest with `dotnet tool restore`; do not install or update a global CLI. Build `Microsoft.Maui.BuildTasks.slnf` using the repository's normal prerequisites before launching Sandbox. Do not upgrade workloads to conceal a compatibility failure.

Sandbox instrumentation is **off by default**. The launcher explicitly enables `EnableMauiDevFlow=true` for an Android/iOS **Debug, in-tree** build. It does not change the application ID or default page, enable Blazor tooling, or instrument HostApp/device-test applications. Network capture and profiling are disabled in the bootstrap.

Use an explicitly available device; coordinate its ownership before deploying. Installing Sandbox replaces that device's Sandbox application, so do not use another session's target. Keep Appium/XHarness and DevFlow from controlling the same target concurrently.

The launcher never boots or erases devices, stops simulators, restarts global ADB, uninstalls unrelated packages, starts brokers, or falls back to Appium.

## Select a target and broker

Discover once; retain the selected identifier:

```powershell
dotnet tool run maui -- device list --platform android --json
dotnet tool run maui -- device list --platform apple --json
```

Filter the saved JSON to the relevant platform/runtime rather than repeatedly dumping every device. In the pinned CLI, Apple discovery can prefix stdout with `Info:` lines; the launcher handles that known prefix. Preserve stderr separately from machine-readable command results.

Boot only the chosen target when needed:

```powershell
dotnet tool run maui -- android emulator start "SELECTED_AVD_NAME" --wait
dotnet tool run maui -- apple simulator start "SELECTED_SIMULATOR_UDID" --no-open
```

After an Android boot, discover its **running serial**, such as `emulator-5554`; the AVD name is not the launcher's device ID. For iOS, retain the exact simulator UDID. The launcher verifies the selected device is virtual and running.

Check broker status:

```powershell
dotnet tool run maui -- devflow broker status --json
```

Reuse a suitable running broker. If none exists, start this command as a **session-attached background process** (or in a terminal retained for this session), then verify status:

```powershell
dotnet tool run maui -- devflow broker start --foreground
```

Do not omit `--foreground`, silently detach the process, or stop another session's broker. An unused broker can exit after its idle timeout during a long cold build. If the launcher reports that, start a foreground broker again and rerun the now-incremental build; do not hide the failure with a detached replacement.

## Launch and retain the connection

Read the issue/reproduction first, preserve user edits, and give relevant controls stable AutomationIds. The permanent Sandbox page remains empty; add only the scenario being investigated.

Use a new/empty artifact directory, preferably under this session's persistent files directory:

```powershell
pwsh -NoProfile .github/skills/maui-devflow/scripts/Start-DevFlowSandbox.ps1 `
    -Platform Android -DeviceId "emulator-5554" -ArtifactDirectory "/path/to/session/files/devflow-run"

pwsh -NoProfile .github/skills/maui-devflow/scripts/Start-DevFlowSandbox.ps1 `
    -Platform iOS -DeviceId "SELECTED_SIMULATOR_UDID" -ArtifactDirectory "/path/to/session/files/devflow-ios"
```

Without `-ArtifactDirectory`, the launcher creates a unique directory under the system temporary directory and reports its path. Optional `-TimeoutSeconds` bounds Agent readiness; `-BuildTimeoutSeconds` bounds each SDK build/launch process. Unsupported configurations fail explicitly.

The launcher uses normal SDK builds, an explicit Android `AdbTarget`, or targeted Xcode simulator install/launch commands. It scrubs credentials at each child-process boundary and retains stdout, stderr, and binlogs. These are local-development artifacts, not trusted CI verdict files.

Each launch embeds a unique `MauiDevFlowSessionId`. Readiness is filtered by project/platform and then checked against that fresh session, TFM, CLI/Agent version, actual reported port, and app health. The pinned broker does not report a universal physical-device ID: device targeting is established by the scoped SDK deployment plus the fresh build identity, not by treating `--device` as an iOS selector.

Keep `ready.json` and reuse its connection:

```powershell
$ready = Get-Content "/path/to/session/files/devflow-run/ready.json" -Raw | ConvertFrom-Json
$connection = [string[]]$ready.connectionArguments
dotnet tool run maui -- devflow agent status @connection
```

`status: ready` and `testsRun: false` mean **the app is ready for inspection**, not that a bug is reproduced or fixed. A crashed app, failed SDK command, stale/ambiguous Agent, malformed response, or timeout does not produce a ready record. In preview.10, `devflow wait` can time out with exit code zero and plain text; never treat its exit code alone as success.

## Inspect with bounded output

Use the connection arguments from the selected run, not a guessed port or a new global Agent selection:

```powershell
dotnet tool run maui -- devflow ui query --automationId "ScenarioButton" --fields id,type,automationId,text --format compact @connection
dotnet tool run maui -- devflow ui tap --automationId "ScenarioButton" @connection
dotnet tool run maui -- devflow ui query --automationId "ScenarioResult" --fields id,text --format compact @connection
```

Assert the **specific expected value/state**, not merely a nonempty query or successful activation. Re-resolve ambiguous/stale elements rather than selecting the first match. This semantic loop does not prove physical input behavior.

Only expand discovery or evidence when needed:

```powershell
dotnet tool run maui -- devflow ui tree --depth 3 --fields id,type,automationId,text --format compact @connection
dotnet tool run maui -- devflow logs --limit 20 @connection
dotnet tool run maui -- devflow ui screenshot --output "/path/to/session/files/scenario.png" --max-width 480 @connection
```

Use an element ID or selector to target a screenshot when appropriate. Do not request a screenshot after every successful command, use unlimited tree depth (`0`), or stream logs indefinitely. The CLI may retain JSON output when piped even with `--format compact`; field selection still bounds its content.

For a sequence already understood, `devflow batch` accepts command lines on stdin and emits JSONL. Consult its installed help/schema once, keep the same connection, inspect every result, and stop on failure. Do not batch speculative actions or use `--continue-on-error` as a success shortcut.

## Compatibility and fallback

The initial contract is CLI/Agent `0.1.0-preview.10.26274.3`. The [upstream source at the package commit](https://github.com/dotnet/maui-labs/tree/40fa9a00211aa034ebf299232df3df190c38089d/src/DevFlow) is experimental; do not assume later upstream commands exist in this pin. In particular, `ui diagnostics` and `ui gesture` are not baseline commands. A zero exit from unknown-command help does not prove support; inspect the actual command list/schema.

Use an explicit fallback for a missing capability, uninstrumented app, unsupported platform, or native-input requirement. Keep HostApp/NUnit/Appium, XHarness device tests, integration tests, and fail-without-fix verification authoritative. Do not substitute a semantic `SendClicked`/command activation for hit-testing, disabled-input, occlusion, gesture, or accessibility coverage.

For the legacy Appium Sandbox path, follow [the Sandbox instructions](../../../instructions/sandbox.instructions.md). It requires a scenario-specific `CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs`, not an untouched template. Retain Android-only `appium:noReset=true` for Fast Deployment. Coordinate an isolated environment first: the existing runner's shared deployment helpers can shut down other simulators or perform broad Android recovery. A `DeviceUdid` argument does not make those helpers multi-session-safe.

## Maintaining this integration

Keep the Agent version in `eng/Versions.props` aligned with the local CLI manifest. Use existing repository NuGet sources; standard NuGet restore resolves the package from `dotnet-public`.

The Agent transitively references the Controls metapackage. Sandbox excludes **only the build/buildTransitive assets of its two build-only dependencies**, Controls.Build.Tasks and Resizetizer, so those packages cannot duplicate the worktree's task imports. Keep the Agent itself as a runtime dependency: `PrivateAssets=all` omits it from the iOS publish bundle. Do not exclude all Agent assets, suppress duplicate-import diagnostics, or switch to published MAUI implementation assemblies.

The project checks resolved framework references remain project references and rejects stale Agent assets after switching opt-in/configuration without restoring. Always restore when changing `EnableMauiDevFlow`. Ordinary Debug/Release builds must contain no Agent dependency/bootstrap; do not commit smoke-fixture UI or generated metadata.

iOS copies bundle assemblies after `IncrementalClean`. `TrackMauiDevFlowBundleFiles` registers the optional Agent DLLs with normal `FileWrites` tracking so the next opted-out build removes them. Keep this registration independent of copy-task outputs, including when the copy is skipped as up-to-date.

Run the focused, device-free Pester checks after launcher or bundle-output tracking changes:

```powershell
Invoke-Pester .github/skills/maui-devflow/scripts/Start-DevFlowSandbox.Tests.ps1
```

Also exercise opted-out Debug/Release, opted-in Android/iOS, unsupported settings, incremental opt-in changes, and a real query/action/state assertion on an explicitly selected target. Keep missing-platform evidence marked unavailable, not passed.
