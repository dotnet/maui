# .NET release tooling — design

Status: **draft**, first pass. Scope: replace the release logic currently carried by
`eng/pipelines/ci-official-release.yml`, `eng/pipelines/common/non-workload-publish.yml`
and `eng/scripts/nuget_release_packages.ps1` in `dotnet/maui` with a shared, testable
tool that other repositories can consume.

This repository is a staging ground. It is intended to graduate into a standalone
team-wide "releasing" repository, so it contains release tooling and nothing else.

---

## 1. What the release does today

The existing pipeline is one 304-line inline PowerShell script inside a YAML task, plus a
221-line helper script that is copied into the build artifact and re-verified by hash at
twelve separate points.

Shared preparation:

1. Validate the repository against a hard-coded allow-list.
2. Look up the required BAR channel for non-workload repositories.
3. Resolve the BAR build, either by repository + commit or by BAR ID.
4. Verify the resolved build's repository, exact commit, and channel membership.
5. `darc gather-drop` restricted to shipping nupkgs (asset filter `^[^/]+$` excludes
   symbol and other path-shaped blob assets).
6. Apply include/exclude filters.
7. Read each nupkg's nuspec for its real ID and version.
8. Reject duplicate file names and duplicate id/version pairs.
9. Stage artifacts.

Then it diverges:

- **Workload repositories** (`dotnet/maui`, `dotnet/android`, `dotnet/macios`) split packs
  from manifests, derive the .NET band from the manifest file name to select a workload-set
  channel and feed, optionally promote the build to that channel, and then perform two
  sequentially gated publishes: packs first, manifests second. Manifests reference packs,
  so the ordering is load-bearing.
- **Non-workload repositories** (`dotnet/android-libraries` on `.NET 10`/5172,
  `dotnet/skiasharp` on `.NET Libraries`/1648) form one package set, reject workload
  manifests outright, and perform a single gated publish.

Both paths then filter out identities already live on NuGet.org, push with
`1ES.PublishNuget@1` through the `nuget.org (dotnetframework)` service connection, and poll
NuGet.org until every expected package is indexed, with a 30-minute deadline.

### Problems being addressed

| Problem | Consequence |
|---|---|
| One script, ~20 distinct failure modes, one generic error message | Failures are undiagnosable from the log |
| `darc get-build` failure text goes to **stdout**, is captured into `$buildJson`, and only `$LASTEXITCODE` is checked | The real reason ("Could not any builds matching the given criteria") is discarded. This has already cost a production investigation |
| Normalized version derived by `BaseName.Substring(("$id.").Length)` | A string hack standing in for NuGet version normalization |
| Hand-rolled zip + XML + XPath nuspec reading | ~60 lines reimplementing `PackageArchiveReader` |
| Hand-rolled HTTP HEAD + retry against the flat-container API | Reimplements `FindPackageByIdResource` |
| Contract spread across `expected-packages.json`, `release-audit.json`, four `##vso` output variables, and twelve repeated hash-check blocks | No single reviewable artifact; integrity checks are copy-pasted |
| Policy embedded in PowerShell literals inside YAML | Not reviewable, not diffable, not testable, not shareable |

---

## 2. Design principles

### P1 — The tool mutates nothing outside its own output directory

This is the organising constraint and it explains nearly every other decision.

The tool reads BAR, reads a drop directory on local disk, queries NuGet.org read-only, and
writes a plan file plus a staging directory. The single exception is that `release filter`
deletes staged `.nupkg` files it has **proven** are already live — a deletion inside its own
staging directory, never a remote effect.

Every genuinely mutating operation stays an explicit, auditable step in the pipeline YAML:

| Mutation | Owner | Why not the tool |
|---|---|---|
| Push packages to NuGet.org | `1ES.PublishNuget@1` | Compliance requirement (§3) |
| Gather the drop | `darc gather-drop` YAML step | Feed discovery, auth, retries, `--include-released` (§4) |
| Promote a build to a channel | `darc add-build-to-channel` YAML step | Explicit and auditable in the pipeline definition |
| Approve a release | `ManualValidation@0` | Human decision |

Consequences: the tool never holds or needs publish credentials; its blast radius is
confined to reading and to writing its own output; and it becomes genuinely easy to test,
because every remaining external interaction is a read behind an interface.

### P2 — No subprocesses, anywhere

No `Process.Start`, no shelling out, no stdout scraping, no exit-code plumbing. There is no
process-execution abstraction in the codebase at all. Where an external tool is genuinely
the right implementation (`gather-drop`, `add-build-to-channel`), it is invoked from YAML,
where Azure DevOps owns the exit code natively and `--verbose` output lands in the build log.

### P3 — Pure policy first

All policy and validation lives in `DotNet.Release.Core`, which performs no I/O. Decisions
are pure functions over plain data. I/O lives in thin adapters behind interfaces. This is
enforced by an architecture test (§8), not by convention.

### P4 — Fail closed

Anything not explicitly permitted is an error. Unknown repository, unknown workload band,
missing channel, ambiguous build — all fail. Every failure carries a stable error code.

---

## 3. Decided constraint: `1ES.PublishNuget@1` owns the push

**Settled, not an open question.** `1ES.PublishNuget@1` must perform the actual upload to
NuGet.org. It carries SBOM, provenance, and compliance obligations that the tool cannot
satisfy and must not attempt to.

Therefore:

- The tool has **no** push verb, **no** `--push` flag, and no code path that uploads.
- `NuGet.Protocol` is used strictly read-only, to ask whether an ID/version is already live.
- The publish job shape is fixed at three steps:
  `release filter --plan` → `1ES.PublishNuget@1` → `release verify --plan`.
  Two tool invocations bracketing the compliance task.

This is a real architectural benefit, not merely a constraint accepted under protest.
Credential handling stays entirely with 1ES and the `nuget.org (dotnetframework)` service
connection. The tool never holds a NuGet.org API key, so no amount of tool misbehaviour —
including a bug, a bad plan file, or a malicious input — can publish a package.

**Trade-off, stated honestly:** the push cannot be folded into a single command. That is
precisely why `filter` and `verify` are separate verbs operating on the same plan file
rather than one `publish` verb that brackets its own upload.

---

## 4. Decided constraint: `gather-drop` stays in YAML

`darc gather-drop` performs feed discovery (which internal feed holds each asset),
authentication, retry, and `--include-released` handling. Reimplementing that would be
exactly the "roll your own" that is prohibited, and would be a large, poorly-tested surface
duplicating a maintained tool.

By contrast `darc get-build` is a simple query. It is replaced by the typed
`Microsoft.DotNet.ProductConstructionService.Client`, which makes today's
stdout-swallowing failure mode structurally impossible: there is no stdout to
misinterpret, and a failed lookup is a typed result, not a string.

The three darc invocations in the current pipeline are therefore treated differently:

| Invocation | Where | Treatment |
|---|---|---|
| `darc get-build` | prepare | **Replaced** by the typed client inside `release plan` |
| `darc gather-drop` | prepare | **Stays** as a plain YAML step between `plan` and `stage` |
| `darc add-build-to-channel` | workload promote | **Stays** as a plain YAML step; it is a mutation |

---

## 5. Verb model

Four verbs. Each has one responsibility, so a failure is attributable to a specific step.

### `release plan`

```
release plan --config config/repositories.json \
             --repo dotnet/skiasharp --commit <sha> [--bar-id N] \
             --out ./stage
```

Resolves the BAR build through the typed client, then verifies it against the request and
the declarative policy: repository identity, exact commit, required channel membership, and
workload classification. Writes `./stage/plan.json` (a `ResolvedRelease`).

Emits exactly one pipeline variable:
`##vso[task.setvariable variable=BarId;isOutput=true]<id>`, because the `gather-drop` step
that follows needs the BAR ID that this step discovered. **This is the only
pipeline-variable side effect in the entire tool**, and it is covered by a test.

`plan` has no package-reading and no feed code compiled into it.

### `release stage`

```
release stage --plan ./stage/plan.json --drop $(dropPath) \
              [--include '<glob>;...'] [--exclude '<glob>;...'] \
              --out ./stage
```

Reads the gathered drop from local disk, reads each nupkg's identity from its nuspec via
`PackageArchiveReader`, normalizes versions via `NuGetVersion.ToNormalizedString()`, applies
include/exclude filters, rejects duplicates and malformed packages, splits packs from
manifests for workload releases, derives the workload band, copies files into per-set
staging directories, and writes the final hashed `release-plan.json` plus a human-readable
audit.

### `release filter`

```
release filter --plan ./stage/release-plan.json [--skip '<glob>;...']
```

Queries NuGet.org read-only for every planned identity and **deletes already-published
`.nupkg` files in place**, so that the `packagesToPush` glob in `1ES.PublishNuget@1` picks
up only what still needs publishing. Writes a `release-filter.json` sidecar recording each
package's disposition. Sets `NuGetPackagesToPublish` so the publish task can be skipped
entirely when nothing remains.

### `release verify`

```
release verify --plan ./stage/release-plan.json --max-duration-minutes 30
```

Polls NuGet.org until **every** package in the plan is indexed — including ones that
`filter` removed, because those were removed precisely on the grounds that they were already
live. Fails with the missing identities when the deadline expires.

---

## 6. The plan files

Two files, two schemas. Both are strongly typed with no "valid only after a later step"
nullable fields.

### `plan.json` — `ResolvedRelease`

Written by `plan`, read by `stage`.

```jsonc
{
  "schemaVersion": 1,
  "toolVersion": "1.0.0",
  "createdUtc": "2026-08-27T18:00:00Z",
  "repository": "dotnet/skiasharp",
  "repositoryUrl": "https://github.com/dotnet/skiasharp",
  "commit": "f14581760a...",
  "barBuildId": 123456,
  "repositoryOrigin": "GitHubRepository",   // or "AzureDevOpsMirrorConvention"
  "workload": false,
  "channel": { "name": ".NET Libraries", "id": 1648 }
}
```

### `release-plan.json` — `ReleasePlan`

Written by `stage`. **The single contract artifact**, replacing today's
`expected-packages.json` + `release-audit.json` + four `##vso` variables.

```jsonc
{
  "schemaVersion": 1,
  "toolVersion": "1.0.0",
  "createdUtc": "2026-08-27T18:04:00Z",
  "source": { /* the full ResolvedRelease above */ },
  "workloadSet": null,        // or { "band": 10, "channel": ".NET 10 Workload Release", "feed": "dotnet10-workloads" }
  "tool": { "fileName": "release.exe", "sha256": "…" },
  "sets": [
    {
      "name": "NuGet packages",
      "order": 0,
      "artifactName": "NuGetPackagesForRelease",
      "packages": [
        { "id": "SkiaSharp", "version": "3.119.0", "normalizedVersion": "3.119.0",
          "fileName": "SkiaSharp.3.119.0.nupkg", "sha256": "…" }
      ]
    }
  ]
}
```

Three things this buys:

1. **Ordering is data, not YAML topology.** `order` encodes pack-before-manifest. The YAML
   still enforces it as a stage dependency (§7), but the plan records the intent
   independently and it is unit-testable.
2. **Per-package `sha256` closes a real gap.** Today only the *helper script* is hashed; the
   packages themselves are not. Now `filter` can prove that the file it is about to hand to
   1ES is byte-identical to the one `stage` validated.
3. **One hash replaces twelve hash-check blocks.** The tool's own hash is recorded *inside*
   the plan, so hashing the plan transitively pins the tool. The publish job needs exactly
   one pinned value, `ReleasePlanHash`, and one verification block:
   - check `sha256(release-plan.json) == $(ReleasePlanHash)`;
   - read `tool.sha256` from the now-trusted plan and check the tool binary;
   - everything downstream is covered.

### Plan immutability and the `filter` sidecar

`filter` deletes files, so after it runs the directory legitimately no longer contains every
file the plan lists. Resolving this by editing the plan would invalidate its hash.

Instead **the plan is immutable** and dispositions live in a sidecar, `release-filter.json`:

| Disposition | Meaning | File on disk |
|---|---|---|
| `Pending` | Not on NuGet.org; must be pushed | present |
| `AlreadyPublished` | Proven live; removed from the push set | removed |
| `PreviouslyAttempted` | Matched a `--skip` recovery filter | removed |

The invariant is exact and testable:

> for every package in the plan: *file present* ⟺ *disposition is `Pending`*

`filter` fails closed if a `Pending` package's file is missing or its hash does not match,
which is precisely the "publish job fails closed if the plan lists a file the directory does
not contain" requirement.

---

## 7. Pipeline shape, and why each split is forced

```
prepare (agent)
  release plan  →  darc gather-drop  →  release stage
      ↓ artifacts + $(ReleasePlanHash)
promote_workload_set (workload only, compile-time ${{ if }})
  darc add-build-to-channel
      ↓
publish_packs                          publish            (non-workload)
  job approval   (pool: server)          job approval
  job push       (checkout: none)        job push
      ↓
publish_manifests (workload only)
  job approval   (pool: server)
  job push       (checkout: none)
```

None of these splits is stylistic:

- **approval job vs push job** — `ManualValidation@0` requires `pool: server`. Agentless
  jobs cannot download artifacts or run scripts, so the gate physically cannot live in the
  same job as the publish. This is requirement 1 and it is imposed by Azure DevOps.
- **packs stage vs manifests stage** — a manifest points at packs. Publishing manifests
  first yields an unresolvable workload manifest on NuGet.org, and NuGet.org packages are
  immutable, so it cannot be repaired. The ordering must be a *stage* dependency so that
  manifest approval is literally unreachable until pack verification has succeeded. A job
  dependency inside one stage would still expose the manifest approval prompt.
- **prepare stage vs publish stage** — publish runs `checkout: none` inside a
  `templateContext.type: releaseJob` with production credentials. Prepare needs the repo and
  Maestro credentials. Splitting them means the production job's only inputs are an
  SBOM-backed artifact and one pinned hash.
- **prepare as three steps rather than one** — accepted deliberately. Three steps with one
  responsibility each are attributable on failure; today's single 304-line script can fail
  in about twenty ways behind one message.

### Dry run is structurally incapable of publishing

Two independent barriers, as required:

1. **No push code exists.** `plan` and `stage` do not reference any feed-write capability;
   the tool as a whole has none. This is not a flag that could be inverted — it is absent
   from the binary, and enforced by the architecture test in §8.
2. **Compile-time stage exclusion.** The publish stages are wrapped in `${{ if }}`, so on a
   dry run they do not exist in the expanded YAML at all.

---

## 8. Code layout

```
src/DotNet.Release.Core/       pure policy, validation, planning — NO I/O
src/DotNet.Release.Maestro/    typed BAR client adapter (interface impl)
src/DotNet.Release.NuGet/      read-only feed + nupkg reading adapters
src/DotNet.Release.Cli/        verbs and argument parsing
tests/DotNet.Release.Core.Tests/
config/repositories.json
templates/release.yml
```

`Core` defines the interfaces (`IBuildRegistry`, `IPackageIdentityReader`,
`IPackageAvailabilityProbe`); the adapters implement them. Interfaces are not I/O, so they
belong with the policy that consumes them.

**The architecture test enforces P3 by reflection**, asserting that `DotNet.Release.Core`
references none of `NuGet.Protocol`, `NuGet.Packaging`, `Microsoft.DotNet.*`, `Azure.*`,
`System.Net.Http`, or `System.Diagnostics.Process`. That last one is how P2 ("no
subprocesses") is kept honest rather than aspirational.

`Core` does reference `NuGet.Versioning`, which is a pure parser with no I/O, because
version normalization is policy and must not be a substring hack.

---

## 9. Declarative policy

`config/repositories.json`, checked in and reviewable:

```jsonc
{
  "schemaVersion": 1,
  "repositories": {
    "dotnet/maui":              { "workload": true },
    "dotnet/android":           { "workload": true },
    "dotnet/macios":            { "workload": true },
    "dotnet/android-libraries": { "workload": false, "channel": { "name": ".NET 10",        "id": 5172 } },
    "dotnet/skiasharp":         { "workload": false, "channel": { "name": ".NET Libraries", "id": 1648 } }
  },
  "workloadSets": {
    "8":  { "channel": ".NET 8 Workload Release",  "feed": "dotnet8-workloads"  },
    "9":  { "channel": ".NET 9 Workload Release",  "feed": "dotnet9-workloads"  },
    "10": { "channel": ".NET 10 Workload Release", "feed": "dotnet10-workloads" },
    "11": { "channel": ".NET 11 Workload Release", "feed": "dotnet11-workloads" }
  }
}
```

Anything not listed fails closed. The workload-band → channel/feed map moves out of a
PowerShell `if/elseif` chain, so adding .NET 12 becomes a config change with a test.

**Known asymmetry, inherited from today:** workload repositories have no required channel,
while non-workload ones do. That is preserved deliberately so this change is behaviour-
preserving, and it is flagged here as a candidate to tighten later.

---

## 10. Error codes

Failures carry stable codes rather than ad-hoc prose, so they can be documented, searched
for in logs, and asserted in tests. Requirement 2's fail-closed list maps onto:

| Code | Condition |
|---|---|
| `REPO_NOT_ALLOWED` | Repository absent from policy |
| `REPO_UNPARSEABLE` | Repository string or URL not understood |
| `WORKLOAD_MISMATCH` | Build classification disagrees with policy |
| `BAR_BUILD_NOT_FOUND` / `BAR_BUILD_NOT_UNIQUE` | Zero, or more than one, matching build |
| `BAR_REPO_MISMATCH` | Build belongs to another repository |
| `BAR_COMMIT_MISMATCH` | Build is for a different commit |
| `BAR_CHANNEL_MISSING` | Build not on the required channel |
| `BAR_MIRROR_NAME_INVALID` | AzDO mirror name does not follow `<owner>-<name>` |
| `PACKAGE_MALFORMED` | Missing/invalid nuspec identity, or file name disagrees with ID |
| `PACKAGE_DUPLICATE_FILENAME` / `PACKAGE_DUPLICATE_IDENTITY` | Duplicates within a set |
| `PACKAGE_SET_EMPTY` | Filters selected nothing |
| `MANIFEST_IN_NON_WORKLOAD` | Workload manifest in a non-workload release |
| `WORKLOAD_BAND_UNRESOLVED` / `WORKLOAD_BAND_AMBIGUOUS` / `WORKLOAD_SET_NOT_CONFIGURED` | Band derivation |
| `FILTER_UNMATCHED` | A recovery filter matched no planned package |
| `PLAN_HASH_MISMATCH` / `PLAN_SCHEMA_INVALID` / `PACKAGE_HASH_MISMATCH` / `PACKAGE_FILE_MISSING` / `PACKAGE_FILE_UNEXPECTED` | Integrity |
| `POLICY_INVALID` | The policy file itself is malformed |

---

## 11. The NULL `gitHubRepository` case

BAR sometimes records no GitHub URL for a build. Arcade derives the URL from the AzDO mirror
name (lowercase, first `-` becomes `/`) and verifies it against the GitHub API, silently
nulling the field on a 404. This is a real, currently-hit case, not a hypothetical.

The tool therefore supports resolution by BAR ID **and still verifies identity**:

- Resolution by ID does not imply trust. Commit and repository are verified on every path.
- When `gitHubRepository` is null, the mirror convention is reproduced to obtain a
  verifiable identity, and the plan records `repositoryOrigin:
  "AzureDevOpsMirrorConvention"` so the audit trail states which path was taken.
- A mirror name without a `-` is `BAR_MIRROR_NAME_INVALID`, not a guess.

---

## 12. Behaviour deliberately preserved

Subtleties in the current script that are easy to "fix" by accident and are kept:

- **Include filters apply to packs but not to manifests**; exclude filters apply to both.
- Workload releases require **both** the pack and manifest sets to be non-empty.
- `--skip` patterns that match nothing are an **error**, not a no-op.
- Skipped packages stay in the expected set for `verify` even though their files are removed.
- `verify` requires every planned identity, not merely the ones that were pushed.

### Behaviour deliberately changed

- **Recovery filters are generalized rather than special-cased.** Today there are two named
  parameters, `nugetAlreadyAttemptedPackFilters` and `nugetAlreadyAttemptedManifestFilters`,
  and non-workload releases *reject* both — a rule that exists only because the parameter
  names hard-code the two workload sets. Here `filter --skip` applies to whichever set is
  being published, so the rule dissolves: there is nothing left to reject. The protective
  part is kept, and strengthened by a test: a `--skip` pattern that matches nothing in the
  set being filtered is an error.

- **Normalized version** now comes from `NuGetVersion.ToNormalizedString()` instead of
  `fileName.Substring(id.Length + 1)`. The old hack breaks on any case difference between
  the file name and the nuspec ID and on non-normalized version strings in the file name.
- **Wildcard semantics** are `*` and `?` only, via `FileSystemName.MatchesSimpleExpression`.
  PowerShell `-like` additionally supports `[a-z]` character classes. No current filter uses
  them, and a character class in a *release* package filter is a footgun, so the narrowing
  is intentional and documented here rather than silent.

---

## 13. Arcade onboarding

The repository is a standard Arcade repo, not a bespoke build:

| Piece | Value |
|---|---|
| `global.json` | pins the SDK and the `Microsoft.DotNet.Arcade.Sdk` MSBuild SDK |
| `eng/Version.Details.xml` | authoritative dependency manifest; Arcade is a tracked `<Dependency>` |
| `eng/Versions.props` | versioning and package-version properties |
| `eng/common/**` | Arcade-provided, darc-managed; not hand-written |
| `Directory.Build.props` / `.targets` | import `Sdk.props` / `Sdk.targets` from the Arcade SDK |
| `NuGet.config` | `dotnet-eng` (Arcade, PCS client), `dotnet-public`, `dotnet-tools` |
| `build.sh` / `build.cmd` | Arcade's standard entry points, delegating to `eng/common` |
| Solution | `DotNet.Release.slnx`; Arcade 10.0.0-beta.26070.104 discovers `.slnx` natively |

Arcade defaults `xunit.runner.visualstudio` to 3.1.3. It is overridden to 3.1.5 in
`eng/Versions.props`, which is the documented Arcade mechanism — every property in Arcade's
`DefaultVersions.props` is conditioned on being unset — rather than pinning the package
somewhere Arcade does not know about.

### What is not finished

`Microsoft.DotNet.ProductConstructionService.Client` is **not** yet in
`eng/Version.Details.xml`. A `<Dependency>` entry requires a real `<Version>` and `<Sha>`,
and inventing them would produce a manifest that looks authoritative and is not. It must be
added with:

```
darc add-dependency --name Microsoft.DotNet.ProductConstructionService.Client \
                    --type product --repo https://github.com/dotnet/arcade-services
```

which also resolves open question 4 in section 14.

---

## 14. Open questions

1. **Asset download without `gather-drop`.** Resolved for now — `gather-drop` stays (§4).
   `Microsoft.DotNet.ProductConstructionService.Client` does return asset locations, so a
   future native download is conceivable, but it would have to reimplement feed selection
   and auth. Not worth it.
2. **Getting the tool onto the `checkout: none` publish agent.** Recommended: publish the
   CLI self-contained into the artifact and pin it via `tool.sha256` inside the plan (§6).
   The alternative — restoring a `dotnet tool` on the production agent — adds network
   dependency and feed auth to the most sensitive job. Needs confirmation against 1ES image
   constraints.
3. **Whether workload repositories should also require an explicit channel** (§9).
4. **Verifying the typed client's exact API surface.**
   `Microsoft.DotNet.ProductConstructionService.Client` could not be restored in the offline
   environment this was drafted in, so `IBuildRegistry` is currently defined from the shape
   of the data the pipeline needs rather than from the client's real signatures. The adapter
   must be reconciled with the package before it is trusted.
