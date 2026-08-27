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

## Layout

```
src/DotNet.Release.Core/        pure policy, validation, planning — no I/O
src/DotNet.Release.Maestro/     read-only BAR client adapter (typed client)
src/DotNet.Release.NuGet/       read-only feed + nupkg adapters         (not yet implemented)
src/DotNet.Release.Cli/         verbs and argument parsing              (not yet implemented)
tests/DotNet.Release.Core.Tests/
tests/DotNet.Release.Maestro.Tests/
config/repositories.json        declarative release policy
templates/                      thin ADO template consumers extend      (not yet implemented)
docs/design.md
```
