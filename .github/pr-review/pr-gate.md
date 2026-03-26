# PR Gate — Test Verification

> **⛔ This phase MUST pass before continuing to Try-Fix. If it fails, stop and inform user.**

> 🚨 Gate verification MUST run via task agent — never inline.

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

3. **Run verification via task agent** (MUST use task agent — never inline):
   ```
   Invoke the `task` agent with this prompt:

   "Invoke the verify-tests-fail-without-fix skill for this PR:
   - Platform: {platform}
   - RequireFullVerification: true

   Report back: Did tests FAIL without fix? Did tests PASS with fix? Final status?"
   ```

**Why task agent?** Running inline allows substituting commands and fabricating results. Task agent runs in isolation.

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

- ❌ Running inline — MUST use task agent
- ❌ Adding verbose explanations to gate/content.md — use the exact template above
- ❌ Copying gate results into try-fix/content.md or report/content.md — gate results belong ONLY in gate/content.md
- ❌ Skipping gate because tests are device tests, not UI tests — the skill supports all test types
