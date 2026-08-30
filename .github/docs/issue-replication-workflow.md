# .NET MAUI issue replication workflow

The `maui-copilot` Azure DevOps pipeline can run in two manual modes:

| Mode | Target | Result |
| --- | --- | --- |
| `review` | Pull request number | Existing automated PR review and AI Summary |
| `replicate` | GitHub issue number | On-device reproduction evidence and, only when fully validated, a draft product-fix PR |

`review` remains the default. Comment-triggered `/replicate` support is intentionally deferred; use the Azure Pipeline **Run pipeline** form during the initial rollout.

## Running replication mode

1. Select the implementation branch or the maintained reviewer branch after merge.
2. Set `Mode` to `replicate`.
3. Set `PRNumber` to `0`.
4. Set `IssueNumber` to the `dotnet/maui` issue number.
5. Choose `android` or `windows`. Windows replication is restricted to the
   capability-free packaged Sandbox and one Windows-only Controls device test.
   iOS Simulator and Mac Catalyst remain review-only because those pools do not
   provide a verified process-tree and app egress boundary.

Maintainers can use `Mode=feedback` with both target numbers set to `0` for a lightweight authenticated snapshot. It migrates any missing attributed comments, exports discussion comments, reviews, inline comments, and commits from open `kubaflo/maui` replication PRs, and skips device reproduction. It does not create or migrate pull requests.

The feedback snapshot is data-only and bounded. In addition to the discussion
surfaces it includes normalized `qualityContract`, typed `selector`, `evidence`,
and advisory `review` disclosures parsed from the PR body. It never executes or
imports scripts from a PR or from feedback text; malformed values are reduced
to safe unknown fields.

Replication targets `main` in the first version. The issue must describe a scenario that can be reconstructed from its text, inline snippets, and allowed raster screenshots.

## What a successful run does

1. Fetches and sanitizes the issue through the GitHub API.
2. Reconstructs a transient scenario in `Controls.Sample.Sandbox`.
3. Builds and runs it on the selected device, simulator, or desktop target.
4. Records a bounded MP4 plus GIF/PNG preview.
5. Creates the lightest useful automated test:
   - unit or XAML test first;
   - device test when native/platform behavior is required;
   - UI test only when lower-level tests cannot prove the issue.
6. Runs the exact test normally and confirms the expected assertion fails on the unfixed baseline.
7. Authors a product fix and certifies the same exact test through four causal arms: baseline red, trigger-removed green, product-fix green, and fix-reverted red.
8. Validates, on a fresh credentialless agent, that the trusted tree matches the pinned pipeline revision, that the certification binding matches every artifact in hand, that the test patch is add-only and restricted to approved test locations, and that the fix patch changes only the validated product files.
9. Publishes evidence to the repository's public, asset-only `review-tests-assets-v2` branch.
10. Pushes the validated test and product fix to `MauiBot/maui` and opens a `[maui-bot-fix]` draft PR against `kubaflo/maui:main` while the workflow is being tested.

The PR embeds the GIF/thumbnail linked to the MP4. GitHub does not provide a supported API for uploading a video attachment directly into a PR body.

If no product fix completes all validation, the run retains the reproduction, diagnostics, and any incomplete fix material as pipeline artifacts only. It does not create or migrate a reproduction-only PR. An existing reproduction-only PR does not block a later validated fix; the fix publisher may replace it automatically. An existing fix PR remains protected unless `SupersedeExisting` is explicitly enabled.

## Test semantics

Generated reproduction tests are unconditional: they use no environment variable, command-line switch, category override, skip condition, or other opt-in gate. The exact targeted test must fail on the unfixed baseline during a normal test run.

Environment-sensitive tests must explicitly arrange and verify every required locale, platform format, time zone, theme, orientation, accessibility, permission, or input setting. They may use an environment-relative oracle only when it still proves the reported defect; runner-dependent hard-coded localized output is rejected.

Tests must preserve the issue's existing product contract. Requests for a new public API cannot be reinterpreted as failures of a different existing event, and real device actions such as rotation cannot be replaced with synthetic layout calls. Event/state tests use a non-passing sentinel and separately prove the post-trigger transition occurred before asserting its result.

Visible rendering tests must prove visible/native output, not only managed MAUI bounds. Size and position tests first prove the intended item's presence, identity, and location, then assert an absolute issue-derived dimension or invariant so missing or reordered content cannot masquerade as a size regression. The automated test must retain the recorded Sandbox's meaningful hierarchy, assets, sizing constraints, and dynamic action sequence; reports involving resize, rotation, mutation, scrolling, drift, disappearance, or flicker cannot be reduced to a single fixed layout.

Recordings keep the affected control and its visible state on screen. A separate result/status element starts with a visible `PASS:` or `NO BUG:` value and changes to `BUG REPRODUCED:` only when the defect is observed. Every Appium step locates that mutable result element by a stable identifier independent of its current verdict, so a completed negative run is distinguishable from element lookup or infrastructure failure. On Android, navigation targets used after text entry also require stable identifiers because the open keyboard can make visible-label lookup unreliable; the workflow does not dismiss the keyboard when doing so would change the reported trigger. The trusted runner resolves the visible native text node when the stable MAUI accessibility wrapper exposes an empty text property. Ordinary semantic text remains exact; the reserved `PASS:`, `NO BUG:`, and `BUG REPRODUCED:` verdicts may include explanatory suffixes and are resolved by exact prefix. The verdict is only supplemental; it must not replace the affected control's text, title, content, geometry, or other observable state. Transition defects show the pre-trigger reference state and post-trigger failure state continuously.

Timing-sensitive, intermittent, and race-condition reports preserve their repetition prerequisite. If an unsuccessful trigger can be reset safely, the Appium plan performs two to five reset-and-trigger cycles in one device session rather than spending full Sandbox regeneration attempts on the same one-shot plan.

Keyboard, SafeArea, and ScrollView range tests must use native inset-aware geometry and behavior rather than arbitrary deltas. System-inset propagation tests must verify a nonzero runtime inset and exercise normal root-window propagation instead of directly dispatching an inset callback to the tested child. Runtime property changes must occur after attachment when that is the reported trigger. iOS-only `.iOS.cs` tests must explicitly exclude Mac Catalyst compilation.

Tests of automatic bindable-property propagation must not call `Handler.UpdateValue` or a mapper manually unless the issue itself reports that direct API. Potentially asynchronous native refreshes use a bounded repository-standard eventual assertion or real completion event rather than an immediate sample.

### Contract-level quality and selector data

Sandbox and test proposals carry one bounded `qualityContract`. It is a
disclosure-only object with closed fields for the user-visible contract and
trigger, primary/optional independent oracle and independence rationale,
scenario/precondition/trigger/transition/observable identity, optional affected
control identity, risk-based adjacent and lifecycle states, semantic blast
radius (affected type/control, ownership, shared consumers, unchanged
behavior), media alignment, and advisory review findings. Review categories are
`grounded-product-defect`, `missing-evidence-coverage`, `advisory-hardening`,
`unsupported-speculative`, or `unknown`; grounding, confidence, and
corroboration are bounded closed values. Missing or malformed data becomes
`unknown` and can never authorize a file, write, tool, network call, execution,
selector, count, credential, gate, or publication. A lifecycle case lists
relevant lifecycle states; a static case may explicitly say stateless testing
is not applicable. No universal stateless matrix is generated.

Recording and test evidence are compared using the same scenario,
precondition, trigger, transition, observable identity, and affected-control
identity. Trusted comparison renders media alignment as `verified`, `partial`,
or `not-measured`; an agent-generated verdict is never used. The typed selector
union is centralized in the clean validator and preserved into publication and
feedback:

| Variant | Allowed test type | Trusted raw syntax |
| --- | --- | --- |
| `ui-parameterized-fixture` | UI | `FullyQualifiedName~Issue<N>` |
| `device-category-only` | device | `Category=Issue<N>` |
| `fully-qualified-name` | unit/XAML | `FullyQualifiedName=Namespace.Class.Method` |

Each selector also carries normalized project/project path/class/method/
platform and trusted discovered/executed counts. Exactly one variant must
agree with the test type and platform suffix/folder. Zero, ambiguous,
whole-suite, cross-platform, and cross-variant selections fail closed; the
model may describe a filter but cannot choose runner grammar or counts.

Device tests that customize handler registration use the repository's `EnsureHandlerCreated` pattern and register standard handlers for every attached hierarchy family alongside the custom handler. Missing-handler exceptions are classified as setup failures, not product reproductions.

The draft PR contains both the regression test and the validated product fix. A reproduction-only result is never published as a PR.

Successful publication requires both:

- empirical on-device reproduction with valid recording evidence; and
- the exact resolved project, class, and method failing with the expected assertion signature in that test's parsed failure message; and
- a product fix that passes the exact test and completes all four causal control arms.

A video alone, a test alone, a compilation error, a timeout, an infrastructure failure, or a missing snapshot baseline does not qualify.

Fix scope, fix comparison, and independent review are also contract-aware:
they record the root-cause path, ownership, dynamic state, threading, teardown,
shared consumers, unchanged behavior, and semantic blast radius, preferring a
narrow mechanism. Findings are advisory and grounded findings alone can drive
at most one bounded repair/reselection pass. Model severity alone never vetoes
a proven fix. If that pass changes the selected diff, trusted code reruns the
unchanged fix-green and restoration-red arms. A legitimate trigger-removed
control is required for publication; it is never fabricated.

## Negative control

A red test proves only that the test is red. After the reproduction verifies, the workflow runs the same test a second time with the reported trigger removed and requires it to pass.

The control must remove only the trigger. Its assertion statements are compared with the reproduction's and must be byte-identical in the same order and number, it must keep the namespace, class, method, attributes, and usings so the same filter selects it, and it must not be skipped, ignored, conditioned out, commented out, or emptied. Both sources are published alongside the result so the credential-free gate re-checks the comparison rather than trusting the manifest's description of it.

The control shares the reproduction's output directory so the two arms can be compared, and it writes its own console and result files so it can never overwrite the reproduction's evidence.

Outcomes:

- the control runs and passes: the failure depends on the reported trigger, and the candidate is graded accordingly;
- the control runs and stays red: the reproduction does not measure the defect it claims, and the candidate is rejected;
- the control could not be authored, refused as impossible, or failed to build or run: nothing is established either way, so the grade is downgraded rather than the reproduction discarded.

For an exact Windows app-crash report, the final trusted action may assert that the specific Sandbox process launched by the runner exited after a recorded ready-state check and exact trigger. Generic window loss, navigation, or a process that was already absent before the trigger remains infrastructure failure.

## Safety boundary

Issue content and generated code are untrusted.

- The pipeline never clones or downloads linked repositories, archives, binaries, scripts, packages, videos, or arbitrary files.
- Only bounded GitHub user-attachment raster screenshots may be fetched; SVG and content with invalid MIME/decoded image data are rejected and accepted images are re-encoded.
- The replication agent has no shell, network, GitHub, Azure, or publishing capability.
- Copilot write approval is exact-file only, and never names a trusted root or anything under one. A planning pass selects new issue-specific test paths, trusted code validates them, and later authoring/repair passes can modify only those files and their proposal manifest.
- Appium interactions are bounded JSON data interpreted by trusted code; the agent cannot author host-executable Appium code.
- Generated Sandbox and test sources are capability-scanned before any credentialless execution. URL-capable XML readers/resolvers (including the `DataSet`/`DataTable` filename overloads), service-model channels, platform-native web/network APIs, URI/image/browser sinks, and constructed/encoded addresses are rejected.
- Generated execution is launched inside an enforced outbound-network boundary. Linux/Android must fail live DNS, direct metadata TCP, and HTTP probes before it starts; it uses a systemd cgroup with privilege, user-manager, container-runtime, supplementary-group, and Git-hook/config escape paths removed, plus a replication-only Android manifest without `INTERNET` materialized from the immutable PR Git blob into the attested trusted-root source overrides. Host-only generated tests run in a private network namespace, so local VSTest communication remains available while neither external addresses nor the host loopback ADB server are reachable. The hosted workspace and immutable trusted scripts are explicitly remounted into a temporary home view; arbitrary home-directory content remains hidden. Trusted setup enables airplane mode, disables Wi-Fi/mobile data, installs IPv4/IPv6 guest OUTPUT chains that reject everything except loopback, removes both guest default routes, and re-verifies those controls after device-control execution. Windows uses a different boundary: trusted host code restores, builds, signs, installs, records, and drives the app, while every model-authored byte executes only in an MSIX whose source, built, and installed manifests require `appContainer` trust and declare no capabilities or extensions. The trusted launcher verifies the running token is an AppContainer and that its package identity matches before Appium attaches. Windows test generation is limited to one `.Windows.cs` Controls device test; unit, XAML, shared device, other device-project, and host UI tests are rejected. iOS Simulator and Mac Catalyst remain withheld.
- Product-fix scope uses one default-deny classifier everywhere. Source generators, analyzers, build tasks/targets, tooling, Resizetizer, provisioning, workload/packaging roots, generated/assembly-wide inputs, tests, and runtime-tree files linked into Roslyn/build projects can never be model-authored fix paths. Every changed product file is scanned in full after the patch is applied, so a deletion, condition inversion, or harmless-looking edit cannot activate a dangerous sink that was already present elsewhere in the file.
- Trusted scripts run builds, tests, Appium, recording, patch validation, uploads, and publication.
- The publisher runs from a clean trusted checkout, validates artifacts before extracting the persisted checkout credential, and never executes generated code.
- The checkout credential is scoped to immutable asset publication, fix-branch push, and draft PR creation, then cleared.

### Trusted-tree attestation

The trusted copy of `.github/scripts`, `.github/skills`, and `eng/scripts` used to be
protected by `chmod -R a-w` alone — a mode bit the same user can remove, on a tree
nothing ever re-read. It is now materialized from the immutable
`$(Build.SourceVersion)` Git blobs and attested. Using Git blobs also prevents a
Windows CRLF checkout from producing different hashes than the Linux validation
agent for identical repository content.

`Assert-TrustedTreeAttestation.ps1` captures a canonical manifest at
`$(Build.SourceVersion)`: every file by normalized relative path, mode, size and
SHA-256, with regular-file and no-link enforcement, reduced to a deterministic
tree hash. Beside it the attestation records a mode-free content hash (for
cross-agent comparison), the pipeline source commit, the SHA-256 of
`ci-copilot.yml`, and the digest of each key runner and gate script. The document
is written outside the tree it describes and travels with the artifacts as
`trusted-tree.json`.

`Replicate-Issue.ps1` re-verifies the whole tree immediately before and
immediately after every model invocation and every generated app, test, or fix
execution, in a `finally` so a phase that threw cannot leave a mutated tree for
the next one. A mutated file that kept its name, an added file, a deleted file, a
symlink replacing a regular file, and a changed mode all fail closed. No
agent-authored artifact can replace a trusted script or gate.

Product fixes are complete-source and syntax-aware, not added-line scans.
Trusted validation applies the strictly parsed patch to a clean checkout,
compares the result with the trusted `HEAD` pre-image, and capability-scans every
complete resulting source file. Offline Roslyn and XML parsing additionally
compare changed C# members, conditional regions, declarations, and XAML
elements. This catches deletion-only guard removal, condition inversion, and
cross-member activation of an existing process/network/file/native/reflection
sink. The policy is deliberately conservative: a file that already contains a
prohibited capability is outside model-authored fix scope even when the proposed
edit is elsewhere in that file. The publisher repeats the same
apply/scan/restore check before credentials. Unsupported syntax and whole-member
removal fail closed.

### Environment allowlist for generated execution

Generated code and everything it starts get a constructed environment rather than
the agent's. `Get-ReplicationExecutionEnvironment` keeps only explicitly required
runtime variables — `PATH`, `HOME`, temp, `DOTNET_*`, `MSBUILD*`, Java, Android,
Xcode, Appium, and the device UDID — and `Invoke-BoundedProcess` clears the
inherited set before starting the child. `GH_TOKEN`, `GITHUB_TOKEN`,
`COPILOT_GITHUB_TOKEN`, `SYSTEM_ACCESSTOKEN`, `AZURE_*`, service-connection
endpoints, git askpass/config/credential-helper variables, proxy variables,
NuGet and dotnet feed credentials, and anything shaped like a token, secret,
password, key, or PAT are absent from children and grandchildren alike.

A per-run tracer, `MAUI_REPLICATION_SECRET_CANARY`, is set only on the replicate
step. It must never reach a generated process, an artifact, a log, a patch, or a
JSON document; `Assert-ReplicationNoSecretMarkers` fails the run when it — or a
real credential shape — appears in what the run published.

The allowlisted environment is passed to a trusted isolation launcher. Android's
`Invoke-ReplicationNetworkIsolatedProcess.ps1` enters the cgroup boundary, sets a
private attestation marker, proves DNS, direct TCP, and HTTP cannot leave the job,
removes the marker, and only then starts the requested runner. Windows admits only
three exact trusted host runners; their generated payload can be activated only
after source/MSIX/installed-manifest and process-token checks. A missing tool,
failed boundary check, forbidden capability, wrong package/process identity, or
unsupported lane stops before generated code executes.

### Certification binding

Every `candidate.json` is written alongside a `certification-binding.json` that
says what the grade was earned on: the trusted pipeline source commit and
`ci-copilot.yml` digest, the trusted tree hash, the replication base SHA, the
execution HEAD SHA, the SHA-256 of `test.patch` and `fix.patch`, the typed
selector identity together with the trusted discovered and executed counts, the
trusted verifier/runner/validator/orchestrator digests, and the SHA-256 of every
evidence file. One digest covers the whole set, taken over a canonical rendering
that does not depend on the JSON serializer.

The binding does not grade anything. The four causal arms remain the only source
of a certification level; the binding only refuses to let a level be read as
applying to inputs it was not computed over. A mismatch, a missing field, a
malformed field, or a field nobody expected all fail closed.

### Where validation happens

Certification runs in `ValidateReplication`, a stage of its own between the
device stage and the publisher:

| Stage | Credentials | What it does |
| --- | --- | --- |
| `ReviewPR` | `COPILOT_GITHUB_TOKEN`, `GH_COMMENT_TOKEN` | Runs models and generated code on a device agent; attests the trusted tree and writes the binding |
| `ValidateReplication` | none | Fresh agent, clean `persistCredentials: false` checkout of `$(Build.SourceVersion)`; re-derives the trusted tree, validates the candidate, fix, test, media and binding, emits two bounded JSON documents |
| `PublishReplication` | checkout credential, `GH_COMMENT_TOKEN` | Rechecks the bundle digest and every referenced artifact hash, then publishes |

`ValidateReplication` proves at runtime that it holds no `GH_*`, `GITHUB_*`,
`COPILOT_*`, `AZURE_*`, `System.AccessToken`, service-connection, proxy, or git
credential, and that its checkout persisted neither an `extraheader` nor a
credential helper. It stages trusted validators from its own pinned clean
checkout, downloads artifacts outside that checkout, and publishes only
`validated-candidate.json` and `validation-summary.json` from outside it.

`PublishReplication` never performs the first certification. Before any
credential is in scope it recomputes the binding digest over the artifacts it
downloaded, re-hashes every file the binding references, requires both the
validated document and the summary to name that digest and the same pipeline
revision, and re-scans both artifact trees for secret markers. An artifact
mutated between the two jobs is caught there.

See `.github/instructions/ci-copilot-pipeline-security.instructions.md` for mandatory implementation rules.

## Outcomes

| Outcome | Publication behavior |
| --- | --- |
| Reproduced, exact failing test, valid media, and four-arm-certified product fix | Upload evidence and create a `[maui-bot-fix]` draft PR |
| Reproduced but no complete validated product fix | Publish pipeline artifacts only; do not create or migrate a PR |
| Issue not reproducible | Publish pipeline artifacts only |
| Test passes or fails with a different signature | Publish diagnostics only |
| Build, device, Appium, recording, or test infrastructure fails | Publish diagnostics only |
| Patch or evidence validation fails | Do not expose publisher credentials; publish diagnostics only |
| Trusted tree, certification binding, or secret-marker check fails | Do not expose publisher credentials; publish diagnostics only |
| Matching open fix PR already exists | Do not create a duplicate unless `SupersedeExisting` is enabled |
| Matching open reproduction-only PR already exists | Continue fix generation; replace it only after a validated fix is open |

Public evidence is stored under an immutable, build-specific path on
`review-tests-assets-v2` similar to:

```text
pr-<issue-number>/replication/<platform>/<build-id>-<attempt>/
```

PR media uses commit-pinned `raw.githubusercontent.com` URLs, so it renders
without Azure DevOps authentication and remains stable when the asset branch
advances.

## Publication credentials

No replication-specific token, storage account, service connection, or fork
configuration is required. The clean trusted publisher job reuses the GitHub
service-connection credential already persisted by `checkout: self`, the same
credential pattern used for UI-test screenshot assets. Candidate validation
finishes in a separate credentialless stage, and the publisher's recheck
finishes, before that credential is extracted.

## Trusted scripts and artifacts

| Trusted script | Staged into | Purpose |
| --- | --- | --- |
| `shared/Assert-TrustedTreeAttestation.ps1` | trusted capture, validation stage, publisher | Capture and verify the trusted tree; refuse write approvals inside it |
| `shared/Assert-ReplicationExecutionEnvironment.ps1` | trusted capture, validation stage, publisher | Build the generated-execution environment; scan artifacts for secret markers |
| `shared/Assert-ReplicationCertificationBinding.ps1` | trusted capture, validation stage, publisher | Write, read, and re-check the certification binding |

| Artifact | Produced by | Contents |
| --- | --- | --- |
| `ReplicationArtifacts` | `ReviewPR` | Candidate manifest, patches, evidence, verification output, `trusted-tree.json`, `certification-binding.json` |
| `ReplicationValidation` | `ValidateReplication` | Exactly `validated-candidate.json` and `validation-summary.json`, each a bounded JSON document written outside the checkout |
| `ReplicationPublication` | `PublishReplication` | Published evidence and pull request result |

A new trusted script used by replication has to be added to the Setup copy block,
to the validation stage's staging list, and to the publisher's staging list. When
the binding must name it, add it to the key-script list in
`Assert-TrustedTreeAttestation.ps1` as well.

## Initial pilot set

Before each run, confirm the issue is still open, has no equivalent active fix PR, and can be reconstructed without prohibited downloads:

- Android: `#37440`
- iOS: `#31059`
- Mac Catalyst: `#35516`
- Windows duplicate gate: `#37886`
- Windows packaged publication: `#37540`
