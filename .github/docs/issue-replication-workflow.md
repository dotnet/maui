# .NET MAUI issue replication workflow

The `maui-copilot` Azure DevOps pipeline supports these manual modes:

| Mode | Target | Result |
| --- | --- | --- |
| `review` | Pull request number | Existing automated PR review and AI Summary |
| `replicate` | GitHub issue number | On-device reproduction evidence and, only when fully validated, a draft product-fix PR |
| `feedback` | None | Authenticated replication-PR feedback snapshot without device work |
| `publication-smoke` | None | Create, verify, close, and delete the branch for a temporary MauiBot draft PR |
| `replication-checks` | None | Focused trusted pipeline-contract checks without devices |
| `ios-harness-probe` | None | Run one checked-in iOS infrastructure fixture without issue generation or publication |
| `ios-vm-capability-probe` | None | Probe host Hypervisor.framework capabilities without running a guest or authorizing generated execution |

`review` remains the default. Comment-triggered `/replicate` support is intentionally deferred; use the Azure Pipeline **Run pipeline** form during the initial rollout.

The checked-in Cake directives pin bootstrap packages to `dotnet-public`, but
replication switches to the product baseline before provisioning. Replication
and the iOS harness probe therefore pass Cake's `--nuget_source` override at
invocation time, so those baseline directives do not query SDK-specific
ephemeral feeds for tooling. Package versions and product/workload restore feeds
remain unchanged. Restore failures are not ignored and retries remain bounded.

## Running replication mode

1. Select the implementation branch or the maintained reviewer branch after merge.
2. Set `Mode` to `replicate`.
3. Set `PRNumber` to `0`.
4. Set `IssueNumber` to the `dotnet/maui` issue number.
5. Choose `android`, `ios`, `windows`, or `catalyst`. Windows replication is restricted
   to the capability-free packaged Sandbox and one Windows-only Controls device
   test. Mac Catalyst uses a signed App Sandbox without network entitlements and
   proves the live process is denied `network-outbound`. iOS Simulator uses the
   shared review runners with credential-free child execution and trusted
   source/runner restrictions, but no independent outbound-network isolation.
   It does not require or produce a hypervisor-egress attestation.

Keep `PublishIssueOutcome=false` to suppress not-reproduced comments and labels
on the issue. This is not a dry-run switch for fix publication: a fully validated
fix can still become a draft PR. Keep `SupersedeExisting=false` to protect an
existing fix PR.

Maintainers can use `Mode=feedback` with both target numbers set to `0` for a lightweight authenticated snapshot. It migrates any missing attributed comments, exports discussion comments, reviews, inline comments, and commits from open `kubaflo/maui` replication PRs, and skips device reproduction. It does not create or migrate pull requests.

Use `Mode=publication-smoke` with both target numbers set to `0` to test the
real MauiBot publication credential and transport without running generated
code. The mode creates a temporary branch in `dotnet/maui`, opens a draft PR
against `dotnet/maui:main`, verifies the author, draft state, head, and base,
then closes the PR and deletes the branch. GitHub retains the closed PR record
as the durable audit result; the `ReplicationPublicationSmoke` artifact records
the same checks. Normal `replicate` mode remains ordered as reproduction,
validated product fix, evidence publication, and draft PR.

Use `Mode=ios-harness-probe`, `Platform=ios`, and both target numbers set to
`0` on the trusted `copilot/replicate-issues-pipeline` branch to diagnose iOS
workload, simulator, native Button/Label attachment and text, and XHarness execution. This
separate job uses the existing iOS pool, provisioning template, and device-test
runner against the exact pipeline checkout. It inherits that checkout's shared
Xcode pins to match its workloads, rather than the newer-Xcode override used by
the other modes; Xcode version validation stays enabled. It executes only a checked-in
infrastructure fixture, not issue-derived Sandbox code, tests, or fixes. The
`IosHarnessProbe` artifact retains raw build/runner logs and native results.
Its `scope.json` reports whether the external isolation marker was advertised;
that observation is not an attestation. A passing probe is infrastructure
evidence only, not issue reproduction, certification, or publication.

Use `Mode=ios-vm-capability-probe`, `Platform=ios`, and both target numbers set to
`0` for the separate host-capability diagnostic. It creates and destroys an empty
Hypervisor.framework VM without guest execution, devices, or networking.
Neither probe authorizes generated execution or proves network isolation;
normal iOS replication uses the review-host controls described above.

The feedback snapshot is data-only and bounded. In addition to the discussion
surfaces it includes normalized `qualityContract`, typed `selector`, `evidence`,
and advisory `review` disclosures parsed from the PR body. It never executes or
imports scripts from a PR or from feedback text; malformed values are reduced
to safe unknown fields.

Replication targets `main` in the first version. The issue must describe a scenario that can be reconstructed from its text, inline snippets, and allowed raster screenshots.

Issue ingestion preserves inline/fenced code literals, including HTML entities
and XAML, as inert untrusted data rather than decoding or stripping them.
Reported workarounds are retained in both agent and human context. Unsafe URLs,
logging directives, and instruction markers are still filtered, including
encoded forms, and attachment-looking text inside code is never downloaded.
The agent must verify a workaround experimentally before using it as a control.

## What a successful run does

1. Fetches and sanitizes the issue through the GitHub API.
2. Reconstructs a transient scenario in `Controls.Sample.Sandbox`.
3. Builds and runs it on the selected device, simulator, or desktop target.
4. Records a bounded MP4 plus GIF/PNG preview.
5. Creates the lightest supported automated test: unit or XAML for purely
   managed Android-lane scenarios, otherwise a device test. Windows, iOS, and
   Mac Catalyst require one platform-scoped Controls device test. Generated
   host UI tests are withheld; an unsupported recorded interaction remains
   an artifact rather than being replaced with a different scenario.
6. Runs the exact test normally and confirms the expected assertion fails on the unfixed baseline.
7. Authors a product fix and certifies the same exact test through four causal arms: baseline red, trigger-removed green, product-fix green, and fix-reverted red.
8. Validates, on a fresh credentialless agent, that the trusted tree matches the pinned pipeline revision, that the certification binding matches every artifact in hand, that the test patch is add-only and restricted to approved test locations, and that the fix patch changes only the validated product files.
9. Publishes evidence to the repository's public, asset-only `review-tests-assets-v2` branch.
10. Pushes the validated test and product fix to a temporary `dotnet/maui` branch using the trusted checkout credential, then MauiBot opens a `[maui-bot-fix]` draft PR against `dotnet/maui:main`.

The PR embeds the GIF/thumbnail linked to the MP4. GitHub does not provide a supported API for uploading a video attachment directly into a PR body.

If no product fix completes all validation, the run retains the reproduction, diagnostics, and any incomplete fix material as pipeline artifacts only. It does not create or migrate a reproduction-only PR. An existing reproduction-only PR does not block a later validated fix; the fix publisher may replace it automatically. An existing fix PR remains protected unless `SupersedeExisting` is explicitly enabled.

An unsupported generated-test scenario has a separate bounded
`agent/test-blocked.json` refusal channel. A valid declaration stops authoring
immediately with `unsupported_scenario`, rather than spending every attempt
asking for the same impossible test. Missing declarations retain the ordinary
repair path; malformed declarations fail closed. A refusal is not a test result
or a claim that the issue does not reproduce, and never authorizes publication.

Recorder diagnostics prioritize final verdicts and exceptions over progress
lines, then retain the most recent steps within the existing output budget.
An exit code of 134 alone does not distinguish an app crash from a host-runner
assertion failure; retained verdict and termination evidence determine the
classification. Missing observations never count as successful reproduction.
Thumbnails use the final scene of the normalized recording on every platform,
with a small decoding margin. The original clip retains its pre-trigger lead-in;
desktop teardown is excluded before choosing a frame.

When a report demonstrates an initially working interaction, the Sandbox must
perform that interaction and verify the affected callback or state before the
runtime update. An initialized result caption or unrelated pointer-delivery
counter is not that proof. Do not drop a failing prerequisite to obtain a later
bug caption; disclose an unsupported interaction instead.
The trusted `doubleTap` action performs exactly two press/release cycles at the
same located element in one bounded input sequence. It requires a locator and
`value: null`; callers cannot supply a count, duration, or executable argument.

MainPage normally has a ContentPage root. A report whose initial NavigationPage
directly hosts a TabbedPage may instead use a TabbedPage XAML root and matching
code-behind base class, with its initial children created before attachment.
The existing application host remains unchanged. Do not recreate that initial
tree with post-load navigation mutations; they introduce extra transitions
before the reported trigger. Namespace, class-identity, source and host-write
restrictions still apply.

## Test semantics

Generated reproduction tests are unconditional: they use no environment variable, command-line switch, category override, skip condition, or other opt-in gate. The exact targeted test must fail on the unfixed baseline during a normal test run.

Environment-sensitive tests must explicitly arrange and verify every required locale, platform format, time zone, theme, orientation, accessibility, permission, or input setting. They may use an environment-relative oracle only when it still proves the reported defect; runner-dependent hard-coded localized output is rejected.

Tests must preserve the issue's existing product contract. Requests for a new public API cannot be reinterpreted as failures of a different existing event, and real device actions such as rotation cannot be replaced with synthetic layout calls. Event/state tests use a non-passing sentinel and separately prove the post-trigger transition occurred before asserting its result.

Do not manufacture a regression by adding settings with documented consequences.
For example, [iOS accessibility grouping](https://learn.microsoft.com/dotnet/maui/fundamentals/accessibility#isinaccessibletree)
makes children unreachable when their parent has `IsInAccessibleTree=true`;
setting a semantic description on the parent has the same limitation. A report
that only opts a child into accessibility must not be reconstructed by adding
these parent settings and then declaring the resulting grouping a product bug.

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

Android and iOS now share a narrowly reusable native Label rendered-text
profile. It registers exactly `Label`/`LabelHandler`, attaches the same
pre-gate Label through the immutable window helper, waits for its handler and
loaded state, and compares the direct native `handler.PlatformView.Text` to an
issue-derived literal. The issue supplies the actual input and a truthful
non-trigger initializer; the profile does not hard-code an issue number or
expected text. It does not admit bitmap/layout helpers, native writes, larger
hierarchies, or simulated input. Static profile acceptance is not proof that a
particular alternate input passes on a device: all four causal arms remain
required.

The Apple Label CharacterSpacing profile keeps a common managed spacing update
after attachment while the trusted gate selects HTML versus plain-text mode
before attachment. Its oracle calls the existing
`AssertionExtensions.GetCharacterSpacing` on the same native Label's attributed
text. The helper's repository blob and normalized source hash must match the
reviewed immutable source. Only a matching positive integer-valued spacing
literal up to 10000 is accepted; zero/missing native attributes cannot pass.
The profile does not permit raw native attribute access, generated observers,
or native writes, and retains the iOS/Mac Catalyst execution boundaries.

The iOS Label tap-count profile retains one TapGestureRecognizer and one
coexisting PointerGestureRecognizer on the same Label. Its gate selects the
pre-attachment tap count; the reported managed count assignment always remains
after attachment. The unchanged oracle reads NumberOfTapsRequired from the
single native UITapGestureRecognizer on that handler, using only the exact
typed OfType/Single chain. Counts are distinct positive integer literals up to
10, and the native expectation matches the common runtime assignment. This
profile admits neither simulated input nor callback-counter proxies, native
writes, alternate views, generated observers, or other platforms. A native
infrastructure probe exercises the non-trigger observation path separately
from issue reproduction and four-arm fix verification.

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

The control must remove only the trigger. The selected test is parameterless, has exactly one trusted test attribute, may additionally have one trusted `Category`, and has no data-source or other method attribute. No generated assembly/type/other-member/parameter/return attributes are allowed; this prevents discovery-time source attribute constructors or named setters from executing. The test class implements no interfaces and inherits only from the closed trusted contract, preventing disposal or other lifecycle callbacks outside the selected method. Generated C# declares one `applyReportedTrigger = true` local as a top-level statement in the verifier-selected class/method and uses it once as a later top-level braced `if` condition. The semantic gate runs before intent-to-add, build, or baseline execution so malformed gates enter test repair. The true branch contains one direct trusted MAUI invocation or property assignment. Static calls use the exact global-qualified contract type; instance receivers use stable selected-method locals/parameters of contract type. When omission would leave the unchanged oracle in a buggy/precondition state, an optional else may perform one trusted MAUI alternate action on the same affected-state symbol. Both branches must normalize to the same property family; `SetDynamicResource` versus direct assignment additionally requires one exact pre-gate key-to-alternate-value resource mapping. Trusted code changes only the literal to `false`; both actions and every assertion stay byte-identical. Calls outside the gate are default-denied except exact lifecycle/observation operations. Await is limited to direct trusted lifecycle invocations, while foreach/using/query syntax is rejected to remove implicit source-defined calls. The closed metadata contract mirrors admitted handler constraints and MAUI/device/UIKit lifecycle/observation types, rejects unresolved/name-only exceptions, unrelated resolved APIs, source/generated or boxed types and handlers, unsafe arguments, alternate-state/value changes, implicit construction, throw paths, and vacuous top-level oracles, and reports bounded symbol/kind/syntax/file/line diagnostics. The mandatory top-level oracle directly reads a nonstatic trusted framework property that test code did not assign outside the trusted gate, including through aliases or object initializers. Its receiver is declared before the gate or comes from an exact pure observation chain over pre-gate locals; post-gate construction or rebinding cannot create a shadow oracle. Computed locals are supplemental and cannot certify the control.

Its assertion statements are compared with the reproduction's and must be byte-identical in the same order and number, it must keep the namespace, class, method, attributes, and usings so the same filter selects it, and it must not be skipped, ignored, conditioned out, commented out, or emptied. The selected method binds to exactly one trusted external xUnit, NUnit, or MSTest test attribute and may additionally carry one `Category`; data-source and other method attributes are rejected. Trusted assertions bind to the external framework contract, including bounded NUnit `Is` constraints, and at least one is a top-level statement after the trigger. Device lifecycle/setup lambdas may use resolved external members plus an explicit bounded MAUI/UIKit observation surface. Generated helpers/getters/indexers/constructors/method groups/operators, unresolved members outside that surface, source-typed objects, deconstruction/ref aliases, direct assertion writes, object-dispatching conversions, clock/random/environment state, and return/yield/goto oracle escapes fail closed. Rejections name the offending symbol/kind, bounded syntax, file, and line so test repair can act. Both sources are published alongside the result. The credential-free validator rejects a separate scene-oracle snapshot, matches the baseline to exactly one candidate-patch C# file, reruns the same semantic transformation over all candidate sources, and requires the retained variant to equal its own result.

The control shares the reproduction's output directory so the two arms can be compared, and it writes its own console and result files so it can never overwrite the reproduction's evidence. Before each run, trusted orchestration removes any prior control and verifier-machine result and checks the replacement file's freshness. A control-author retry after verification starts is permitted only when a fresh, bounded, regular, exact-schema result matches the issue/platform/test identity and records `executedCount = 0` plus an infrastructure failure. Missing, stale, malformed, mismatched, or coercive evidence forbids retry. Any result with an executed test is terminal: a changed failure mode is inconclusive and a repeated baseline failure refutes the reproduction. A passing control counts only when the trusted verifier's fresh machine result records exactly one executed selected test; a console pass banner or stale preseed cannot synthesize execution evidence. Orchestration and clean validation both require the final closed-schema result to report every requested run executed and passed, no infrastructure failure, no changed mode, and no observed failure message.

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
- Generated execution uses platform-specific controls. Linux/Android must fail live DNS, direct metadata TCP, and HTTP probes before it starts; it uses a systemd cgroup with privilege, user-manager, container-runtime, supplementary-group, and Git-hook/config escape paths removed, plus a replication-only Android manifest without `INTERNET` materialized from the immutable PR Git blob into the attested trusted-root source overrides. Host-only generated tests run in a private network namespace, so local VSTest communication remains available while neither external addresses nor the host loopback ADB server are reachable. The hosted workspace and immutable trusted scripts are explicitly remounted into a temporary home view; arbitrary home-directory content remains hidden. Trusted setup enables airplane mode, disables Wi-Fi/mobile data, installs IPv4/IPv6 guest OUTPUT chains that reject everything except loopback, removes both guest default routes, and re-verifies those controls after device-control execution. Windows uses a different boundary: trusted host code restores, builds, signs, installs, records, and drives the app, while every model-authored byte executes only in an MSIX whose source, built, and installed manifests require `appContainer` trust and declare no capabilities. Source manifests must contain no extensions. Generated, packaged, and installed manifests may contain only the complete normalized package-identity profile recorded in the attested `ReplicationWindowsWinUiRegistrations.json` snapshot; runtime validation never trusts or reads the mutable NuGet cache. The snapshot documents the Windows App SDK/WinUI, Win2D, WebView2, and MSIX-generator versions, source paths and hashes, exact per-profile extension/record counts and canonical hashes, and the provenance/raw hashes of bounded build-derived Sandbox and DeviceTests fixtures. Any unknown package identity, application extension, extra category/path/class, malformed container, duplicate, missing record, or changed normalized hash fails closed. A profile mismatch writes a bounded per-attempt extracted manifest, normalized record list, actual/expected counts and hashes, and source MSIX hash under the trusted artifact root, labels it diagnostic-only, and still throws; an observation can inform a later reviewed policy commit but can never authorize its own run. AppX operations use one attested fixed-argument Windows PowerShell 5.1 bridge because the inbox AppX module cannot load in pwsh 7. The trusted launcher verifies the running token is an AppContainer and that its package identity matches before Appium attaches. Trusted setup prewarms the exact `win-x64` runtime pack before isolation and shuts down build servers before bounded runtime-cache deletion. Mac Catalyst is ad-hoc signed with immutable App Sandbox entitlement files containing no network entitlement; Debug soft-debugger injection is disabled, the final signature is audited, and `sandbox_check` must report `network-outbound` denied for the exact executable. Device tests launch that executable directly with an empty inherited environment and write xUnit XML inside the app container; trusted host code copies the bounded result after process exit. No XHarness TCP listener or loopback exception is used. Windows and Catalyst generation is limited to one platform-suffixed Controls device test; unit, XAML, shared device, other device-project, and host UI tests are rejected.
- iOS Simulator uses the same native startup, app build/deploy, and XHarness runners as review mode, without requiring a hypervisor-egress attestation. The `ios-review-host-no-network-isolation` command mode keeps the replication credential-free child environment, trusted-runner and argument allowlists, source capability restrictions, and one platform-guarded `.iOS.cs` Controls device test. Independent outbound-network isolation is not enforced. Neither this mode nor its legacy `-EnforceNetworkIsolation` runner argument is proof of network denial on iOS; that argument retains no-restore and trusted execution controls. Fresh reproduction/control/fix/revert evidence, immutable binding, credentialless validation, and the clean publisher recheck are unchanged.
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
