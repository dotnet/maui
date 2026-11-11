# .NET MAUI Release Process Using Arcade

The .NET MAUI release process uses Arcade and 1ES to build, sign, gather, and publish packages.

For the proposed development-branch roles and servicing-release rotation, see the
[versioned release branch workflow](ReleaseBranchWorkflow.md). That proposal
keeps the canonical publishing/approval process below. Candidate integration can
automatically create staging and prepare a reviewed PR restoring net11 CI
settings. A maintainer merges that normalization PR with **Create a merge commit**;
next-lane activation and forward flow wait for its pinned ancestry. The workflow
does not directly write protected `net11` or grant a bypass. Independently, the
publication workflow automatically opens a **servicing/stable configuration PR**
against `automation/release/<band>-srN`, before a real release exists. Review
it and merge using **Create a merge commit**, then manually create
`release/<band>-srN` at that exact reviewed servicing merge commit. An old real
release ref cannot block staging preparation or authorize publication. There is
no second post-cut servicing PR.

Completed rollover also prepares a draft canonical-state PR advancing
`currentSr`; it waits for verified staging, normalization and next lanes, not a real cut, and
never merges itself. `nightlyMode` is retained. Switching from legacy to
versioned nightly builds requires separate owner-reviewed deployment evidence;
in versioned mode, every cycle advance must verify its next selected nightly
lane before the state PR is merged.

Tags are a manual release-captain action, separate from configured servicing
progress. No tag is created or moved by the cut/normalization/next-lane/signing
preparation flow. Its readiness inventories use pinned GA/prior release-branch
snapshots, not required tags. Missing or newly created tags do not block or
advance the cycle or claim that packages shipped.

The public **Prepare signed release publication** workflow waits for that
human-reviewed staging servicing merge, the exact manual release cut and its
matching internal mirror commit, queues existing
signed pipeline1095, and after success queues canonical release pipeline1445
with `pushPackages=false`. This automatically prepares artifacts/audit, not
NuGet.org publication or workload promotion. Existing federation, internal
permissions and mirror deployment are required; the workflow grants none.
Authentication uses an in-process GitHub OIDC exchange with the existing
`AZDO_TRIGGER_CLIENT_ID` and `AZDO_TRIGGER_TENANT_ID` secrets, not `azure/login`,
which the repository's selected-action policy blocks before any jobs start.
The exchange occurs only after an eligible approved release is found; an empty
sweep or `-PlanOnly` does not authenticate or queue builds. Tokens remain in the
dispatcher's process, never in an Azure CLI cache, workflow output, or artifact.
Ambiguous queue responses retain durable intent refs and never blindly resubmit.
Completed historical cycles retain their verified release/preparation source
identities when a release branch later advances; a later hotfix is not
implicitly signed or published, and does not block the next cycle. Offline plans
cannot certify completed Azure runs.
Final public publishing still requires the normal owner-approved canonical run
and its manual approval/protected-service-connection gates.

## Build and pack

`azure-pipelines-internal.yml` builds, signs, and packs .NET MAUI on the internal Azure DevOps mirror. It registers the resulting assets in Maestro's Build Asset Registry (BAR). The BAR record and source commit identify the immutable assets used by a release.

### Branch-derived package labels

`eng/Versions.props` selects `PreReleaseVersionLabel` from Azure Pipelines'
full `BUILD_SOURCEBRANCH` ref before Arcade calculates package versions:

| Source ref | Default label |
|---|---|
| `refs/heads/inflight/current` | `ci.inflight` |
| `refs/heads/inflight/<major>.0.<band>xx-sr<N>` | `ci.inflight` |
| `refs/heads/release/<major>.0.<band>xx-sr<N>` | `servicing` |
| All other refs, or no source ref | `ci.main` |

The versioned patterns require positive decimal major, band, and SR numbers
without leading zeros, and match the complete ref. For example,
`refs/heads/inflight/11.0.1xx-sr2` is recognized. `main`, `net11`, `net11.0`,
candidate branches, Preview/RC and GA release branches, unknown or malformed
branches, and `refs/pull/*` retain the `ci.main` default. PRs use their existing
explicit `-p:PreReleaseVersionLabel=ci.pr<PR number>` override; the target branch
is not used to infer servicing branding. Local builds without
`BUILD_SOURCEBRANCH` also use the default, regardless of the checked-out branch.
To evaluate another lane locally, supply the full ref with
`-p:BUILD_SOURCEBRANCH=refs/heads/release/11.0.1xx-sr1`.

`StabilizePackageVersion` defaults to `true` only for a recognized SR release
source ref with an effective `servicing` label. The default is `false` on
`main`, `net11`, candidate and inflight branches, Preview/RC/GA release branches,
unknown or malformed refs, and local builds without a source ref. Both defaults
are evaluated in `eng/Versions.props`, before Arcade calculates versions.

For example, `refs/heads/release/11.0.1xx-sr1` automatically produces stable
`11.0.10` packages, sets `DotNetFinalVersionKind=release`, and removes the
workload prerelease suffix. No separate stabilization flip is needed for a
release branch containing this policy. The existing `IsServicingBuild` rule
still evaluates to `true` when the effective label is `servicing`; that rule is
independent of an explicit stabilization override. To build prerelease
servicing packages instead, pass `-p:StabilizePackageVersion=false`.

Stable package versions do **not** authorize publication or prove that a release
has shipped. SDK bands, patch and assembly versions, previous-version testing,
signing, manual approval, and public publication gates are unchanged.

Explicit MSBuild command-line properties retain precedence, including
`PreReleaseVersionLabel=preview` or `rc` with `PreReleaseVersionIteration`,
`PreReleaseVersionLabel=ci.pr<PR number>`, `IsServicingBuild`, and
`StabilizePackageVersion=true` or `false`. A Preview, RC, or PR-label override
keeps the default stabilization `false`, even on a recognized SR source ref,
so prerelease branding is not stripped accidentally. An explicit stabilization
`true` still wins on any source ref. Preview/RC branch names alone do not select
preview branding or servicing settings. Scheduled builds keep the branch-derived
label and stable default, plus the existing `nightly` build tag; they no longer
overwrite the label after package and workload versions have been calculated.

Inspect the resulting version without building, signing, or publishing:

```bash
dotnet msbuild src/Workload/Microsoft.Maui.Sdk/Microsoft.Maui.Sdk.csproj -nologo \
  -p:OfficialBuildId=20261107.1 \
  -p:BUILD_SOURCEBRANCH=refs/heads/release/11.0.1xx-sr1 \
  -getProperty:PreReleaseVersionLabel,PackageVersion,IsServicingBuild,StabilizePackageVersion,DotNetFinalVersionKind,WorkloadVersionSuffix,SdkBandVersionForWorkload_FromRuntimeVersions
```

## Canonical release pipeline

`eng/pipelines/ci-official-release.yml` is the only release pipeline. It is manually run in the internal `dnceng/internal` project.

The pipeline accepts:

- `ghOwner` and `ghRepo`: the GitHub owner and repository name used to resolve
  the BAR build. Keep `ghOwner: dotnet` and enter `ghRepo: android-libraries`
  for an Android libraries release or `ghRepo: maui-labs` for a MAUI Labs
  release. For SkiaSharp, use `ghOwner: mono` and
  `ghRepo: SkiaSharp`. For SkiaSharp Extended, use `ghOwner: mono` and
  `ghRepo: SkiaSharp.Extended`.
- Workload behavior is inferred for `dotnet/android`, `dotnet/macios`, and
  `dotnet/maui`. Other enabled repositories use the ordinary NuGet package path.
- `commitHash`: the source commit registered in BAR. The first matching build
  returned by Darc is used, preserving the existing workload-release behavior.
  SkiaSharp is registered with Maestro through a manual-frequency dependency
  subscription, so it uses the same repository-and-commit lookup without
  receiving automatic dependency updates. The default value, `skip`, prevents
  all preparation, approval, workload-channel, and NuGet publishing jobs.
- `pushWorkloadSet`: adds the resolved BAR build to the matching .NET workload release channel.
- `pushNugetOrg`: enables the NuGet.org release stages.
- `pushPackages`: when `false`, gathers and publishes release artifacts without promoting a workload build, running approvals, or requesting the production NuGet service connection.
- `nugetIncludeFilters` and `nugetExcludeFilters`: semicolon-separated wildcard filters applied to package file names. For workload releases, include filters select packs while manifests remain selected unless excluded, preserving the previous behavior. For non-workload releases, the filters select packages for the single release set.
- `nugetAlreadyAttemptedPackFilters` and `nugetAlreadyAttemptedManifestFilters`: workload-only recovery filters for pack or manifest files that a previous task invocation already submitted to NuGet.org. These packages stay in the expected verification set but are withheld from another push while NuGet.org validation is still pending. Use only the parameter for the affected publish stage; non-workload releases reject them.

### Non-workload NuGet packages

For repositories outside the inferred workload set, the generic non-workload path validates the
BAR build against the requested GitHub repository and gathers its shipping
NuGet packages with `darc gather-drop --id <BAR ID>`. Repeated releases of the
same BAR build may encounter packages whose downloadable BAR location was
replaced by NuGet.org after an earlier partial release. The gather continues in
that case, but every selected package that was not downloaded must be confirmed
at its exact ID/version on NuGet.org; an unavailable unpublished package fails
the release. The pipeline also rejects malformed or duplicate packages, applies
the package filters, and filters exact ID/version pairs already on NuGet.org.
All selected `.nupkg` files form one release set. Workload manifests
are rejected so a workload build cannot accidentally bypass MAUI's pack-before-
manifest ordering.

The `NuGetReleaseAudit` artifact records the selected and exact staged package
lists before manual approval. `pushPackages: false` performs the
same resolution, gather, validation, filtering, and audit without promotion or
publishing. Immediately before a real release, the pipeline filters the staged
set again so packages published since preparation do not cause conflicts. It
then uses `1ES.PublishNuget@1` and verifies every selected package on NuGet.org.

Android-libraries BAR builds contain per-build deltas, not the union of channel
assets, so release each pending build in commit order.
Before the first push for any repository, verify that the existing
`nuget.org (dotnetframework)` service connection owns every package ID in the
audit artifact.

Example dry-run inputs:

```yaml
ghOwner: dotnet
ghRepo: android-libraries
commitHash: <FULL_COMMIT_SHA>
pushWorkloadSet: false
pushNugetOrg: true
pushPackages: false
```

For a SkiaSharp dry run, select `mono/SkiaSharp` and provide the source commit:

```yaml
ghOwner: mono
ghRepo: SkiaSharp
commitHash: <FULL_COMMIT_SHA>
pushWorkloadSet: false
pushNugetOrg: true
pushPackages: false
```

### Preparation

The preparation stage resolves the BAR build once from the repository and
commit, then runs one `darc gather-drop`, filtered to BAR NuGet package assets.
Symbol and other path-based blob assets are not NuGet.org release inputs and are
not downloaded. Workload releases gather fail-fast, apply the include and
exclude filters, and require non-empty pack and manifest sets. Non-workload
releases use the single package set described above; gathering continues on
individual download errors, then preparation rejects every selected package that
is neither gathered nor already available on NuGet.org.

For every gathered package, the stage reads the package ID and version from its nuspec. If a selected non-workload package was not gathered, its ID and version come from the BAR asset metadata and that exact identity must already be available on NuGet.org. The stage reports the selected identities and counts. Workload releases publish two 1ES pipeline outputs:

- `MauiPacksForNuGet`
- `MauiManifestsForNuGet`

These outputs include `expected-packages.json` and the package-availability helper used by the release jobs. They are SBOM-backed by the 1ES pipeline template and are the immutable inputs for publishing and recovery. The preparation stage records the helper's SHA-256 hash, and each production step verifies that hash before executing the artifact copy. A dry run (`pushPackages: false`) produces these artifacts but does not validate service-connection authorization, external NuGet.org authentication, or egress.

### Workload publishing and ordering

Workload packs and manifests have separate manual approval points. Their production jobs:

- use `templateContext.type: releaseJob` with `isProduction: true`;
- download the corresponding prepared pipeline artifact through `templateContext.inputs`;
- use `checkout: none`; and
- invoke `1ES.PublishNuget@1` directly with the `nuget.org (dotnetframework)` service connection.

Before each publish, the job checks every expected package identity against NuGet.org and removes already-published package files from the task input. The 1ES task therefore receives only missing packages and is skipped when every package already exists. This is required because the NuGetCommand-backed external-feed task treats HTTP 409 as a failure even when `allowPackageConflicts` is set. Task-level retry is intentionally disabled because retrying a partially published batch without filtering can fail on the packages that succeeded during the first attempt.

NuGet.org may reserve a package version and return HTTP 409 before that package becomes visible through the flat-container API. When recovering during this validation window, set `nugetAlreadyAttemptedPackFilters` or `nugetAlreadyAttemptedManifestFilters` to the affected file names accepted by the previous task invocation. Leave the parameter for the other stage at `skip`. The availability step removes those files without requiring flat-container visibility, while post-publish verification still requires every expected identity to become available.

After each publish, the pipeline polls NuGet.org for every expected package ID and normalized version with a 30-minute deadline, then fails with the missing identities if indexing does not complete. The manifest stage depends on successful pack publication and verification, so manifest approval is unavailable until every selected pack is resolvable.

The production service connection must be protected by Azure DevOps Environment and/or service-connection approval checks outside repository YAML. Before production use, release owners must confirm that it owns every selected package ID and has enough quota for the maximum release payload. If one identity cannot cover the payload, publishing must be split into deterministic sequential batches rather than reintroducing repository API keys.

## Workload sets and Visual Studio insertion

Publishing a MAUI build to a workload release channel is the handoff to
[`dotnet/workload-versions`](https://github.com/dotnet/workload-versions), not a
direct Visual Studio insertion. The workload set selects the workload versions
that ship together. The release handoff is:

1. Promote the intended MAUI BAR build to the matching workload release channel
   using `pushWorkloadSet: true` and `pushPackages: true` in the MAUI release
   pipeline. This makes the build available for consumption; it does not by itself
   update a workload set or create a Visual Studio PR. Release-branch default
   channel setup is documented in
   [Build Asset Manifest Promotion](../eng/README.md#build-asset-manifest-promotion).
2. Review and merge the dependency update in the appropriate
   `dotnet/workload-versions` release branch. Confirm that its
   `eng/Version.Details.xml` selects the intended MAUI manifest version and source
   commit, and follow that repository's
   [release process](https://github.com/dotnet/workload-versions/blob/main/doc/release-process.md)
   for workload set branding and verification.
3. After the update flows to the internal mirror, run the workload staging
   pipeline for that source branch. Visual Studio insertion is opt-in: the
   [official pipeline](https://github.com/dotnet/workload-versions/blob/eng/eng/pipelines/official.yml)
   defaults `createVSInsertion` to `false`. For an insertion, enable it, include
   `maui` in `workloadDropNames`, and select the intended Visual Studio release
   branches in `primaryVsInsertionBranches` (components and packs) and, if needed,
   `secondaryVsInsertionBranches` (packs only). Do not assume the default target
   of `main` is correct for a service release.
4. The workload staging pipeline prepares the workload drops and its
   [insertion job](https://github.com/dotnet/workload-versions/blob/eng/eng/pipelines/templates/jobs/workload-insertion-job.yml)
   automatically creates or updates a Visual Studio insertion PR for each
   configured target branch. The
   [insertion task](https://github.com/dotnet/workload-versions/blob/eng/eng/pipelines/templates/steps/workload-insertion-steps.yml)
   updates the workload component JSON values, including the selected MAUI
   payload. Track the generated PR and its validation through merge; a successful
   MAUI release or workload set dependency update alone does not establish that
   Visual Studio has consumed the change.

Workload set staging, Visual Studio insertion, and NuGet.org publishing are
separate pipeline choices. Follow the workload repository's release approvals
and verification steps rather than assuming that a dependency PR merge triggers
all three.

## Required internal validation

Production publishing cannot be fully tested by a normal dry run because NuGet.org packages are immutable. Record the following internal evidence before the first production release:

| Phase | Required evidence | NuGet.org risk |
|---|---|---|
| YAML preview | 1ES expansion, conditions, task inputs, artifact wiring, and service-connection reference are valid. | None |
| `commitHash: skip` | No gather, approval, service connection, workload-channel, or publish job runs. | None |
| Artifact dry run | A real release commit performs one gather; filters, identities, counts, and SBOMs in both artifacts are correct. | None |
| Test-feed run | The same `1ES.PublishNuget@1` shape publishes to an approved non-production feed. | None |
| Duplicate/recovery run | An existing-version run confirms that published packages are removed before the task, and a partial-publish rerun sends only the remaining packages. | None |
| Production preflight | Package ownership, authorization checks, external-feed access, and quota are confirmed without exposing credentials. | None |
| Controlled production no-op | Only if required and explicitly approved, select one version already on NuGet.org to validate conflict handling. | Low |
| Scheduled release | Use the new path for a planned release only after all earlier phases pass. | Production |

An internal Azure Artifacts feed proves task mechanics but not the exact external-feed authentication and egress path. If no representative external test feed exists, record that limitation before production use.

## Recovery

If publishing partially succeeds, rerun with the same commit and selection filters. The availability step removes packages that are already visible on NuGet.org. For workload packages accepted by the previous invocation but still undergoing NuGet.org validation, set the affected stage's `nugetAlreadyAttemptedPackFilters` or `nugetAlreadyAttemptedManifestFilters` from the package names in the prior task log, leaving the other recovery parameter at `skip`. Non-workload releases have no recovery filter; rerun after accepted packages become visible. Post-publish verification checks the complete expected set. Do not release from a different BAR drop.

Published package contents cannot be replaced. If an incorrect version is published, follow NuGet.org's process to remove it from package search results.

## Build environment

Official builds and releases run from the internal Azure DevOps mirror at `https://dev.azure.com/dnceng/internal/_git/dotnet-maui`, where signing and protected service connections are available. The public GitHub repository remains the source for development, and the mirror keeps released source aligned with it.

## References

- [Arcade SDK documentation](https://github.com/dotnet/arcade/blob/main/Documentation/README.md)
- [.NET MAUI workloads documentation](https://github.com/dotnet/maui/blob/main/src/Workload/README.md)
- [Darc documentation](https://github.com/dotnet/arcade/blob/main/Documentation/Darc.md)
- [Maestro and BAR overview](https://github.com/dotnet/arcade/blob/main/Documentation/Maestro.md)
