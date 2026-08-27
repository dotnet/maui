# .NET release tooling — design

Status: **draft**, first pass. Scope: replace the release logic currently carried by
`eng/pipelines/ci-official-release.yml`, `eng/pipelines/common/non-workload-publish.yml`
and `eng/scripts/nuget_release_packages.ps1` in `dotnet/maui` with a shared, testable
tool that other repositories can consume.

**This repository is the release system**, not a library and not a template others extend.
One Azure DevOps pipeline (`eng/pipelines/release.yml`) is hooked up to it once; everyone
triggers that pipeline with parameters naming the repository and commit to release. The
repositories being released do not change and do not reference this one.

It is a staging ground intended to graduate into a standalone team-wide "releasing"
repository, so it contains release tooling and nothing else.

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
| Normalized version derived by `BaseName.Substring(("$id.").Length)` | Correct today only by coincidence — see section 12 |
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

All policy and validation lives in `Policy/`, which performs no I/O. Decisions are pure
functions over plain data; I/O lives in thin adapters behind interfaces, so tests never touch
a network. Since the projects were merged this is a convention rather than an assembly
boundary — see §8 for what that trade did and did not cost.

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

Emits one pipeline variable:
`##vso[task.setvariable variable=BarId;isOutput=true]<id>`, because the `gather-drop` step
that follows needs the BAR ID that this step discovered.

`plan` has no package-reading and no feed code compiled into it.

> **Compatibility note.** The tool sets a second pipeline variable,
> `NuGetPackagesToPublish`, from `release filter`. This is not a new contract: today's
> `nuget_release_packages.ps1` sets the same variable under the same name, and three
> existing conditions consume it (`ci-official-release.yml` twice,
> `non-workload-publish.yml` once). It exists because the publish task is skipped via an
> Azure DevOps `condition`, and a condition can only read a variable. Both variables are
> covered by tests, and they are the only two.
>
> The plan *hash* is deliberately not among them: the pipeline computes it with
> `Get-FileHash`, because a value that pins an artifact must not be sourced from that
> artifact.

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
   packages themselves are not. **Threat model:** the publish job runs `checkout: none` and
   executes a script out of the downloaded artifact, so anything crossing that job boundary
   unpinned is a supply-chain gap. Now `filter` can prove that the file it is about to hand
   to 1ES is byte-identical to the one `stage` validated.
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

**Why the sidecar earns its keep**, stated plainly for a reviewer who would otherwise see
extra machinery: `1ES.PublishNuget@1` pushes whatever its `packagesToPush` glob matches. It
does not consult the plan. So today, a file that appears in the staging directory without
being in the release set is published without ever having been validated, and nothing
notices. The invariant above is what closes that hole — `ValidateFiltered` rejects both a
missing `Pending` file *and* any file the plan does not list.

---

### What the single plan gave up, and how it is bought back

Worth stating precisely, because it explains a bug this design had and the pipeline it
replaces could not have.

The current pipeline has **no central plan**. Each set gets its own directory containing its
own `expected-packages.json`, written in the same loop that copies its packages, and the
helper script resolves that manifest *from inside the path it is given*:

```powershell
$manifestPath = Join-Path $PackagesPath 'expected-packages.json'
```

`-PackagesPath` is the only input, and the manifest travels with the packages. Point it at
the packs artifact and it is **structurally incapable** of reasoning about manifests — there
is no other manifest in scope to be confused by. Cross-set contamination is unrepresentable,
not merely untested.

A single hashed plan is better for supply-chain integrity: one root of trust instead of *N*
manifests to pin separately. But it means `filter` and `verify` must be **told** which set
they are operating on. **That replaces a structural guarantee with a passed argument**, and
passed arguments depend on the caller being right.

That is not hypothetical. This design shipped with exactly that bug: both verbs operated on
every set in the plan, so a workload packs stage looked for manifest files in a directory
that did not exist and waited for manifests that had not been published yet. A workload
release could never have succeeded; a non-workload one worked by accident, having only one
set. `--set` (§7) fixes it, and `PACKAGE_SET_NOT_FOUND` catches a *misspelled* set.

`PACKAGE_SET_NOT_FOUND` does **not** catch a *valid but wrong* set. Asking the packs
directory for `ReleaseManifests` resolves cleanly and fails later as `PACKAGE_FILE_MISSING`
— the same error a genuinely broken artifact upload produces, pointing an on-call engineer
at the wrong problem. That is the diagnostic failure this project rejects elsewhere, when it
refuses to flatten an HTTP 500 into "no such build".

So `stage` writes two things into **each** set directory alongside its packages:

| File | Purpose |
|---|---|
| `release-plan.json` | Identical bytes in every set, so **one hash still pins them all** |
| `release-set.json` | The set's own name, artifact name, BAR build and commit |

`filter` and `verify` then assert that the directory they were pointed at **declares itself
to be the set they were asked for, for the release they were asked about**. A mismatch is
`PACKAGE_SET_MISMATCH`, distinct from a missing file and worded to name the wiring rather
than the symptom.

Two properties follow:

- The invariant holds even if a future template refactor gets the wiring wrong — which is
  the actual residual risk, since the wiring is now the thing that has to be right.
- Tampering with a marker can only cause a **failure**, never a silently wrong publish: the
  package identities still come from the hashed plan.

This also fixes a second bug with the same root cause. `stage` previously wrote
`release-plan.json` to the *parent* of the set directories, while each set directory is
published as its own pipeline artifact — so the plan never entered the artifact, and the
publish job, running `checkout: none`, would have failed at its first integrity step. An
artifact consumed without a checkout has to be self-contained.

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

### One pipeline, many repositories

This is a shared system: a single pipeline definition releases every enabled repository, so
the repository is a parameter rather than a fork of the YAML. Two consequences follow.

**Workload classification must be known at compile time.** It decides which stages exist — a
workload release needs two separately gated publishes in a fixed order, and stage structure
cannot be chosen at run time. So `eng/pipelines/release.yml` derives it from the parameters:

```yaml
- name: isWorkload
  value: ${{ and(eq(parameters.ghOwner, 'dotnet'), in(parameters.ghRepo, 'maui', 'android', 'macios')) }}
```

Derived, not asked for, so an operator cannot set it wrong. But it is a *second* source of
truth alongside `config/repositories.json`, which is authoritative. Rather than leave those to
drift, they are pinned from both ends:

- **at build time**, `PipelineDefinitionTests` fails if the pipeline's repository dropdown or
  its workload list disagrees with the policy;
- **at release time**, the pipeline passes its answer to `release plan --expect-workload`, and
  `ReleasePolicy.VerifyWorkloadClassification` fails closed with `WORKLOAD_MISMATCH`.

The failure being guarded against is specific: a workload repository misclassified as
non-workload would publish packs and manifests through a single stage, losing the
pack-before-manifest ordering — and NuGet.org packages are immutable, so that is
unrecoverable.

**The publish stages are emitted only when publishing.** `publishPackages` defaults to
`false`, so the default run of a shared production pipeline is a dry run, and on a dry run the
publish stages do not exist in the expanded YAML at all.

### Publish stages are scoped to one package set

`release filter` and `release verify` both take `--set <artifactName>`, and the template
passes the stage's own artifact name.

This is load-bearing for workload releases and easy to get wrong, because a single-set
release works without it. Each publish stage downloads only its own artifact, so a stage
that operated on every set in the plan would:

- look for the *other* stage's `.nupkg` files in a directory that does not exist, and fail
  with `PACKAGE_FILE_MISSING`; and
- wait for packages that, by design, have not been published yet — burning the full
  verification deadline before failing.

A workload release could therefore never have succeeded, while a non-workload one worked by
accident. An unknown `--set` name fails closed with `PACKAGE_SET_NOT_FOUND` rather than
selecting nothing, so a template typo cannot produce a publish stage that verifies zero
packages and reports success. Covered by `WorkloadStageScopingTests`.

### Dry run is structurally incapable of publishing

Two independent barriers, as required:

1. **No push code exists.** `plan` and `stage` do not reference any feed-write capability;
   the tool as a whole has none. This is not a flag that could be inverted — it is absent
   from the binary, and enforced by the architecture test in §8.
2. **Compile-time stage exclusion.** The publish stages are wrapped in `${{ if }}`, so on a
   dry run they do not exist in the expanded YAML at all.

---

## 8. Code layout

One tool, one assembly. The layering that matters is expressed by folders:

```
eng/pipelines/release.yml      THE pipeline — the entry point hooked up in Azure DevOps
eng/pipelines/stages/          internal stage templates it includes

src/DotNet.Release/
  Cli/                         verbs and argument parsing
  Policy/                      pure decisions over plain data — no I/O
  Model/                       the data those decisions operate on, and its serialization
  Adapters/                    the only I/O: read-only BAR and NuGet, reading .nupkg files
  Abstractions/                the interfaces Policy and Cli depend on
  Pipeline/                    Azure DevOps logging-command formatting

tests/DotNet.Release.Tests/    one test project, zero network
config/repositories.json       declarative release policy
```

### Why one project rather than four

This started as `Core` / `Maestro` / `NuGet` / `Cli`. Three of those splits bought nothing:
the adapters are I/O wrappers with exactly one consumer each, and separating them added
project files, package plumbing and cross-assembly `using` statements without ever preventing
a mistake. The tool only ever does one thing — prepare a release and hand it to 1ES — so it
is one program.

**The abstractions that earn their place are kept.** `IBuildRegistry`,
`IPackageIdentityReader` and `IPackageAvailabilityProbe` remain interfaces, because they are
what allow every test to run with no network, no BAR account and no real feed. Removing them
would not simplify anything; it would make the test suite impossible.

### What the merge traded away, stated plainly

The `Core` boundary enforced one real thing: an architecture test asserting that the pure
policy assembly referenced no I/O-capable type. That enforcement is gone. "Policy performs no
I/O" is now a convention, expressed by the `Policy/` and `Adapters/` folders and by a weaker
test that checks policy types mention no file or network types in their public signatures.

That is an acceptable trade because it was an *internal* discipline. The two guarantees that
are externally load-bearing are unaffected, and are in fact now enforced **more** strongly:

| Guarantee | Before | Now |
|---|---|---|
| The tool cannot push a package | Scanned only `Core` — the one assembly that could never have pushed. The assembly that references `NuGet.Protocol`, and therefore `PackageUpdateResource`, was not scanned at all. | Scans the whole tool. There is nowhere for a violation to hide. |
| The tool cannot start a process | Scanned only `Core` | Scans the whole tool |
| Policy performs no I/O | Assembly reference scan | Convention + public-signature check |

The first row is the important one: the previous arrangement checked the assembly that could
not break the rule and skipped the one that could. Merging removed that blind spot.

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
| `PACKAGE_SET_NOT_FOUND` | `--set` names a set the plan does not contain |
| `PACKAGE_SET_MISMATCH` | A staged directory is not the set the stage asked for |
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
  `fileName.Substring(id.Length + 1)`.

  This is **latent fragility, not a bug that fires today.** The substring reads the version
  out of the *file name*, and NuGet feeds serve packages under normalized file names, so in
  practice the file name already *is* the normalized version and the substring returns the
  right answer. Verified against NuGet.org: four-part versions are indexed as-is
  (`HarfBuzzSharp` `8.3.1.5`, `14.2.1.1`, `14.2.1.2`), and no version ends in a fourth
  component of `.0`, consistent with normalization dropping only trailing zeros. Producing a
  wrong answer requires a package whose file name is not already normalized — real, but it
  needs an unusual producer.

  The objection is that this is correctness *by coincidence* rather than by construction: it
  holds because of a property of the feeds upstream, which nothing in the release checks or
  is even aware of. And when it does break, it breaks silently — the availability query is
  built from the normalized version, so a wrong value reports a published package as missing
  and `verify` never succeeds, with nothing in the log pointing at the cause.

  So the substring is replaced, and — this is the part that actually matters — `StagePlanner`
  independently rejects any reader whose normalized version disagrees with
  `NuGetVersion.ToNormalizedString()`. That turns a silent wrong answer into a loud one at
  stage time, which is the real improvement.
- **Wildcard semantics** are `*` and `?` only, via `FileSystemName.MatchesSimpleExpression`.
  PowerShell `-like` additionally supports `[a-z]` character classes. No current filter uses
  them, and a character class in a *release* package filter is a footgun, so the narrowing
  is intentional and documented here rather than silent.

---

## 13. Migration from the current pipeline

Cutover is not drop-in. These are the observable differences a consumer must handle.

### Artifact names changed

The tool is shared, so artifact names are repository-neutral rather than MAUI-specific.
**Any consumer pipeline or downstream tooling that references the old names must be updated
on cutover.**

| Today | Here |
|---|---|
| `MauiPacksForNuGet` | `ReleasePacks` |
| `MauiManifestsForNuGet` | `ReleaseManifests` |
| `NuGetPackagesForRelease` | `ReleasePackages` |
| `NuGetReleaseAudit` | folded into `release-plan.json` |

### Files changed

| Today | Here |
|---|---|
| `expected-packages.json` | `release-plan.json` (`sets[].packages`) |
| `release-audit.json` | `release-plan.json` (`source` + `sets`) |
| `nuget_release_packages.ps1` copied into the artifact | the tool, pinned by `tool.sha256` inside the plan |

### Pipeline variables changed

Four output variables (`BarId`, `WorkloadSetsChannel`, `WorkloadSetsFeed`,
`PackageStatusScriptHash`) become one output variable (`BarId`) plus one pinned value
(`ReleasePlanHash`). `WorkloadSetsChannel` and `WorkloadSetsFeed` move into the plan file, so
the promote step reads them from `workloadSet` rather than from `stageDependencies`.

### Parameters changed

`nugetAlreadyAttemptedPackFilters` and `nugetAlreadyAttemptedManifestFilters` collapse into a
single `--skip` on `release filter`, scoped to the set being published (§12).

### Deliberate divergences from upstream

The source pipeline moved after this design was drafted. Upstream commit `3013cd846f`,
*"[ci] Remove repository-specific channel policy"*, changes what is being migrated *from*,
so the differences below are design positions rather than parity gaps. Both were verified
against that commit rather than taken on report.

**1. Channel verification is kept here; upstream removed it.**

`3013cd846f` deletes the `$requiredChannelName` / `$requiredChannelId` conditionals *and*
the whole `$channelMatches` verification block, leaving only "resolve the commit to exactly
one matching BAR build".

This tool keeps the check, and `config/repositories.json` keeps the per-repository channel.
The reasoning: **identity verification and channel verification answer different questions.**
Commit and repository checks prove *which* build is being released. Channel membership
proves that build *reached a quality bar* — someone promoted it deliberately. Dropping the
second means any build for the right commit is releasable, including one that was never
promoted.

That is a real reduction in safety, not a simplification, so it is not adopted. It is
recorded here as a divergence so a reviewer sees a decision rather than drift. If the
channel requirement becomes genuinely burdensome the answer is to remove it from
`repositories.json` — a reviewable config change with a test — not to delete the code path.

**2. Resolution by BAR ID, and the null-repository guard, are ahead of upstream.**

After `3013cd846f` the upstream prepare step resolves a build one way only:

```powershell
$buildJson = & $darc get-build --ci --repo $sourceRepository --commit "$env:COMMIT_HASH" ...
$repository = ([Uri] $build.gitHubRepository).GetLeftPart([UriPartial]::Path)...
```

Two problems, on what is now the *only* path:

- `--repo` + `--commit` cannot resolve a build BAR recorded without a GitHub URL. It exits
  42. That is not theoretical — it is what a SkiaSharp dry run actually does.
- `[Uri] $build.gitHubRepository` has no null guard, and that field *is* null in production
  for BAR build 328857.

So `--bar-id` resolution (§5) and `BarBuildMapper`'s boundary normalisation (§15) are not
defensiveness against a hypothetical. They handle two cases the current pipeline cannot,
one of which blocks a real release today. A reviewer comparing the two implementations
should read them as fixes, not as extra machinery.

---

## 14. Arcade onboarding

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

### Dependency manifest

`Microsoft.DotNet.ProductConstructionService.Client` is tracked in
`eng/Version.Details.xml` at `1.1.0-beta.26426.2`, with `<Uri>` and `<Sha>` read from the
package's own nuspec `<repository>` metadata (`dotnet/arcade-services`,
`596c9990a8259057dc5e864c20af37f50dc87f84`) rather than assumed. Updates should flow through
darc from there.

---

## 15. The verified BAR client surface

`IBuildRegistry` was originally shaped from what the pipeline needs. It has since been
reconciled against the real package by reflection, because an interface designed around
wishful thinking is the one thing that could have invalidated this design.

| Question | Verified answer |
|---|---|
| Fetch by BAR ID | `IBuilds.GetBuildAsync(int id, bool? includeAssetLocation, CancellationToken)` → `Task<Build>`. A *single* build, not a list; a missing build is `RestApiException` with `Response.Status == 404`. |
| Fetch by repository + commit | `IBuilds.ListBuildsAsync(… string commit, … string repository, …)` → `Azure.AsyncPageable<Build>`. A filtered *list*, so zero-or-many is real and Core's "exactly one" rule is load-bearing. |
| Channels | `Build.Channels` is `List<Channel>`; `Channel` exposes both `Id` (`int`) and `Name` (`string`). Both are available, so the existing name-**and**-id check is preserved exactly. |
| Is `gitHubRepository` nullable? | **The assembly is not nullable-annotated.** `Build.GitHubRepository`, `Build.AzureDevOpsRepository`, `Build.Commit` and `Channel.Name` are declared plain `string` with nullability state `Unknown`. |

The last row is the important one. The typed client gives **no** compile-time protection
here: `GitHubRepository` is null in production — SkiaSharp BAR build 328857 — and nothing in
the type system says so. `BarBuildMapper` therefore normalizes every string crossing the
boundary explicitly, and the mapper is a pure function kept separate from the registry so
that behaviour is tested without any paging plumbing.

Two consequences for the adapter:

- **404 is an answer, not a failure.** It maps to an empty result so Core produces
  `BAR_BUILD_NOT_FOUND`. Every other status stays an exception — flattening a 500 into "no
  such build" would send an operator hunting the wrong problem during an outage.
- **`IBuilds` also exposes `CreateAsync` and `UpdateAsync`,** which write to BAR. The adapter
  uses only `GetBuildAsync` and `ListBuildsAsync`; the test fake throws on the mutating
  members, so if that ever changes a test fails rather than the guarantee quietly lapsing.

---

## 16. Operational behaviour observed in production

Evidence from Azure DevOps build **3059242**, which released 41 packages end to end.

### NuGet.org indexing is slow and non-uniform

```
19:35:44  waiting for 41   (attempt  2)
19:39:01  waiting for 22   (attempt 10)
19:41:55  waiting for  1   (attempt 18)
19:46:04  Verified all 41 packages.   (attempt 29)
```

**10m30s, 29 polls, and one package alone held the tail for 4m11s across 11 polls.**

Three design consequences, each pinned by a test in `VerificationBudgetTests`:

1. **The default budget stays at 30 minutes / 20-second polls** — roughly 90 polls, about
   three times what the observed run needed. `A_five_minute_budget_would_have_failed_a_healthy_release`
   exists so nobody tunes it down on the assumption that indexing is quick.
2. **The deadline covers the set, not each package.** A per-package budget would have to be
   large enough for that 4-minute straggler and would then be wildly generous for the set.
   Pinned by `A_single_straggler_is_covered_by_the_set_budget`.
3. **The availability probe must not cache.** `SourceCacheContext.NoCache` is set. Against a
   4-minute tail, a cached negative would let a later run re-push a package that had in fact
   landed, and the 1ES NuGet task treats the resulting HTTP 409 as fatal.

### Job result is not a release-success oracle

The same run finished `partiallySucceeded`, with both pack sets pushed and verified
correctly. The cause was two **non-blocking 1ES checks**:

```
Branch Validation (1ES PT):      Production releases must use a production branch
Validate Source Build (1ES PT):  Artifacts downloaded in a production release job
                                 must be built on a production branch
```

Any job using `templateContext: type: releaseJob, isProduction: true` inherits this.

- **Expected, not a defect:** template validation and any end-to-end rehearsal from a
  non-production branch will report `partiallySucceeded` even when the release is entirely
  correct. Green is not the success signal here.
- **Therefore the authoritative signal is `release verify`'s own exit code**, which is why
  it is a distinct verb with an explicit exit contract rather than a step folded into the
  publish. It returns a non-zero exit code and names every missing identity; it never warns
  and continues. Pinned by
  `Verification_failure_is_a_non_zero_exit_code_with_the_missing_identities`, and the
  template propagates it with `if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }`.

---

## 17. Independent review findings

An independent review (Claude Opus 5, read-only, with access to this branch, the pipeline
being replaced, and the BAR service source) found **four blocking defects** that all 270
tests passed straight through. They are recorded here because each says something about
where this design's tests were weak, not merely that a line was wrong.

| # | Defect | Root cause |
|---|---|---|
| D1 | `loadCollections: false` meant BAR returned **no channels**, so the required-channel check failed 100% of the time for `skiasharp` and `android-libraries` — the only two repositories it exists for | The test fake silently discarded the flag, so no test could see it |
| D2 | `$(toolPath)` was never defined in `release.yml`, and the tool was never built | Templates were YAML-parsed, never executed |
| D3 | The `_tool/` directory `publish-set.yml` reads was never produced | An open question was written as though it were closed |
| D4 | `DescribeSelf()` hashed `Environment.ProcessPath` — the `dotnet` muxer or an apphost stub, never the managed code | The claim "hashing the plan transitively pins the tool" was untested |

**D1 is the one worth dwelling on.** The failure message would have read *"must be assigned
to '.NET Libraries' (channel 1648), but has 0 such assignment"* — sending an operator to BAR
to inspect a build that is, in fact, correctly assigned. Verified against the service source
at the exact commit the client package was built from:

```csharp
if (loadCollections ?? false) { query = query.Include(b => b.BuildChannels)... }
```

The lesson is about fakes, not flags. `FakeBuilds` recorded `LastIncludeAssetLocation` for
one method but ignored `loadCollections` for the other. **A fake more permissive than the
real service converts an integration bug into a passing test.** It now records the flag, and
two tests assert it.

**D4 invalidated a documented guarantee.** Section 6 claims hashing the plan transitively
pins the tool. It did not: run as `dotnet release.dll` the hash was of the shared host; run
via the apphost it was of a ~70 KB native stub containing none of the IL, so
the tool's own managed code could be swapped without changing it. `stage` now takes `--tool`
pointing at the published single-file binary, hashes that, and copies it into each artifact
`_tool/` directory — which also closes D3.

### The architecture tests were largely theatre

The review's sharpest structural point. `ArchitectureTests` scanned only
the then-separate `DotNet.Release.Core` project — the one assembly that was I/O-free *by
construction* and could not have violated the guarantee. The assembly that **could** publish
was the one referencing `NuGet.Protocol`, and therefore `PackageUpdateResource.Push`, and
nothing scanned it. The only barrier was a comment in a `.csproj`.

`ArchitectureTests` now drives the PE TypeRef scan over the whole tool, forbidding
`PackageUpdateResource` and `System.Diagnostics.Process`, including non-public members. It
also asserts `PackageUpdateResource` really is present in the dependency graph — otherwise the
absence test would pass for the wrong reason and keep passing if the dependency were swapped.
Merging the projects (§8) made this strictly stronger: there is now no assembly to forget.

That is the difference between P1 and P2 being enforced and being aspirational.

### Also fixed

- **D6** — a transient NuGet.org error aborted `verify` instead of continuing to poll. Over
  roughly a thousand requests to a public feed that is a likely event, and the recovery for a
  failed verify is a re-run, which is the path that risks a fatal 409. It now logs and keeps
  polling; only the deadline fails it.
- **D7** — two same-named `.nupkg` files in different drop subdirectories could reach
  `ToDictionary` and throw a raw `ArgumentException`, bypassing the coded-error contract.
- **D8** — `schemaVersion` was never validated on the plan, so an older tool would silently
  reinterpret a future schema in the one file that gates a push.
- **R1** — `ReadStagedHashes` now enumerates `AllDirectories`. It previously agreed with the
  `*.nupkg` push glob only by coincidence, and a nested package was invisible to both.
  Recursive enumeration is strictly safer and decouples the rule from the YAML.
- **R2** — the plan hash is documented as byte-domain-sensitive, since `Get-FileHash` hashes
  raw bytes while `File.ReadAllText` strips a BOM.
- `verify` now takes `--expected-plan-hash`; it is the authoritative success signal and was
  reading an unpinned file.
- `--set` is now **required** on both publish verbs, so the marker check cannot silently
  degrade to unenforced.
- `stage` now runs `ValidateStaged` as a self-check. It was tested but never called in
  production — coverage that wasn't.

### Confirmed sound by the review

Pack-before-manifest ordering ("airtight"); `RepositoryId.FromGitHubUrl` host handling;
`BarBuildMapper` null handling; `BuildResolver` verifying commit and repository on every
path; `ReleaseSetMarker` being outside the plan hash (it can only turn a correct publish into
a failure, never the reverse); and `SourceCacheContext.NoCache`.

### Still open

`promote_workload_set` runs in parallel with `publish_packs` — both `dependsOn:
prepare_release`. Whether channel promotion must land before packages go live could not be
established from the code. It matches the pipeline being replaced, so it is not a regression,
but it is currently an accident of topology rather than a decision.

---

## 18. Open questions

1. **Asset download without `gather-drop`.** Resolved for now — `gather-drop` stays (§4).
   The client does return asset locations, so a future native download is conceivable, but
   it would have to reimplement feed selection and auth. Not worth it.
2. **Getting the tool onto the `checkout: none` publish agent.** Recommended: publish the
   CLI self-contained into the artifact and pin it via `tool.sha256` inside the plan (§6).
   The alternative — restoring a `dotnet tool` on the production agent — adds network
   dependency and feed auth to the most sensitive job. Needs confirmation against 1ES image
   constraints.
3. **Whether workload repositories should also require an explicit channel** (§9).
4. ~~**Verifying the typed client's exact API surface.**~~ **Resolved** — see §15. The
   package restored, the surface was confirmed by reflection, and `IBuildRegistry` matches
   it.

   One residual, narrowed. The two risks here were originally treated as one and should not
   be:
   - The **null-field path is confirmed in production**. BAR build 328857 has
     `gitHubRepository: null`; Arcade derives the identity from the AzDO mirror name,
     verifies it against the GitHub API, gets a 404, and nulls the field as a `LogMessage`
     rather than an error. Pinned by `Production_build_328857_has_a_null_github_repository`.
     No longer a risk.
   - The **missing-build path is still unobserved**. That a lookup for a non-existent BAR ID
     surfaces as `RestApiException` with `Response.Status == 404` is inferred from the
     client's exception model and covered by a faked exception, not seen against a live
     service, because no live BAR calls were permitted. This is the only outstanding
     verification.
