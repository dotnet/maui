Policy Service Readme
=====================

Policy Service is an automated tool that responds to changes in issues, PRs, or runs on a schedule. It can modify issues and PRs by adding/removing labels, adding comments, or performing other operations.

## Controlled reset (phase 1)

The resource-management policy is intentionally removed while investigating duplicate deployed policy revisions in [#37531](https://github.com/dotnet/maui/issues/37531).

Its [disabled snapshot](policy-reset/resourceManagement.yml.disabled) is outside the active `policies` folder and retained only as trusted validator identity data for `/issue triage`. It is not an active Policy Service configuration.

Merging this removal requests reconciliation of the repository-managed policy. It can temporarily pause all Policy Service automation, including automatic labels, replies, stale reminders and closure, and automated inter-branch PR approvals and auto-merge. A stale server-side registration may still execute; removing the YAML alone does not establish that cleanup succeeded.

### Gate before restoration

1. Confirm that Policy Service has reconciled the deletion from the default branch.
2. On a designated test issue without `s/needs-info`, record the existing comments, apply the label once, and record the label event and any new Policy Service replies. Remove the test label after observation.
3. Require zero new Policy Service replies for that event. If any reply remains, or reconciliation cannot be confirmed, stop and ask the Policy Service owners to remove the stale registration. Do not restore the policy or treat an unobserved event as a passing check.

### Restoration (phase 2)

Only after the gate passes, move the disabled snapshot back to `.github/policies/resourceManagement.yml` and restore the validator data path in `.github/scripts/IssueTriage.ps1` together in a separate PR. Compare with the [pre-reset configuration](https://github.com/dotnet/maui/blob/10fb0b2883597d5c78cc0488a574f8168c46a28b/.github/policies/resourceManagement.yml) and incorporate any subsequently intended policy changes.

After restoration reconciles, verify that controlled issue and PR responder events each produce exactly one expected reply, with the current single-`@` author mention rather than a second legacy `@@` reply. Keep #37531 open until restoration and these checks are complete.

For information about the service and the YAML schema for configuring automation, visit https://eng.ms/docs/products/gitops/apps/resource-management (Microsoft Internal).
