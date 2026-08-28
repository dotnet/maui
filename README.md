# .NET package release system

**This repository is the release system.** It is not a library, and nothing consumes it as a
template. One Azure DevOps pipeline is hooked up to it once, and everyone triggers *that*
pipeline with parameters describing what to release.

Releasing repositories do not change. They are named by parameter.

> **Status: prototype.** Intended to graduate into a standalone team-wide "releasing"
> repository. It contains release tooling and nothing else.

## Releasing something

1. Open the release pipeline in Azure DevOps and press **Run pipeline**.
2. Fill in:

   | Parameter | Meaning |
   |---|---|
   | **GitHub owner** / **GitHub repository** | What to release. The dropdown is exactly the set enabled in `config/repositories.json`. |
   | **Commit** | The full SHA, as registered in BAR. Required. |
   | **BAR build ID** | Only when BAR has no GitHub URL for the build. Otherwise leave empty. |
   | **PUBLISH to NuGet.org** | **Off by default.** Leave it off for a dry run. |
   | **Promote to workload-set channel** | Workload repositories only. |
   | Include / exclude filters | Optional package selection. |
   | Recovery filters | Only when resuming a partially-completed release. |

The repository and commit have no defaults. Azure DevOps requires the operator to choose
both for every run.

3. The run prepares the release and validates the matching package-set jobs.
4. Review `release-plan.json` in the published artifact — it lists every package that will be
   pushed, with its exact version and content hash.
5. On a publish run, approve the matching gate. The packages are pushed and verified.

**The default run publishes nothing.** `PUBLISH to NuGet.org` is off, and on a dry run the
package-set stages download and validate the artifact, while their approval, NuGet.org
filter, push, and verification jobs do not exist in the expanded pipeline.

### Adding a repository

Add it to `config/repositories.json` and to the `ghRepo` dropdown in
`eng/pipelines/ci-official-release.yml`. Workload classification comes only from the config.
A test fails the build if the dropdown and policy disagree.

## How it works

```
prepare_release
  build the tool  →  release plan  →  darc gather-drop  →  release stage
      ↓ artifacts + pinned plan hash
matching package-set stage
  validate artifact download, plan, marker, package hashes, and tool
  if publishing:
    approval  →  release filter  →  1ES.PublishNuget@1  →  release verify
```

Workload repositories publish **packs first, then manifests**, as two separately gated
stages, because manifests reference packs and NuGet.org packages are immutable.

## Dry-run boundary

The dry run is not "no filesystem mutation" and not "no network." It runs on a disposable
agent, reads production BAR, downloads the package drop, and uploads review artifacts and
logs to Azure DevOps.

The boundary is narrower and more important:

- **A dry run never contacts NuGet.org.** The matching package-set stage executes
  `release validate`, which is local-only. `release filter`, `release verify`, the
  `nuget.org (dotnetframework)` service connection, and `1ES.PublishNuget@1` are nested
  inside the internal template's `${{ if eq(parameters.publishPackages, true) }}` block and
  do not exist in the expanded dry-run YAML.
- **A dry run does not mutate BAR or GitHub.** `darc add-build-to-channel` is also compile-time
  excluded unless `publishPackages` and `promoteWorkloadSet` are both true, and it has its own
  manual gate. Preparation uses read-only BAR methods and `darc gather-drop`.
- **The release tool itself never pushes anything.** There is no push verb, no `--push` flag,
  and no upload code path. The load-bearing property is that **the tool is never given a
  NuGet.org credential**: Azure DevOps injects the service-connection secret only into
  `1ES.PublishNuget@1`. The architecture test is a useful tripwire, not the guarantee.
- **The tool never shells out to `darc`.** `darc` is invoked from pipeline YAML, where Azure
  DevOps owns the exit code and the log. (Authentication does start `az` transitively via
  `Azure.Identity`; the codebase itself starts no process.)
- **Everything fails closed.** Unknown repository, wrong commit, missing channel, duplicate
  package, or tampered artifact — all are errors with stable codes.

## Before first production use

Two things must be confirmed by whoever first runs this against the real services.

1. **The missing-build path is unobserved.** That a lookup for a non-existent BAR ID surfaces
   as `RestApiException` with `Response.Status == 404` is inferred from the typed client's
   exception model and covered by a faked exception. No live BAR call was permitted during
   development. Everything else about the client surface was verified by reflection against
   the shipped package, and the null-`gitHubRepository` case is confirmed from production
   build 328857.

2. **A production release job on a non-production branch reports `partiallySucceeded`.** Two
   non-blocking 1ES checks (`Branch Validation`, `Validate Source Build`) fail for any
   `templateContext: type: releaseJob, isProduction: true` job that is not on a production
   branch. This is expected during rehearsal and is **not** a defect. Green is not the success
   signal — `release verify`'s own exit code is.

## Hooking up the pipeline

The Azure DevOps pipeline entry point is
`eng/pipelines/ci-official-release.yml`. The pipeline definition needs:

- to use the **Azure Repos mirror** (`dnceng/internal/dotnet-maui`) as its source, not a
  GitHub service connection. This prevents pipeline-definition features such as "Report build
  status" or source labeling from writing a Check, status, or tag to GitHub independently of
  this YAML;
- the `Darc: Maestro Production` service connection (BAR reads, `gather-drop`, channel promotion);
- the `nuget.org (dotnetframework)` service connection (the 1ES publish task);
- an environment or service-connection approval check on the production connection, configured
  outside this repository's YAML;
- to run in the internal project, where those connections and the 1ES production template are
  available.

The checked-in dry-run graph itself contains no NuGet.org task, client, or service
connection. The mandated 1ES compliance template is external code and may perform public
registry metadata lookups for component governance; if "no NuGet.org contact" means a
literal network-level prohibition including compliance tooling, enforce that with agent
egress policy in addition to this pipeline's structural exclusion.

## Layout

```
eng/pipelines/ci-official-release.yml  THE pipeline. Existing AzDO entry point.
eng/pipelines/stages/           internal stage templates used by it
src/DotNet.Release/             the tool - one project
  Program.cs / Verbs.cs         the CLI entry point and commands
  Policy/                       pure decisions over plain data - no I/O
  Model/                        the data those decisions operate on
  Adapters/                     read-only BAR/NuGet interfaces and implementations
tests/DotNet.Release.Tests/     one test project, zero network
config/repositories.json        declarative release policy
docs/design.md                  full rationale
```

## Building

Standard Arcade:

```bash
./build.sh --restore --build --test      # Linux/macOS
build.cmd -restore -build -test          # Windows
```

## The verbs

You will not normally run these by hand — the pipeline does. They are documented because the
audit trail refers to them.

```
release plan   --config config/repositories.json --repo <owner/name> --commit <sha> [--bar-id N] --out ./stage
release stage  --plan ./stage/plan.json --drop <dropPath> [--include '…'] [--exclude '…'] --out ./stage
release validate --plan <release-plan.json> --stage <releaseArtifact> --set <setDirectory> --expected-plan-hash <sha256>
release filter --plan <release-plan.json> --set <setDirectory> [--recovery-filters '…'] [--expected-plan-hash <sha256>]
release verify --plan <release-plan.json> --set <setDirectory> [--expected-plan-hash <sha256>] [--max-duration-minutes 30]
```
