# .NET release tooling

A shared, testable tool for releasing .NET packages to NuGet.org from a Build Asset
Registry (BAR) build.

> **Status: prototype.** This branch is a greenfield staging ground intended to graduate
> into a standalone team-wide "releasing" repository. It contains release tooling and
> nothing else.

Read [`docs/design.md`](docs/design.md) first. It explains the verb model, the plan-file
contract, the policy schema, and — importantly — *why* the pipeline is shaped the way it is.

## What this replaces

The release logic currently carried by `dotnet/maui`:

| Today | Here |
|---|---|
| A 304-line inline PowerShell script in a YAML task | Four verbs with one responsibility each |
| `eng/scripts/nuget_release_packages.ps1`, copied into the artifact and hash-checked in twelve places | One hashed plan file that transitively pins the tool |
| `expected-packages.json` + `release-audit.json` + four `##vso` variables | One `release-plan.json` |
| Repository policy as PowerShell literals inside YAML | `config/repositories.json`, reviewable and tested |
| Version normalization by substring of the file name | `NuGetVersion.ToNormalizedString()`, plus a check that rejects a disagreeing reader |
| Hand-rolled zip + XML + XPath nuspec reading | `PackageArchiveReader` |
| Hand-rolled HTTP HEAD + retry loop | `FindPackageByIdResource` |

## Safety properties

These are structural, not conventional, and there are tests that fail if they stop being true.

- **The tool never pushes anything.** `1ES.PublishNuget@1` performs every upload — it is a
  compliance requirement. There is no push verb, no `--push` flag, and no upload code path.
  The tool never holds a NuGet.org credential.
- **The tool never starts a subprocess.** No `Process.Start`, no darc shell-out, no stdout
  scraping. `darc gather-drop` and `darc add-build-to-channel` are plain YAML steps.
- **The tool mutates nothing outside its own output directory.** It reads BAR, reads a drop
  directory, queries NuGet.org read-only, and writes a plan.
- **Everything fails closed.** Unknown repository, wrong commit, missing channel, duplicate
  package, ambiguous build — all are errors with stable codes.

## Verbs

```
release plan   --config config/repositories.json --repo <owner/name> --commit <sha> [--bar-id N] --out ./stage
release stage  --plan ./stage/plan.json --drop <dropPath> [--include '<glob>;…'] [--exclude '<glob>;…'] --out ./stage
release filter --plan ./stage/release-plan.json [--skip '<glob>;…']
release verify --plan ./stage/release-plan.json --max-duration-minutes 30
```

`plan` and `stage` bracket the `darc gather-drop` step; `filter` and `verify` bracket the
`1ES.PublishNuget@1` step.

## Building

Standard Arcade:

```bash
./build.sh --restore --build --test      # Linux/macOS
build.cmd -restore -build -test          # Windows
```

Or directly:

```bash
dotnet build DotNet.Release.slnx
dotnet test DotNet.Release.slnx
```

## Before first production use

Two things must be confirmed by whoever first runs this against the real services. They are
recorded here rather than only in the design doc so that a PR description can pick them up.

1. **The missing-build path is unobserved.** That a lookup for a non-existent BAR ID
   surfaces as `RestApiException` with `Response.Status == 404` is inferred from the typed
   client's exception model and covered by a faked exception. No live BAR call was permitted
   during development, so it has never been seen against the real service. Everything else
   about the client surface was verified by reflection against the shipped package, and the
   null-`gitHubRepository` case is confirmed from production build 328857.

2. **A production release job on a non-production branch reports `partiallySucceeded`.**
   Two non-blocking 1ES checks (`Branch Validation`, `Validate Source Build`) fail for any
   `templateContext: type: releaseJob, isProduction: true` job that is not on `main`,
   `release/*`, `internal/release/*` or a `netN.0` branch. This is expected during rehearsal
   and is **not** a defect. Green is not the success signal — `release verify`'s own exit
   code is.

## Layout

```
src/DotNet.Release.Core/        pure policy, validation, planning — no I/O
src/DotNet.Release.Maestro/     read-only BAR client adapter (typed PCS client)
src/DotNet.Release.NuGet/       read-only feed queries + nupkg reading
src/DotNet.Release.Cli/         the four verbs (System.CommandLine)
tests/                          one test project per source project, zero network
config/repositories.json        declarative release policy
templates/release.yml           thin ADO template consumers extend
templates/publish-set.yml       one gated publish of one package set
docs/design.md
```
