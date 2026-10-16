# Agent Skills in the Controls NuGet package

`Microsoft.Maui.Controls` can carry versioned, text-only upgrade guidance at
`skills/<skill-name>/SKILL.md` inside the NuGet archive. Supporting Markdown lives
below the same skill directory. This is guidance for an agent, not an MSBuild
target: restoring, building, or upgrading a package does not execute or install a
skill.

Controls is the direct package used by MAUI application templates. Do not duplicate
the same skill in Controls.Core, XAML, build-task, workload, or transitive packages.
Do not put consumer guidance in this repository's `.github/skills`, which contains
maintainer automation rather than the files packed for applications.

## Discovery and activation

An agent can inspect skills from the exact restored Controls package or from its
`.nupkg`. With `dotnet-inspect` 0.26.0, the tested commands are:

```shell
dotnet-inspect project MyApp.csproj -S Skills --count
dotnet-inspect project MyApp.csproj -S Skills --print --row 1 --raw
dotnet-inspect package Microsoft.Maui.Controls@VERSION -S 'Package skill files' --paths
```

Use the installed tool's help to select a returned row; do not assume row 1 is
always the Controls skill. Package inspection does not itself install guidance
into the repository. The agent should read the skill only when its description
matches the user's request.

Repository installation is a separate, user-directed operation. A package-skills
installer can copy guidance from restored direct packages into the agent's skill
directory. `.agents/skills` is the prototype installer's default;
`.github/skills` was independently tested with Copilot CLI. Choose a destination
supported by the actual agent, and inspect existing files before replacing them.

The packaging prototype was validated with
[dotnet-package-skills](https://github.com/kartheekp-ms/dotnet-package-skills) at
commit `59d3bc0d4fc80188d33bcc257d2a83c99a4cbfa3`, built locally from its public
source. The public NuGet tool index was unavailable during this validation. This
is **not** evidence that a public `dotnet tool install` command is currently
available. Consult the project's current installation instructions before
recommending tool installation.

Keep a stable skill name across package versions. Refresh the installed skill
after updating the package rather than introducing a new folder for every patch.
The tested installer records ownership in `.dotnet-package-skills.json`;
noninteractive refresh replaces tracked files, including local edits. Its
interactive mode adds skills but does not refresh/remove them, and stale skills
require explicit uninstall. Do not promise automatic refresh merely because
NuGet restore completed.

## Release placement

To help **before** an upgrade to MAUI 11, include the same target-oriented skill
in an agreed MAUI 10 servicing release as a bridge, and carry it forward in MAUI
11 packages. A skill found only after restoring MAUI 11 cannot help an agent
discover prerequisites for the initial upgrade.

MAUI 8/9 projects should first stage a supported SDK/workload and MAUI 10 baseline;
this prototype is not tested as a direct 8/9-to-11 migration. It does not provide
MAUI 6/7 or Xamarin.Forms migration coverage.

This checkout is the MAUI 10 bridge prototype. Servicing release ownership and the
corresponding MAUI 11 branch change still require agreement. Reconcile preview/RC
claims against the final package, SDK, workload, and public documentation before
release. Never describe this prototype as already published.

## Authoring and validation

Keep the entry skill short, describe when it applies in `description`, and link
larger topic-specific guidance under `references/`. Ground platform/API claims
in the exact package's source and current public documentation. Documentation
headlines are not a substitute for checking a preview package's actual default
handler or feature switch.

Do not bundle executable migration scripts, evaluator artifacts, unpublished
drafts, credentials, or device-specific test answers. Preserve an application's
behavior; do not silence build errors or pin a compatibility implementation simply
to make a migration appear successful.

The Controls project packs Markdown with an explicit destination **file** path.
Using only a directory `PackagePath` duplicated `RecursiveDir` under the tested
NuGet targets; an archive inspection must catch that regression.

`PackageSkillsTests.ControlsPackageContainsVersionedSkillsAndReferences` in the
existing integration-test project checks the actual Controls artifact against
the Markdown source: exact archive paths, bytes, frontmatter names/descriptions,
and linked references. CI uses
`artifacts/Microsoft.Maui.Controls.<MAUI_PACKAGE_VERSION>.nupkg`. For a deliberately
selected local artifact, set `MAUI_CONTROLS_TEST_PACKAGE` to its full path. Run the
focused test through the integration-test workflow; do not replace real archive
validation with inspecting the MSBuild item alone.

Packaging/discovery/refresh correctness is separate from migration usefulness.
Evaluate source controls, runtime behavior, and exact target defaults before
comparing agent attempts. Separate infrastructure failures and invalid evaluators
from agent/API errors, keep guidance identical within a comparison batch, and
record whether the skill was actually selected. A small pilot does not establish
general upgrade reliability, Apple-platform coverage, or certified cost savings.
