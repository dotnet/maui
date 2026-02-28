# PR Review Plan Template

**Reusable checklist** for the 4-phase PR Agent workflow.

**Source documents:**
- `.github/agents/pr.md` - Phases 1-2 (Pre-Flight, Gate)
- `.github/agents/pr/post-gate.md` - Phases 3-4 (Fix, Report)
- `.github/agents/pr/SHARED-RULES.md` - Critical rules (blockers, git, templates)
- `.github/instructions/test-only-prs.instructions.md` - Test-only PR handling

---

## 🚨 Critical Rules (Summary)

See `SHARED-RULES.md` for complete details. Key points:
- **Environment Blockers**: STOP immediately, report, ask user (strict retry limits)
- **No Git Commands**: Never checkout/switch branches - agent is always on correct branch
- **Gate via Task Agent for issue-fix PRs**: Never run inline (prevents fabrication); test-only PRs may run direct test commands
- **Multi-Model try-fix**: 5 models, SEQUENTIAL only
- **Follow Templates**: No `open` attributes, no "improvements"

---

## Work Plan

### Phase 0: Detect PR Type ⚠️

**CRITICAL: Check if this is a test-only PR before proceeding**

- [ ] **Step 1 - Check intent signals (hints only):**
  - Title contains `[Testing]` tag, OR
  - PR has `area-testing` label, OR
  - Description explicitly states it's for test coverage only

- [ ] **Step 2 - Confirm via diff classification (REQUIRED):**
  - Review changed files in the PR
  - Verify **ALL** changed files are test files (see patterns in `.github/instructions/test-only-prs.instructions.md`)
  - **If ANY non-test file is changed** → treat as issue-fix PR, regardless of title/labels

**If test-only PR confirmed (ZERO non-test files changed):**
- [ ] ✅ Continue with Phase 1 (Pre-Flight) - understand what tests do
- [ ] ✅ Continue with Phase 2 (Gate) - **run tests to verify they PASS**
- [ ] ❌ **SKIP Phase 3 (Fix)** - no bug to fix, no alternatives to explore
- [ ] ✅ **Simplified Phase 4 (Report)** - pr-finalize + code review only (no try-fix comparison)
- [ ] Document in state file: `PR Type: Test-Only - Phase 3 skipped, Phase 4 simplified`

**If issue-fix PR (has non-test file changes):** Continue with all phases normally

**Mixed PRs:** If PR has functional code changes AND tests, treat as issue-fix PR (run full workflow)

---

### Phase 1: Pre-Flight
- [ ] Create state file: `CustomAgentLogsTmp/PRState/pr-XXXXX.md`
- [ ] Gather PR metadata (title, body, labels, author)
- [ ] **Detect PR type** (test-only vs issue-fix) - record in state file
- [ ] Fetch and read linked issue
- [ ] Fetch PR comments and review feedback
- [ ] Check for prior agent reviews (import and resume if found)
- [ ] Document platforms affected
- [ ] Classify changed files (fix vs test)
- [ ] Document PR's fix approach in Fix Candidates table (skip if test-only)
- [ ] Update state file: Pre-Flight → ✅ COMPLETE
- [ ] Save state file

**Boundaries:** No code analysis, no fix opinions, no test running

### Phase 2: Gate ⛔
**🚨 Cannot continue if Gate fails**

**For issue-fix PRs:** Run full verification (tests fail without fix, pass with fix)
**For test-only PRs:** Run tests directly to verify they PASS

- [ ] Check if tests exist (if not, let the user know and suggest using `write-tests-agent`)
- [ ] Select platform (must be affected AND available on host)
- [ ] **If issue-fix PR:** Invoke via **task agent** (NOT inline):
  ```
  "Run verify-tests-fail-without-fix skill
   Platform: [X], TestFilter: 'IssueXXXXX', RequireFullVerification: true"
  ```
- [ ] **If test-only PR:** Select and run the correct test command based on the routing table in `.github/instructions/test-only-prs.instructions.md`:
  ```bash
  # Examples (choose based on test type from routing table):
  # UI Tests: pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"
  # Unit Tests: dotnet test src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj
  # XAML Tests: dotnet test src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj
  # Integration/Device: Use appropriate skill
  ```
- [ ] ⛔ If environment blocker: STOP, report, ask user
- [ ] **Issue-fix PR:** Verify tests FAIL without fix, PASS with fix
- [ ] **Test-only PR:** Verify tests PASS (see `.github/instructions/test-only-prs.instructions.md` for success criteria)
- [ ] If Gate fails: STOP, request test fixes
- [ ] Update state file: Gate → ✅ PASSED
- [ ] Save state file

### Phase 3: Fix 🔧
*(Only if Gate ✅ PASSED)*
**⏭️ SKIP this phase if PR is test-only** (no bug to fix)

**Round 1: Run try-fix with each model (SEQUENTIAL)**
- [ ] claude-sonnet-4.6
- [ ] claude-opus-4.6
- [ ] gpt-5.2
- [ ] gpt-5.3-codex
- [ ] gemini-3-pro-preview
- [ ] ⛔ If blocker: STOP, report, ask user
- [ ] Record: approach, result, files, failure analysis

**Round 2+: Cross-Pollination (MANDATORY)**
- [ ] Invoke EACH model: "Any NEW fix ideas?"
- [ ] Record responses in Cross-Pollination table
- [ ] Run try-fix for new ideas (SEQUENTIAL)
- [ ] Repeat until ALL 6 say "NO NEW IDEAS" (max 3 rounds)

**Completion:**
- [ ] Cross-Pollination table has all 6 responses
- [ ] Mark Exhausted: Yes
- [ ] Compare passing candidates with PR's fix
- [ ] Select best fix (results → simplicity → robustness)
- [ ] Update state file: Fix → ✅ COMPLETE
- [ ] Save state file

### Phase 4: Report 📋
*(Only if Phases 1-3 complete)*

**For test-only PRs:** Simplified workflow (skip try-fix comparison)
**For issue-fix PRs:** Full report with try-fix comparison

**Test-only PR workflow:**
- [ ] Run `pr-finalize` skill
- [ ] Perform code review (test quality, platform guards, AutomationIds)
- [ ] Post pr-finalize comment
- [ ] Update state file: Report → ✅ COMPLETE

**Issue-fix PR workflow:**

- [ ] Run `pr-finalize` skill
- [ ] Generate review: root cause, candidates, recommendation
- [ ] Post AI Summary comment (PR phases + try-fix):
  ```bash
  pwsh .github/skills/ai-summary-comment/scripts/post-ai-summary-comment.ps1 -PRNumber XXXXX -SkipValidation
  pwsh .github/skills/ai-summary-comment/scripts/post-try-fix-comment.ps1 -IssueNumber XXXXX
  ```
- [ ] Post PR Finalization comment (separate):
  ```bash
  pwsh .github/skills/ai-summary-comment/scripts/post-pr-finalize-comment.ps1 -PRNumber XXXXX -SummaryFile CustomAgentLogsTmp/PRState/pr-XXXXX.md
  ```
- [ ] Update state file: Report → ✅ COMPLETE
- [ ] Save final state file

---

## Quick Reference

| Phase | Key Action | Blocker Response | Test-Only PR? |
|-------|------------|------------------|---------------|
| Pre-Flight | Create state file, detect PR type | N/A | ✅ Run |
| Gate | Task agent → verify script (issue-fix) OR direct test run (test-only) | ⛔ STOP, report, ask | ✅ Run (simplified) |
| Fix | Multi-model try-fix (5 models) | ⛔ STOP, report, ask | ❌ Skip |
| Report | Post via skill | ⛔ STOP, report, ask | ✅ Run (simplified - pr-finalize only) |

**State file:** `CustomAgentLogsTmp/PRState/pr-XXXXX.md`

**Never:** Mark BLOCKED and continue, claim success without tests, bypass scripts

**Test-Only PRs:** Run Pre-Flight + Gate (test verification) + pr-finalize (skip Fix phase, no try-fix comparison)
