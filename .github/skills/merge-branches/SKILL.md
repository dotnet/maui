---
name: merge-branches
description: Merges a source branch into a target branch and creates a PR with the p/0 label for priority review.
---

# Merge Branches

Automates the process of merging one branch into another and creating a pull request.

## When to Use

Use this skill when:
- Asked to "merge main into net10.0" or "merge main into net11.0"
- Asked to perform a "weekly branch merge"
- Asked to "create merge PRs" for branch synchronization
- Need to merge any source branch into a target branch

## Instructions

### Manual Execution

Run the PowerShell script directly:

```bash
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net10.0"
```

### Parameters

| Parameter | Required | Default | Description |
|-----------|----------|---------|-------------|
| `-Remote` | Yes | - | The git remote to operate against (e.g., "origin", "upstream") |
| `-TargetBranch` | Yes | - | The branch to merge into (e.g., "net10.0", "net11.0") |
| `-SourceBranch` | No | "main" | The branch to merge from |
| `-DryRun` | No | false | If specified, shows what would happen without making changes |

### Examples

```bash
# Merge main into net10.0
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net10.0"

# Merge main into net11.0
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net11.0"

# Merge net10.0 into net11.0
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net11.0" -SourceBranch "net10.0"

# Dry run to preview changes
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net10.0" -DryRun
```

## What the Script Does

1. **Fetches latest** - Gets the most recent changes from the remote
2. **Checks for existing PRs** - Fails if an open PR already exists for this merge
3. **Creates merge branch** - Creates a branch named `merge/{source}-to-{target}`
4. **Merges source into target** - Performs the actual merge
5. **Pushes and creates PR** - Pushes the merge branch and creates a PR with the `p/0` label

## Automated Workflow

The `weekly-branch-merge.yml` workflow provides automated scheduling:

**Scheduled runs (automatic):**
- **Schedule**: Every Monday at 9:00 AM UTC
- **Targets**: Merges `main` into both `net10.0` and `net11.0`

**Manual trigger:**
- Can be triggered manually via workflow_dispatch with custom source/target branches
- Allows specifying a single target branch and optional dry-run mode

## Related Files

- **Script**: `.github/scripts/MergeBranchAndCreatePR.ps1`
- **Workflow**: `.github/workflows/weekly-branch-merge.yml`

## Error Handling

The script will fail if:
- An open PR already exists for the merge (previous week's PR wasn't merged)
- Merge conflicts are detected (requires manual resolution)
- Git operations fail for any reason

When an existing PR is detected, the script will output the PR URL and ask you to merge or close it before retrying.
