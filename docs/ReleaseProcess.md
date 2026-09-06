# .NET MAUI Release Process Using Arcade

The .NET MAUI release process uses Arcade and 1ES to build, sign, gather, and publish packages.

## Build and pack

`azure-pipelines-internal.yml` builds, signs, and packs .NET MAUI on the internal Azure DevOps mirror. It registers the resulting assets in Maestro's Build Asset Registry (BAR). The BAR record and source commit identify the immutable assets used by a release.

## Canonical release pipeline

`eng/pipelines/ci-official-release.yml` is the only release pipeline. It is manually run in the internal `dnceng/internal` project.

The pipeline accepts:

- `ghOwner` and `ghRepo`: the GitHub owner and repository name used to resolve
  the BAR build. Keep `ghOwner: dotnet` and enter `ghRepo: android-libraries`
  for an Android libraries release.
- Workload behavior is inferred for `dotnet/android`, `dotnet/macios`, and
  `dotnet/maui`. Other enabled repositories use the ordinary NuGet package path.
- `commitHash`: the source commit registered in BAR. The first matching build
  returned by Darc is used, preserving the existing workload-release behavior.
  The default value, `skip`, prevents
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

### Preparation

The preparation stage resolves the BAR build once and runs one `darc gather-drop`, filtered to BAR NuGet package assets. Symbol and other path-based blob assets are not NuGet.org release inputs and are not downloaded. Workload releases gather fail-fast, apply the include and exclude filters, and require non-empty pack and manifest sets. Non-workload releases use the single package set described above; gathering continues on individual download errors, then preparation rejects every selected package that is neither gathered nor already available on NuGet.org.

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
