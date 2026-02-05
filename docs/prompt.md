You are implementing MAUI Dev Tools Client, a unified CLI tool (invoked as dotnet maui <command>) plus an optional service API that simplifies .NET MAUI dev environment setup, device management, and enables IDE + AI-agent automation.  ￼

1) Product goals

Implement a cross-platform tool that:
	•	Dramatically reduces MAUI environment setup friction (Android + Apple tooling).
	•	Provides doctor diagnostics with a high auto-fix rate.
	•	Emits machine-first output (--json on all commands).
	•	Can be used by IDEs and AI agents to query, diagnose, and remediate issues programmatically.  ￼

Target metrics (use as acceptance goals, not hard-coded tests):
	•	Setup time: <10 minutes for 90% of users
	•	Auto-fix rate: >80% of detected issues
	•	API coverage: JSON-RPC for all operations  ￼

2) Key design principles (non-negotiable)
	1.	Delegate to native tools: wrap existing system tooling (sdkmanager, xcrun simctl, etc.). No custom download/install logic beyond orchestrating native tools.  ￼
	2.	Consolidate existing VS tooling repos: design to replace capabilities currently spread across ClientTools.android-acquisition and android-platform-support.  ￼
	3.	Stateless-first: no daemon required; optionally introduce a background service later only for IDE performance.  ￼
	4.	Machine-first output: every command supports --json; stable schemas designed for automation and AI agents.  ￼

3) CLI surface area (minimum viable commands)

Implement the following commands with help text, examples, and --json output:

Environment diagnostics
	•	dotnet maui doctor
	•	Detect missing/incorrect components for Android and Apple flows.
	•	Offer auto-fixes where safe.
	•	Produce structured diagnostics and structured errors (see Error Contract).

Android bootstrap & management
	•	dotnet maui android bootstrap --accept-licenses
	•	Fully bootstrap Android dev from a fresh machine.  ￼
	•	Install in this order:
	1.	OpenJDK 17 → ~/Library/Developer/Android/jdk
	2.	Android SDK → ~/Library/Developer/Android/sdk
	3.	Recommended packages: platform-tools, build-tools, emulator, system image  ￼
	•	Must rely on native tools / OS package managers / official commands where possible.
	•	dotnet maui android avd create
	•	dotnet maui android avd start
	•	Wrap emulator/AVD flows (via Android tooling).

Apple simulator management (macOS only)
	•	dotnet maui apple simulator list
	•	dotnet maui apple simulator boot
	•	Wrap xcrun simctl operations.  ￼

Unified device operations
	•	dotnet maui device list
	•	dotnet maui device screenshot
	•	dotnet maui device logs
	•	Provide a unified abstraction over devices/simulators where feasible.  ￼

Deployment
	•	dotnet maui deploy
	•	Deploy apps to devices (initially focus on a thin wrapper around existing platform deploy tooling).  ￼

4) Output & UX requirements
	•	Default output: human-friendly, concise, actionable.
	•	--json: returns structured data (never mixed with human text on stdout).
	•	Support --verbosity and --diagnostics (or equivalent) for logs.
	•	Ensure commands are composable and scriptable (exit codes meaningful).

5) Error Contract (required)

All failures that are actionable should return structured machine-readable errors (especially under --json) following this pattern:  ￼

{
  "code": "E2001",
  "message": "Android SDK licenses not accepted",
  "remediation": {
    "type": "auto_fixable",
    "command": "dotnet maui android sdk accept-licenses"
  }
}

Requirements:
	•	Define an error code taxonomy (at least: Android SDK missing, JDK missing, licenses not accepted, Xcode missing, simctl failure, permissions).
	•	If remediation exists, include it (auto-fixable vs manual).
	•	For AI-agent usage, include enough context to troubleshoot without dumping secrets.

6) Optional service API (phase 2, but design for it now)

Design the code so the CLI can either:
	•	Execute directly (stateless mode), or
	•	Call a local JSON-RPC service (daemon) with the same operations for IDE performance.  ￼

Implement at minimum:
	•	A JSON-RPC contract definition (methods corresponding to CLI commands).
	•	A transport plan (stdio or local socket) but keep it simple.

7) Copilot / AI integration hooks

When auto-fix fails, provide structured context that can be consumed by AI tooling (MCP-style integration mentioned in the spec).  ￼
Deliverables:
	•	A “diagnostic bundle” object in JSON mode (detected environment, attempted fixes, tool outputs summarized, recommended next commands).

8) Engineering constraints & quality bar
	•	Cross-platform: Windows + macOS; Linux best-effort where tooling exists.
	•	No “magic”: prefer wrapping official tools and documenting prerequisites.
	•	Strong unit/integration tests for command parsing, JSON schemas, and error mapping.
	•	Add a small end-to-end test harness that can run in CI (mock external tools).
	•	Clear folder structure: commands, platform adapters, process execution, JSON schemas, error codes.

9) Acceptance criteria
	•	Each command listed exists, has --help, and supports --json.
	•	doctor detects at least: missing JDK, missing SDK, licenses not accepted; returns remediation commands.
	•	android bootstrap --accept-licenses orchestrates the stated install order and reports what it did.
	•	Apple simulator commands wrap xcrun simctl and return structured output.
	•	Error contract implemented consistently across commands.
