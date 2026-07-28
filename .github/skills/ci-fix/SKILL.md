---
name: ci-fix
description: Common safety and triage protocol for .NET MAUI CI-fixer workflows. Use whenever investigating, creating, or advancing a `[ci-fix]` or `[ci-fix-net11]` pull request from a `ci-scan` issue, including manual single-issue dispatches and scheduled sweeps.
---

# CI-fix triage protocol

Use this protocol before proposing a code change or emitting a CI-fixer safe
output. The calling workflow supplies its branch, labels, title prefix, attempt
limit, and allowed write surface; preserve those caller-specific restrictions.

## Return a decision

For each candidate, state one terminal decision before taking an action:

- `Decision: Continue` — no dedup or safety gate blocks current-evidence analysis.
- `Decision: Watch` — exclusively an open workflow-owned CI-fix PR owns the
  issue; advance only that PR.
- `Decision: Stop` — an explicit human PR owns the issue; do not monitor it or
  plan a fallback CI-fix PR.
- `Decision: Skip` — the issue is stale, visual, already fixed, out of scope, or
  cannot be safely advanced.

Explain the concrete evidence for the decision. A search result is evidence to
inspect, not a decision by itself.

## Deduplicate without false ownership

1. An open CI-fix PR with the workflow's title prefix and exact `Refs:
   dotnet/maui#<issue>` is the only **Watch** case. Never open a second CI-fix
   PR for that issue.
2. A merged CI-fix PR with that exact reference is a **Skip** case until current
   evidence establishes a new regression.
3. A closed-unmerged CI-fix PR with that exact reference is a **Skip** case. A
   human closing the PR is a stop signal for the keep-one-PR loop.
4. An open human PR is always a **Stop** case when its own title or body
   explicitly declares that it addresses the tracking issue. Do not monitor it
   or plan a fallback CI-fix PR. Accept a `Refs:` declaration or a
   closing/reference keyword such as `Fix`, `Fixes`, `Fixed`, `Close`, `Closes`,
   `Closed`, `Resolve`, `Resolves`, or `Resolved` followed by the issue
   reference, including `#<issue>`, `dotnet/maui#<issue>`, or
   `https://github.com/dotnet/maui/issues/<issue>`.

GitHub search normally includes comments and incidental status text. Do not
treat a raw issue number in a comment, check summary, commit message, diff, or
unrelated list as human ownership. Inspect the PR title and body before stopping.
If every result is incidental, continue triage.

## Require current, specific evidence

Before a fresh attempt:

1. Confirm the issue is in the caller's branch and label scope.
2. Skip screenshot and visual-regression failures; do not modify baselines.
3. Verify the cited failure still reproduces on the latest completed base-branch
   build. A historical failure alone is not a reason to create a PR.
4. Trace the exact failing test, platform path, and deepest relevant stack frame.
   Check whether an intervening merged PR already fixed that path before proposing
   a duplicate or adjacent workaround.
5. Preserve the test's assertion. Do not mute, ignore, retry, weaken, or extend a
   timeout to hide a failure. A legitimate de-flake makes the state transition or
   synchronization deterministic.

## Bound autonomous work

Advance only the existing CI-fix PR and respect the caller's effective attempt
counter. When the cap is reached, defer rather than opening a replacement.
Never claim the target test is fixed from a category-level result or an unrelated
green leg; require evidence for the specific test on the CI-fix PR's current
head.
