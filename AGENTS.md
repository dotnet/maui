# Contributor and agent branch routing

Choose the intended pull request base **before starting a new change**:

| Work | Initial PR base |
| --- | --- |
| New features, API changes, and .NET 12 work, including .NET 12 fixes | `main` |
| .NET 11 servicing fixes | Literal `net11`, for review |
| Older servicing, release integration, infrastructure, or documentation | The branch designated for that work; do not guess |

For a .NET 11 fix, start from `net11`, not next-major `main`. After approval, a
maintainer manually retargets the PR to the active versioned inflight branch,
validates CI on that final base, and merges there. Agents must not retarget or
merge PRs. Individual fixes are not merged directly into `net11`; candidate
integration is separate release work.

Read [.github/CONTRIBUTING.md](.github/CONTRIBUTING.md) and resolve current roles
from [eng/release-rollover.json](eng/release-rollover.json). Follow
[docs/ReleaseBranchWorkflow.md](docs/ReleaseBranchWorkflow.md); do not hard-code
an inflight SR number or treat historical `net11.0` as literal `net11`.
The highest retained `netN.0` name is not the development target. Read the
selected branch's `global.json` for its SDK; migration status does not change
the intended PR route.

If the issue's intended release is unclear, ask a maintainer before selecting a
base. Respect an explicitly assigned release branch and applicable
[Copilot instructions](.github/copilot-instructions.md) and
[path-specific instructions](.github/instructions).
