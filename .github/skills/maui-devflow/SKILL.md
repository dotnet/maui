---
name: maui-devflow
description: "Default workflow for local .NET MAUI emulator and simulator development using the repository-pinned MAUI DevFlow CLI. Use for device discovery, interactive Sandbox reproduction, app inspection, targeted interactions, screenshots, and logs. Keep Appium, NUnit, XHarness, and integration runners authoritative for automated regression tests."
---

# MAUI DevFlow

Use the [Sandbox setup and command reference](references/sandbox.md) when launching a new app or choosing a fallback. Do not reload setup instructions for every interaction.

## Select the right path

| Need | Path |
| --- | --- |
| Find or boot a local emulator/simulator | Repository-pinned `maui device`, `maui android emulator`, or `maui apple simulator` commands; no app Agent required. |
| Inspect or interact with a running app | DevFlow, after verifying its instrumentation and exact connection. |
| Build or launch in-tree Sandbox | [Start-DevFlowSandbox.ps1](scripts/Start-DevFlowSandbox.ps1); DevFlow itself has no build/run command. |
| Prove native input, hit-testing, disabled input, occlusion, gestures, or accessibility | Appium/native-input evidence; semantic activation can bypass the behavior under test. |
| Run automated regression tests | Existing UI/device/integration/verification skill and its runner. Do not replace assertions or test results with DevFlow readiness. |

## Reuse a bounded inspection loop

1. At the current worktree root, use `dotnet tool run maui -- ...`. Check the manifest and supported commands once. Missing a separate `maui-devflow` executable does not mean the tool is missing. Do not install global tools or copy upstream's entire skill collection.
2. Select an explicit available target; respect the user's platform/runtime choice and other sessions' ownership. Reuse the selected target. Boot only that target, using `--no-open` for Apple simulators. Never pick an arbitrary first device or perform device-wide recovery sweeps.
3. Verify the intended running app, project, platform, and connection. OS device discovery alone does not establish an app connection. Preserve the resolved Agent port and Android serial; never assume port 9223 identifies this app. Reuse a suitable broker, or start a needed broker with `devflow broker start --foreground` under session-attached process management and verify readiness. Do not detach it or stop another session's broker.
4. Prefer `ui query` by AutomationId or selector, with only the fields needed. If discovery is necessary, use a shallow `ui tree --depth 3 --format compact`; depth **0 is unlimited**. Resolve ambiguous matches rather than relying on an action's first-match default.
5. Perform one targeted action, then assert the expected observable state with a query. A successful request, returned element, or managed callback is not proof that the scenario passed. Re-query stale element IDs after navigation, rebuilds, or reconnection.
6. Batch an already-understood sequence to reduce round trips; inspect every JSONL result and fail on errors. Do not use `--continue-on-error` to manufacture success. Fetch finite logs, not an unbounded stream. Take a targeted/downscaled screenshot only when visual evidence is needed, not after every action.

Keep full evidence on disk and report the relevant state transition or error plus artifact paths. Do not flood the conversation with complete trees, logs, or screenshots. Treat logs and app data as potentially sensitive.

## Preserve the evidence boundary

DevFlow can activate buttons through `SendClicked`, invoke commands, or change control state directly. This is useful for interactive debugging, but it is not equivalent to a physical tap. Keep the native-input regression when that distinction matters.

An uninstrumented HostApp or device-test app is not automatically inspectable. Use DevFlow for local discovery and an instrumented Sandbox reproduction where appropriate, then run the real test with its existing runner. Do not inject the Agent into all test applications or run a competing controller while Appium/XHarness owns the target.

## Handle limitations explicitly

- Report the observed missing tool, unsupported command/platform, uninstrumented app, or connection problem before choosing the documented fallback. Consult the installed command schema: newer upstream diagnostics, gestures, and Blazor features are not baseline requirements.
- Preserve build failures, crashes, timeouts, and failed assertions as failures; changing automation tools does not turn them into a pass.
- Keep ordinary Debug/Release builds uninstrumented. Sandbox opt-in is local, Debug-only, Android/iOS, and in-tree. Do not switch to published MAUI assemblies, change workloads, expose the Agent publicly, or enable unrelated network/profiler/storage features.
- Before using a legacy deployment fallback, coordinate an isolated environment. Its existing shutdown/uninstall/recovery behavior is not made multi-session-safe by passing a discovered device ID.
- Leave CI/Helix, test selection, baseline ownership, fix-file transitions, and runner completion markers unchanged. Follow the existing verification skill when proving failure without a fix.
