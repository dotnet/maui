# Shared .NET Package Release System

## Status

This document specifies the current release system implemented by this repository.

The repository is not a template or library consumed by product repositories. It is the
central release system:

- one Azure DevOps pipeline is configured against this repository;
- release operators queue that pipeline with the repository and commit or BAR build to
  release;
- product repositories do not import this repository or copy its YAML;
- packages are gathered from the Build Asset Registry (BAR), not built from a product
  repository checkout.

The Azure DevOps entry point is:

```
eng/pipelines/ci-official-release.yml
```

The filename is part of the Azure DevOps pipeline definition contract.

## Goals

The system MUST:

1. release approved shipping NuGet packages from an exact BAR build;
2. support workload and non-workload repositories through one pipeline;
3. fail closed when repository, commit, channel, package identity, or artifact integrity
   cannot be established;
4. make the default queued run a non-publishing dry run;
5. keep NuGet.org publishing inside `1ES.PublishNuget@1`;
6. enforce pack-before-manifest ordering for workload releases;
7. support safe retry after partial publication;
8. produce a human-readable, content-addressed audit artifact.

## Non-goals

The release tool does NOT:

- build product repositories;
- push a package to any feed;
- hold a NuGet.org credential;
- add a BAR build to a channel;
- approve a release;
- invoke `darc` as a subprocess.

Remote mutations are explicit pipeline operations:

| Mutation | Owner |
|---|---|
| Push packages to NuGet.org | `1ES.PublishNuget@1` |
| Add a build to a workload-set channel | `darc add-build-to-channel` pipeline step |
| Approve either operation | `ManualValidation@0` agentless job |

`darc gather-drop` also remains a pipeline step because Darc owns feed discovery,
authentication, released-asset handling, and download retries.

## Safety model

### Credential boundary

The strongest NuGet.org safety property is the Azure DevOps credential boundary.

The `nuget.org (dotnetframework)` service connection is declared only by
`1ES.PublishNuget@1`. Azure DevOps injects its secret only into that task. The release tool,
including `prune-published` and `verify`, cannot access it.

The tool also contains:

- no push verb;
- no `--push` option;
- no use of NuGet's `PackageUpdateResource`;
- no raw NuGet.org upload implementation.

Architecture tests scan the tool for known push and process-execution APIs. These tests are
tripwires, not the primary security boundary: metadata scans cannot prove the absence of
reflection or arbitrary HTTP code. Credential scoping is authoritative.

### Dry-run contract

`releaseMode` defaults to **Preview the package release without making changes**.

For either preview mode, the expanded pipeline MUST NOT contain:

- `1ES.PublishNuget@1`;
- the `nuget.org (dotnetframework)` service connection;
- `release verify`;
- `darc add-build-to-channel`;

The matching package-set stage remains present. Its `preflight` job downloads the `Release`
artifact, verifies manifest/tool integrity, queries NuGet.org read-only, prunes already-published
versions, validates the remaining package set, and publishes that exact local artifact for
review.

The dry run therefore:

- queries NuGet.org for exact package availability but performs no NuGet.org mutation;
- does not write or update GitHub;
- does not mutate BAR, channels, or release state.

The dry run DOES:

- authenticate to production Maestro/BAR;
- read BAR;
- download assets from Azure DevOps feeds or blob storage;
- read repository dependency metadata when Darc requires it;
- write and copy files on the disposable agent;
- upload the manifest, packages, and release-tool bundle as Azure DevOps artifacts;
- pause at the same package-set `ManualValidation@0` gates used by a publish run;
- run the same production `releaseJob`, including approved-artifact download, integrity
  validation, and the final read-only prune;
- produce logs, build tags, audit records, and mandatory 1ES compliance output.

The workload-promotion preview additionally runs its approval and normal agent job, obtains
Darc through Arcade, and reports the BAR build and channel it would use. The
`add-build-to-channel` command is absent.

The 1ES pipeline template is external code. Component-governance tooling may perform
public-registry metadata lookups. If policy requires a literal network-level prohibition on
all HTTP traffic to NuGet.org, agent egress policy MUST enforce it in addition to the
pipeline's exclusion of all NuGet.org release operations.

### Compile-time exclusion

The root maps the human-readable `releaseMode` dropdown onto booleans once. The package-set
template accepts only the derived `publishPackages` boolean; it does not compare display
strings. Mutating publish steps are selected with:

```yaml
- ${{ if eq(parameters.publishPackages, true) }}:
```

This is a compile-time barrier around the NuGet.org task and post-publish verification.
When false, those steps do not exist in the expanded YAML. The approval and production
`releaseJob` remain so a dry run exercises their topology, artifact input, SDK setup,
integrity checks, and final prune.

The package-set stages themselves always exist so a dry run validates stage selection,
artifact download, SDK setup, tool execution, and local integrity.

The promotion stage is present only in the two modes that mention promotion. Its mutation
step is present only in **Publish packages and promote the workload build**:

```yaml
- ${{ if eq(variables.promoteWorkloadSet, 'true') }}:
```

Its runtime condition additionally requires the `IsWorkload` output that the pipeline
loads from `release-manifest.json`.

### Queue-parameter safety

Queue-time parameters are operator-controlled and MUST NOT be interpolated into executable
PowerShell text.

Every queue-time string enters scripts through an `env:` binding and is read through
`$env:...`. The release CLI validates:

- `buildIdentifier` as either a full 40-character commit SHA or a positive BAR build ID;
- GitHub owner and repository names against a restricted character set.

This validation runs before the release tool authenticates.

### Source-control safety

The pipeline source MUST be the internal Azure Repos mirror, not a GitHub service
connection. The normal prepare and optional promotion jobs check out `self` with
`persistCredentials: false`. Package preflight and 1ES release jobs consume immutable
artifacts and use `checkout: none`; 1ES release jobs prohibit source checkout.

The released repository is never checked out. Its GitHub identity is data used to query BAR.
No checked-in step invokes `git push`, `gh`, Octokit, or the GitHub write API.

## Repository layout

```
eng/pipelines/ci-official-release.yml  Azure DevOps entry point
eng/pipelines/stages/publish-set.yml   internal reusable publish-stage definition

src/DotNet.Release/
  Program.cs                           CLI entry point
  DotNetReleaseException.cs            expected CLI failure
  Cli/                                 command handlers and human-readable output
  Policy/                              pure release decisions
  Model/                               transport and value models
  Clients/                             read-only Maestro and NuGet clients

tests/DotNet.Release.Tests/            unit, contract, architecture, and pipeline tests
config/repositories.json               release policy
```

The implementation is one executable project. Interfaces are retained where they make
remote reads testable:

- `IMaestroClient`;
- `INuGetClient`.

`NuGetClient` owns local package identity reading plus the NuGet protocol call, identity
deduplication, and bounded concurrency behind that one batch-facing interface. Tests inject
a delegate into the implementation when they need to observe individual lookups; there is
no second production abstraction.

No dependency-injection container is required. The CLI composition root constructs the
production adapters directly.

Expected release failures throw `DotNetReleaseException`, which `Program.Main` reports once
with exit code 1. Validation that can find several actionable problems still aggregates
their messages into one exception. Successful command output uses `TextWriter` so tests can
capture the exact human-readable audit without a custom console or logging framework.

## Pipeline inputs

| Parameter | Required | Default | Purpose |
|---|---:|---|---|
| `ghOwner` | yes | `dotnet` | GitHub repository owner |
| `ghRepo` | yes | `select-repository` | Fail-closed sentinel that the operator replaces with an enabled repository |
| `buildIdentifier` | yes | `enter-bar-id-or-commit-sha` | Full commit SHA to resolve, or an exact BAR build ID |
| `releaseMode` | yes | Preview the package release without making changes | Select package preview/publication, with optional workload-promotion preview/publication |
| `includeFilters` | no | `skip` | Semicolon-separated package filename globs |
| `excludeFilters` | no | `skip` | Semicolon-separated package filename globs |
| `recoveryFilters` | no | `skip` | Packages already submitted by this release |
| `pool` | infrastructure | internal Windows pool | Agent pool definition |

Azure DevOps runtime parameters cannot be optional. An omitted parameter either uses a
default or, for an allowed-values list, silently selects its first value. Explicit sentinels
make that platform behavior visible and safe: `select-repository` fails repository policy,
`enter-bar-id-or-commit-sha` fails build-identifier validation, and `skip` is normalized to
no optional filter before the CLI is invoked.

Runs are tagged `DRY-RUN` or `PUBLISH`. After BAR resolution they also receive Arcade's
established `BAR ID - <id>` tag and a matching `REPO - <owner/name>` tag, so release history
can be filtered by either resolved identity.

The four values are deliberately descriptive:

- **Preview the package release without making changes**
- **Preview packages and workload promotion without making changes**
- **Publish packages to NuGet.org**
- **Publish packages and promote the workload build**

A promotion mode on a package-only repository fails instead of silently ignoring the
operator's selection.

## Pipeline topology

### Prepare stage

The prepare stage always runs, including on a dry run:

```
checkout release-system source
  -> install pinned .NET SDK
  -> build and pin Release/_tool
  -> release resolve validates BAR and writes resolved-build.json
  -> darc gather-drop downloads the verified BAR ID
  -> release stage creates release-manifest.json
  -> pin release-manifest.json
  -> publish Azure DevOps artifacts
```

The gathered drop is placed under `Agent.TempDirectory`, not the artifact staging root.

The prepare job builds the framework-dependent tool once with `dotnet build`, before any
task acquires the Maestro production identity. The uncompressed output remains in the
initial artifact staging directory for 1ES binary scanning. `global.json` and that output
are also compressed into one `release-tool.zip`; later jobs verify and execute that exact
bundle after approval without restoring or rebuilding it.

### Workload-set promotion stage

When a promotion mode is requested for a workload repository:

1. an agentless `ManualValidation@0` job displays the resolved target for review;
2. a normal agent job verifies the manifest and obtains Darc through Arcade;
3. a preview reports what would change, while the publishing mode runs
   `darc add-build-to-channel --skip-assets-publishing`.

Whenever promotion is included, `publish_packs` depends on successful completion of that
stage. The preview therefore exercises the same ordering as publication. Manifests remain
ordered after verified packs.

### Package-set stages

Every package set uses the same internal stage definition:

```
preflight
  -> download Release artifact
  -> verify manifest and tool hashes
  -> release prune-published
  -> count the validated remaining nupkgs
  -> publish exact pruned artifact for review

ManualValidation@0
  -> production releaseJob
    -> download approved artifact
    -> verify manifest and tool hashes
    -> refresh prune
    -> count the validated remaining nupkgs

when the selected mode publishes packages:
    -> 1ES.PublishNuget@1
    -> release verify

otherwise:
    -> confirm publishing operations were compile-time excluded
```

The preflight job proves that a normal agent job can download the artifact, install the SDK,
load the approved tool, query NuGet.org, interpret the manifest, and validate the exact pending
set before approval. The dry run then exercises the production `releaseJob` without giving
it a publishing task or credential. The release job repeats the prune after approval; the
approved set can only shrink.

The manual validation task MUST run in its own `pool: server` job. An agentless job cannot
download artifacts or execute scripts, so the gated operation runs in a dependent agent
job.

### Workload ordering

Workload releases produce two stages:

```
publish_packs
  -> publish_manifests
```

`publish_manifests` depends on successful completion of `publish_packs`. Pack verification
is the final step of the pack publish job, so manifest approval is unreachable until every
pack is resolvable on NuGet.org.

This ordering is load-bearing: manifests reference packs, and NuGet.org package versions
are immutable.

Non-workload releases produce one `publish_packages` stage.

## Repository policy

`config/repositories.json` is authoritative. Repositories not listed in the file MUST fail
before BAR or package work begins.

```jsonc
{
  "schemaVersion": 1,
  "repositories": {
    "dotnet/maui": { "workload": true },
    "dotnet/skiasharp": {
      "workload": false,
      "channel": { "name": ".NET Libraries", "id": 1648 }
    }
  },
  "workloadSets": {
    "10": {
      "channel": ".NET 10 Workload Release",
      "feed": "dotnet10-workloads"
    }
  }
}
```

For repositories with a required channel, both the case-sensitive channel name and numeric
ID MUST match exactly once.

Channel verification establishes a property distinct from source identity:

- repository and commit checks establish which build is being released;
- channel membership establishes that the build reached the configured release quality bar.

### Workload classification

`config/repositories.json` is the only source of workload classification.

`release resolve` reads the policy, detects BAR ID versus commit SHA, validates the build,
and records the workload classification in `resolved-build.json`. After gathering, `release
stage` rechecks that classification against current policy while creating the completed
manifest. The pinning step reads that manifest and emits `IsWorkload` for downstream stage
conditions. When publishing is enabled, the pipeline contains the workload and non-workload
stage shapes:

- `publish_packs` and `publish_manifests` require `IsWorkload=true`;
- `publish_packages` requires `IsWorkload=false`;
- `promote_workload_set` requires `IsWorkload=true`.

This keeps stage ordering explicit without duplicating repository classification in YAML.

## BAR resolution

`release resolve` accepts either:

```text
repository + exact commit
```

or:

```text
repository + BAR build ID
```

For a commit selector, the typed PCS query requires exactly one repository-and-commit match
and returns its BAR ID. The query starts from the commit rather than a GitHub repository
filter, so it can also find older builds whose `gitHubRepository` is null and identify them
from the Azure DevOps mirror. For a BAR ID selector, the typed PCS query returns its commit.
Both paths then verify:

- exactly one build was found;
- the BAR ID and commit are valid and consistent with the selector;
- the build belongs to the requested repository;
- any required channel is present exactly once.

Supplying a BAR ID does not imply trust; repository, commit, and channel checks complete
before Darc gathers any packages.

### Repository identity

When BAR provides `gitHubRepository`, the value is normalized and validated as a GitHub
URL. Accepted variations include:

- host casing and `www.github.com`;
- trailing slash;
- `.git` suffix;
- query and fragment components.

Other hosts are rejected.

BAR can store `gitHubRepository: null`. This is a real production shape, including
SkiaSharp BAR build 328857. In that case, identity is derived from the Azure DevOps mirror
name using Arcade's convention:

```text
dotnet-SkiaSharp -> dotnet/skiasharp
```

The first hyphen separates owner and repository. A value that does not follow this
convention fails with an explicit mirror-identity error.

The resolve log records whether identity came from `GitHubRepository` or
`AzureDevOpsMirrorConvention`; this diagnostic does not need to cross job boundaries.

### Typed BAR client

The tool uses `Microsoft.DotNet.ProductConstructionService.Client`:

- `IBuilds.GetBuildAsync` for BAR ID selectors;
- `IBuilds.ListBuildsAsync` for commit selectors.

Commit lookup requests channel collections so required-channel validation uses the same
data on both selector paths.

The client model is not nullable-annotated. Adapter code treats repository, commit, and
channel strings as untrusted nullable input.

## Package gathering

The pipeline runs:

```text
darc gather-drop
  --id <barBuildId>
  --include-released
  --asset-filter '^[^/]+$'
```

The asset filter selects slash-free package assets and excludes symbol and other
path-shaped blob assets.

The pipeline follows the [official Darc setup
guidance](https://github.com/dotnet/arcade-services/blob/main/docs/Darc.md#setting-up-your-darc-client)
through Arcade's `Get-Darc` helper in `eng/common/tools.ps1`. The helper invokes
`darc-init.ps1`, asks Maestro's Darc-version endpoint for the matching supported version,
and installs it from `dotnet-eng`; no Darc version is duplicated in this repository's YAML
or dependency files. The normal prepare and promotion jobs each obtain Darc independently.
Darc is never copied into a release artifact or used by a package release job.
`--include-released` permits downloading a build already marked released; it does not
change release state.

## Package validation and selection

`release stage` reads each `.nupkg` using `PackageArchiveReader`.

For every package it records:

- ID from the nuspec;
- full version from the nuspec;
- normalized version from `NuGetVersion.ToNormalizedString()`;
- filename;
- SHA-256 of package bytes.

The stage fails closed on:

- malformed or missing nuspec metadata;
- a filename not beginning with its nuspec ID;
- a filename containing a directory;
- invalid or inconsistent normalized version;
- duplicate filename, case-insensitively;
- duplicate package ID/version, case-insensitively;
- no selected packages.

Include and exclude filters support case-insensitive `*` and `?` simple expressions.
Exclude wins over include.

### Workload package sets

Workload packages are classified by filename:

- names containing `Manifest` and ending in `.nupkg` are manifests;
- all other selected packages are packs.

Include filters select packs. Manifests remain selected unless explicitly excluded.
Exclude filters apply to both sets.

A workload release requires at least one pack and at least one manifest.

The workload band is parsed from:

```text
.Manifest-<major>.
```

All manifests must resolve to one configured band.

### Non-workload package set

A non-workload release produces one package set and explicitly rejects workload manifests.

## Release manifest contract

`release-manifest.json` is the only JSON release artifact. `release stage` creates it once,
after verified build resolution and package gathering. PowerShell pins those completed bytes
before the file crosses the prepare/publish job boundary; it is immutable from creation.

The manifest is a machine transport object, not the human approval interface. The CLI MUST
print every field in the resolved build, final manifest, and in-memory prune result in a
readable summary. In particular, the preflight log immediately before approval lists every
package's identity, file name, hash, and final disposition.

```jsonc
{
  "toolVersion": "1.0.0",
  "createdUtc": "2026-08-28T00:00:00Z",
  "source": {
    "repository": "dotnet/skiasharp",
    "repositoryUrl": "https://github.com/dotnet/skiasharp",
    "commit": "<full-sha>",
    "barBuildId": 328857,
    "repositoryOrigin": "AzureDevOpsMirrorConvention",
    "workload": false,
    "channel": { "name": ".NET Libraries", "id": 1648 }
  },
  "workloadSet": null,
  "sets": [
    {
      "name": "NuGet packages",
      "order": 0,
      "artifactName": "ReleasePackages",
      "packages": [
        {
          "id": "SkiaSharp",
          "version": "3.119.0",
          "normalizedVersion": "3.119.0",
          "fileName": "SkiaSharp.3.119.0.nupkg",
          "sha256": "<sha256>"
        }
      ]
    }
  ]
}
```

Unknown schema versions fail before the manifest is used.

## Artifact contract

The prepare stage publishes one artifact named `Release`. Package sets are directories
within that artifact:

| Release type | Set directory |
|---|---|
| Non-workload | `ReleasePackages` |
| Workload packs | `ReleasePacks` |
| Workload manifests | `ReleaseManifests` |

The artifact layout is:

```
Release/
  release-manifest.json
  release-tool.zip
  _tool/
    release.dll
    release.deps.json
    <runtime dependencies>
  ReleasePackages/
    *.nupkg
```

Workload releases contain `ReleasePacks/` and `ReleaseManifests/` instead of
`ReleasePackages/`. There is one authoritative `release-manifest.json` at the artifact root.

Each matching preflight publishes a prepared artifact containing the pinned root manifest,
the pinned tool bundle, and only that stage's pruned package-set directory. The approval
artifact therefore cannot contain an unpruned sibling workload set and the release job
needs no source checkout.

## Integrity chain

The prepare stage:

1. builds the C# project into `Release/_tool` for execution and binary scanning;
2. bundles `global.json` and `_tool` into `release-tool.zip`;
3. hashes that bundle once as `ToolBundleHash`;
4. executes `_tool/release.dll` for `resolve` and `stage`;
5. records every package SHA-256 in the manifest;
6. writes one authoritative manifest into the release artifact root;
7. computes the manifest hash independently with `Get-FileHash`;
8. exports `ToolBundleHash` and `ReleaseManifestHash`.

The publish job:

1. downloads the stage's exact prepared artifact without checking out source;
2. verifies `release-tool.zip` with `ToolBundleHash`;
3. expands it and installs the SDK selected by its `global.json`;
4. compares raw manifest bytes with `ReleaseManifestHash`;
5. executes the bundled DLL for `prune-published`;
6. validates all pending package hashes before invoking 1ES;
7. executes the same DLL for `verify` with the same expected manifest hash.

Unexpected `.nupkg` files, missing pending files, and changed package bytes fail closed.
Companion JSON and executable files are excluded by the `.nupkg` enumeration.

## Pruning and publication

`release prune-published` queries NuGet.org read-only using `FindPackageByIdResource`.

For each manifest package it records one disposition:

| Disposition | Meaning | File after pruning |
|---|---|---|
| `Pending` | Not visible on NuGet.org | present |
| `AlreadyPublished` | Exact ID/version is visible | removed |
| `PreviouslyAttempted` | Matched an operator recovery filter | removed |

The immutable manifest is not rewritten. Dispositions are printed in full to the job log.
The following PowerShell step counts the validated `.nupkg` files that remain in the package
set directory; pruning produces no JSON sidecar.

Before deleting anything, all artifact and package names are revalidated as a single
relative path component.

After pruning, the invariant is:

```text
file is present <=> disposition is Pending
```

The pipeline translates an empty validated package-set directory into
`NuGetPackagesToPublish=false`, which skips `1ES.PublishNuget@1`.

`1ES.PublishNuget@1` globs only top-level `.nupkg` files and publishes through
`nuget.org (dotnetframework)`.

## Verification and recovery

`release verify` polls every package in the current package set until:

- every identity is resolvable on NuGet.org; or
- the whole-set deadline expires.

Defaults:

```text
deadline: 30 minutes
poll interval: 20 seconds
```

The deadline applies to the set, not to each package.

The NuGet client uses `NoCache=true`. A cached negative could cause a retry to re-submit a
package that already landed, and the publish task treats HTTP 409 as fatal.

Transient query failures are logged and retried until the deadline. Cancellation still
terminates immediately.

For partial publication:

1. queue the same repository, commit, filters, and BAR build;
2. `prune-published` removes packages already visible on NuGet.org;
3. if NuGet.org accepted a package but it is not yet visible, add its filename to
   `recoveryFilters`;
4. each recovery filter must match a package anywhere in the release manifest or the run fails;
5. verification still requires every manifest package, including recovered packages.

Recovery filters are release-scoped. A pack-only filter remains valid while the manifest
set is processed, and a manifest-only filter remains valid while the pack set is processed.
Within each set, only matching files are withheld.

Inside an existing run, retry a failed `publish` job with **Rerun failed jobs**. Do not rerun
the whole stage: Azure DevOps pipeline artifact names are immutable within a run, so its
prepared artifact name already exists. The retried publish job downloads that approved
artifact and performs the monotonic NuGet.org re-prune again.

### Known recovery limit

Staging requires every selected package file to be downloadable by `darc gather-drop`.
The system does not synthesize package identities solely from BAR asset metadata when a
package file is unavailable. A repeated non-workload release whose internal package asset
has expired therefore fails during gathering or staging, even if that identity already
exists on NuGet.org.

Supporting that case requires the resolve/stage flow to carry selected BAR asset identities and the
stage step to reconcile downloaded files against them. BAR versions must be normalized with
`NuGetVersion` before any NuGet.org lookup.

## Build and dependency management

The repository follows Arcade conventions:

- `global.json` pins the .NET and Arcade SDKs;
- `eng/Versions.props` owns package versions;
- `eng/Version.Details.xml` records flowed dependencies with URI and SHA;
- `eng/common/**` is Arcade-provided;
- `build.sh` and `build.cmd` delegate to Arcade;
- `NuGet.config` clears inherited sources and uses the configured dnceng feeds.

Tracked release dependencies include:

- `Microsoft.DotNet.ProductConstructionService.Client`;
- `NuGet.Packaging`;
- `NuGet.Protocol`;
- `NuGet.Versioning`;
- `System.CommandLine`.

## Operational configuration

The Azure DevOps pipeline definition MUST:

- use the internal Azure Repos mirror as source;
- point to `eng/pipelines/ci-official-release.yml`;
- have access to `Darc: Maestro Production`;
- have access to `nuget.org (dotnetframework)`;
- use the configured internal 1ES Windows pool;
- protect production service connections with Azure DevOps checks outside YAML;
- not use a GitHub-backed source definition that can independently report build status or
  add source labels to GitHub.

The production job may report `partiallySucceeded` on a non-production branch because of
non-blocking 1ES branch/source validation. `release verify` exit status is the authoritative
package-publication result.

## Required first-run validation

Before using the system for a production push:

1. queue **Preview the package release without making changes**;
2. confirm matching preflight jobs query NuGet.org and produce pruned artifacts;
3. complete the package-set approval and confirm the production release job runs;
4. confirm no 1ES push, verification, or BAR mutation step exists;
5. confirm the run is tagged `DRY-RUN`;
6. confirm no NuGet.org service connection is requested;
7. inspect the `Release` and pruned package-set artifacts;
8. use a workload repository for at least one dry run, proving both `ReleasePacks` and
   `ReleaseManifests` directories are produced;
9. confirm the missing-build behavior of the typed PCS client against a real nonexistent
   BAR ID;
10. expand the pipeline at the exact commit used for the first production run and confirm
   parameter sentinels, prepared-artifact wiring, and topology conditions;
11. use a completed release for a publish-enabled rehearsal so the 1ES step is present while
    pruning leaves no packages to upload.

The missing-build check remains the only known unobserved PCS client behavior: tests model
a missing BAR build as `RestApiException` with HTTP 404, but this has not been observed
against the live service.

## Test-enforced invariants

Tests MUST fail when:

- the pipeline repository dropdown differs from policy;
- workload and non-workload stages do not use the classification loaded from
  `release-manifest.json`;
- a remote publish operation is not structurally nested beneath the derived publish boolean;
- package-set local validation is nested beneath the derived publish boolean;
- BAR mutation is not structurally nested beneath the derived promotion boolean;
- queue-time parameters appear in executable PowerShell text;
- the NuGet.org task, connection, or known push mechanisms appear in the root dry-run graph;
- Darc is not obtained through Arcade in normal jobs or appears in package release artifacts;
- `ManualValidation@0` is not an agentless predecessor of the mutating job;
- pack/manifests stage ordering is broken;
- the tool references NuGet's push API;
- the tool references `System.Diagnostics.Process`;
- the manifest, package, or executable hash chain is inconsistent;
- an unexpected or changed `.nupkg` enters the publish set.
