---
description: Detects UI test and device test categories in PR diffs and posts a comment listing which test categories should run on CI pipelines
on:
  pull_request:
    types: [opened, synchronize, reopened, ready_for_review]
  issue_comment:
    types: [created]
  workflow_dispatch:
    inputs:
      pr_number:
        description: 'PR number to detect categories for'
        required: true
        type: number

if: >-
  (github.event_name == 'pull_request' && github.event.pull_request.draft == false) ||
  github.event_name == 'workflow_dispatch' ||
  (github.event_name == 'issue_comment' &&
   github.event.issue.pull_request &&
   startsWith(github.event.comment.body, '/detect-categories'))

permissions:
  contents: read
  issues: read
  pull-requests: read

engine:
  id: copilot
  model: claude-sonnet-4.6

safe-outputs:
  add-comment:
    max: 1
    target: "*"
  noop:
  messages:
    footer: "> 🏷️ *Category detection by [{workflow_name}]({run_url})*"
    run-started: "🔍 Detecting test categories… [{workflow_name}]({run_url})"
    run-success: "✅ Category detection complete! [{workflow_name}]({run_url})"
    run-failure: "❌ Category detection failed. [{workflow_name}]({run_url}) {status}"

tools:
  github:
    toolsets: [default]

network: defaults

concurrency:
  group: "detect-categories-${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number || github.run_id }}"
  cancel-in-progress: true

timeout-minutes: 15

steps:
  - name: Gate — skip if no relevant source files in diff
    if: github.event_name == 'pull_request'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number }}
    run: |
      RELEVANT_FILES=$(gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only \
        | grep -E '\.(cs|xaml)$' \
        | grep -iE '(src/Controls/|src/Core/|src/Essentials/|src/Graphics/|src/BlazorWebView/|tests/)' \
        || true)
      if [ -z "$RELEVANT_FILES" ]; then
        echo "⏭️ No relevant source or test files found in PR diff. Skipping category detection."
        exit 1
      fi
      echo "✅ Found relevant files to analyze:"
      echo "$RELEVANT_FILES" | head -30

  - name: Gather PR diff analysis
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}
    run: |
      echo "Gathering PR analysis for PR #$PR_NUMBER..."

      DIFF=$(gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" 2>/dev/null || true)
      if [ -z "$DIFF" ]; then
        echo "NO_DIFF=true" > "$GITHUB_WORKSPACE/category-results.txt"
        echo "⚠️ Could not fetch PR diff"
        exit 0
      fi

      # ── UI Test Categories ───────────────────────────────────────────
      # Extract [Category(UITestCategories.X)] from added lines
      UI_CATEGORIES=$(echo "$DIFF" \
        | grep -E '^\+.*\[Category\(UITestCategories\.' \
        | grep -oE 'UITestCategories\.[A-Za-z0-9_]+' \
        | sed 's/UITestCategories\.//' \
        | sort -u \
        || true)

      UI_NAMEOF=$(echo "$DIFF" \
        | grep -E '^\+.*\[Category\(' \
        | grep -oE 'nameof\(UITestCategories\.[A-Za-z0-9_]+\)' \
        | sed 's/nameof(UITestCategories\.//;s/)//' \
        | sort -u \
        || true)

      UI_QUOTED=$(echo "$DIFF" \
        | grep -E '^\+.*\[Category\("' \
        | grep -oE '\[Category\("[A-Za-z0-9_]+"\)' \
        | sed 's/\[Category("//;s/")//' \
        | sort -u \
        || true)

      ALL_UI_CATEGORIES=$(echo -e "${UI_CATEGORIES}\n${UI_NAMEOF}\n${UI_QUOTED}" \
        | grep -v '^$' | sort -u || true)

      # ── Device Test Categories ───────────────────────────────────────
      # Extract [Category(TestCategory.X)] from added lines
      DEVICE_CATEGORIES=$(echo "$DIFF" \
        | grep -E '^\+.*\[Category\(TestCategory\.' \
        | grep -oE 'TestCategory\.[A-Za-z0-9_]+' \
        | sed 's/TestCategory\.//' \
        | sort -u \
        || true)

      DEVICE_NAMEOF=$(echo "$DIFF" \
        | grep -E '^\+.*\[Category\(' \
        | grep -oE 'nameof\(TestCategory\.[A-Za-z0-9_]+\)' \
        | sed 's/nameof(TestCategory\.//;s/)//' \
        | sort -u \
        || true)

      ALL_DEVICE_CATEGORIES=$(echo -e "${DEVICE_CATEGORIES}\n${DEVICE_NAMEOF}" \
        | grep -v '^$' | sort -u || true)

      # ── Changed file paths ──────────────────────────────────────────
      CHANGED_FILES=$(gh pr diff "$PR_NUMBER" --repo "$GITHUB_REPOSITORY" --name-only 2>/dev/null || true)

      TEST_FILES=$(echo "$CHANGED_FILES" \
        | grep -iE '(tests?/|TestCases|UnitTests|DeviceTests)' \
        | grep -E '\.(cs|xaml)$' \
        || true)

      SOURCE_FILES=$(echo "$CHANGED_FILES" \
        | grep -E '\.(cs|xaml)$' \
        | grep -v -iE '(tests?/|TestCases|UnitTests|DeviceTests)' \
        || true)

      # ── Write results ───────────────────────────────────────────────
      {
        echo "PR_NUMBER=$PR_NUMBER"

        if [ -n "$ALL_UI_CATEGORIES" ]; then
          echo "UI_CATEGORIES<<EOF"
          echo "$ALL_UI_CATEGORIES"
          echo "EOF"
        fi

        if [ -n "$ALL_DEVICE_CATEGORIES" ]; then
          echo "DEVICE_CATEGORIES<<EOF"
          echo "$ALL_DEVICE_CATEGORIES"
          echo "EOF"
        fi

        echo "---TEST_FILES---"
        echo "$TEST_FILES"
        echo "---SOURCE_FILES---"
        echo "$SOURCE_FILES"
        echo "---ALL_FILES---"
        echo "$CHANGED_FILES"
      } > "$GITHUB_WORKSPACE/category-results.txt"

      echo "=== Summary ==="
      [ -n "$ALL_UI_CATEGORIES" ] && echo "UI categories: $ALL_UI_CATEGORIES" || echo "UI categories: (none detected)"
      [ -n "$ALL_DEVICE_CATEGORIES" ] && echo "Device categories: $ALL_DEVICE_CATEGORIES" || echo "Device categories: (none detected)"
      echo "Test files changed: $(echo "$TEST_FILES" | grep -c '.' || echo 0)"
      echo "Source files changed: $(echo "$SOURCE_FILES" | grep -c '.' || echo 0)"

  - name: Checkout PR and restore agent infrastructure
    if: github.event_name == 'workflow_dispatch'
    env:
      GH_TOKEN: ${{ github.token }}
      PR_NUMBER: ${{ inputs.pr_number }}
    run: pwsh .github/scripts/Checkout-GhAwPr.ps1
---

# Detect Test Categories for Regression Detection

You are a CI assistant that analyzes PR diffs to determine which UI test and device test categories should run on the `maui-pr-regression-detection` pipeline. You MUST post a comment with your findings.

## Context

- **Repository**: ${{ github.repository }}
- **PR Number**: ${{ github.event.pull_request.number || github.event.issue.number || inputs.pr_number }}

## Instructions

### Step 1: Read detection results

Read `category-results.txt` from the workspace root. It contains:
- `PR_NUMBER=<number>`
- `UI_CATEGORIES` block — UI test categories detected from `[Category(UITestCategories.X)]` in added lines
- `DEVICE_CATEGORIES` block — device test categories detected from `[Category(TestCategory.X)]` in added lines
- `---TEST_FILES---` section — changed test file paths
- `---SOURCE_FILES---` section — changed source (non-test) file paths
- `---ALL_FILES---` section — all changed files

### Step 2: Intelligent category inference

If categories were detected directly from test annotations, use those.

If **NO categories** were detected (no test files changed, or test files don't have new Category attributes), you MUST **infer** which categories are likely affected based on the changed **source files**. Use this mapping:

#### Source Path → UI Test Category mapping

| Source path pattern | UI Test Categories | Device Test Categories |
|--------------------|--------------------|----------------------|
| `src/Controls/src/Core/Button/` or `*Button*.cs` | Button | Button |
| `src/Controls/src/Core/Label/` or `*Label*.cs` | Label | Label |
| `src/Controls/src/Core/Entry/` or `*Entry*.cs` | Entry | Entry |
| `src/Controls/src/Core/Editor/` or `*Editor*.cs` | Editor | Editor |
| `src/Controls/src/Core/CollectionView/` | CollectionView | CollectionView |
| `src/Controls/src/Core/CarouselView/` | CarouselView | CarouselView |
| `src/Controls/src/Core/ListView/` or `*ListView*.cs` | ListView | ListView |
| `src/Controls/src/Core/Shell/` or `*Shell*.cs` | Shell | Shell |
| `src/Controls/src/Core/NavigationPage/` or `*Navigation*.cs` | Navigation | NavigationPage |
| `src/Controls/src/Core/Layout/` or `*Layout*.cs` | Layout | Layout |
| `src/Controls/src/Core/ScrollView/` | ScrollView | ScrollView |
| `src/Controls/src/Core/WebView/` | WebView | WebView |
| `src/Controls/src/Core/Image/` or `*Image.cs` | Image | Image |
| `src/Controls/src/Core/ImageButton/` | ImageButton | ImageButton |
| `src/Controls/src/Core/SearchBar/` | SearchBar | SearchBar |
| `src/Controls/src/Core/Picker/` | Picker | Picker |
| `src/Controls/src/Core/DatePicker/` | DatePicker | DatePicker |
| `src/Controls/src/Core/TimePicker/` | TimePicker | TimePicker |
| `src/Controls/src/Core/Switch/` | Switch | - |
| `src/Controls/src/Core/Slider/` | Slider | Slider |
| `src/Controls/src/Core/Stepper/` | Stepper | Stepper |
| `src/Controls/src/Core/CheckBox/` | CheckBox | CheckBox |
| `src/Controls/src/Core/RadioButton/` | RadioButton | RadioButton |
| `src/Controls/src/Core/ProgressBar/` | ProgressBar | ProgressBar |
| `src/Controls/src/Core/ActivityIndicator/` | ActivityIndicator | ActivityIndicator |
| `src/Controls/src/Core/Border/` or `*Border*.cs` | Border | Border |
| `src/Controls/src/Core/Frame/` | Frame | - |
| `src/Controls/src/Core/BoxView/` | BoxView | - |
| `src/Controls/src/Core/RefreshView/` | RefreshView | RefreshView |
| `src/Controls/src/Core/SwipeView/` | SwipeView | SwipeView |
| `src/Controls/src/Core/IndicatorView/` | IndicatorView | IndicatorView |
| `src/Controls/src/Core/FlyoutPage/` | FlyoutPage | FlyoutPage |
| `src/Controls/src/Core/TabbedPage/` | TabbedPage | TabbedPage |
| `src/Controls/src/Core/Page/` or `*Page.cs` (base) | Page | Page |
| `src/Controls/src/Core/Window/` | Window | Window |
| `src/Controls/src/Core/Shapes/` or `*Shape*.cs` | Shape | - |
| `src/Controls/src/Core/Shadow/` | Shadow | - |
| `src/Controls/src/Core/Brush/` | Brush | - |
| `src/Controls/src/Core/Gestures/` or `*Gesture*.cs` | Gestures | Gesture |
| `src/Controls/src/Core/DragDrop/` | DragAndDrop | - |
| `src/Controls/src/Core/Accessibility/` | Accessibility | Accessibility |
| `src/Controls/src/Core/Handlers/Items/` | CollectionView, CarouselView | CollectionView, CarouselView |
| `src/Controls/src/Core/Handlers/Items2/` | CollectionView, CarouselView | CollectionView, CarouselView |
| `src/Core/src/Handlers/` | (match handler name) | (match handler name) |
| `src/Core/src/Platform/` | Layout, Page, Window | Layout, Page, Window |
| `src/Core/src/Fonts/` | Fonts | Fonts |
| `src/Essentials/` | - | (Essentials device tests) |
| `src/Graphics/` | GraphicsView | Graphics |
| `src/BlazorWebView/` | WebView | BlazorWebView |
| `src/Controls/src/Core/Layout/Grid*` | Layout | Layout, FlexLayout |
| `src/Controls/src/Core/Toolbar/` | ToolbarItem | Toolbar |

**Important inference rules:**
1. If a handler file changes (e.g., `ButtonHandler.Android.cs`), always include that control's category
2. If platform-specific code changes (e.g., files in `Platform/Android/`), consider broader categories
3. If core infrastructure changes (e.g., `VisualElement.cs`, `Element.cs`), recommend running ALL categories
4. If only build/pipeline files change, recommend NO test categories
5. When in doubt, include the category — it's better to test more than miss a regression

### Step 3: Determine device test recommendation

Based on the categories and source files:
- If **Controls handler/platform code** changed → recommend device tests
- If **Core handler/platform code** changed → recommend device tests
- If **only UI test files** changed → device tests optional
- If **Essentials/Graphics/BlazorWebView** code changed → recommend device tests for those projects

### Step 4: Post the comment

Use `add_comment` with `item_number` set to the PR number. Use this format:

```markdown
## 🏷️ Test Categories for Regression Detection

{1-2 sentence summary of what was detected/inferred}

### UI Test Categories

| Category | Source |
|----------|--------|
| {Category} | {Detected from test / Inferred from source change} |

**Pipeline filter:** `{comma-separated UI categories}`

### Device Test Categories

| Category | Project | Source |
|----------|---------|--------|
| {Category} | {Controls/Core/Essentials/Graphics/BlazorWebView} | {Detected/Inferred} |

**Recommendation:** {Run device tests: Yes/No} — {reason}

### 🚀 Run Regression Detection Pipeline

To run targeted tests for this PR, trigger the `maui-pr-regression-detection` pipeline:

**UI Tests only:**
> Pipeline: `maui-pr-regression-detection`
> Branch: `{PR branch}`
> Parameters: `uiTestCategories: {comma-separated categories}`

**UI + Device Tests:**
> Pipeline: `maui-pr-regression-detection`
> Branch: `{PR branch}`
> Parameters: `uiTestCategories: {comma-separated categories}`, `runDeviceTests: true`

<details>
<summary>📁 Changed files ({total count})</summary>

**Test files ({count}):**
{list}

**Source files ({count}):**
{list}

</details>

> ℹ️ Categories are detected from `[Category()]` attributes in the diff and inferred from changed source file paths.
```

If NO categories could be detected or inferred (e.g., only docs/build files changed):

```markdown
## 🏷️ Test Categories for Regression Detection

No test categories could be determined for this PR. The changes don't appear to affect any testable control or platform code.

<details>
<summary>📁 Changed files ({count})</summary>

{list}

</details>
```

