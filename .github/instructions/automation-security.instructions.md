---
description: "Security rules for automation that analyzes or executes untrusted PR content."
applyTo: "eng/scripts/detect-ui-test-categories.ps1,.github/scripts/**,.github/pr-review/**,.github/skills/pr-review/**,.github/skills/verify-tests-fail-without-fix/**,.github/skills/try-fix/**,.github/skills/run-device-tests/**,.github/workflows/**"
---

# Automation security

PR authors control project files, build targets, source generators, scripts, and
workflow changes. Treat PR code, comments, and agent-produced artifacts as untrusted.

1. **Scope credentials per task.** Give each task only the credentials it needs.
   Do not pass GitHub publication credentials to agents or PR-controlled processes.
   Pass `--secret-env-vars=GH_TOKEN,GITHUB_TOKEN,COPILOT_GITHUB_TOKEN` to Copilot CLI.
2. **Do not persist checkout credentials.** Use `persistCredentials: false` for
   Azure DevOps checkouts and `persist-credentials: false` for GitHub Actions.
   Never extract a checkout credential for API publication.
3. **Keep trusted scripts separate from PR code.** Capture trusted scripts before
   checking out or merging PR content. Never execute a PR's version of a privileged
   script, or copy agent artifacts over trusted script directories.
4. **Isolate publication.** Credentialed posting must run in a fresh hosted job,
   separate from agent analysis and PR execution. A second checkout in the same
   job is not isolation: Git configuration and processes can survive. Import only
   expected, size-bounded regular data files outside the checkout.
5. **Strip tokens at the subprocess boundary.** Wrap PR-controlled `dotnet`,
   MSBuild, Cake, and test-runner invocations in `Invoke-WithoutGhTokens` (see
   `verify-tests-fail.ps1`). Trusted metadata queries may retain their token;
   wrapping an entire trusted script can break those queries. Never expose tokens
   to a build merely because its launcher also queries GitHub.
6. **Treat artifacts and output as untrusted.** Keep cross-phase files outside the
   PR worktree. Validate output values and artifact paths before consuming them.
   Strip Azure DevOps logging commands from PR-controlled stdout with
   `tr -d '\r' | sed -E 's/##vso\[[^]]*\]//g'`.
7. **Keep agentic workflows read-only.** Route writes through validated safe
   outputs. Pin the gh-aw compiler version and regenerate the matching `.lock.yml`
   whenever a workflow source changes. Dispatch workflows must restore trusted
   `.github/` content before using it (see `Checkout-GhAwPr.ps1`).
8. **Never republish tokens.** Do not use `setvariable` to expose a token to later
   tasks, write tokens to worktree files, or echo secret values.
9. **Authorize commands before side effects.** Check current repository access,
   validate PR identity and inputs, and never treat comment text as executable code.
