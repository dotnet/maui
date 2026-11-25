# PR Journey tracker

The `PR Journey` workflow ([`.github/workflows/pr-journey.yml`](../workflows/pr-journey.yml)) keeps one
**"🧭 Where is this PR?"** comment on every pull request. The comment shows the steps a contribution goes
through, the current step, and **who has to act next**. The workflow edits this comment in place and
never posts a new one.

## Steps

| Step | Done when |
|------|-----------|
| Submitted | The PR is open and not a draft |
| Build & tests (CI) | The head commit's combined check status is `SUCCESS` |
| AI review | The `/review` pipeline added an outcome label (`s/agent-approved`, `s/agent-changes-requested`, `s/agent-reviewed`) |
| Improvements | The AI review needed no changes, or the PR received commits after the reviewed commit, or the author commented `/ready` |
| Maintainer review | A maintainer (owner, member, or collaborator other than the author or a bot) approved |
| Merge | The PR was merged. For `net11` PRs, a maintainer first retargets the PR to the active release branch |

Maintainer change requests and failing CI take priority, and the comment says what to do next.
"Waiting on" is one of: you (the author), the MAUI agent, CI, a maintainer, or the MAUI team.

## Contributor choice

The PR template has a **Help from the MAUI agent** section:

- **Review and improve automatically** (`fix`): the MAUI agent and maintainers may push fixes for review
  findings to the PR branch. The workflow adds `s/agent-fix-allowed` so that tooling and maintainers can find
  these PRs. On forks this requires **Allow edits by maintainers**; the tracker warns when it is off.
- **Review only** (`suggest`): the author addresses findings.
- **No AI review** (`none`): the AI review and improvement steps are skipped.

## Commands

The command must be the first line of a PR comment:

| Command | Allowed for | Effect |
|---------|-------------|--------|
| `/agent fix` | PR author | Switches the PR body choice to "Review and improve automatically" |
| `/agent stop` | PR author or users with write access | Switches to "Review only" |
| `/agent off` | PR author | Switches to "No AI review" |
| `/ready` | PR author or users with write access | Marks the AI review findings as addressed for the current head commit |

The workflow reacts with 👍 when it runs a command and 😕 when it refuses one.

## Updates

The workflow runs on PR events (including label changes from the review pipeline), PR comments, and same-repo
reviews. It also runs every 30 minutes to refresh existing trackers. The scheduled run catches CI results and
reviews on fork PRs, which do not trigger workflows with a write token. Trackers are only created for open PRs.

## Security

- It never checks out or runs PR code. Only `.github/scripts/PrJourney.cjs` is checked out, from the
  base or default branch, with `persist-credentials: false`.
- PR bodies and comments are parsed with fixed patterns and never evaluated.
- It uses the workflow `GITHUB_TOKEN` with `issues`/`pull-requests: write` and `contents: read` only.

## Enabling

The workflow runs only when the repository variable `PR_JOURNEY_ENABLED` is `true`:

```bash
gh variable set PR_JOURNEY_ENABLED --body true --repo <owner>/<repo>
```

`PR_JOURNEY_BOT_LOGINS` (optional, comma-separated) adds logins whose reviews count as AI reviews, in
addition to `MauiBot`, `maui-bot`, and `github-actions[bot]`.
