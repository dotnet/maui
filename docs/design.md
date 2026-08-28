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

`publishPackages` defaults to `false`.

For a dry run, the expanded pipeline MUST NOT contain:

- `1ES.PublishNuget@1`;
- the `nuget.org (dotnetframework)` service connection;
- `release verify`;
- `darc add-build-to-channel`;
- a manual publish or promotion gate.

The matching package-set stage remains present. Its `preflight` job downloads the `Release`
artifact, verifies plan/tool integrity, queries NuGet.org read-only, prunes already-published
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
- upload plans, packages, markers, and the release executable as Azure DevOps artifacts;
- produce logs, build tags, audit records, and mandatory 1ES compliance output.

The 1ES pipeline template is external code. Component-governance tooling may perform
public-registry metadata lookups. If policy requires a literal network-level prohibition on
all HTTP traffic to NuGet.org, agent egress policy MUST enforce it in addition to the
pipeline's exclusion of all NuGet.org release operations.

### Compile-time exclusion

Mutating publish jobs and the promotion stage are selected with Azure DevOps template
expressions:

```yaml
- ${{ if eq(parameters.publishPackages, true) }}:
```

This is a compile-time barrier around mutating release jobs. When false, approval,
publishing, and verification do not exist in the expanded YAML.

The package-set stages themselves always exist so a dry run validates stage selection,
artifact download, SDK setup, tool execution, and local integrity.

Workload-set promotion is compile-time excluded unless both operator opt-ins are true:

```yaml
and(
  eq(parameters.promoteWorkloadSet, true),
  eq(parameters.publishPackages, true))
```

Its runtime condition additionally requires the `IsWorkload` output emitted by
`release plan`.

### Queue-parameter safety

Queue-time parameters are operator-controlled and MUST NOT be interpolated into executable
PowerShell text.

Every queue-time string enters scripts through an `env:` binding and is read through
`$env:...`. The pipeline validates:

- `commitHash` as exactly 40 hexadecimal characters;
- `barBuildId`, when present, as a positive decimal integer;
- GitHub owner and repository names against a restricted character set.

This validation runs before the release tool authenticates.

### Source-control safety

The pipeline source MUST be the internal Azure Repos mirror, not a GitHub service
connection. `checkout: self` uses `persistCredentials: false`.

The released repository is never checked out. Its GitHub identity is data used to query BAR.
No checked-in step invokes `git push`, `gh`, Octokit, or the GitHub write API.

## Repository layout

```
eng/pipelines/ci-official-release.yml  Azure DevOps entry point
eng/pipelines/stages/publish-set.yml   internal reusable publish-stage definition

src/DotNet.Release/
  Program.cs                           CLI entry point
  Commands/                            handlers and shared artifact validation
  ConsoleReporting.cs                  command output and exit codes
  Policy/                              pure release decisions
  Model/                               plan, policy, marker, and result types
  Adapters/                            read-only interfaces and implementations
  Pipeline/                            Azure DevOps logging-command formatting

tests/DotNet.Release.Tests/            unit, contract, architecture, and pipeline tests
config/repositories.json               release policy
```

The implementation is one executable project. Interfaces are retained where they make
remote reads testable:

- `IBuildRegistry`;
- `IPackageAvailabilityProbe`.

NuGet availability has one additional internal seam:

- `IPackageExistenceChecker` models the per-identity NuGet protocol call;
- `IPackageAvailabilityProbe` models the batched operation consumed by prune and verify.

The distinction is intentional. Probe tests substitute the per-identity checker to validate
deduplication, concurrency, and exception behavior. Verb tests substitute the batch probe to
model poll-level availability and transient failures.

No dependency-injection container is required. The CLI composition root constructs the
production adapters directly.

## Pipeline inputs

| Parameter | Required | Default | Purpose |
|---|---:|---|---|
| `ghOwner` | yes | `dotnet` | GitHub repository owner |
| `ghRepo` | yes | `select-repository` | Fail-closed sentinel that the operator replaces with an enabled repository |
| `commitHash` | yes | none | Exact full commit registered in BAR |
| `barBuildId` | no | `skip` | Direct BAR lookup when commit lookup is insufficient |
| `publishPackages` | yes | `false` | Include gated NuGet.org jobs in matching set stages |
| `promoteWorkloadSet` | yes | `false` | Emit the gated BAR channel-promotion stage |
| `includeFilters` | no | `skip` | Semicolon-separated package filename globs |
| `excludeFilters` | no | `skip` | Semicolon-separated package filename globs |
| `recoveryFilters` | no | `skip` | Packages already submitted by this release |
| `pool` | infrastructure | internal Windows pool | Agent pool definition |

Azure DevOps runtime parameters cannot be optional. An omitted parameter either uses a
default or, for an allowed-values list, silently selects its first value. Explicit sentinels
make that platform behavior visible and safe: `select-repository` fails repository policy,
and `skip` is normalized to no optional argument before the CLI is invoked.

Runs are tagged `DRY-RUN` or `PUBLISH`.

If `promoteWorkloadSet=true` and `publishPackages=false`, promotion is omitted and the run
logs a warning.

## Pipeline topology

### Prepare stage

The prepare stage always runs, including on a dry run:

```
checkout release-system source
  -> install pinned .NET SDK
  -> install and pin Release/_darc
  -> build and pin Release/_tool
  -> release plan
  -> darc gather-drop
  -> release stage
  -> pin release-plan.json
  -> publish Azure DevOps artifacts
```

The gathered drop is placed under `Agent.TempDirectory`, not the artifact staging root.

The prepare job builds the framework-dependent tool once with `dotnet build`, before any
task acquires the Maestro production identity. The complete build output is hashed and
stored in the immutable `Release` artifact. Publish jobs verify and execute that exact tool
directory after approval without restoring or rebuilding it.

### Workload-set promotion stage

When requested for a workload repository:

1. an agentless `ManualValidation@0` job displays the plan for review;
2. after approval, `darc add-build-to-channel --skip-assets-publishing` adds the BAR build
   to the workload-set channel recorded in the plan.

Promotion is excluded from dry runs.

When both promotion and package publishing are requested, `publish_packs` depends on
successful promotion. Promotion therefore completes before the first NuGet.org approval;
manifests remain ordered after verified packs.

### Package-set stages

Every package set uses the same internal stage definition:

```
preflight
  -> download Release artifact
  -> verify plan and tool hashes
  -> release prune-published
  -> publish exact pruned artifact for review

when publishPackages=true:
  ManualValidation@0
    -> refresh prune
    -> 1ES.PublishNuget@1
    -> release verify
```

The preflight job proves that a normal agent job can download the artifact, install the SDK,
load the approved tool, query NuGet.org, interpret the plan, and validate the exact pending
set before approval. The production `releaseJob` is a different 1ES execution mode and is
validated only by a publish-enabled rehearsal. The publish job repeats the prune after
approval; the approved set can only shrink.

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
with `REPO_NOT_ALLOWED`.

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

`release plan` emits `IsWorkload` as an Azure DevOps output variable. When publishing is
enabled, the pipeline contains the workload and non-workload stage shapes, and each stage
uses a runtime condition against that output:

- `publish_packs` and `publish_manifests` require `IsWorkload=true`;
- `publish_packages` requires `IsWorkload=false`;
- `promote_workload_set` requires `IsWorkload=true`.

This keeps stage ordering explicit without duplicating repository classification in YAML.

## BAR resolution

`release plan` accepts either:

```text
repository + exact commit
```

or:

```text
BAR build ID + exact commit
```

Every resolution path verifies:

- exactly one build was found;
- the build's commit equals the full requested commit;
- the build belongs to the requested repository;
- any required channel is present exactly once.

Resolution by BAR ID does not imply trust; repository and commit checks still run.

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
convention fails with `BAR_MIRROR_NAME_INVALID`.

The plan records whether identity came from `GitHubRepository` or
`AzureDevOpsMirrorConvention`.

### Typed BAR client

The tool uses `Microsoft.DotNet.ProductConstructionService.Client`:

- `IBuilds.GetBuildAsync` for BAR ID lookup;
- `IBuilds.ListBuildsAsync` for repository and commit lookup.

`ListBuildsAsync` MUST use `loadCollections: true`, because the service omits channel data
otherwise.

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
by invoking Arcade's `eng/common/darc-init.ps1`. Without `-darcVersion`, the initializer
asks Maestro's Darc-version endpoint for the matching supported version and installs it
from `dotnet-eng`; no Darc version is duplicated in this repository's YAML or dependency
files. The pipeline uses the script's supported `-toolpath` option to install directly into
the immutable `Release` artifact before acquiring the Maestro identity. `gather-drop`
executes that exact path to read BAR and repository metadata and download package assets.
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

A non-workload release produces one package set and rejects any workload manifest with
`MANIFEST_IN_NON_WORKLOAD`.

## Release plan contract

`release-plan.json` is the immutable contract crossing the prepare/publish job boundary.

```jsonc
{
  "schemaVersion": 1,
  "toolVersion": "1.0.0",
  "createdUtc": "2026-08-28T00:00:00Z",
  "source": {
    "schemaVersion": 1,
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

Unknown schema versions fail with `PLAN_SCHEMA_INVALID`.

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
  release-plan.json
  _darc/
    darc.exe
    <runtime dependencies>
  _tool/
    release.dll
    release.deps.json
    <runtime dependencies>
  ReleasePackages/
    *.nupkg
    release-set.json
```

Workload releases contain `ReleasePacks/` and `ReleaseManifests/` instead of
`ReleasePackages/`. There is one authoritative `release-plan.json` at the artifact root.

Each matching preflight publishes a prepared artifact containing the pinned root plan, the
pinned tool directory, and only that stage's pruned package-set directory. The approval
artifact therefore cannot contain an unpruned sibling workload set.

### Set marker

`release-set.json` declares:

- schema version;
- set display name;
- artifact name;
- BAR build ID;
- commit.

`prune-published` and `verify` require `--set` and check the marker before using the directory. A
valid but wrong artifact therefore fails with `PACKAGE_SET_MISMATCH` instead of a misleading
missing-file error.

The marker does not source package identities. A missing, changed, or swapped marker can
only fail the release; it cannot cause a different package to publish.

## Integrity chain

The prepare stage:

1. installs the Arcade/Maestro-selected Darc CLI into `Release/_darc`;
2. computes a deterministic hash over every file and relative path in `_darc` as `DarcHash`;
3. builds the C# project into `Release/_tool`;
4. computes the same directory hash over `_tool` as `ToolHash`;
5. executes `_tool/release.dll` for `plan` and `stage`;
6. records every package SHA-256 in the plan;
7. writes one authoritative plan into the release artifact root;
8. computes the plan hash independently with `Get-FileHash`;
9. exports `DarcHash`, `ToolHash`, and `ReleasePlanHash`.

The publish job:

1. downloads the stage's exact prepared artifact;
2. compares raw plan bytes with `ReleasePlanHash`;
3. recomputes the complete `_tool` directory hash and compares it with `ToolHash`;
4. executes that DLL for `prune-published`;
5. validates all pending package hashes before invoking 1ES;
6. executes the same DLL for `verify` with the same expected plan hash.

Unexpected `.nupkg` files, missing pending files, and changed package bytes fail closed.
Companion JSON and executable files are excluded by the `.nupkg` enumeration.

## Pruning and publication

`release prune-published` queries NuGet.org read-only using `FindPackageByIdResource`.

For each planned package it records one disposition:

| Disposition | Meaning | File after pruning |
|---|---|---|
| `Pending` | Not visible on NuGet.org | present |
| `AlreadyPublished` | Exact ID/version is visible | removed |
| `PreviouslyAttempted` | Matched an operator recovery filter | removed |

The immutable plan is not rewritten. Dispositions are written to
`release-prune.json`.

Before deleting anything, all artifact and package names are revalidated as a single
relative path component.

After pruning, the invariant is:

```text
file is present <=> disposition is Pending
```

`NuGetPackagesToPublish=false` skips `1ES.PublishNuget@1` when nothing remains.

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
4. each recovery filter must match a package anywhere in the release plan or the run fails with
   `FILTER_UNMATCHED`;
5. verification still requires every planned package, including recovered packages.

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

Supporting that case requires the plan step to carry selected BAR asset identities and the
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

1. queue a dry run with `publishPackages=false`;
2. confirm matching preflight jobs query NuGet.org and produce pruned artifacts;
3. confirm no approval, push, verification, or BAR mutation job exists;
4. confirm the run is tagged `DRY-RUN`;
5. confirm no NuGet.org service connection is requested;
6. inspect the `Release` and pruned package-set artifacts;
7. use a workload repository for at least one dry run, proving both `ReleasePacks` and
   `ReleaseManifests` directories are produced;
8. confirm the missing-build behavior of the typed PCS client against a real nonexistent
   BAR ID;
9. expand the pipeline at the exact commit used for the first production run and confirm
   parameter sentinels, prepared-artifact wiring, and topology conditions;
10. use a completed release for a publish-enabled rehearsal so the production `releaseJob`
    runs while pruning leaves no packages to upload.

The missing-build check remains the only known unobserved PCS client behavior: tests model
a missing BAR build as `RestApiException` with HTTP 404, but this has not been observed
against the live service.

## Test-enforced invariants

Tests MUST fail when:

- the pipeline repository dropdown differs from policy;
- workload and non-workload stages do not use the `release plan` classification output;
- a remote publish operation is not structurally nested beneath `publishPackages=true`;
- package-set local validation is nested beneath `publishPackages=true`;
- BAR promotion is not nested beneath both publish and promotion opt-ins;
- queue-time parameters appear in executable PowerShell text;
- the NuGet.org task, connection, or known push mechanisms appear in the root dry-run graph;
- Darc is not installed, pinned, carried, hashed, and invoked by exact path;
- `ManualValidation@0` is not an agentless predecessor of the mutating job;
- pack/manifests stage ordering is broken;
- the tool references NuGet's push API;
- the tool references `System.Diagnostics.Process`;
- the plan, marker, package, or executable hash chain is inconsistent;
- a future plan schema is read by an older tool;
- a package-set marker does not match its directory;
- an unexpected or changed `.nupkg` enters the publish set.
