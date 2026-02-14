# PR Agent: Shared Rules

This file contains critical rules that apply across all PR agent phases. Referenced by `pr.md`, `post-gate.md`, and `PLAN-TEMPLATE.md`.

---

## Phase Completion Protocol

**Before changing ANY phase status to ✅ COMPLETE:**

1. **Read the state file section** for the phase you're completing
2. **Find ALL ⏳ PENDING and [PENDING] fields** in that section
3. **Fill in every field** with actual content
4. **Verify no pending markers remain** in your section
5. **Save the state file** (it's in gitignored `CustomAgentLogsTmp/`)
6. **Then change status** to ✅ COMPLETE

**Rule:** Status ✅ means "documentation complete", not "I finished thinking about it"

---

## Follow Templates EXACTLY

When creating state files, use the EXACT format from the documentation:
- **Do NOT add attributes** like `open` to `<details>` tags
- **Do NOT "improve"** the template format
- **Do NOT deviate** from documented structure
- Downstream scripts depend on exact formatting (regex patterns expect specific structure)

---

## Agent Labels (Automated by Review-PR.ps1)

After all phases complete, `Review-PR.ps1` automatically applies GitHub labels based on phase outcomes. The agent does NOT need to apply labels — just write accurate `content.md` files.

### Label Categories

**Outcome labels** (mutually exclusive — exactly one per PR):
| Label | When Applied |
|-------|-------------|
| `s/agent-approved` | Report recommends APPROVE |
| `s/agent-changes-requested` | Report recommends REQUEST CHANGES |
| `s/agent-review-incomplete` | Agent didn't complete all phases |

**Signal labels** (additive — multiple can coexist):
| Label | When Applied |
|-------|-------------|
| `s/agent-gate-passed` | Gate phase passes |
| `s/agent-gate-failed` | Gate phase fails |
| `s/agent-fix-win` | Agent found a better alternative fix than the PR |
| `s/agent-fix-lose` | PR's fix is the best — agent couldn't beat it |

**Tracking label** (always applied):
| Label | When Applied |
|-------|-------------|
| `s/agent-reviewed` | Every completed agent run |

### How Labels Are Determined

Labels are parsed from `content.md` files:
- **Outcome**: from `report/content.md` — looks for `Final Recommendation: APPROVE` or `REQUEST CHANGES`
- **Gate**: from `gate/content.md` — looks for `PASSED` or `FAILED`
- **Fix**: from `try-fix/content.md` — looks for alternative selected (win = agent beat PR) vs `Selected Fix: PR` (lose = PR was best)

**Agent responsibility**: Write clear, parseable `content.md` with standard markers (`✅ PASSED`, `❌ FAILED`, `Selected Fix: PR`, `Final Recommendation: APPROVE`).

---

## Agent Labels (Automated by Review-PR.ps1)

After all phases complete, `Review-PR.ps1` automatically applies GitHub labels based on phase outcomes. The agent does NOT need to apply labels — just write accurate `content.md` files.

### Label Categories

**Outcome labels** (mutually exclusive — exactly one per PR):
| Label | When Applied |
|-------|-------------|
| `s/agent-approved` | Report recommends APPROVE |
| `s/agent-changes-requested` | Report recommends REQUEST CHANGES |
| `s/agent-review-incomplete` | Agent didn't complete all phases |

**Signal labels** (additive — multiple can coexist):
| Label | When Applied |
|-------|-------------|
| `s/agent-gate-passed` | Gate phase passes |
| `s/agent-gate-failed` | Gate phase fails |
| `s/agent-fix-win` | Agent found a better alternative fix than the PR |
| `s/agent-fix-pr-picked` | PR's fix is the best — agent couldn't beat it |

**Tracking label** (always applied):
| Label | When Applied |
|-------|-------------|
| `s/agent-reviewed` | Every completed agent run |

### How Labels Are Determined

Labels are parsed from `content.md` files:
- **Outcome**: from `report/content.md` — looks for `Final Recommendation: APPROVE` or `REQUEST CHANGES`
- **Gate**: from `gate/content.md` — looks for `PASSED` or `FAILED`
- **Fix**: from `try-fix/content.md` — looks for alternative selected (win = agent beat PR) vs `Selected Fix: PR` (lose = PR was best)

**Agent responsibility**: Write clear, parseable `content.md` with standard markers (`✅ PASSED`, `❌ FAILED`, `Selected Fix: PR`, `Final Recommendation: APPROVE`).

---

## No Direct Git Commands

**Never run git commands that change branch or file state.**

The agent is always invoked from the correct branch. All file state management is handled by PowerShell scripts (`verify-tests-fail.ps1`, `try-fix`, etc.).

**What to do instead:**
- Use `gh pr diff` or `gh pr view` to see PR info (read-only GitHub CLI)
- Use `gh pr diff <number> --name-only` to list changed files
- Let scripts handle all file manipulation internally

**Never run these commands:**
- ❌ `git checkout` (any form)
- ❌ `git switch`
- ❌ `git stash`
- ❌ `git reset`
- ❌ `git revert`
- ❌ `gh pr checkout`
- ❌ `git fetch` (for branch switching purposes)

---

## Use Skills' Scripts - Don't Bypass

When a skill provides a PowerShell script:
- **Run the script** - don't interpret what it does and do it manually
- **Fix inputs if script fails** - don't bypass with manual `gh` commands
- **Use `-DryRun` to debug** - see what the script would produce before posting
- Scripts handle formatting, API calls, and section management correctly

---

## Stop on Environment Blockers

If you encounter an environment or system setup blocker that prevents completing a phase:

**STOP IMMEDIATELY. Do NOT continue to the next phase.**

### Common Blockers

- Missing Appium drivers (Windows, iOS, Android)
- WinAppDriver not installed or returning errors
- Xcode/iOS simulators not available (on Windows)
- Android emulator not running or not configured
- Developer Mode not enabled
- Port conflicts (e.g., 4723 in use)
- Missing SDKs or tools
- Server errors (500, timeout, "unknown error occurred")

### Retry Limits (STRICT ENFORCEMENT)

| Blocker Type | Max Retries | Then Do |
|--------------|-------------|---------|
| Missing tool/driver | 1 install attempt | STOP and ask user |
| Server errors (500, timeout) | 0 | STOP immediately and report |
| Port conflicts | 1 (kill process) | STOP and ask user |
| Configuration issues | 1 fix attempt | STOP and ask user |

### When Blocked

1. **Stop all work** - Do not proceed to the next phase
2. **Do NOT keep troubleshooting** - After the retry limit, STOP
3. **Report the blocker** clearly (use template below)
4. **Ask the user** how to proceed
5. **Wait for user response** - Do not assume or work around

### Blocker Report Template

```
⛔ BLOCKED: Cannot complete [Phase Name]

**What failed:** [Step/skill that failed]
**Blocker:** [Tool/driver/error type]
**Error:** "[Exact error message]"

**What I tried:** [List retry attempts, max 1-2]

**I am STOPPING here. Options:**
1. [Option for user - e.g., investigate setup manually]
2. [Alternative platform]
3. [Skip with documented limitation]

Which would you like me to do?
```

### Never Do

- ❌ Keep trying different fixes after retry limit exceeded
- ❌ Mark a phase as ⚠️ BLOCKED and continue to the next phase
- ❌ Claim "verification passed" when tests couldn't actually run
- ❌ Skip device/emulator testing and proceed with code review only
- ❌ Install multiple tools/drivers without asking between each
- ❌ Spend more than 2-3 tool calls troubleshooting the same blocker

---

## Multi-Model Configuration

Phase 4 uses these 5 AI models for try-fix exploration (run SEQUENTIALLY):

| Order | Model |
|-------|-------|
| 1 | `claude-sonnet-4.5` |
| 2 | `claude-opus-4.5` |
| 3 | `gpt-5.2` |
| 4 | `gpt-5.2-codex` |
| 5 | `gemini-3-pro-preview` |

**Note:** The `model` parameter is passed to the `task` tool, which supports model selection. This is separate from agent YAML frontmatter (which is VS Code-only).

**⚠️ SEQUENTIAL ONLY**: try-fix runs modify the same files and use the same device. Never run in parallel.

---

## Platform Selection

**Choose a platform that is BOTH affected by the bug AND available on the current host.**

### Step 1: Identify affected platforms from Pre-Flight
- Check the "Platforms Affected" checkboxes in the state file
- Check issue labels (e.g., `platform/iOS`, `platform/Android`)
- Check which platform-specific files the PR modifies

### Step 2: Match to available platforms

| Host OS | Available Platforms |
|---------|---------------------|
| Windows | Android, Windows |
| macOS | Android, iOS, MacCatalyst |

### Step 3: Select the best match
1. Pick a platform that IS affected by the bug
2. That IS available on the current host
3. Prefer the platform most directly impacted by the PR's code changes

**⚠️ Do NOT test on a platform that isn't affected by the bug** - the test will pass regardless of whether the fix works.
