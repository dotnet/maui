# DevSkim banned-API analysis

The `DevSkim` stage of the `ci.yml` and `ci-official.yml` pipelines scans the `src` and `eng` trees with
[DevSkim](https://github.com/microsoft/DevSkim) and fails the build if it reports a banned API. It supports
the Microsoft.Security.SystemsADM.10082 requirement that code analysis tools must not detect banned APIs.
Running the scanner doesn't establish compliance by itself.

The stage (`eng/pipelines/arcade/stage-devskim.yml`) is independent of the build stages and takes a few minutes
on a Linux agent. It installs DevSkim 1.0.100 from the repository's `NuGet.config`, because the official agents
can't reach nuget.org, and then runs `Invoke-DevSkimScan.ps1`.

## What fails the stage

- Any result tagged `API.DangerousAPI.BannedFunction`, at any severity. The general rule (`DS154189`) is only
  Moderate. DevSkim runs with inline suppressions disabled, so a suppression comment doesn't hide a finding.
  There are no baselines and no exclusions for individual APIs.
- A scan that can't be trusted: an unexpected DevSkim version, a missing options file, DevSkim not reporting the
  banned `strcpy` calls in canary files that the script writes, a source tree where DevSkim analyzed no files,
  or a missing or malformed report.

Findings from other rules stay in the reports for triage. DevSkim also logs `Failed to parse ... as a YML
document` for Azure Pipelines templates that repeat a key such as `${{ else }}`; that doesn't affect the result.

DevSkim applies its banned-function rules only to C, C++ and Objective-C files (`.c`, `.h`, `.cpp`, `.hpp`,
`.cc`, `.hh`, `.cxx`, `.hxx`, `.inl` and `.m`). The repository has no such files today, so the stage guards
against new ones. It doesn't cover banned Java or JavaScript APIs.

## What is scanned

The `src` and `eng` trees, minus the globs in `devskim-options.json`:

| Glob                          | Reason                                      |
|-------------------------------|---------------------------------------------|
| `**/.git`, `**/.git/**`       | Git metadata.                               |
| `**/bin/**`, `**/obj/**`      | Build output.                               |
| `**/tests/**`, `**/test/**`   | Test projects, which aren't shipped.        |
| `**/src/TestUtils/**`         | Test infrastructure, which isn't shipped.   |
| `**/samples/**`               | Sample apps, which aren't shipped.          |
| `**/wwwroot/lib/bootstrap/**` | Vendored Bootstrap in the Blazor templates. |

Everything else is in scope, including Arcade's `eng/common`. The other top-level folders (`docs`, `loc`,
`grounding`, `Assets`, `.github`) and the build scripts in the repository root aren't scanned.

DevSkim matches the globs against absolute paths. If the checkout is below a directory with one of these names,
for example `tests`, the globs exclude everything and the script fails because no files were analyzed.

## Results

The stage publishes the `DevSkim-<attempt>` artifact, even when it fails. It has a SARIF report and a log for
each scan (`src`, `eng` and `canary`) and the output of the tool installation.

## Run it locally

You need PowerShell 7 and the .NET 10 SDK. Install the tool outside the repository, so that `dotnet` ignores
its `global.json`, and run the script from the repository root:

```bash
repository="$PWD"
(cd /tmp && dotnet tool install Microsoft.CST.DevSkim.CLI --version 1.0.100 \
  --tool-path /tmp/devskim --configfile "$repository/NuGet.config")
pwsh eng/automation/devskim/Invoke-DevSkimScan.ps1 -DevSkim /tmp/devskim/devskim -OutputDirectory /tmp/devskim-reports
```

The script's tests need Pester 5: `Invoke-Pester eng/automation/devskim/DevSkim.Tests.ps1`.

## Update DevSkim

1. Change the version in `stage-devskim.yml`.
2. Check the new version against what the script relies on: the command-line flags, the rule tags in
   `properties.tags` of each SARIF result, and the `Files analyzed` line at the end of the log that
   `--console-verbosity Debug` produces.
3. Update `$validatedVersion` in `Invoke-DevSkimScan.ps1`, run the tests and run the script.
