---
name: agentic-labeler
description: >-
  Labels issues and pull requests in the dotnet/maui repository based on
  technical content, area-matching rules, and platform-file conventions.
  Used by the gh-aw agentic-labeler workflow and available for batch
  evaluation and interactive Copilot CLI usage.
metadata:
  author: dotnet-maui
  version: "1.0"
---

# Agentic Labeler

Labeling rules for the [dotnet/maui](https://github.com/dotnet/maui) repository. These rules are the canonical source of truth for how issues and PRs should be labeled. They are consumed by the `agentic-labeler` gh-aw workflow and can also be used standalone for batch evaluation or interactive labeling.

## Label discovery

- Fetch the current list of labels using the `list_label` MCP tool (provided by the `labels` toolset). Note the **singular** name — it is `list_label`, not `list_labels`.
- **Important pagination caveat:** the `list_label` tool only returns the first ~100 labels (no pagination). This repo has ~440 labels, so many `area-*`, `platform/*`, and status labels will be missing from the listing. If you have a strong candidate label name in mind that isn't in the listing, **verify it exists** with the `get_label` tool before adding it. The label families enumerated below (`area-*`, `platform/*`, `t/*`, `s/*`, `i/*`, `p/*`) are reliable guides; use `get_label` for anything else.
- You may apply **any** existing label, not just `area-*` and `platform/*`. Examples of other useful label families that exist in this repo (with **exact** names — emoji suffixes are part of the label and must be matched verbatim):
  - **Kind:** `t/bug`, `t/enhancement ☀️`, `t/docs 📝`, `t/breaking 💥`, `t/native-embedding`, `t/desktop`, `t/a11y`
  - **Status / signal (issues):** `i/regression`, `s/needs-repro`, `s/needs-info`, `s/needs-attention`, `s/duplicate 2️⃣`, `s/no-repro`, `s/not-a-bug`
  - **Priority:** `p/0`, `p/1`, `p/2`, `p/3`
- **PR-specific status caveat:** **do not** apply `s/needs-info` or `s/needs-repro` to pull requests — repo automation rewrites or removes them and posts a comment. On PRs, use `s/pr-needs-author-input` instead when more information is needed.
- Do **not** create new labels. Only labels that already exist in the repository will be accepted.

## Labeling rules

### `area-*` labels (issues and PRs)

Pick one or more `area-*` labels based on the subject matter:

- Specific control mentioned → matching `area-controls-<name>` (e.g., `CollectionView` → `area-controls-collectionview`, `Entry` → `area-controls-entry`, `Map` / `Maps` → `area-controls-map`, `Window` → `area-controls-window`, `WebView` → `area-controls-webview`, `HybridWebView` → `area-controls-hybridwebview`). **Always** use the `area-controls-<name>` prefix — never invent shorter aliases (e.g., the Maps area is `area-controls-map`, **not** `area-maps`).
- Layout, measure/arrange, sizing issues → `area-layout`.
- Navigation, Shell routing, page navigation → `area-navigation` (or `area-controls-shell` when Shell-specific).
- XAML parsing, markup extensions, XamlC, source generators → `area-xaml`.
- Hot reload, build, MSBuild, workload, project templates, tooling → `area-tooling`, `area-templates`, or `area-setup` as appropriate.
- BlazorWebView / Blazor hybrid → `area-blazor`.
- Essentials APIs (non-UI: connectivity, sensors, preferences, etc.) → `area-essentials`.
- Drawing / Microsoft.Maui.Graphics → `area-drawing`.
- Gestures (tap, pan, swipe, pinch) → `area-gestures`.
- Lifecycle, hosting, app startup, DI → `area-core-lifecycle` / `area-core-hosting`.
- Dispatcher / main thread / threading → `area-core-dispatching`.
- Localization / RTL / culture → `area-localization`.
- Docs only → `area-docs`.
- **CI, build pipelines, Maestro / dependency flow, branch mirroring, GitHub workflows, agentic-workflow / skill files (when these are the primary subject of the PR; see Mixed PRs below)** → `area-infrastructure`. This covers:
  - `[dnceng-bot]` codeflow/branch-mirroring issues (the standard "Branch `…` can't be mirrored to Azdo" issues) → `area-infrastructure` (do **not** noop these — they have a clear area).
  - PRs touching only `.github/workflows/`, `.github/skills/`, `.github/scripts/`, `eng/pipelines/`, `eng/common/`, or other CI/agent-infra files → `area-infrastructure` (prefer this over `area-tooling`, which is for the dev-build/MSBuild/workload surface that ships to users).
  - **Mixed PRs (infra-primary + small product edits):** if the PR is dominated by CI/agent-infra changes but also has incidental edits to product code, still apply `area-infrastructure` (alongside any relevant `area-*` for the product area). If the product-code change is the focus and the infra change is incidental (e.g., a small workflow tweak that supports a feature), prefer the product `area-*` label and omit `area-infrastructure`.

Prefer the most specific label. It is fine to apply both a generic and a specific area label (e.g., `area-layout` + `area-controls-collectionview`) when both clearly apply.

### `platform/*` labels

This is the most important behavior for PRs.

**For pull requests**, infer `platform/*` labels primarily from the **changed files**, using the rules below. Each rule maps a file pattern to one or more platform labels. Apply a `platform/*` label if **any** changed file matches that pattern. The path patterns intentionally target the established MAUI source-layout conventions (`Platform/<Name>/` and `Platforms/<Name>/`) — do not match on bare `/Android/`, `/iOS/`, `/Windows/`, etc., as those occur in templates, docs, and unrelated tooling paths.

Note on iOS / MacCatalyst: file-extension patterns and directory patterns map differently because of MAUI's compilation conventions — they are split into separate rows below.

| File pattern (changed in the PR) | Label(s) to apply |
| --- | --- |
| `*.android.cs`, `*.Android.cs`, paths containing `/Platform/Android/`, `/Platforms/Android/`, `/AndroidNative/`, or handler subdirectories like `/Handlers/*/Android/` | `platform/android` |
| `*.ios.cs`, `*.iOS.cs` (file-extension pattern — these compile for **both** iOS and MacCatalyst) | `platform/ios` **and** `platform/macos` |
| Paths containing `/Platform/iOS/` or `/Platforms/iOS/` (directory pattern — these compile **only** for the iOS TFM) | `platform/ios` only |
| `*.maccatalyst.cs`, `*.MacCatalyst.cs`, paths containing `/Platform/MacCatalyst/`, `/Platforms/MacCatalyst/` | `platform/macos` |
| `*.windows.cs`, `*.Windows.cs`, paths containing `/Platform/Windows/`, `/Platforms/Windows/` | `platform/windows` |
| `*.tizen.cs`, paths containing `/Platform/Tizen/`, `/Platforms/Tizen/` | `platform/tizen` |

Notes:

- If a PR touches **only shared / cross-platform code** (e.g., `src/Core/src/*.cs` without a platform suffix, or `src/Controls/src/Core/`), do **not** apply any `platform/*` label.
- If a PR touches **multiple platforms**, apply each matching `platform/*` label.
- `.ios.cs` files compile for both iOS and MacCatalyst (see split table rows above).
- `.maccatalyst.cs` files do **not** compile for iOS — apply only `platform/macos` for those.

**For issues**, infer `platform/*` labels only if the reporter clearly indicates a platform (explicit mention of Android / iOS / macOS / Windows / Tizen in the title, body, or attached logs/stack traces). Do not guess. If the report says "all platforms" or doesn't specify, apply no `platform/*` label.

### When to noop (no labels)

Some items should **not** be labeled. If any of the following apply, skip labeling entirely:

- **Automated inter-branch merge PRs** — titles like `[automated] Merge branch 'main' => 'net11.0'` or similar bot-created merge PRs. These are infrastructure, not feature/bug work.
- **Dependency bump PRs** that already have `dependencies` and `area-infrastructure` labels.
- **Items where no label clearly fits** — when the content is too vague or ambiguous to determine area or platform with confidence.

> ⚠️ **Do NOT noop `[dnceng-bot]` codeflow/branch-mirroring issues.** Despite being bot-authored, they have a clear area (`area-infrastructure`) and should be labeled, not noop'd. The noop rule for automated PRs above is specifically about `[automated] Merge branch …` titles.

### What NOT to do

- Do **not** create new labels — apply only labels that already exist in the repository.
- Do **not** add `platform/*` labels to PRs that don't touch platform-specific files.
- Do **not** post a comment summarizing the labels — labels speak for themselves.
- Do **not** close, lock, or otherwise modify the issue/PR beyond labeling.
- Do **not** label automated merge PRs — these are infrastructure, not actionable items.
- Be conservative; precision beats recall. Only apply labels that clearly fit.
