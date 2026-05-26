---
description: "Security rules for the Copilot PR-review pipeline. Read before editing."
applyTo: "eng/pipelines/ci-copilot.yml, .github/scripts/**, .github/skills/pr-review/**, .github/skills/verify-tests-fail-without-fix/**, .github/skills/try-fix/**, .github/skills/run-device-tests/scripts/**, .github/pr-review/**, .github/workflows/review-trigger.yml, .github/workflows/pr-review-queue.yml, .github/workflows/copilot-evaluate-tests.md, .github/workflows/copilot-evaluate-tests.lock.yml, eng/scripts/detect-ui-test-categories.ps1"
---

# CI Copilot pipeline — security rules

This pipeline runs **untrusted PR code** on AzDO agents with these tokens in scope:

- `GH_COMMENT_TOKEN` / `GH_TOKEN` — `maui-bot` PAT (post comments, labels, reviews on any PR)
- `COPILOT_GITHUB_TOKEN` — Copilot CLI install token
- AzDO GitHub service-connection PAT — repo contents, PRs, checks, workflows

Exfil of any of these = account/repo takeover. Once the PR is merged into the worktree, the author controls every `.csproj`, `Directory.Build.targets`, source generator, analyzer, test, `.ps1`, and `.yml` the pipeline subsequently runs.

## Rules

1. **Per-task `env:` scoping.** Only put tokens a task needs. The Copilot-agent task gets `COPILOT_GITHUB_TOKEN` only — never `GH_TOKEN`. Pass `--secret-env-vars=GH_TOKEN,GITHUB_TOKEN,COPILOT_GITHUB_TOKEN` to the Copilot CLI.

2. **`persistCredentials: false` on every `checkout: self`** unless the task pushes. Default checkout writes the service-connection PAT into `.git/config` as `extraheader`, readable by any subprocess.

3. **Trusted-copy scripts before merging the PR.** Setup task (still on `main`) copies `.github/scripts`, `.github/skills`, `eng/scripts` to `$(Build.ArtifactStagingDirectory)/trusted-github/`, then `chmod -R a-w`. Later tasks invoke scripts from `$TRUSTED/...`, never from the merged worktree. In PowerShell use `$ScriptsDir` / `$SkillsDir` / `$EngScriptsDir` (canonical impl in `Review-PR.ps1`). New post-merge scripts must be added to the Setup copy block.

4. **Strip tokens before invoking PR-controlled code.** Wrap every `dotnet build|test|run|pack`, `msbuild`, `dotnet cake`, `BuildAndRun*.ps1`, `Run-DeviceTests.ps1`, `verify-tests-fail.ps1`, `Invoke-UITestWithRetry.ps1` in `Invoke-WithoutGhTokens { ... }` (defined in `Review-PR.ps1` — saves/clears/restores `GH_TOKEN`, `GITHUB_TOKEN`, `COPILOT_GITHUB_TOKEN`). Exception: trusted scripts that only call `gh` for PR metadata (`Detect-TestsInDiff.ps1`, `Find-RegressionRisks.ps1`, `detect-ui-test-categories.ps1`) keep the token.

5. **Cross-phase signal files in `$(Agent.TempDirectory)`** (or `$TRUSTED`), never `$RepoRoot/...`. PR code can overwrite anything in the worktree, including a gate verdict. Readers must not silently fall back to a worktree path if the trusted one is missing.

6. **Strip `##vso[...]` from PR-controlled stdout.** Pipe through `tr -d '\r' | sed -E 's/##vso\[[^]]*\]//g'` — bare `sed` misses CRLF lines and the agent will execute the directive.

7. **`gh-aw` workflows.** Pin compiler version (≥ v0.68.4 strips `pull-requests: write` per `gh-aw#28767`). Regenerate `.lock.yml` with `gh aw compile` in the **same commit** as any `.md` frontmatter edit (stale lock ⇒ all dispatches fail). `workflow_dispatch` triggers must restore trusted `.github/` from main (see `Checkout-GhAwPr.ps1`).

8. **No token republish.** Don't `setvariable` a token (visible to every later task, even with `issecret=true`). Don't write tokens to worktree files. Don't echo token names.

## Review checklist

- [ ] New `checkout: self` has `persistCredentials: false`.
- [ ] New `env:` block lists only the tokens that task needs; Copilot task has no `GH_TOKEN`.
- [ ] New post-merge script invoked via `$ScriptsDir` / `$SkillsDir` / `$EngScriptsDir`, not `$RepoRoot/...`, AND added to Setup copy block.
- [ ] New invocation of PR-controlled code (`dotnet test|build|run`, `BuildAndRun*`, `Run-DeviceTests`, `verify-tests-fail`, `Invoke-UITestWithRetry`) is wrapped in `Invoke-WithoutGhTokens`.
- [ ] New cross-phase state file lives under `$(Agent.TempDirectory)` / `$TRUSTED`.
- [ ] New PR-stdout pipe uses `tr -d '\r' | sed -E 's/##vso\[[^]]*\]//g'`.
- [ ] Edited `.github/workflows/*.md` has matching `.lock.yml` regenerated in same commit.

## Grep these during review

```bash
git grep -nE 'dotnet (test|build|run|pack)' eng/pipelines/ci-copilot.yml .github/scripts .github/skills | grep -v Invoke-WithoutGhTokens
git grep -nE 'Join-Path \$RepoRoot ".*\.(ps1|sh)"' .github/scripts .github/skills
git grep -nA1 'checkout: self' eng/pipelines/ci-copilot.yml | grep -v persistCredentials
git grep -nE 'Set-Content.*\$RepoRoot.*(gate-result|sentinel|verdict)' .github/scripts .github/skills
git grep -nE 'sed.*##vso' eng/pipelines/ci-copilot.yml | grep -v 'tr -d'
```
