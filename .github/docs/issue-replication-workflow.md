# .NET MAUI issue replication workflow

The `maui-copilot` Azure DevOps pipeline can run in two manual modes:

| Mode | Target | Result |
| --- | --- | --- |
| `review` | Pull request number | Existing automated PR review and AI Summary |
| `replicate` | GitHub issue number | On-device reproduction evidence and, when fully validated, a draft failing-test PR |

`review` remains the default. Comment-triggered `/replicate` support is intentionally deferred; use the Azure Pipeline **Run pipeline** form during the initial rollout.

## Running replication mode

1. Select the implementation branch or the maintained reviewer branch after merge.
2. Set `Mode` to `replicate`.
3. Set `PRNumber` to `0`.
4. Set `IssueNumber` to the `dotnet/maui` issue number.
5. Choose `android`, `ios`, `catalyst`, or `windows`. Android is the default.

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
6. Runs the exact test with `MAUI_REPRODUCTION_ISSUE=<issue>` and confirms the expected assertion fails.
7. Validates that the patch is add-only and restricted to approved test locations.
8. Uploads evidence to public read-only Azure Blob URLs.
9. Pushes the validated patch to the configured bot fork and opens a draft PR against `dotnet/maui:main`.

The PR embeds the GIF/thumbnail linked to the MP4. GitHub does not provide a supported API for uploading a video attachment directly into a PR body.

## Test semantics

Generated reproduction tests must not intentionally fail normal MAUI CI. Each test is guarded by the exact issue number and runs only when:

```text
MAUI_REPRODUCTION_ISSUE=<issue-number>
```

The draft PR is evidence, not merge-ready work. A product fix should remove the reproduction guard and make the test pass before the test is merged.

Successful publication requires both:

- empirical on-device reproduction with valid recording evidence; and
- the exact targeted test failing with the expected assertion signature.

A video alone, a test alone, a compilation error, a timeout, an infrastructure failure, or a missing snapshot baseline does not qualify.

## Safety boundary

Issue content and generated code are untrusted.

- The pipeline never clones or downloads linked repositories, archives, binaries, scripts, packages, videos, or arbitrary files.
- Only bounded GitHub user-attachment raster screenshots may be fetched; SVG and content with invalid MIME/decoded image data are rejected and accepted images are re-encoded.
- The replication agent has no shell, network, GitHub, Azure, or publishing capability.
- Copilot write approval is exact-file only. A planning pass selects new issue-specific test paths, trusted code validates them, and later authoring/repair passes can modify only those files and their proposal manifest.
- Appium interactions are bounded JSON data interpreted by trusted code; the agent cannot author host-executable Appium code.
- Generated Sandbox and test sources are capability-scanned before any credentialless execution.
- Trusted scripts run builds, tests, Appium, recording, patch validation, uploads, and publication.
- The publisher runs from a clean checkout and validates artifacts before receiving Azure or GitHub credentials.
- Azure evidence upload and GitHub PR publication use separate least-privilege credentials.

See `.github/instructions/ci-copilot-pipeline-security.instructions.md` for mandatory implementation rules.

## Outcomes

| Outcome | Publication behavior |
| --- | --- |
| Reproduced, exact failing test, valid media, safe patch | Upload evidence and create a draft PR |
| Issue not reproducible | Publish pipeline artifacts only |
| Test passes or fails with a different signature | Publish diagnostics only |
| Build, device, Appium, recording, or test infrastructure fails | Publish diagnostics only |
| Patch or evidence validation fails | Do not expose publisher credentials; publish diagnostics only |
| Matching open reproduction PR already exists | Do not create a duplicate |

Public evidence is stored under an immutable path similar to:

```text
maui-copilot/issue-<number>/<platform>/<build-id>/
```

Retention is controlled by the configured Azure Storage lifecycle policy.

## Required pipeline configuration

- `MAUI_REPLICATION_AZURE_SERVICE_CONNECTION`: federated Azure service connection with create-only access to the evidence container.
- `MAUI_REPLICATION_STORAGE_ACCOUNT`, `MAUI_REPLICATION_STORAGE_CONTAINER`, and `MAUI_REPLICATION_PUBLIC_BASE_URL`: public anonymous-read evidence container and lifecycle-managed Blob endpoint.
- `MAUI_REPLICATION_FORK_OWNER` and `MAUI_REPLICATION_FORK_REPOSITORY`: bot fork target.
- Dedicated `GH_REPLICATION_TOKEN` able to push to the bot fork and open draft PRs in `dotnet/maui`.

The pipeline variables are documented next to the `PublishReplication` job in `eng/pipelines/ci-copilot.yml`.

## Initial pilot set

Before each run, confirm the issue is still open, has no equivalent active fix/reproduction PR, and can be reconstructed without prohibited downloads:

- Android: `#37440`
- iOS: `#31059`
- Mac Catalyst: `#36716`

Windows recording and runner behavior is covered by focused validation in the implementation PR; a live Windows issue is not required for the initial pilot.
