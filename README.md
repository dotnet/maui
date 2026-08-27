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
   | **BAR build ID** | Only when BAR has no GitHub URL for the build — the run will tell you if so. Otherwise leave as `skip`. |
   | **PUBLISH to NuGet.org** | **Off by default.** Leave it off for a dry run. |
   | **Promote to workload-set channel** | Workload repositories only. |
   | Include / exclude filters | Optional package selection. |
   | Recovery filters | Only when resuming a partially-completed release. |

3. The run prepares and validates everything, then **waits at a manual approval gate**.
4. Review `release-plan.json` in the published artifact — it lists every package that will be
   pushed, with its exact version and content hash.
5. Approve. The packages are pushed and then verified on NuGet.org.

**The default run publishes nothing.** `PUBLISH to NuGet.org` is off, and on a dry run the
publish stages are not merely skipped — they do not exist in the expanded pipeline.

### Adding a repository

Add it to `config/repositories.json` **and** to the `ghRepo` dropdown in
`eng/pipelines/release.yml` (and to the `isWorkload` list if it is a workload repository).
A test fails the build if those disagree, and the tool fails the release if they disagree at
run time.

## How it works

```
prepare_release
  build the tool  →  release plan  →  darc gather-drop  →  release stage
      ↓ artifacts + pinned plan hash
publish_* (only when publishing)
  approval  (agentless, pool: server)
  publish   (checkout: none)
     release filter  →  1ES.PublishNuget@1  →  release verify
```

Workload repositories publish **packs first, then manifests**, as two separately gated
stages, because manifests reference packs and NuGet.org packages are immutable.

## Safety properties

Structural, not conventional — there are tests that fail if they stop being true.

- **The tool never pushes anything.** `1ES.PublishNuget@1` performs every upload; it is a
  compliance requirement. There is no push verb, no `--push` flag, and no upload code path in
  any shipping assembly. The tool never holds a NuGet.org credential.
- **The tool never starts a subprocess.** `darc` is invoked from pipeline YAML, where Azure
  DevOps owns the exit code and the log.
- **The tool mutates nothing outside its own output directory.** It reads BAR, reads a drop
  directory, queries NuGet.org read-only, and writes a plan.
- **Everything fails closed.** Unknown repository, wrong commit, missing channel, duplicate
  package, workload misclassification, tampered artifact — all are errors with stable codes.

## What this replaces

The release logic previously carried inside `dotnet/maui`:

| Before | Now |
|---|---|
| A 304-line inline PowerShell script in a YAML task | Four verbs with one responsibility each |
| A helper script copied into the artifact and hash-checked in twelve places | One hashed plan that transitively pins the tool |
| `expected-packages.json` + `release-audit.json` + four `##vso` variables | One `release-plan.json` |
| Repository policy as PowerShell literals inside YAML | `config/repositories.json`, reviewable and tested |
| Version normalization by substring of the file name | `NuGetVersion.ToNormalizedString()`, plus a check that rejects a disagreeing reader |
| Hand-rolled zip + XML + XPath nuspec reading | `PackageArchiveReader` |
| Hand-rolled HTTP HEAD + retry loop | `FindPackageByIdResource` |
| Each repository maintaining its own copy | One shared pipeline, parameterised |

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

Point a new Azure DevOps pipeline at `eng/pipelines/release.yml`. It needs:

- the `Darc: Maestro Production` service connection (BAR reads, `gather-drop`, channel promotion);
- the `nuget.org (dotnetframework)` service connection (the 1ES publish task);
- an environment or service-connection approval check on the production connection, configured
  outside this repository's YAML;
- to run in the internal project, where those connections and the 1ES production template are
  available.

## Layout

```
eng/pipelines/release.yml       THE pipeline. Hook this up.
eng/pipelines/stages/           internal stage templates used by it
src/DotNet.Release.Core/        pure policy, validation, planning — no I/O
src/DotNet.Release.Maestro/     read-only BAR client adapter (typed PCS client)
src/DotNet.Release.NuGet/       read-only feed queries + nupkg reading
src/DotNet.Release.Cli/         the four verbs (System.CommandLine)
tests/                          one test project per source project, zero network
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
release plan   --config config/repositories.json --repo <owner/name> --commit <sha> [--bar-id N] [--expect-workload <bool>] --out ./stage
release stage  --plan ./stage/plan.json --drop <dropPath> --tool <publishedTool> [--include '…'] [--exclude '…'] --out ./stage
release filter --plan <release-plan.json> --set <artifactName> [--skip '…'] [--expected-plan-hash <sha256>]
release verify --plan <release-plan.json> --set <artifactName> [--expected-plan-hash <sha256>] [--max-duration-minutes 30]
```
