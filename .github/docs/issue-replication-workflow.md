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
5. Choose `android`, `ios`, `catalyst`, or `windows`. Android is the default.

Maintainers can use `Mode=feedback` with both target numbers set to `0` for a lightweight authenticated snapshot. It migrates any missing attributed comments, exports discussion comments, reviews, inline comments, and commits from open `kubaflo/maui` replication PRs, and skips device reproduction. It does not create or migrate pull requests.

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
8. Validates that the test patch is add-only and restricted to approved test locations and that the fix patch changes only the validated product files.
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

Device tests that customize handler registration use the repository's `EnsureHandlerCreated` pattern and register standard handlers for every attached hierarchy family alongside the custom handler. Missing-handler exceptions are classified as setup failures, not product reproductions.

The draft PR contains both the regression test and the validated product fix. A reproduction-only result is never published as a PR.

Successful publication requires both:

- empirical on-device reproduction with valid recording evidence; and
- the exact resolved project, class, and method failing with the expected assertion signature in that test's parsed failure message; and
- a product fix that passes the exact test and completes all four causal control arms.

A video alone, a test alone, a compilation error, a timeout, an infrastructure failure, or a missing snapshot baseline does not qualify.

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
- Copilot write approval is exact-file only. A planning pass selects new issue-specific test paths, trusted code validates them, and later authoring/repair passes can modify only those files and their proposal manifest.
- Appium interactions are bounded JSON data interpreted by trusted code; the agent cannot author host-executable Appium code.
- Generated Sandbox and test sources are capability-scanned before any credentialless execution.
- Trusted scripts run builds, tests, Appium, recording, patch validation, uploads, and publication.
- The publisher runs from a clean trusted checkout, validates artifacts before extracting the persisted checkout credential, and never executes generated code.
- The checkout credential is scoped to immutable asset publication, fix-branch push, and draft PR creation, then cleared.

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
finishes before that credential is extracted.

## Initial pilot set

Before each run, confirm the issue is still open, has no equivalent active fix PR, and can be reconstructed without prohibited downloads:

- Android: `#37440`
- iOS: `#31059`
- Mac Catalyst: `#35516`

Windows recording and runner behavior is covered by focused validation in the implementation PR; a live Windows issue is not required for the initial pilot.
