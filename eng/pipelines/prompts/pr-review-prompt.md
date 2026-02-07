Review PR #${PR_NUMBER}

Follow the 5-phase PR Agent workflow, but leverage the existing agent review state:

1. **Phase 1: Gate** - Run tests FIRST. If gate fails, STOP IMMEDIATELY. Do not proceed.
2. **Phase 2: Pre-Flight** - Import prior agent state instead of re-doing completed phases
3. **Phase 3: Tests** - Verify reproduction tests exist
4. **Phase 4: Fix** - EXHAUSTIVE exploration:
   - Consult 5+ different AI models for diverse fix ideas
   - Run try-fix skill with Opus 4.5 for EACH unique idea
   - Keep iterating until completely out of alternatives
   - Compare ALL candidates to determine best approach
5. **Phase 5: Report** - Generate final recommendation with full comparison

## Work Plan

### Phase 1: Gate (Test Verification) - MUST PASS FIRST ⛔
**THIS IS A BLOCKING GATE - If tests don't behave correctly, STOP ALL WORK IMMEDIATELY.**

- [ ] Run verification script with `-RequireFullVerification`:
  ```bash
  pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 -Platform android -RequireFullVerification
  ```
- [ ] Confirm tests FAIL without fix (bug reproduced)
- [ ] Confirm tests PASS with fix (bug fixed)
- [ ] **IF GATE FAILS**: Stop immediately, do not proceed to any other phase. Report failure and exit.
- [ ] Mark Gate ✅ PASSED (or ❌ FAILED and STOP)

### Phase 2: Pre-Flight (Context Gathering)
- [ ] Checkout PR branch (`pr-33687`)
- [ ] Gather PR metadata (title, body, labels, files)
- [ ] Read linked issue #19256
- [ ] Fetch PR comments and review feedback
- [ ] Check for prior agent review (FOUND: `.github/agent-pr-session/pr-19256.md`)
- [ ] Create local state file importing prior agent's findings
- [ ] Mark Pre-Flight COMPLETE

### Phase 3: Tests (Verify Reproduction Tests Exist)
- [ ] Confirm PR includes UI tests (already present per file list)
- [ ] Verify test file locations:
  - HostApp: `src/Controls/tests/TestCases.HostApp/Issues/Issue19256.cs`
  - NUnit: `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue19256.cs`
- [ ] Verify tests follow naming convention (`Issue19256`)
- [ ] Mark Tests COMPLETE

### Phase 4: Fix (EXHAUSTIVE Independent Analysis)

**Goal:** Explore ALL possible alternative solutions until no more ideas remain.

#### Step 4.1: Multi-Model Brainstorming (Consult 5+ Models)
- [ ] Read `.github/agents/pr/post-gate.md` for Phase 4-5 instructions
- [ ] Consult **Claude Sonnet 4** for fix ideas
- [ ] Consult **Claude Opus 4.5** for fix ideas
- [ ] Consult **GPT-5.2** for fix ideas
- [ ] Consult **GPT-5.1-Codex** for fix ideas
- [ ] Consult **Gemini 3 Pro** for fix ideas
- [ ] Consolidate unique approaches from all models
- [ ] Deduplicate and categorize fix strategies

#### Step 4.2: Iterative try-fix Exploration (Opus 4.5) - SEQUENTIAL

**CRITICAL: try-fix attempts MUST be run SEQUENTIALLY, one at a time.**
- Each try-fix must COMPLETE before starting the next
- NO parallel execution - wait for full result before proceeding
- Learn from each attempt to inform the next

For EACH unique fix idea, run try-fix skill with `claude-opus-4.5`:
- [ ] **Attempt 1:** [First alternative approach] → WAIT FOR COMPLETION
- [ ] **Attempt 2:** [Second alternative approach] → WAIT FOR COMPLETION
- [ ] **Attempt 3:** [Third alternative approach] → WAIT FOR COMPLETION
- [ ] **Attempt 4:** [Continue until exhausted...] → WAIT FOR COMPLETION
- [ ] **Attempt N:** Keep going until try-fix reports "no more ideas"

**Sequential Iteration Rule:**
1. Start try-fix with ONE idea
2. **WAIT** for try-fix to complete fully (tests run, result recorded)
3. If tests PASS → Record as viable alternative
4. If tests FAIL → Analyze failure reason
5. Use learnings from this attempt to inform the NEXT attempt
6. Start next try-fix (go to step 1)
7. Continue until ALL brainstormed ideas are tested
8. Then ask Opus 4.5 to generate MORE ideas based on all learnings so far
9. Repeat until Opus 4.5 confirms "exhausted all approaches"

#### Step 4.3: Comparative Analysis
- [ ] Create comparison matrix of ALL fix candidates (PR's + alternatives)
- [ ] Evaluate each on: correctness, simplicity, performance, maintainability
- [ ] Document why PR's fix is/isn't the best approach
- [ ] Select best fix with full justification

**Exit Criteria for Phase 4:**
- At least 5 models consulted for ideas
- All unique ideas tested via try-fix
- try-fix confirms "no more alternative approaches"
- Comparison matrix complete
- Best fix selected with rationale

### Phase 5: Report (Final Recommendation)
- [ ] Generate comprehensive review summary
- [ ] Provide final recommendation: APPROVE / REQUEST CHANGES
- [ ] Post review to PR if requested

## Key Files in This PR

| File | Type | Purpose |
|------|------|---------|
| `src/Core/src/Handlers/DatePicker/DatePickerHandler.Android.cs` | Fix | ShowEvent handler to re-apply min/max dates |
| `src/Core/src/Platform/Android/DatePickerExtensions.cs` | Fix | Reset MinDate/MaxDate before setting new values |
| `src/Controls/tests/TestCases.HostApp/Issues/Issue19256.cs` | Test | UI test page with dependent DatePickers |
| `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue19256.cs` | Test | NUnit test with screenshot verification |
| `.github/agent-pr-session/pr-19256.md` | Meta | Prior agent review session (all phases COMPLETE) |

## Prior Agent Review Summary

The existing agent review file shows:
- **Root Cause:** Known Android platform bug - DatePicker caches MinDate/MaxDate and ignores updates unless reset first
- **Fix Approach:** Reset values before setting + ShowEvent handler to re-apply after dialog initialization
- **Tests:** Screenshot-based verification with two states (FutureDate, EarlierDate)
- **Verdict:** ✅ VALID FIX - follows established 10+ year old Android workaround

## Notes

- This is an **Android-only** issue (platform/android label)
- The fix is based on a well-documented workaround from [StackOverflow #19616575](https://stackoverflow.com/questions/19616575)
- Issue has been open since Dec 2023 (regression in 8.0.3)
- PR includes +429 lines (mostly tests and documentation)

## Risks & Considerations

1. **Gate verification required** - Must empirically confirm tests behave correctly
2. **Phase 4 is exhaustive** - Will consult 5+ models and iterate try-fix until ALL ideas explored
3. **Android emulator required** - Tests need to run on Android platform
4. **Time investment** - Exhaustive Phase 4 may take significant time but ensures thorough review
5. **Model availability** - Need access to multiple models (Sonnet, Opus, GPT-5.x, Gemini)

## Models to Consult in Phase 4

| Model | Purpose |
|-------|---------|
| `claude-sonnet-4` | Baseline fix ideas |
| `claude-opus-4.5` | Deep analysis + try-fix iterations |
| `gpt-5.2` | Alternative perspective |
| `gpt-5.1-codex` | Code-focused suggestions |
| `gemini-3-pro-preview` | Third-party perspective |

## Success Criteria

Phase 4 is complete when:
- ✅ All 5+ models have been consulted
- ✅ Every unique fix idea has been tested via try-fix
- ✅ Opus 4.5 confirms "no more alternative approaches to explore"
- ✅ Comprehensive comparison matrix exists
- ✅ Clear rationale for final fix selection
