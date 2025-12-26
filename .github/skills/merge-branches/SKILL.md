---
name: merge-branches
description: Merges a source branch into a target branch and creates a pull request with p/0 label. Use this skill for weekly branch synchronization or when asked to merge branches. Target branch is required.
license: MIT
metadata:
  author: dotnet-maui
  version: "1.2"
compatibility: Requires git, gh CLI, and PowerShell. Must have write access to the repository.
---

# Branch Merge Skill

This skill automates the workflow of merging a source branch (default: `main`) into a target branch and creating a PR. It NEVER pushes directly to the target branch - always creates a merge branch for the PR.

## When to Use This Skill

Use this skill when the user asks to:
- "merge main into net10"
- "merge main into net11"
- "weekly branch merge"
- "merge main to net10.0"
- "sync net10 with main"

## Instructions

### Step 1: Run the Merge Script

Execute the PowerShell script with the required remote and target branch:

```bash
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net10.0"
```

For multiple branches, run the script once per branch:

```bash
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net10.0"
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net11.0"
```

### Step 2: Verify Results

The script will:
1. Fetch latest changes from origin
2. Create a merge branch (e.g., `merge/main-to-net10.0-20251226`) - NEVER pushes to target branch directly
3. Merge source into the merge branch
4. Push the merge branch (not the target branch)
5. Create a PR with `p/0` label

### Step 3: Handle Conflicts (if any)

If merge conflicts occur:
1. The script will report which files have conflicts
2. Resolve them manually using standard git conflict resolution
3. After resolution, push and create the PR manually

## Script Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `Remote` | string | **Yes** | - | Git remote to operate against (e.g., "origin", "upstream") |
| `TargetBranch` | string | **Yes** | - | Branch to merge into |
| `SourceBranch` | string | No | `main` | Branch to merge from |
| `DryRun` | switch | No | false | Preview without making changes |

## Examples

### Merge main into net10.0
```bash
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net10.0"
```

### Merge main into net11.0
```bash
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net11.0"
```

### Preview what would happen (dry run)
```bash
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -TargetBranch "net10.0" -DryRun
```

### Custom source branch
```bash
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "origin" -SourceBranch "net10.0" -TargetBranch "net11.0"
```

### Using upstream remote
```bash
pwsh .github/scripts/MergeBranchAndCreatePR.ps1 -Remote "upstream" -TargetBranch "net10.0"
```

## Safety Features

- **Never pushes to target branch** - Always creates a separate merge branch
- **PRs labeled with p/0** - High priority for review
- **Dry run mode** - Preview changes before executing

## Automated Execution

This skill can also run automatically via GitHub Actions:
- **Schedule**: Every Monday at 9:00 AM UTC (runs for net10.0 and net11.0)
- **Workflow**: `.github/workflows/weekly-branch-merge.yml`
- **Manual trigger**: Available from the Actions tab with customizable target branch

## Related Files

- Script: `.github/scripts/MergeBranchAndCreatePR.ps1`
- Workflow: `.github/workflows/weekly-branch-merge.yml`
