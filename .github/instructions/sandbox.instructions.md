---
description: "Sandbox testing guidance with DevFlow-first local inspection and explicit Appium fallback rules"
applyTo: "src/Controls/samples/Controls.Sample.Sandbox/**"
---

# Sandbox Testing Guide

Use the Sandbox sample for local repros, manual PR validation, and focused UI debugging. **Default to DevFlow for local interactive work.** Keep the existing automated runners and Appium workflows for the cases where they are still the authoritative source of evidence.

## When This Applies

Use this guidance when you:
- Work in `src/Controls/samples/Controls.Sample.Sandbox/`
- Reproduce a GitHub issue in Sandbox
- Validate a PR fix locally in Sandbox
- Need hands-on inspection of Android or iOS Sandbox behavior

Do **not** use this workflow to claim HostApp UITests, device tests, integration tests, Helix, or CI passed. Those remain owned by their existing runners and skills.

## Routing Summary

| Goal | Preferred Workflow | Why |
|---|---|---|
| Local emulator/simulator discovery, app inspection, semantic interaction, bounded debugging | `maui-devflow` skill | Fastest path for explicit device selection, app inspection, targeted actions, and observable state checks |
| Need native hit-testing, occlusion, disabled-input, gesture, or accessibility evidence | Legacy `BuildAndRunSandbox.ps1` + Appium fallback | Semantic DevFlow activation is not sufficient evidence for these regressions |
| Need automated regression tests in HostApp/NUnit | `write-ui-tests` / `verify-tests-fail-without-fix` | Sandbox validation does not replace authoritative test verification |
| Need baseline fail/pass transitions across a fix | `verify-tests-fail-without-fix` | That skill owns the state transition and baseline workflow |

## Core Rules

- Stay on the **current branch** unless the task explicitly names a PR to test.
- Never switch branches or use `git checkout`, `git restore`, `git reset`, or similar just to prove a broken baseline. Route that need to `verify-tests-fail-without-fix`.
- Preserve existing Sandbox edits. Distinguish **temporary task-created smoke fixtures** from **user-authored Sandbox work**.
- Preserve user reproduction content. Prefer adapting it over replacing it.
- Pick scenario source in this order:
  1. **Issue reproduction steps**
  2. **PR UI test or issue test page**
  3. **Custom scenario** only if neither of the above exists
- Choose affected platforms from the issue, PR title/labels, or changed file paths. If nothing narrows it and the scenario is cross-platform, start with Android for speed, then add iOS only when needed.
- Reports must stay concise and include: scenario source, explicit target, expected vs. observed behavior, evidence, and any tool/input limitations.
- **“App launched” or “page ready” is not the same as “bug fixed” or “tests passed.”**

## DevFlow-First Workflow

For local interactive Sandbox work, start with the `maui-devflow` skill and its Sandbox reference:

- `.github/skills/maui-devflow/SKILL.md`
- `.github/skills/maui-devflow/references/sandbox.md`

### 1. Use the repo-pinned CLI, not a separate install

- The repository already pins `Microsoft.Maui.Cli 0.1.0-preview.10.26274.3`.
- Invoke it from the worktree root as `dotnet tool run maui -- ...`.
- Do **not** tell users to install a separate `maui-devflow` executable, a global tool, a plugin, an MCP server, or a newer SDK just to use this workflow.

### 2. Select an explicit device you own

- Device-management commands work without app instrumentation.
- Select an explicit Android emulator serial or iOS simulator UDID that you own for this session.
- Reuse the same target for the rest of the session instead of rediscovering devices before every command.

### 3. Launch or attach Sandbox through the Sandbox launcher

Build and launch stay in the normal in-tree workflow. DevFlow itself does **not** provide the build/run step here.

The launcher is the authoritative path for DevFlow-enabled Sandbox startup:

- It opts Sandbox into DevFlow with `EnableMauiDevFlow=true`
- That opt-in is **Debug-only**, **Android/iOS-only**, and **in-tree**
- Ordinary Debug/Release builds remain uninstrumented unless that property is explicitly enabled
- It does not make HostApp, device-test apps, or integration-test apps inspectable

If you need to show a launcher example, keep it limited to the supported arguments:

```powershell
pwsh .github/skills/maui-devflow/scripts/Start-DevFlowSandbox.ps1 -Platform Android -DeviceId <android-serial>
pwsh .github/skills/maui-devflow/scripts/Start-DevFlowSandbox.ps1 -Platform iOS -DeviceId <ios-simulator-udid>
```

Do not invent extra launcher capabilities or parameters.

### 4. Reuse a verified Agent session when possible

- App-level DevFlow inspection requires a connected Agent.
- Reuse an existing verified Agent/broker/session when possible.
- Broker reuse/start coordination belongs to the `maui-devflow` skill. If a broker must be started, use `devflow broker start --foreground` and keep it **foreground, session-attached, and non-detached**.
- Preserve the actual resolved Agent/project/platform/port values once established.
- Reuse the launcher's returned `ready.json` and its `connectionArguments` instead of guessing a port or reselecting the app connection.
- `ready.json` with `testsRun: false` means the app is ready for inspection, not that the scenario was reproduced or validated.

### 5. Interact efficiently

Use a tight loop:

1. Bounded query by `AutomationId` or narrow selector
2. Ask for only the few fields needed
3. Perform one targeted action
4. Assert the observable state change

Typical loop:

```powershell
$ready = Get-Content "/path/to/ready.json" -Raw | ConvertFrom-Json
$connection = [string[]]$ready.connectionArguments
dotnet tool run maui -- devflow ui query --automationId "ScenarioButton" --fields id,type,automationId,text --format compact @connection
dotnet tool run maui -- devflow ui tap --automationId "ScenarioButton" @connection
dotnet tool run maui -- devflow ui query --automationId "ScenarioResult" --fields id,text --format compact @connection
```

Keep the session focused:
- Use a shallow, compact tree only if the targeted query is insufficient
- Use finite log windows rather than tailing indefinitely
- Capture screenshots only for genuinely visual evidence
- Do not repeat device setup, broad tree dumps, or screenshots after every command

### 6. Respect the pinned-preview capability boundary

- Treat preview10 as the baseline. Newer upstream/source features must be **capability-gated**, not assumed.
- `--device` mainly configures Android forwarding in the pinned CLI; do not assume it is universal iOS targeting for every `ui` command.
- `--project` is supported on `devflow wait`; do not demonstrate it as available on every `ui` command.
- `ui diagnostics` and `ui gesture` are **not** guaranteed in pinned preview10. Only use them when the skill/reference confirms availability.
- Current scope is the in-tree Sandbox Android/iOS integration. Do not imply initial Blazor support.

## Evidence Rules

DevFlow can perform **semantic activation**. That is useful, but it changes what the evidence means.

### What DevFlow *can* prove well

- An element with a known identifier or selector exists
- The app exposes the expected state or text
- A bounded semantic action causes an observable state change
- The app logs or reports a value consistent with the expected behavior

### What DevFlow semantic activation does **not** prove

DevFlow tap can call `SendClicked`, invoke commands, or set state directly. By itself, that does **not** prove:

- Native hit-testing
- Occlusion/z-order behavior
- Disabled input behavior
- Gesture routing
- Accessibility wiring

For those checks, use the legacy Appium/native-input fallback.

## Legacy Appium Fallback

Use the existing `BuildAndRunSandbox.ps1` flow only when DevFlow is not enough evidence or when the app is not Agent-enabled.

### When to fall back

- You need native hit-testing or gesture evidence
- You need accessibility-path evidence
- You need to compare semantic activation vs. native input
- The Sandbox app is not yet Agent-enabled on the target

### Safety boundary

Treat the legacy fallback as **not multi-session-safe**:

- `BuildAndRunSandbox.ps1`
- shared `Build-AndDeploy`
- shared `Start-Emulator`

Current implementations may:
- Shut down unrelated iOS simulators
- Uninstall broad Android package matches
- Restart global ADB

Because of that:
- Require an **isolated environment** or explicit user coordination before relying on the fallback
- Do **not** claim `-DeviceUdid` makes the workflow safe for shared multi-session use

### Fallback evidence rules

- Keep existing fallback output/log conventions under `CustomAgentLogsTmp/Sandbox/`
- `RunWithAppiumTest.cs` must be tailored to the actual Sandbox `AutomationId`s
- Script completion markers, readiness markers, or an exit code alone do **not** prove scenario assertions happened
- Use real assertions and observed behavior, not HTTP 200 responses or “completed successfully” text

### Android-only Appium requirement

Preserve this **only** for Android fallback runs:

```csharp
options.AddAdditionalAppiumOption("appium:noReset", true);
```

- This is required for Android Fast Deployment
- Do **not** add it to iOS

## Reporting

Use a concise report with:

- **Scenario source**: issue repro / PR UI test / custom
- **Explicit target**: platform + concrete device
- **Expected behavior**
- **Observed behavior**
- **Evidence**: state assertions, relevant logs, screenshots only if visual
- **Limitations**: what DevFlow or fallback could not prove

Good verdict language:
- `Observed in Sandbox via DevFlow`
- `Observed via native-input/Appium fallback`
- `Blocked by missing Agent / shared-device safety`
- `Ready for follow-up testing`

Avoid:
- “Tests passed” when you only observed local state
- “Fix validated” if you did not run the authoritative automated runner

## Related Workflows

- `write-ui-tests` still owns HostApp/NUnit UI test authoring
- `verify-tests-fail-without-fix` still owns broken-baseline and fixed-baseline verification
- `run-device-tests` still owns local XHarness device-test execution
- `run-integration-tests` still owns integration template/device runs

Do not inject the Sandbox Agent package or DevFlow expectations into HostApp, device-test apps, or integration-test apps unless a separate approved plan explicitly adds that instrumentation.
