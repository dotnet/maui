<!--
Open .NET 11 fix PRs against net11 for review; target main for next-major features.
After approval, a maintainer manually retargets the fix PR to the active inflight
branch and merges it directly there. Do not merge individual fixes into net11.
Bots must not retarget or merge. Candidate integration into net11 and release/*
changes are separate, explicitly coordinated release work.
-->

### Target Branch

| This PR is for | Initial base |
| --- | --- |
| New features, API changes, or .NET 12 work (including .NET 12 fixes) | `main` |
| .NET 11 servicing fixes | `net11`, for review |
| Older servicing, release integration, infrastructure, or documentation | The branch designated for that work |

- [ ] I selected the applicable base branch using the [contribution guide](https://github.com/dotnet/maui/blob/main/.github/CONTRIBUTING.md#choose-your-pr-base-before-starting).

For .NET 11 fixes, only a maintainer retargets the approved PR to the active
inflight lane and merges it after CI validates that final base. Do not merge
individual fixes directly into `net11`; agents must not retarget or merge.

### Description of Change

<!-- Enter description of the fix in this section -->

### Issues Fixed

<!-- Please make sure that there is a bug logged for the issue being fixed. The bug should describe the problem and how to reproduce it. -->

Fixes #

<!--
Verify the target branch follows the release workflow in docs/ReleaseBranchWorkflow.md.
Infrastructure and documentation changes should target the branch where they apply.
-->
