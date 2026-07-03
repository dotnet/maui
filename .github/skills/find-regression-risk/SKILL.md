# find-regression-risk

Detects potential regression risks in a PR by cross-referencing removed lines against lines added by recent labeled bug-fix PRs.

## How It Works

Purely mechanical — no AI/LLM. Five-step algorithm:

1. **PR diff** — collects lines REMOVED by the PR under review.
2. **Git history** — `git log --follow --since=6mo` finds recent PRs that touched the same files.
3. **Label filter** — keeps PRs (or their linked issues) labeled `i/regression`, `t/bug`, `p/0`, or `p/1`.
4. **Fix diff** — fetches each fix PR's diff and collects lines it ADDED to the same file.
5. **Compare** — whitespace-insensitive string equality:
   - 🔴 **REVERT** — removed line matches a line a fix PR added (highest risk).
   - 🟡 **OVERLAP** — same file modified, but no exact line revert.
   - 🟢 **CLEAN** — no bug-fix PRs touch the same files.

## Standalone Invocation

```powershell
# Analyze a specific PR (auto-detects files)
pwsh -NoProfile -Command '& ./.github/scripts/Find-RegressionRisks.ps1 -PRNumber 33908 -OutputDir /tmp/out'

# Analyze specific files only
pwsh -NoProfile -Command '& ./.github/scripts/Find-RegressionRisks.ps1 -PRNumber 33908 -OutputDir /tmp/out -FilePaths @("src/Core/src/Platform/Android/MauiWindowInsetListener.cs")'
```

## Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `-PRNumber` | Yes | — | PR number to analyze |
| `-Repo` | No | `dotnet/maui` | Repository in `owner/name` form |
| `-FilePaths` | No | auto-detect | Implementation files to check |
| `-MonthsBack` | No | `6` | History window for git log |
| `-MaxRecentPRsPerFile` | No | `20` | Rate-limit guard per file |
| `-BaseBranch` | No | `main` | Base branch for `git log` scope |
| `-OutputDir` | No | — | Directory for output files |
| `-WriteInlineFindings` | No | off | Emit `inline-findings.json` |

## Outputs

When `-OutputDir` is specified:

- **`result.txt`** — single token: `CLEAN`, `OVERLAP`, or `REVERT`
- **`risks.json`** — structured findings for downstream agents
- **`content.md`** — markdown summary for the PR comment
- **`inline-findings.json`** — (only with `-WriteInlineFindings`) inline annotations

## Integration

The script runs as **STEP 4** in `Review-PR.ps1` (Regression Cross-Reference, after UI test detection and before the Gate step). Its `content.md` is assembled into the AI summary review by `post-ai-summary-comment.ps1`.

When REVERT risks are detected, the regression tests from the reverted fix PRs are executed:
- **UI tests** → `BuildAndRunHostApp.ps1 -Platform <plat> -TestFilter <filter>`
- **Device tests** → `Run-DeviceTests.ps1 -Project <proj> -Platform <plat> -TestFilter <filter>`
- **Unit/XAML tests** → `dotnet test <project> --filter <filter>`

The expert reviewer agent (`maui-expert-reviewer.md`, dimension #6) reads `risks.json` to check for REVERT entries.

## Known Limitations

- **Inline findings**: The `-WriteInlineFindings` flag emits deletion-side (LEFT) annotations, but `post-inline-review.ps1` currently only posts RIGHT-side comments. LEFT-side findings are silently dropped. This is documented as future work.
- **Whitespace-only changes**: By design, an indent-only change to a fix line won't trigger a REVERT (the normalization collapses whitespace). This avoids false positives from reformatting.
- **`pwsh -File` array parameters**: When invoking standalone from bash, use `pwsh -Command '& ./script.ps1 -FilePaths @(...)'` syntax. `pwsh -File` doesn't evaluate `@()` expressions.

## Tests

```powershell
pwsh -NoProfile -File .github/scripts/tests/Test-FindRegressionRisks.ps1
```
