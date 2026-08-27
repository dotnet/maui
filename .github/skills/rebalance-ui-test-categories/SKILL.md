---
name: rebalance-ui-test-categories
description: Rebalances a UI-test umbrella category into additive method-level CI shards using historical Azure DevOps test durations.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires PowerShell 7, Azure CLI with the azure-devops extension, and Pester 5 for tests.
---

# Rebalance UI Test Categories

Use this skill when a category in `maui-pr-uitests` needs multiple ordinary
matrix legs and each leg must fit a target duration.

## Rules

- Historical Azure test results are required. Never substitute source test
  counts for timing evidence.
- Keep the umbrella category on every test.
- Add exactly one numbered shard category to each method carrying the umbrella
  category. Never put shard categories on a class.
- Keep every method in a fixture containing NUnit `Order` attributes on the
  same shard. The tags remain method-level, but ordered setup and shared state
  must not be split across CI processes.
- Use a conservative per-test/platform p80 of recent valid run totals.
- Aggregate measured fixed job overhead with nearest-rank p80.
- Reserve a configurable safety margin (2 minutes by default), so every
  projection is strictly below `TargetMinutes - SafetyMarginMinutes`.
- Assignment is deterministic multidimensional longest-processing-time
  scheduling over individual methods and cohesive ordered fixtures. It
  minimizes the worst projected platform/shard duration.
- Fail if one test plus overhead exceeds the target or if `-MaxShards` cannot
  satisfy the target.
- Applying a category maintains the analyzer's reusable shard-prefix set,
  preserving previously registered umbrellas, and performs mandatory
  plan-to-source validation after editing.
- Dedicated configuration stages that use `testConfigurationArgs`, such as
  `ios_ui_tests_mono_cv1`, are intentionally outside ordinary category-matrix
  shard management. Their samples and overhead are recorded separately so
  they cannot make ordinary shard projections look artificially cheaper.

## Commands

Gather, plan, report, and apply in one operation:

```powershell
pwsh .github/skills/rebalance-ui-test-categories/scripts/Rebalance-UITestCategories.ps1 `
  -Mode All -Category CollectionView -TargetMinutes 60 `
  -SafetyMarginMinutes 2 -BuildId 1561395,1563447 `
  -UnmeasuredTestPolicy ClassPlatformMax `
  -OutputPath artifacts/CollectionView-rebalance.json -Apply
```

Use `-RecentBuildCount N` instead of `-BuildId` to query recent completed builds
from pipeline definition 313. To work offline, explicitly pass
`-EvidencePath path/to/evidence.json`; absence of Azure access or an evidence
file is an error.

Use `-MinimumShards N -MaxShards N` when rebalancing an existing matrix without
changing its job count.

Separate deterministic phases are also available:

```powershell
# Azure -> evidence JSON
... -Mode Gather -Category CollectionView -BuildId 1563447 `
  -OutputPath artifacts/CollectionView-evidence.json

# Evidence -> assignment report
... -Mode Plan -Category CollectionView `
  -EvidencePath artifacts/CollectionView-evidence.json `
  -OutputPath artifacts/CollectionView-plan.json

# Existing report -> source/YAML/constants
... -Mode Apply -Category CollectionView `
  -PlanPath artifacts/CollectionView-plan.json -Apply
```

Review the JSON report's `projectedShardMinutes`, `fixedOverheadMinutes`,
`safetyMarginMinutes`, separately recorded configuration-stage evidence,
unmatched evidence, and source mapping before accepting the result.

The planner fails when active source tests have no historical sample. For a
known-complete historical run where such methods were not reported, the
explicit `-UnmeasuredTestPolicy ClassPlatformMax` option conservatively imputes
the maximum measured method duration from the same class/platform (falling
back to the platform-wide maximum) and records every imputation in the report.
