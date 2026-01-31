# PR Review Plan Template

## Overview

This is a **reusable template** for reviewing PRs using the 5-phase PR Agent workflow. Copy this plan and fill in the PR-specific details.

**Source documents:**
- `.github/agents/pr.md` - Phases 1-3 (Pre-Flight, Tests, Gate)
- `.github/agents/pr/post-gate.md` - Phases 4-5 (Fix, Report)

---

## 🚨 Critical Rules

### Rule 1: Stop on Environment Blockers
If ANY phase cannot complete due to missing tools/drivers/devices:
1. **STOP immediately** - Do NOT continue to next phase
2. **Do NOT keep troubleshooting** - Strict retry limits apply
3. **Report the blocker** - What failed, what's missing, error message
4. **Ask user** for resolution options
5. **Wait for response** - Do not assume or work around

**Retry Limits (STRICT):**
| Blocker Type | Max Retries | Then Do |
|--------------|-------------|---------|
| Missing tool/driver | 1 install attempt | STOP and ask |
| Server errors (500, timeout) | 0 retries | STOP immediately |
| Port conflicts | 1 (kill process) | STOP and ask |
| Configuration issues | 1 fix attempt | STOP and ask |

**Common blockers:** Appium drivers, WinAppDriver errors, Xcode, emulators, Developer Mode, port conflicts, SDKs

**Blocker Report Template:**
```
⛔ BLOCKED: Cannot complete [Phase Name]

**What failed:** [Step/skill that failed]
**Blocker:** [Tool/driver/error type]
**Error:** "[Exact error message]"

**What I tried:** [List retry attempts, max 1-2]

**I am STOPPING here. Options:**
1. [Option for user]
2. [Alternative platform]
3. [Skip with documented limitation]

Which would you like me to do?
```

### Rule 2: Gate via Task Agent Only
Gate verification MUST run as a `task` agent invocation, NOT inline commands.
- The script does TWO test runs (revert fix → test, restore fix → test)
- Running inline allows fabrication of results

### Rule 3: Multi-Model try-fix (Phase 4)
Run try-fix with 5 different models SEQUENTIALLY:
1. `claude-sonnet-4.5`
2. `claude-opus-4.5`
3. `gpt-5.2`
4. `gpt-5.2-codex`
5. `gemini-3-pro-preview`

Then cross-pollinate: Share ALL results with ALL models, ask for NEW ideas, repeat until exhaustion.

### Rule 4: Follow Templates Exactly
- Do NOT add attributes like `open` to `<details>` tags
- Do NOT "improve" template formats
- Downstream scripts depend on exact regex patterns

### Rule 5: Use Skills' Scripts
- Run the provided PowerShell scripts, don't bypass with manual commands
- If script fails, fix inputs rather than using manual `gh` commands

---

## Work Plan

### Phase 1: Pre-Flight - Context Gathering
- [ ] Create state file: `CustomAgentLogsTmp/PRState/pr-XXXXX.md`
- [ ] Gather PR metadata (title, body, labels, author)
- [ ] Fetch and read linked issue
- [ ] Fetch PR comments and review feedback
- [ ] Check for prior agent reviews (if found, import and resume)
- [ ] Document platforms affected
- [ ] Identify changed files and classify (fix vs test)
- [ ] Document PR's fix approach in Fix Candidates table
- [ ] Update state file: Pre-Flight → ✅ COMPLETE
- [ ] Commit state file

**Boundaries:** No code analysis, no opinions on fix, no test running

### Phase 2: Tests - Verify Test Existence
- [ ] Check if PR includes UI tests
- [ ] Verify tests follow naming conventions (`IssueXXXXX` pattern)
- [ ] If tests exist: Verify they compile
- [ ] If tests missing: Invoke `write-ui-tests` skill
- [ ] Document test files in state file
- [ ] Update state file: Tests → ✅ COMPLETE
- [ ] Commit state file

### Phase 3: Gate - Verify Tests Catch Bug ⛔
**🚨 BLOCKER PHASE - Cannot continue if Gate fails**

- [ ] Invoke verification via **task agent** (NOT inline):
  ```
  Invoke task agent with: "Run verify-tests-fail-without-fix skill
  - Platform: [android/ios/windows]
  - TestFilter: 'IssueXXXXX'
  - RequireFullVerification: true
  Report: Did tests FAIL without fix? PASS with fix? Final status?"
  ```
- [ ] ⛔ **If environment blocker**: STOP, report, ask user
- [ ] Verify output:
  - ✅ Tests FAIL without fix (bug reproduced)
  - ✅ Tests PASS with fix (fix works)
- [ ] If Gate fails: STOP, request test fixes from author
- [ ] Update state file: Gate → ✅ PASSED
- [ ] Commit state file

### Phase 4: Fix - Multi-Model Exploration 🔧
*(Only proceed if Gate ✅ PASSED)*

**Round 1: Run try-fix with each model (SEQUENTIAL)**
- [ ] Attempt 1: `claude-sonnet-4.5`
- [ ] Attempt 2: `claude-opus-4.5`
- [ ] Attempt 3: `gpt-5.2`
- [ ] Attempt 4: `gpt-5.2-codex`
- [ ] Attempt 5: `gemini-3-pro-preview`
- [ ] ⛔ **If environment blocker on any attempt**: STOP, report, ask user
- [ ] Record each: approach, test result, files changed, failure analysis

**Round 2+: Cross-Pollination Loop**
- [ ] Share bounded summary of ALL attempts with ALL 5 models
- [ ] Ask each: "Any NEW fix ideas not yet tried?"
- [ ] For each new idea: Run try-fix with that model
- [ ] Repeat until NO model proposes new ideas (max 3 rounds)

**Completion:**
- [ ] Mark Exhausted: Yes (all models confirm no new ideas)
- [ ] Compare passing candidates with PR's fix
- [ ] Select best fix (test results → simplicity → robustness → style)
- [ ] Update state file: Fix → ✅ COMPLETE
- [ ] Commit state file

### Phase 5: Report - Final Recommendation 📋
*(Only proceed if Phases 1-4 complete)*

- [ ] Run `pr-finalize` skill to verify title/description accuracy
- [ ] Generate comprehensive review:
  - Root cause analysis
  - All fix candidates with results
  - Final recommendation (APPROVE / REQUEST CHANGES)
  - Reasoning
- [ ] Post review via `ai-summary-comment` skill (NOT manual gh command)
- [ ] Update state file: Report → ✅ COMPLETE
- [ ] Commit final state file

---

## Success Criteria

✅ **APPROVE if:**
- Gate passed (tests catch bug)
- PR's fix works and is equal/better than alternatives
- Tests are comprehensive
- Code changes are minimal and correct

⚠️ **REQUEST CHANGES if:**
- Gate failed (tests don't catch bug)
- Alternative fix is significantly better
- Tests are missing or inadequate
- Code has issues

---

## Environment Blocker Report Template

When blocked, use this format:

```
⛔ BLOCKED: Cannot complete [Phase] phase

**What failed:** [Step description]
**Missing:** [Tool/driver/device]
**Error:** "[Error message]"

**Options:**
1. Install [component]: `[command]`
2. Switch to [alternative platform]
3. Skip with documented limitation

Which would you like me to do?
```

---

## Quick Reference

| Phase | Key Action | Blocker Response |
|-------|------------|------------------|
| Pre-Flight | Create state file, gather context | N/A (no env needed) |
| Tests | Verify/create tests, compile check | N/A (no device needed) |
| Gate | Task agent → verify script | ⛔ STOP, report, ask |
| Fix | Multi-model try-fix (5+ attempts) | ⛔ STOP, report, ask |
| Report | Post via ai-summary-comment skill | ⛔ STOP, report, ask |

**State file:** `CustomAgentLogsTmp/PRState/pr-XXXXX.md`
**Never:** Mark BLOCKED and continue, claim success without running tests, bypass scripts
