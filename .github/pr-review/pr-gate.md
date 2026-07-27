# PR Gate - Test Before and After Fix

> **⛔ This phase MUST pass before continuing to Try-Fix. If it fails, stop and inform user.**

> In CI (Review-PR.ps1), the gate runs `verify-tests-fail.ps1` directly as a script step.
> For manual usage, you can invoke it yourself or via a task agent.

---

## Prerequisites

- Pre-Flight phase must be ✅ COMPLETE before starting
- Platform must be selected (affected by bug AND available on host)

### Platform Selection

Choose a platform that is BOTH affected by the bug AND available on the current host:

| Host OS | Available Platforms |
|---------|---------------------|
| Windows | Android, Windows |
| macOS | Android, iOS, MacCatalyst |

⚠️ Do NOT test on a platform unaffected by the bug — the test will pass regardless.

---

## Steps

1. **Detect tests in PR** using the shared detection script:
   ```bash
   pwsh .github/scripts/shared/Detect-TestsInDiff.ps1 -PRNumber XXXXX
   ```
   This auto-detects all test types: UI tests, device tests, unit tests, XAML tests.
   If NO tests detected → inform user, suggest `write-tests-agent`. Gate is ⚠️ SKIPPED.

2. **Select platform** — must be affected by bug AND available on host (see table above).

3. **Run verification** via `verify-tests-fail.ps1`:
   ```bash
   pwsh .github/skills/verify-tests-fail-without-fix/scripts/verify-tests-fail.ps1 \
     -Platform {platform} -RequireFullVerification
   ```
   In CI, `Review-PR.ps1` calls this script directly. For manual usage, you can also invoke
   it via a task agent for isolation:
   ```
   Invoke the `task` agent with this prompt:

   "Invoke the verify-tests-fail-without-fix skill for this PR:
   - Platform: {platform}
   - RequireFullVerification: true

   Report back: Did tests FAIL without fix? Did tests PASS with fix? Final status?"
   ```

---

## If Gate Fails

- **Tests PASS without fix** → Tests don't catch the bug. Inform user, suggest `write-tests-agent`.
- **Tests FAIL with fix** → PR's fix doesn't work. Skip Try-Fix, proceed to Report with ⚠️ REQUEST CHANGES.

---

## Output File

> 🚨 **CRITICAL OUTPUT RULES:**
> - Write gate results ONLY to `gate/content.md` — NEVER copy gate results into other phases (pre-flight, try-fix, report)
> - Use the EXACT template below — no extra explanations, no "Reason:" paragraphs, no "Notes:" sections
> - Keep it SHORT — the template is the complete output

```bash
mkdir -p CustomAgentLogsTmp/PRState/{PRNumber}/PRAgent/gate
```

Write `content.md` using this **exact** template (fill in values, don't add anything else):

```markdown
### Gate Result: {✅ PASSED / ❌ FAILED / ⚠️ SKIPPED}

**Platform:** {platform}

| # | Type | Test Name | Filter |
|---|------|-----------|--------|
| 1 | {type} | {name} | `{filter}` |

| Step | Expected | Actual | Result |
|------|----------|--------|--------|
| Without fix | FAIL | {FAIL/PASS} | {✅/❌} |
| With fix | PASS | {FAIL/PASS} | {✅/❌} |
```

If gate is SKIPPED (no tests found), write only:

```markdown
### Gate Result: ⚠️ SKIPPED

No tests detected in PR. Suggest adding tests via `write-tests-agent`.
```

---

## Common Mistakes

- ❌ Adding verbose explanations to gate/content.md — use the exact template above
- ❌ Copying gate results into try-fix/content.md or report/content.md — gate results belong ONLY in gate/content.md
- ❌ Skipping gate because tests are device tests, not UI tests — the skill supports all test types
