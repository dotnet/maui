# .NET MAUI Release Process Using Arcade

The .NET MAUI release process uses Arcade and 1ES to build, sign, gather, and publish packages.

## Build and pack

`azure-pipelines-internal.yml` builds, signs, and packs .NET MAUI on the internal Azure DevOps mirror. It registers the resulting assets in Maestro's Build Asset Registry (BAR). The BAR record and source commit identify the immutable assets used by a release.

## Canonical release pipeline

`eng/pipelines/ci-official-release.yml` is the only release pipeline. It is manually run in the internal `dnceng/internal` project.

The pipeline accepts:

- `commitHash`: the source commit registered in BAR. The default value, `skip`, prevents all preparation, approval, workload-channel, and NuGet publishing jobs.
- `pushWorkloadSet`: adds the resolved BAR build to the matching .NET workload release channel.
- `pushNugetOrg`: enables the NuGet.org release stages.
- `pushPackages`: when `false`, gathers and publishes release artifacts without running approvals or requesting the production NuGet service connection.
- `nugetIncludeFilters` and `nugetExcludeFilters`: semicolon-separated wildcard filters applied to package file names. Include filters select workload packs; workload manifests remain selected unless an exclude filter removes them, preserving the previous release behavior.

### Preparation

The preparation stage resolves the BAR build once and runs one fail-fast `darc gather-drop`. It rejects a failed or incomplete gather, applies the include and exclude filters, and requires non-empty workload-pack and workload-manifest sets.

For every selected package, the stage reads the package ID and version from its nuspec and reports the selected identities and counts. It publishes two 1ES pipeline outputs:

- `MauiPacksForNuGet`
- `MauiManifestsForNuGet`

These outputs include `expected-packages.json` and are SBOM-backed by the 1ES pipeline template. They are the immutable inputs for publishing and recovery. A dry run (`pushPackages: false`) produces these artifacts but does not validate service-connection authorization, external NuGet.org authentication, egress, conflicts, or retries.

### Publishing and ordering

Workload packs and manifests have separate manual approval points. Their production jobs:

- use `templateContext.type: releaseJob` with `isProduction: true`;
- download the corresponding prepared pipeline artifact through `templateContext.inputs`;
- use `checkout: none`; and
- invoke `1ES.PublishNuget@1` directly with the `nuget.org (dotnetframework)` service connection.

The publish task allows package conflicts and has task-level retry enabled. After each publish, the pipeline polls NuGet.org for every expected package ID and version and fails with the missing identities if indexing does not complete. The manifest stage depends on successful pack publication and verification, so manifest approval is unavailable until every selected pack is resolvable.

The production service connection must be protected by Azure DevOps Environment and/or service-connection approval checks outside repository YAML. Before production use, release owners must confirm that it owns every selected MAUI package ID and has enough quota for the maximum release payload. If one identity cannot cover the payload, publishing must be split into deterministic sequential batches rather than reintroducing repository API keys.

## Required internal validation

Production publishing cannot be fully tested by a normal dry run because NuGet.org packages are immutable. Record the following internal evidence before the first production release:

| Phase | Required evidence | NuGet.org risk |
|---|---|---|
| YAML preview | 1ES expansion, conditions, task inputs, artifact wiring, and service-connection reference are valid. | None |
| `commitHash: skip` | No gather, approval, service connection, workload-channel, or publish job runs. | None |
| Artifact dry run | A real release commit performs one gather; filters, identities, counts, and SBOMs in both artifacts are correct. | None |
| Test-feed run | The same `1ES.PublishNuget@1` shape publishes to an approved non-production feed. | None |
| Duplicate/retry run | Republishing and a partial test-feed failure prove that `allowPackageConflicts` with `useDotNetTask: false` and task retry are idempotent. | None |
| Production preflight | Package ownership, authorization checks, external-feed access, and quota are confirmed without exposing credentials. | None |
| Controlled production no-op | Only if required and explicitly approved, select one version already on NuGet.org to validate conflict handling. | Low |
| Scheduled release | Use the new path for a planned release only after all earlier phases pass. | Production |

An internal Azure Artifacts feed proves task mechanics but not the exact external-feed authentication and egress path. If no representative external test feed exists, record that limitation before production use.

## Recovery

If publishing partially succeeds, rerun only with the same prepared artifact after duplicate and retry behavior has been validated. Do not gather a different BAR drop. The post-publish verification identifies packages still missing from NuGet.org.

Published package contents cannot be replaced. If an incorrect version is published, follow the NuGet.org unlist process.

## Build environment

Official builds and releases run from the internal Azure DevOps mirror at `https://dev.azure.com/dnceng/internal/_git/dotnet-maui`, where signing and protected service connections are available. The public GitHub repository remains the source for development, and the mirror keeps released source aligned with it.

## References

- [Arcade SDK documentation](https://github.com/dotnet/arcade/blob/main/Documentation/README.md)
- [.NET MAUI workloads documentation](https://github.com/dotnet/maui/blob/main/src/Workload/README.md)
- [Darc documentation](https://github.com/dotnet/arcade/blob/main/Documentation/Darc.md)
- [Maestro and BAR overview](https://github.com/dotnet/arcade/blob/main/Documentation/Maestro.md)
