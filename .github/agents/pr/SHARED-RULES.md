# PR Agent: Shared Rules

This file contains critical rules that apply across all PR agent phases. Referenced by `pr.md`, `post-gate.md`, and `PLAN-TEMPLATE.md`.

---

## Phase Completion Protocol

**Before changing ANY phase status to ✅ COMPLETE:**

1. **Review the phase checklist** for the phase you're completing
2. **Verify all required items** are addressed
3. **Write the phase output to `content.md`** (see Phase Output Artifacts below)
4. **Then mark the phase** as ✅ COMPLETE

**Rule:** Status ✅ means "work complete and verified", not "I finished thinking about it"

---

## Phase Output Artifacts

**After completing EACH phase, write a `content.md` file to the phase's output directory.**

This is MANDATORY. The comment scripts (`post-ai-summary-comment.ps1`) read from these files to build the PR comment.

### Output Directory Structure

```
CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/
├── pre-flight/
│   └── content.md          # Written after Phase 1
├── gate/
│   ├── content.md          # Written after Phase 2
│   └── verify-tests-fail/  # Script output from verify-tests-fail.ps1
├── try-fix/
│   ├── content.md          # Written after Phase 3 (summary of all attempts)
│   └── attempt-{N}/        # Individual attempt outputs from try-fix skill
└── report/
    └── content.md          # Written after Phase 4
```

### What Goes in Each content.md

Each `content.md` should contain the **formatted phase content** — the same content that would appear inside the collapsible `<details>` section in the PR comment.

**Pre-Flight example:**
```markdown
**Issue:** #XXXXX - [Title]
**Platforms Affected:** iOS, Android
**Files Changed:** 2 implementation files, 1 test file

### Key Findings
- [Finding 1]
- [Finding 2]

### Fix Candidates
| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| PR | PR #XXXXX | [approach] | ⏳ PENDING (Gate) | `file.cs` | Original PR |
```

**Gate example:**
```markdown
**Result:** ✅ PASSED
**Platform:** android
**Mode:** Full Verification

- Tests FAIL without fix ✅
- Tests PASS with fix ✅
```

**Fix (try-fix) example:**
```markdown
### Fix Candidates
| # | Source | Approach | Test Result | Files Changed | Notes |
|---|--------|----------|-------------|---------------|-------|
| 1 | try-fix | [approach] | ❌ FAIL | 1 file | Why: [reason] |
| 2 | try-fix | [approach] | ✅ PASS | 2 files | Works! |
| PR | PR #XXXXX | [approach] | ✅ PASS (Gate) | 2 files | Original PR |

**Exhausted:** Yes
**Selected Fix:** PR's fix - [Reason]
```

**Report example:**
```markdown
## ✅ Final Recommendation: APPROVE

### Summary
[Brief summary of the review]

### Root Cause
[Root cause analysis]

### Fix Quality
[Assessment of the fix]
```

### How to Write the File

```bash
# Create the directory (idempotent)
mkdir -p "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pre-flight"

# Write content (use create tool or bash)
cat > "CustomAgentLogsTmp/PRState/$PRNumber/PRAgent/pre-flight/content.md" << 'EOF'
[phase content here]
EOF
```

### Timing

| Phase | When to Write |
|-------|---------------|
| Pre-Flight | After all context gathered and documented |
| Gate | After verification result received from task agent |
| Fix (try-fix) | After all try-fix models explored and best fix selected |
| Report | After final recommendation determined |

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

### 🚨 Autonomous Execution (Default)

When running via `Review-PR.ps1`, there is **NO human operator** to respond to questions.

**NEVER stop and ask the user. NEVER present options and wait for a choice. Nobody will respond.**

Instead, use your best judgment to continue autonomously:

1. **Try ONE retry** (install missing tool, kill conflicting process, etc.)
2. **If still blocked after one retry**, SKIP the blocked phase and continue to the next phase
3. **Document what was skipped and why** in your report
4. **Always prefer continuing with partial results** over stopping completely

**Autonomous decision guide:**

| Blocker Type | Max Retries | Then Do |
|--------------|-------------|---------|
| Missing tool/driver | 1 install attempt | Skip phase, continue |
| Server errors (500, timeout) | 1 retry | Skip phase, continue |
| Port conflicts | 1 (kill process) | Skip phase, continue |
| Build failures in try-fix | 2 attempts | Skip remaining try-fix models, proceed to Report |
| Configuration issues | 1 fix attempt | Skip phase, continue |

**Common autonomous decisions:**
- Gate passes but Fix phase is blocked → **Skip Fix, proceed to Report** with Gate results only
- try-fix builds fail for multiple models → **Stop try-fix exploration, proceed to Report**
- A specific platform fails → **Try alternative platform ONCE**, then skip if still blocked
- Gate fails due to environment → **Report as incomplete**, proceed to Report

### Interactive Mode

When running with `-Interactive` flag, you MAY ask the user for guidance on blockers.

### Common Blockers

- Missing Appium drivers (Windows, iOS, Android)
- WinAppDriver not installed or returning errors
- Xcode/iOS simulators not available (on Windows)
- Android emulator not running or not configured
- Developer Mode not enabled
- Port conflicts (e.g., 4723 in use)
- Missing SDKs or tools
- Server errors (500, timeout, "unknown error occurred")

### Never Do

- ❌ Keep trying different fixes after retry limit exceeded
- ❌ Claim "verification passed" when tests couldn't actually run
- ❌ Install multiple tools/drivers without asking between each
- ❌ Spend more than 2-3 tool calls troubleshooting the same blocker
- ❌ **Stop and present options to the user** - choose the best option yourself
- ❌ **Wait for user response** - nobody will respond

---

## Multi-Model Configuration

Phase 4 uses these 5 AI models for try-fix exploration (run SEQUENTIALLY):

| Order | Model |
|-------|-------|
| 1 | `claude-sonnet-4.5` |
| 2 | `claude-opus-4.6` |
| 3 | `gpt-5.2` |
| 4 | `gpt-5.2-codex` |
| 5 | `gemini-3-pro-preview` |

**Note:** The `model` parameter is passed to the `task` tool, which supports model selection. This is separate from agent YAML frontmatter (which is VS Code-only).

**⚠️ SEQUENTIAL ONLY**: try-fix runs modify the same files and use the same device. Never run in parallel.

---

## Platform Selection

**Choose a platform that is BOTH affected by the bug AND available on the current host.**

### Step 1: Identify affected platforms from Pre-Flight
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
