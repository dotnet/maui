# Intelligent UI Test Execution - Quick Start

## TL;DR

Smart test selection for PRs - runs only the tests you need based on what files changed.

**Time Savings:**
- Documentation PRs: Skip all tests (100% savings)
- Single control fix: ~15 minutes instead of 4 hours (93% savings)
- Multi-control fix: ~30-60 minutes instead of 4 hours (75-87% savings)

## Enable in 3 Steps

### Step 1: Get GitHub Token (2 minutes)

1. Go to https://github.com/settings/tokens
2. Click "Generate new token (classic)"
3. Name: `Azure DevOps MAUI Pipeline`
4. Scope: Check `repo`
5. Generate and **copy the token**

### Step 2: Add to Azure DevOps (1 minute)

1. Open pipeline → Edit → Variables
2. Add variable:
   - Name: `GitHubToken`
   - Value: *paste your token*
   - ✅ Keep this value secret
3. Save

### Step 3: Update Pipeline (1 minute)

Edit `eng/pipelines/ui-tests.yml`:

```yaml
# Change this line:
- template: common/ui-tests.yml

# To this:
- template: common/ui-tests-intelligent.yml
```

Save and commit. Done!

## How It Works

```
PR Changes → GitHub CLI → Analysis Script → Test Categories → Run Only Those Tests
```

**Example:**

You change `Button.cs` → Script detects "Button" → Runs only Button tests → 15 min instead of 4 hours

## What Gets Run?

| You Change | Tests Run | Time |
|-----------|-----------|------|
| Button control | Button only | ~15 min |
| Documentation | None | ~2 min |
| Core framework | All (safety) | ~4 hours |
| Entry + Editor | Entry, Editor | ~30 min |
| Platform-specific | Platform + affected | ~45 min |

## Monitoring

View analysis results:
1. Open PR build
2. Go to "analyze_pr_changes" stage
3. Look for "Test Category Groups to Run"

## Troubleshooting

**Still running all tests?**
- Check you're using `ui-tests-intelligent.yml` template
- Verify `GitHubToken` variable is set
- Confirm it's a PR build (not manual/CI)

**No tests running?**
- Check analysis output in pipeline logs
- Files might be docs-only (intended)
- Or pattern matching might be too restrictive

**Auth errors?**
- Verify GitHub token is set correctly
- Check token hasn't expired
- Ensure token has `repo` scope

## Override

Need to run all tests for a specific PR?

Add this to PR description:
```
[run-all-tests]
```

Update the script to detect it (see full docs).

## Full Documentation

- [Complete Guide](README-INTELLIGENT-TESTS.md)
- [Technical Details](INTELLIGENT-TEST-EXECUTION.md)
- [Analysis Script](../scripts/analyze-pr-changes.ps1)

## Cost Savings

Expected: **~$39K/year** in CI costs

Weekly: 156 fewer compute hours

## Questions?

Open an issue with `[intelligent-tests]` tag.
