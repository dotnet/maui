# PR Review Plan Template

**Reusable checklist** for the 4-phase PR Agent workflow.

**Source documents:**
- `.github/agents/pr.md` - Phases 1-2 (Pre-Flight, Gate)
- `.github/agents/pr/post-gate.md` - Phases 3-4 (Fix, Report)
- `.github/agents/pr/SHARED-RULES.md` - Critical rules (blockers, git, templates)

---

## 🚨 Critical Rules (Summary)

See `SHARED-RULES.md` for complete details. Key points:
- **Environment Blockers**: STOP immediately, report, ask user (strict retry limits)
- **No Git Commands**: Never checkout/switch branches - agent is always on correct branch
- **Gate via Task Agent**: Never run inline (prevents fabrication)
- **Multi-Model try-fix**: 5 models, SEQUENTIAL only
- **Follow Templates**: No `open` attributes, no "improvements"

---

## Work Plan

### Phase 1: Pre-Flight
- [ ] Create state file: `CustomAgentLogsTmp/PRState/pr-XXXXX.md`
- [ ] Gather PR metadata (title, body, labels, author)
- [ ] Fetch and read linked issue
- [ ] Fetch PR comments and review feedback
- [ ] Check for prior agent reviews (import and resume if found)
- [ ] Document platforms affected
- [ ] Classify changed files (fix vs test)
- [ ] Document PR's fix approach in Fix Candidates table
- [ ] Update state file: Pre-Flight → ✅ COMPLETE
- [ ] Save state file

**Boundaries:** No code analysis, no fix opinions, no test running

### Phase 2: Gate ⛔
**🚨 Cannot continue if Gate fails**

- [ ] Check if tests exist (if not, let the user know and suggest using `write-tests-agent`)
- [ ] Select platform (must be affected AND available on host)
- [ ] Invoke via **task agent** (NOT inline):
  ```
  "Run verify-tests-fail-without-fix skill
   Platform: [X], TestFilter: 'IssueXXXXX', RequireFullVerification: true"
  ```
- [ ] ⛔ If environment blocker: STOP, report, ask user
- [ ] Verify: Tests FAIL without fix, PASS with fix
- [ ] If Gate fails: STOP, request test fixes
- [ ] Update state file: Gate → ✅ PASSED
- [ ] Save state file

### Phase 3: Fix 🔧
*(Only if Gate ✅ PASSED)*

**Round 1: Run try-fix with each model (SEQUENTIAL)**
- [ ] claude-sonnet-4.5
- [ ] claude-opus-4.5
- [ ] gpt-5.2
- [ ] gpt-5.2-codex
- [ ] gemini-3-pro-preview
- [ ] ⛔ If blocker: STOP, report, ask user
- [ ] Record: approach, result, files, failure analysis

**Round 2+: Cross-Pollination (MANDATORY)**
- [ ] Invoke EACH model: "Any NEW fix ideas?"
- [ ] Record responses in Cross-Pollination table
- [ ] Run try-fix for new ideas (SEQUENTIAL)
- [ ] Repeat until ALL 5 say "NO NEW IDEAS" (max 3 rounds)

**Completion:**
- [ ] Cross-Pollination table has all 5 responses
- [ ] Mark Exhausted: Yes
- [ ] Compare passing candidates with PR's fix
- [ ] Select best fix (results → simplicity → robustness)
- [ ] Update state file: Fix → ✅ COMPLETE
- [ ] Save state file

### Phase 4: Report 📋
*(Only if Phases 1-3 complete)*

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

| Phase | Key Action | Blocker Response |
|-------|------------|------------------|
| Pre-Flight | Create state file | N/A |
| Gate | Task agent → verify script | ⛔ STOP, report, ask |
| Fix | Multi-model try-fix | ⛔ STOP, report, ask |
| Report | Post via skill | ⛔ STOP, report, ask |

**State file:** `CustomAgentLogsTmp/PRState/pr-XXXXX.md`

**Never:** Mark BLOCKED and continue, claim success without tests, bypass scripts
