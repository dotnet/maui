---
name: agentic-labeler
description: >-
  Labels issues and pull requests in the dotnet/maui repository with
  `area-*` and `platform/*` labels ONLY, based on technical content and
  platform-file conventions. Used by the gh-aw agentic-labeler workflow
  and available for batch evaluation and interactive Copilot CLI usage.
metadata:
  author: dotnet-maui
  version: "2.0"
---

# Agentic Labeler

Labeling rules for the [dotnet/maui](https://github.com/dotnet/maui) repository. These rules are the canonical source of truth for how issues and PRs should be labeled. They are consumed by the `agentic-labeler` gh-aw workflow and can also be used standalone for batch evaluation or interactive labeling.

## 🚨 Scope: `area-*` and `platform/*` ONLY

The labeler applies **only two label families**, and nothing else:

1. **Exactly one `area-*`** — derived from the subject matter (control name, area like layout / navigation / xaml / infrastructure / etc.). Choose the single most specific match for the dominant subsystem; see the tie-breaking rules below.
2. **One or more `platform/*`** — derived from changed-file platform conventions on PRs, or from explicit platform mentions on issues. Apply all that fit. **Exception:** `platform/tizen` is **never** applied by this labeler under any circumstance, even when Tizen files are touched or Tizen is named in an issue (see the platform-label rules below).

**The labeler must NOT apply any other label, ever.** Specifically, **do not** apply:

- `t/*` (kind: `t/bug`, `t/enhancement ☀️`, `t/docs 📝`, `t/breaking 💥`, `t/native-embedding`, `t/desktop`, `t/a11y`, etc.) — the issue/PR author or other automation owns these.
- `i/*` (indicators: `i/regression`, etc.) — set during triage based on investigation, not initial content.
- `s/*` (status: `s/needs-info`, `s/needs-repro`, `s/needs-verification`, `s/needs-attention`, `s/triaged`, `s/verified`, `s/no-repro`, `s/not-a-bug`, `s/duplicate 2️⃣`, `s/pr-needs-author-input`, etc.) — managed by `dotnet-policy-service[bot]` and human triagers.
- `p/*` (priority: `p/0`, `p/1`, `p/2`, `p/3`) — set by maintainers.
- `partner/*` (e.g., `partner/syncfusion`) — set by partner-tracking automation.
- `perf/*` (e.g., `perf/memory-leak 💦`) — set during perf investigation.
- `backport/*`, `regressed-in-*`, `version/*` — set during triage / release management.
- `untriaged`, `:watch: Not Triaged` — applied by repo automation on issue open.
- Anything else that is not literally an `area-*` or `platform/*` label.

If the only labels that clearly apply are not `area-*` or `platform/*`, **noop** instead — see the noop section below.

If neither an `area-*` nor a `platform/*` label clearly applies, **noop**.

## Label discovery

- Fetch the current list of labels using the `list_label` MCP tool (provided by the `labels` toolset). Note the **singular** name — it is `list_label`, not `list_labels`.
- **Important pagination caveat:** the `list_label` tool only returns the first ~100 labels (no pagination). This repo has ~440 labels, so many `area-*` and `platform/*` labels will be missing from the listing. If you have a strong candidate `area-*` or `platform/*` label name in mind that isn't in the listing, **verify it exists** with the `get_label` tool before adding it.
- Do **not** create new labels — only labels that already exist in the repository will be accepted.

## Labeling rules

### `area-*` label (issues and PRs) — exactly one

**Apply exactly one `area-*` label.** Pick the single most specific match for the dominant subsystem:

- Specific control mentioned → matching `area-controls-<name>` (e.g., `CollectionView` → `area-controls-collectionview`, `Entry` → `area-controls-entry`, `Map` / `Maps` → `area-controls-map`, `Window` → `area-controls-window`, `WebView` → `area-controls-webview`, `HybridWebView` → `area-controls-hybridwebview`). **Always** use the `area-controls-<name>` prefix — never invent shorter aliases (e.g., the Maps area is `area-controls-map`, **not** `area-maps`).
- Layout, measure/arrange, sizing issues → `area-layout`.
- Navigation, Shell routing, page navigation → `area-navigation` (or `area-controls-shell` when Shell-specific).
- XAML parsing, markup extensions, XamlC, source generators → `area-xaml`.
- Installation, workload availability, target-framework recognition, requirements, and platform support → `area-setup`.
- Hot reload, debugging, editor experiences, build tasks, and MSBuild/tooling behavior → `area-tooling`.
- Project templates → `area-templates`.
- BlazorWebView / Blazor hybrid → `area-blazor`.
- Essentials APIs (non-UI: connectivity, sensors, preferences, etc.) → `area-essentials`.
- Drawing / Microsoft.Maui.Graphics → `area-drawing`.
- Gestures (tap, pan, swipe, pinch) → `area-gestures`.
- Lifecycle, hosting, app startup, DI → `area-core-lifecycle` / `area-core-hosting`.
- Dispatcher / main thread / threading → `area-core-dispatching`.
- Localization / RTL / culture → `area-localization`.
- Docs only → `area-docs`.
- **Copilot CLI agents, agent skills, agentic workflows, and AI-assisted development** → `area-ai-agents`. This covers `.github/agents/**`, `.github/skills/**`, gh-aw workflow sources and generated locks, and supporting review/test/fix orchestration when the AI-agent behavior is the primary subject.
- **CI, build pipelines, Maestro / dependency flow, branch mirroring, and generic GitHub Actions infrastructure** → `area-infrastructure`. This covers:
  - `[dnceng-bot]` codeflow/branch-mirroring issues (the standard "Branch `…` can't be mirrored to Azdo" issues) → `area-infrastructure` (do **not** noop these — they have a clear area).
  - PRs touching only generic `.github/workflows/*.yml`, `.github/scripts/`, `eng/pipelines/`, `eng/common/`, or other CI infrastructure files → `area-infrastructure` (prefer this over `area-tooling`, which is for the dev-build/MSBuild/workload surface that ships to users).
  - **AI-agent vs infrastructure tie-break:** choose `area-ai-agents` when the change primarily alters an agent's behavior, prompt, skill, evaluation, or AI-assisted developer workflow. Command parsing and trigger/dispatch behavior for AI-assisted review count as agent behavior. Choose `area-infrastructure` when the change primarily alters generic CI execution, authentication, scheduling, or pipeline plumbing without changing the AI-assisted workflow's behavior, even if an agent consumes that infrastructure.
  - **Mixed PRs (infra-primary + small product edits):** if the PR is dominated by CI infrastructure changes but also has incidental edits to product code, still apply `area-infrastructure` (and omit any product `area-*`). If the product-code change is the focus and the infra change is incidental (e.g., a small workflow tweak that supports a feature), prefer the product `area-*` label and omit `area-infrastructure`.

**Tie-breaking when multiple areas could apply** — pick the single most specific:

- **Specific control beats generic area.** `area-controls-tabbedpage` over `area-navigation`; `area-controls-collectionview` over `area-layout`; `area-controls-shell` over `area-navigation`.
- **Sub-area beats parent area.** `area-safearea` over `area-layout`; `area-core-dispatching` over `area-core-lifecycle`.
- **Subject-matter focus beats incidental touch.** If a PR fixes a CollectionView bug by adjusting layout code, the area is the control (`area-controls-collectionview`), not the layout system.
- **When genuinely tied**, prefer the area that names the user-visible feature over the implementation-detail area.

If after applying these heuristics there is still no single best fit, **noop** rather than apply two area labels.

### `platform/*` labels

This is the most important behavior for PRs.

**For pull requests**, infer `platform/*` labels primarily from the **changed files**, using the rules below. Each rule maps a file pattern to one or more platform labels. Apply a `platform/*` label if **any** changed file matches that pattern. The path patterns intentionally target the established MAUI source-layout conventions — match the patterns in the table below (e.g., `/Platform/<Name>/`, `/Platforms/<Name>/`, `/Handlers/*/<Name>/`). Do **not** match on a bare top-level `/Android/`, `/iOS/`, `/Windows/`, or `/MacCatalyst/` segment that is not part of one of the patterns in the table — bare segments occur in templates, docs, and unrelated tooling paths and are not platform-specific source code.

Note on iOS / MacCatalyst: file-extension patterns and directory patterns map differently because of MAUI's compilation conventions — they are split into separate rows below.

| File pattern (changed in the PR) | Label(s) to apply |
| --- | --- |
| `*.android.cs`, `*.Android.cs`, paths containing `/Platform/Android/`, `/Platforms/Android/`, `/AndroidNative/`, or handler subdirectories like `/Handlers/*/Android/` | `platform/android` |
| `*.ios.cs`, `*.iOS.cs` (file-extension pattern — these compile for **both** iOS and MacCatalyst) | `platform/ios` **and** `platform/macos` |
| Paths containing `/Platform/iOS/`, `/Platforms/iOS/`, or handler subdirectories like `/Handlers/*/iOS/` (directory pattern — these compile **only** for the iOS TFM) | `platform/ios` only |
| `*.maccatalyst.cs`, `*.MacCatalyst.cs`, paths containing `/Platform/MacCatalyst/`, `/Platforms/MacCatalyst/`, or handler subdirectories like `/Handlers/*/MacCatalyst/` | `platform/macos` |
| `*.windows.cs`, `*.Windows.cs`, paths containing `/Platform/Windows/`, `/Platforms/Windows/`, or handler subdirectories like `/Handlers/*/Windows/` | `platform/windows` |

Notes:

- If a PR touches **only shared / cross-platform code** (e.g., `src/Core/src/*.cs` without a platform suffix, or `src/Controls/src/Core/`), do **not** apply any `platform/*` label.
- If a PR touches **multiple platforms**, apply each matching `platform/*` label.
- `.ios.cs` files compile for both iOS and MacCatalyst (see split table rows above).
- `.maccatalyst.cs` files do **not** compile for iOS — apply only `platform/macos` for those.
- **Tizen is excluded.** `*.tizen.cs` files and `/Platform/Tizen/` or `/Platforms/Tizen/` paths exist in the source tree, but `platform/tizen` is **never** auto-applied. Treat Tizen files as if they had **no** platform suffix for labeling purposes: pick an `area-*` label normally based on the code's subject matter (e.g., `TabbedPage.tizen.cs` → `area-controls-tabbedpage`) and apply **no** `platform/*` label for the Tizen content. Only noop if the global noop rules below (e.g., no `area-*` clearly fits) apply on their own merits.

**For issues**, infer `platform/*` labels only for platforms the reporter explicitly identifies as **affected**: Android, iOS, macOS / Mac Catalyst, or Windows. This includes the issue-template's "Affected platforms" field, plus clear evidence in the title, body, or attached logs/stack traces. Do not guess.

- If the reporter explicitly lists named affected platforms, apply one `platform/*` label per named platform — even when the list covers every supported platform (e.g., "iOS, Android, Windows, macOS" → apply all four).
- Generic phrases like "all platforms", "every platform", "any platform", or "cross-platform" do **not** by themselves justify any `platform/*` label. **If a generic phrase is accompanied by an explicit affected-platform list, the named list wins** — apply each named platform's label (e.g., "all platforms (iOS, Android, Windows, macOS)" → apply all four).
- Do **not** apply labels for platforms mentioned incidentally ("tested on iOS"), as "not reproduced" / "not affected", or as label requests ("please add platform/android").
- **Never apply `platform/tizen`** — even when the reporter names Tizen, includes Tizen in an "Affected platforms" enumeration, or attaches Tizen logs. Tizen mentions are silently dropped from the platform set.

### When to noop (no labels)

Some items should **not** be labeled. If any of the following apply, skip labeling entirely:

- **Automated inter-branch merge PRs** — titles like `[automated] Merge branch 'main' => 'net11.0'` or similar bot-created merge PRs. These are infrastructure, not feature/bug work.
- **Dependency bump PRs** that already have `dependencies` and `area-infrastructure` labels.
- **Items where no `area-*` or `platform/*` label clearly fits** — when the content is too vague or ambiguous to determine area or platform with confidence, or when the only labels that would apply are outside the allowed `area-*` / `platform/*` scope.

> ⚠️ **Do NOT noop `[dnceng-bot]` codeflow/branch-mirroring issues.** Despite being bot-authored, they have a clear area (`area-infrastructure`) and should be labeled, not noop'd. The noop rule for automated PRs above is specifically about `[automated] Merge branch …` titles.

### What NOT to do

- Do **not** apply any label that is not literally `area-*` or `platform/*`. No `t/*`, `i/*`, `s/*`, `p/*`, `partner/*`, `perf/*`, `backport/*`, `regressed-in-*`, `version/*`, `untriaged`, `:watch: Not Triaged`, or anything else. See the "Scope" section at the top for the full prohibition.
- Do **not** apply `platform/tizen` under any circumstance — Tizen is excluded from this labeler even when Tizen files are touched or Tizen is named in an issue body, title, or affected-platforms field.
- Do **not** create new labels — apply only labels that already exist in the repository.
- Do **not** add `platform/*` labels to PRs that don't touch platform-specific files.
- Do **not** post a comment summarizing the labels — labels speak for themselves.
- Do **not** close, lock, or otherwise modify the issue/PR beyond labeling.
- Do **not** label automated merge PRs — these are infrastructure, not actionable items.
- Be conservative; precision beats recall. Only apply `area-*` or `platform/*` labels that clearly fit.
