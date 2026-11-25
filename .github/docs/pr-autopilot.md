# PR Autopilot

Contributors can let the MAUI agent finish their pull request instead of going through more rounds of review
themselves. The PR template has one checkbox:

> - [ ] **Let the MAUI agent address review feedback and fix remaining issues on this PR.**

The workflow ([`.github/workflows/pr-autopilot.md`](../workflows/pr-autopilot.md)) does nothing unless that box
is ticked. Maintainers always decide whether a PR is merged.

## When it runs

- The AI review (`/review`) adds `s/agent-changes-requested`.
- A maintainer comments `/autopilot` on the PR, for example after leaving review comments.
- A maintainer runs the workflow manually with a PR number.

## What it does

1. It reads the reviews from maintainers (owner, member, or collaborator), the latest AI review, and the failing
   checks. Comments from anyone else, the PR description, and the diff are treated as data, not instructions.
2. It fixes the actionable items with the smallest change that keeps the contributor's approach. It runs the unit
   tests it can.
3. It publishes the fixes:
   - **Same-repository PRs:** it pushes one commit per fix to the PR branch.
   - **Fork PRs:** gh-aw cannot push to contributor forks, so it opens an `[autopilot]` follow-up PR that keeps the
     contributor's commits and adds the fixes.
4. It posts one comment that lists what was addressed, what was not and why, and what was validated.

It only changes files under `src/`. It never merges, approves, retargets, or closes PRs.

## Security

- The agent job has read-only GitHub permissions. Pushes, PRs, and comments go through gh-aw safe outputs, which
  only target the opted-in PR. Threat detection checks the patch first.
- Only users with write access (or the AI review's label) can start it, and only on PRs whose author opted in.
- Pushed commits use `GITHUB_TOKEN`, so they don't start CI by themselves. A maintainer starts CI as usual.
