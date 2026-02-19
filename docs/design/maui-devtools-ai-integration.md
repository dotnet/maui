# MAUI Dev Tools — AI Agent Integration

**Parent Document**: [MAUI Dev Tools Specification](./maui-devtools-spec.md)

This document details how AI agents (GitHub Copilot, IDE assistants) interact with MAUI Dev Tools.

---

## Table of Contents

1. [Design Principle: Machine-First Output](#1-design-principle-machine-first-output)
2. [AI Agent Persona & Journey](#2-ai-agent-persona--journey)
3. [Copilot-Assisted Troubleshooting](#3-copilot-assisted-troubleshooting)
4. [Permission Model](#4-permission-model)
5. [AI Agent Considerations](#5-ai-agent-considerations)

---

## 1. Design Principle: Machine-First Output

**Every command MUST support `--json` output with a stable, versioned schema.**

This tool is designed for three consumers in priority order:
1. **AI Agents** (GitHub Copilot, IDE assistants) — need structured data to reason about
2. **CI/CD Pipelines** — need deterministic exit codes and parseable output
3. **Human Developers** — need readable summaries with color and formatting

**Implication**: If an error message is ambiguous plain text, AI agents will fail to use the tool reliably. Every failure must be expressed as structured data.

```bash
# Human-friendly (default)
maui doctor
# ✓ .NET SDK 9.0.100
# ✗ Android SDK not found

# Machine-friendly (for AI/CI)
maui doctor --json
# { "status": "unhealthy", "checks": [...], "errors": [...] }
```

**Required flags for all commands**:
| Flag | Purpose |
|------|---------|
| `--json` | Output structured JSON instead of human-readable text |
| `--dry-run` | Show what would be done without executing (enables "what will this do?" UX) |
| `--ci` | Strict mode: no interactive prompts, non-zero exit on warnings, machine-readable only |

---

## 2. AI Agent Persona & Journey

**Profile**: Copilot agent assisting a developer with a build error.

**Journey**:
1. User asks: "My Android build is failing with 'SDK not found'"
2. Agent invokes: `maui doctor --json --platform android`
3. Agent receives structured response:
   ```json
   {
     "status": "unhealthy",
     "issues": [
       {
         "id": "ANDROID_SDK_MISSING",
         "severity": "error",
         "message": "Android SDK not found",
         "fixable": true,
         "fix_command": "maui android sdk install --recommended"
       }
     ]
   }
   ```
4. Agent explains the issue and asks: "Would you like me to install the Android SDK?"
5. User confirms; agent invokes fix command with permission gate
6. Agent verifies fix by re-running doctor

**Key Requirements**:
- Structured JSON output with stable schema
- Fix commands included in diagnostic output
- Permission gates for destructive operations
- Correlation IDs for tracing

---

## 3. Copilot-Assisted Troubleshooting

When automated fixes fail or issues are too complex for deterministic resolution, the tool can escalate to GitHub Copilot for AI-assisted troubleshooting. This provides a graceful fallback that combines structured diagnostics with conversational problem-solving.

### Escalation Triggers

The tool escalates to Copilot when:

| Trigger | Example | Escalation Action |
|---------|---------|-------------------|
| Fix failed | SDK install fails with network error | Offer "Ask Copilot for help" |
| Unknown issue | Unrecognized error code from `adb` | Auto-suggest Copilot assistance |
| Complex diagnosis | Multiple interrelated issues | Recommend Copilot review |
| User request | User clicks "Get AI Help" | Open Copilot with context |

### Context Handoff

When escalating to Copilot, the tool provides structured context:

```json
{
  "escalation_type": "fix_failed",
  "tool_version": "1.2.0",
  "doctor_report": { /* full doctor.status output */ },
  "failed_operation": {
    "command": "android.sdk.install",
    "params": { "packages": ["platforms;android-34"] },
    "error": {
      "code": "E4001",
      "message": "Download failed: Connection reset",
      "details": "Failed after 3 retries to dl.google.com"
    }
  },
  "environment": {
    "os": "macOS 15.0",
    "proxy_configured": true,
    "network_type": "corporate"
  },
  "attempted_fixes": [
    { "action": "retry_download", "result": "failed" },
    { "action": "clear_cache", "result": "failed" }
  ]
}
```

### CLI Integration

```bash
$ maui doctor --fix
...
✖ Failed to install Android SDK

Error: Connection reset while downloading (E4001)

Attempted fixes:
  • Retry download (3 times) - failed
  • Clear download cache - failed

Would you like to get help from Copilot? [Y/n] y

Opening Copilot with diagnostic context...
Run: gh copilot explain "MAUI SDK install failure E4001"

Or copy this diagnostic summary:
  maui doctor --json --verbose > /tmp/maui-diag.json
  gh copilot explain --file /tmp/maui-diag.json
```

### MCP Tool Integration

When running as an MCP server, the tool exposes actions that Copilot can invoke (with user approval):

```json
{
  "tools": [
    {
      "name": "maui_doctor_status",
      "description": "Check MAUI development environment health",
      "requires_approval": false
    },
    {
      "name": "maui_doctor_fix",
      "description": "Attempt to fix detected environment issues",
      "requires_approval": true
    }
  ]
}
```

### Example Copilot Conversation

```
User: My MAUI Android build keeps failing with SDK errors

Copilot: Let me check your MAUI environment.
         [Calling maui_doctor_status...]

         I found several issues:
         1. ❌ Android SDK not found at expected location
         2. ⚠️ ANDROID_HOME environment variable not set

         It looks like the Android SDK may not be installed, or it's
         installed in a non-standard location.

         Would you like me to:
         a) Install the Android SDK to the default location
         b) Help you configure ANDROID_HOME to point to an existing SDK

User: Install it please

Copilot: I'll install the Android SDK with recommended components.
         [Requesting approval for maui_doctor_fix...]

         ✓ User approved

         [Calling maui_doctor_fix with issue_id="ANDROID_SDK_MISSING"...]

         ✓ Android SDK installed successfully
         ✓ ANDROID_HOME configured

         Your environment is now ready. Try building again!
```

### Fallback Hierarchy

```
┌─────────────────────────────────────────────────────────────┐
│ 1. Automated Fix (maui doctor --fix)                        │
│    Tool attempts deterministic fix based on known patterns  │
├─────────────────────────────────────────────────────────────┤
│ 2. Guided Manual Fix                                        │
│    Tool provides step-by-step instructions for user         │
├─────────────────────────────────────────────────────────────┤
│ 3. Copilot-Assisted Troubleshooting                         │
│    AI helps diagnose complex/unknown issues conversationally│
├─────────────────────────────────────────────────────────────┤
│ 4. Community/Support Escalation                             │
│    Link to GitHub issues, docs, or support channels         │
└─────────────────────────────────────────────────────────────┘
```

### Privacy Considerations

When escalating to Copilot:

- **Opt-in**: User must explicitly request Copilot assistance
- **Data minimization**: Only relevant diagnostic data shared
- **Redaction**: PII (paths, device names) redacted per privacy rules
- **Transparency**: User can view exact data being shared before sending
- **Local option**: `--offline-diagnosis` generates report without sending to Copilot

---

## 4. Permission Model

### Permission Gates

When invoked by an AI agent, certain operations require explicit user confirmation. The complete mapping of commands to permission gates is defined in the **[Capabilities Model (§7.3)](./maui-devtools-spec.md#73-capabilities-model)** in the main specification.

**Permission summary by gate**:

| Permission | Operations | Confirmation UI |
|------------|-----------|-----------------|
| None | `doctor.status`, `device.list`, `sdk.list`, `simulator.list`, `runtime.list`, `xcode.list` | — |
| `device.capture` | `device.screenshot` | IDE prompt |
| `device.logs` | `device.logs` (streaming) | IDE prompt |
| `device.create` | `android.emulator.create/delete`, `apple.simulator.create/delete` | IDE prompt |
| `environment.modify` | `doctor.fix`, `android.sdk.install`, `android.install`, `android.jdk.install`, `apple.install`, `apple.runtime.install`, `apple.xcode.select`, `apple.xcode.accept-licenses` | IDE prompt with details |

**Permission Flow**:
```
┌─────────────────────────────────────────────────────────────┐
│  AI Agent Request                                           │
│  "Install Android SDK build-tools"                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  The AI assistant wants to install:                         │
│    • build-tools;34.0.0 (52 MB)                            │
│                                                             │
│  This will modify your Android SDK installation.            │
│                                                             │
│  [Allow]  [Allow Once]  [Deny]                             │
│                                                             │
│  □ Remember this choice for this session                   │
└─────────────────────────────────────────────────────────────┘
```

### Sandbox Boundaries

AI agent calls are sandboxed:
- Cannot access arbitrary file system paths
- Cannot execute arbitrary commands
- Limited to defined API surface
- All actions logged with agent identifier

**Safe Paths** (agents may read/write only within):
- Project root directory
- Temp directory (`$TMPDIR` / `%TEMP%`)
- SDK directories (Android SDK, Xcode Developer directory)
- Tool cache directory (`~/.maui-devtools/`)

**Path Validation**:
- Reject paths containing `..`
- Reject absolute paths outside safe paths
- Validate in CLI Layer before dispatching to services

### Permission Storage

| Context | Storage Location | Lifetime |
|---------|------------------|----------|
| IDE Session | In-memory | Until IDE closes |
| Terminal | Per-command | Single invocation |
| CI | Environment variable `MAUI_DEVTOOLS_ALLOW_MODIFY=1` | Pipeline run |
| Persistent | `~/.config/maui-devtools/permissions.json` | Until revoked |

**Persistent Permission Schema**:
```json
{
  "schema_version": 1,
  "agents": {
    "github-copilot": {
      "environment.modify": "allow",
      "device.create": "prompt",
      "device.capture": "allow"
    },
    "default": {
      "environment.modify": "prompt",
      "device.create": "prompt",
      "device.capture": "prompt"
    }
  }
}
```

**Permission Values**:
- `allow`: Permitted without prompt
- `prompt`: Requires user confirmation each time
- `deny`: Blocked (agent receives permission error)

### Elevation Handling

> **See [Elevation Model (§5.4)](./maui-devtools-spec.md#54-security)** in the main spec for full platform-specific elevation flows.

When an operation requires OS elevation, AI agents have additional constraints:

| Scenario | AI Agent Behavior |
|----------|-------------------|
| Elevation needed (Windows UAC) | Agent receives `E5001` error. Cannot auto-approve UAC. Must inform user and suggest running from elevated terminal or using default user-writable paths. |
| Elevation needed (macOS sudo) | Agent receives `E5001` error. Cannot provide sudo password. Must inform user and suggest running the command manually. |
| Default paths (no elevation) | Agent can proceed normally with `environment.modify` permission gate. |
| CI environment (`--ci` flag) | Assumes pre-authorized. No elevation prompts — fails with `E5001` if permissions insufficient. |

**Key rule**: Elevated operations are **never auto-approved** by AI agents. The agent must always inform the user and provide alternatives (e.g., installing to a user-writable default path instead).

---

## 5. AI Agent Considerations

The dotnet/sdk spec explicitly mentions AI agents:

> "This has become more relevant in the AI era, as someone is going to expect AIs in 'agent mode' to build and run their app."

MAUI DevTools is designed with AI agents as first-class consumers:

| Requirement | How DevTools Addresses It |
|-------------|---------------------------|
| Structured output | All commands support `--json` with stable schema |
| Discoverability | `--list-devices` provides machine-parseable device list |
| Non-interactive | `--non-interactive` mode with helpful error messages |
| Deterministic | Same inputs produce same outputs; idempotent operations |
| Permission gates | Explicit confirmation for modifications (see §4) |

**AI Agent Workflow Example**:

```python
# 1. Discover available devices
devices = run("maui device list --platform android --json")

# 2. Select appropriate device (first online emulator)
device = next(d for d in devices if d["status"] == "Online" and d["type"] == "Emulator")

# 3. Build and deploy (with user confirmation via IDE)
run(f"maui deploy --device {device['id']} --project ./MyApp.csproj")

# 4. Capture screenshot for verification
run(f"maui device screenshot --device {device['id']} --output ./screenshot.png")
```

### Error Contract for AI Agents

AI agents depend on predictable error handling. Every error includes:

| Field | Purpose |
|-------|---------|
| `code` | Unique error code (e.g., `E1001`) |
| `category` | Error category for routing |
| `message` | Human-readable description |
| `remediation` | Suggested fix actions |

**Remediation Types**:

| Type | Meaning | AI Agent Behavior |
|------|---------|-------------------|
| `automatic` | Tool can fix this automatically | Call `doctor.fix` with issue ID |
| `manual` | Requires user action | Present instructions to user |
| `escalate` | Tool doesn't know how to fix | Trigger Copilot Handoff |
| `unknown` | Tool doesn't recognize this error | Escalate to Copilot Handoff |
