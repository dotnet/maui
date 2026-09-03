---
description: "Security rules for the Copilot PR-review pipeline. Read before editing."
applyTo: "eng/pipelines/ci-copilot.yml,eng/scripts/detect-ui-test-categories.ps1,.github/scripts/**,.github/pr-review/**,.github/skills/pr-review/**,.github/skills/verify-tests-fail-without-fix/**,.github/skills/try-fix/**,.github/skills/run-device-tests/**,.github/workflows/review-trigger.yml,.github/workflows/review-trigger-recovery.yml,.github/workflows/pr-review-queue.yml,.github/workflows/copilot-evaluate-tests.*"
---

# CI Copilot pipeline — security rules

This pipeline runs **untrusted PR code** on AzDO agents with these tokens in scope:

- `GH_COMMENT_TOKEN` / `GH_TOKEN` — `maui-bot` PAT (post comments, labels, reviews on any PR)
- `COPILOT_GITHUB_TOKEN` — Copilot CLI install token
- AzDO GitHub service-connection PAT — repo contents, PRs, checks, workflows

Once the PR is merged into the worktree, the author controls every `.csproj`, `Directory.Build.targets`, source generator, analyzer, test, `.ps1`, and `.yml` the pipeline subsequently runs.

## Rules

1. **Per-task `env:` scoping.** Only put tokens in tasks that need them. The Copilot-agent task gets `COPILOT_GITHUB_TOKEN` only — never `GH_TOKEN`. Posting tasks run in separate Microsoft-hosted jobs and receive `GH_COMMENT_TOKEN` plus only the explicitly scoped publication token they need. Pass `--secret-env-vars=GH_TOKEN,GITHUB_TOKEN,COPILOT_GITHUB_TOKEN` to the Copilot CLI.

2. **`persistCredentials: false` on every `checkout: self`.** Default checkout writes the service-connection PAT into `.git/config` as `extraheader`, readable by any subprocess. Do not extract or repurpose that checkout credential for API publication; use an explicitly scoped task secret instead.

3. **Trusted-copy scripts before merging the PR.** Setup copies `.github/scripts`, `.github/skills`, and `eng/scripts` to `$(Build.ArtifactStagingDirectory)/trusted-github/` before switching branches or merging the PR. Gate and CopilotReview invoke scripts through `$ScriptsDir`, `$SkillsDir`, and `$EngScriptsDir`, never from the merged worktree. New scripts used by those phases must be added to the Setup copy block.

4. **Run posting from a fresh Microsoft-hosted job.** Post jobs check out `$(Build.SourceVersion)` with `clean: true` and `persistCredentials: false`, then execute `.github/scripts`, `.github/skills`, and `eng/scripts` from that checkout. Prompt-influenced analysis must finish in a different job/agent. Download its result outside the checkout and import only the expected bounded regular file with `Copy-BoundedDiagnosticFile`; never copy artifact content over script directories. A second `clean: true` checkout in the same job is not isolation because `.git` and the agent's global config survive.

5. **Strip tokens before invoking PR-controlled code.** Wrap every `dotnet build|test|run|pack`, `msbuild`, `dotnet cake`, `BuildAndRun*.ps1`, `Run-DeviceTests.ps1`, `Invoke-UITestWithRetry.ps1` in `Invoke-WithoutGhTokens { ... }` (defined in `Review-PR.ps1` and `verify-tests-fail.ps1` — saves/clears/restores `GH_TOKEN`, `GITHUB_TOKEN`, `COPILOT_GITHUB_TOKEN`). **Wrap as close to the subprocess as possible, not at the outer trusted-script boundary** — a trusted script may itself need `gh` for metadata (e.g., `verify-tests-fail.ps1` calls `Detect-TestsInDiff.ps1` which uses `gh api`), so wrapping the whole script breaks its detection path. Wrap only the line that launches the PR-controlled process. Exception: scripts that ONLY call `gh` for PR metadata (`Detect-TestsInDiff.ps1`, `Find-RegressionRisks.ps1`, `detect-ui-test-categories.ps1`) don't need wrapping at all — they keep the token.

6. **Cross-phase and cross-job results.** Same-job phase files belong in `$(Agent.TempDirectory)` or the trusted staging directory, never the merged worktree. Cross-job values use named output variables with a fixed set of expected values or pipeline artifacts. Download artifacts outside the checkout, copy only the required data directory, and never transfer scripts for Post to run.

7. **Strip `##vso[...]` from PR-controlled stdout.** Pipe through `tr -d '\r' | sed -E 's/##vso\[[^]]*\]//g'` — bare `sed` misses CRLF lines and the agent will execute the directive.

8. **`gh-aw` workflows.** Pin compiler version (≥ v0.68.4 strips `pull-requests: write` per `gh-aw#28767`). Regenerate `.lock.yml` with `gh aw compile` in the **same commit** as any `.md` frontmatter edit (stale lock ⇒ all dispatches fail). `workflow_dispatch` triggers must restore trusted `.github/` from main (see `Checkout-GhAwPr.ps1`).

9. **No token republish.** Don't `setvariable` a token (visible to every later task, even with `issecret=true`). Don't write tokens to worktree files. Don't echo token names.

10. **Missed-command recovery stays trusted and deterministic.** The scheduled recovery workflow must execute only from the default branch, never check out PR code, revalidate the commenter's current write access, and dispatch the existing trusted review workflow rather than calling AzDO directly. Its minimum command age must exceed the combined timeout of the normal trigger jobs so polling cannot race an in-progress webhook delivery into a duplicate review.

## Review checklist

- [ ] Every checkout has `persistCredentials: false`; no task extracts the checkout `extraheader`.
- [ ] Prompt-influenced analysis and credentialed posting run in separate Microsoft-hosted jobs.
- [ ] The posting job imports only bounded regular data files from prompt-controlled artifacts and runs no Copilot/PR-controlled process.
- [ ] New `env:` block lists only the tokens that task needs; Copilot task has no `GH_TOKEN`.
- [ ] New Gate or CopilotReview script is invoked through `$ScriptsDir` / `$SkillsDir` / `$EngScriptsDir` and is included in the Setup copy block.
- [ ] Post runs in its own Microsoft-hosted job from a clean checkout of the pipeline revision.
- [ ] Post executes scripts from the checkout and copies only expected result data from pipeline artifacts.
- [ ] Artifact content cannot overwrite `.github/scripts`, `.github/skills`, or `eng/scripts`.
- [ ] Gate labels use the fixed `RunGate.gateResult` output.
- [ ] New invocation of PR-controlled code (`dotnet test|build|run`, `BuildAndRun*`, `Run-DeviceTests`, `Invoke-UITestWithRetry`) is wrapped in `Invoke-WithoutGhTokens` AT THE CALL SITE (not at an outer boundary).
- [ ] New same-job phase state uses `$(Agent.TempDirectory)` / trusted staging; new cross-job state uses an output variable or pipeline artifact.
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
