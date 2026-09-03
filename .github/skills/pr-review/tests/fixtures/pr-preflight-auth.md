# PR Pre-Flight Authentication Guidance

This focused fixture mirrors the Environment & Authentication and local-first
guidance in `.github/pr-review/pr-preflight.md`. Keep it synchronized with that
source when the CI authentication policy changes.

## Environment & Authentication

In the `CopilotReview` CI task, all GitHub tokens are intentionally stripped
for security. Commands that require authenticated `gh` access, such as
`gh pr view`, `gh issue view`, and `gh api`, will fail.

This expected authentication failure is not an environment blocker. Do not
stop, record a blocker, or lower review confidence because `gh` is
unauthenticated.

- The PR branch is already checked out locally. Use local `git` for changed
  files, the diff, and commit messages.
- `dotnet/maui` is public. Read issue and PR data through the unauthenticated
  public REST API with `curl`.
- Authenticated `gh` commands remain suitable for local runs where a token is
  available, but they are not required in CI.

## Local-First Commands

```bash
git diff --name-status <base>..HEAD
git log --oneline -20

curl -s https://api.github.com/repos/dotnet/maui/pulls/XXXXX
curl -s https://api.github.com/repos/dotnet/maui/issues/ISSUE_NUMBER
curl -s "https://api.github.com/repos/dotnet/maui/issues/ISSUE_NUMBER/comments?per_page=100"
curl -s "https://api.github.com/repos/dotnet/maui/pulls/XXXXX/comments?per_page=100"
```
