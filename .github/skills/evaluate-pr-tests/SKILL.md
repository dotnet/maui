---
name: evaluate-pr-tests
description: "Evaluates tests added in a PR for coverage, quality, edge cases, and test type appropriateness. Checks if tests cover the fix, finds gaps, and recommends lighter test types when possible. Prefer unit tests over device tests over UI tests. Triggers on: 'evaluate tests in PR', 'review test quality', 'are these tests good enough', 'check test coverage', 'is this test adequate', 'assess test coverage for PR'."
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires git, PowerShell, and gh CLI for PR context.
---

# Evaluate PR Tests

Evaluates the quality, coverage, and appropriateness of tests added in a PR. Produces a structured report with actionable findings.

## When to Use

- ✅ PR has tests and you want to evaluate their quality
- ⚠️ PR has no test files -- output a ❌ Fix Coverage verdict noting no tests were added; skip remaining criteria
- ✅ Reviewing whether tests adequately cover the fix
- ✅ Checking if a lighter test type could be used instead
- ✅ Before merging a PR, as part of review

## Quick Start

```bash
# Auto-detect PR and base branch
pwsh .github/skills/evaluate-pr-tests/scripts/Gather-TestContext.ps1

# With explicit base branch
pwsh .github/skills/evaluate-pr-tests/scripts/Gather-TestContext.ps1 -BaseBranch "origin/main"
```

## Workflow

### Step 1: Gather Automated Context

Run the script to get file categorization, convention checks, and anti-pattern detection:

```bash
pwsh .github/skills/evaluate-pr-tests/scripts/Gather-TestContext.ps1
```

This produces a report at `CustomAgentLogsTmp/TestEvaluation/context.md` with:
- File categorization (fix files vs test files by type)
- Convention compliance checks (naming, attributes, anti-patterns)
- AutomationId consistency (HostApp ↔ test)
- Existing similar tests
- Platform scope analysis

### Step 2: Understand the Fix

Read the fix files to understand:
- **What changed** — which code paths were modified
- **Why it changed** — the bug being fixed (from PR description or linked issue)
- **Edge cases** — what boundary conditions exist in the changed code

### Step 3: Evaluate the Tests

Read each test file and evaluate against **all criteria** below. For each criterion, provide a verdict (✅ Pass, ⚠️ Concern, ❌ Fail) with explanation.

### Step 4: Produce the Report

Output a structured evaluation report (see Output Format below).

---

## Evaluation Criteria

### 1. Fix Coverage

**Question:** Does the test exercise the actual code paths changed by the fix?

**How to check:**
- Trace the test's actions through the code to the fix location
- Would the test fail if the fix were reverted?
- Does the test assert on the specific behavior that was broken?

**Red flags:**
- Test only checks that a page loads (doesn't exercise the fix)
- Test asserts on a different property/behavior than what was fixed
- Test interacts with the control but doesn't trigger the buggy code path

**Example — Good:**
```csharp
// Fix: CollectionView.SelectedItem setter now clears selection when set to null
// Test: Sets SelectedItem to null and verifies selection is cleared
App.Tap("SelectItem");
App.Tap("ClearSelection");  // Sets SelectedItem = null
var text = App.FindElement("SelectionStatus").GetText();
Assert.That(text, Is.EqualTo("None"));  // Directly tests the fix
```

**Example — Bad:**
```csharp
// Fix: CollectionView.SelectedItem setter
// Test: Just checks CollectionView renders (doesn't test selection clearing)
App.WaitForElement("MyCollectionView");
Assert.That(true);  // Proves nothing about the fix
```

### 2. Edge Cases & Gaps

**Question:** Does the test cover boundary conditions, or only the happy path?

**Check for these common gaps:**

| Gap Type | What to Look For |
|----------|-----------------|
| **Null/empty** | Does the fix handle null? Is it tested? |
| **Boundary values** | Min, max, zero, negative, very large |
| **Repeated actions** | Does calling the action twice cause issues? |
| **Platform-specific** | Does the bug only occur on certain platforms? |
| **Async/timing** | Does the fix involve async code? Race conditions? |
| **State transitions** | Does the test cover before→after state changes? |
| **Error paths** | What happens when the operation fails? |
| **Combination effects** | Does the fix interact with other properties/features? |

**How to suggest missing edge cases:**
- Read the fix code and identify every conditional branch
- For each branch, check if the test covers it
- Look for `if (x == null)`, `if (x <= 0)`, try/catch blocks
- Consider: "What inputs would make this fix NOT work?"

### 3. Test Type Appropriateness

**Question:** Is this the lightest test type that can verify the fix?

**Preference order (lightest → heaviest):**

| Priority | Type | When Appropriate | Project |
|----------|------|-----------------|---------|
| ⭐ 1st | **Unit Test** | Pure logic, property changes, data transformations, binding behavior, event wiring | `*.UnitTests.csproj` |
| ⭐ 1st | **XAML Test** | XAML parsing, XamlC compilation, source generation, markup extensions | `Controls.Xaml.UnitTests` |
| ⭐⭐ 2nd | **Device Test** | Platform-specific rendering, native API interaction, handler mapping | `*.DeviceTests.csproj` |
| ⭐⭐⭐ 3rd | **UI Test** | User interaction flows, visual layout, screenshot comparison, end-to-end scenarios | `TestCases.Shared.Tests` |

**Decision tree:**

```
Does the test need to interact with visual UI elements?
  YES → Is it checking visual layout/appearance?
    YES → UI test (VerifyScreenshot) ✅
    NO  → Could the interaction be tested via handler/control API?
      YES → Device test ⭐⭐
      NO  → UI test ✅
  NO  → Does it need a platform/native context?
    YES → Device test ⭐⭐
    NO  → Does it test XAML parsing/compilation?
      YES → XAML test ⭐
      NO  → Unit test ⭐
```

**Common "could be lighter" patterns:**

| Current Test Does | Could Be Instead | Why |
|-------------------|-----------------|-----|
| UI test: sets property, checks label text | Unit test | Property logic doesn't need UI |
| UI test: verifies event fires | Unit test | Event wiring is testable in isolation |
| UI test: checks control doesn't crash | Device test | Don't need Appium for crash testing |
| UI test: validates XAML binding | XAML test | Binding resolution is compile-time |
| Device test: checks property default | Unit test | Defaults don't need platform context |

### 4. Convention Compliance

**Automated by the script.** Review the script output for:

**UI Tests:**
- File naming: `IssueXXXXX.cs`
- `[Issue()]` attribute on HostApp page
- `[Category()]` attribute — exactly ONE per test class (on the class or method, not both)
- `_IssuesUITest` base class
- `WaitForElement` before interactions
- No `Task.Delay`/`Thread.Sleep`
- No inline `#if ANDROID`/`#if IOS`
- No obsolete APIs (`Application.MainPage`, `Frame`, `Device.BeginInvokeOnMainThread`)
- `UITestEntry`/`UITestEditor` for screenshot tests

**Unit Tests:**
- `[Fact]` or `[Theory]` attributes (xUnit)

**XAML Tests:**
- `[Test]` with `[Values] XamlInflator` parameter
- Issue naming: `MauiXXXXX`

### 5. Flakiness Risk

**Question:** Is this test likely to be flaky in CI?

| Risk Factor | Detection | Mitigation |
|-------------|-----------|------------|
| Arbitrary delays | `Task.Delay`, `Thread.Sleep` | Use `WaitForElement`, `retryTimeout` |
| Missing waits | `App.Tap` without prior `WaitForElement` | Add explicit waits |
| Screenshot timing | `VerifyScreenshot()` without `retryTimeout` | Add `retryTimeout: TimeSpan.FromSeconds(2)` |
| Cursor blink | `Entry`/`Editor` in screenshot test | Use `UITestEntry`/`UITestEditor` |
| External URLs | WebView loading remote content | Use mock URLs or local content |
| Animation timing | Visual check after animation | Use `retryTimeout` |
| Global state | Test modifies `Application.Current` | Ensure cleanup in teardown |

### 6. Duplicate Coverage

**Question:** Does a similar test already exist?

Check the "Existing Similar Tests" section of the script output. If similar tests exist:
- Is the new test covering a **different scenario**? → OK
- Is the new test **redundant**? → Flag as concern
- Could the new test be **merged** with an existing one? → Suggest consolidation

### 7. Platform Scope

**Question:** Does the test run on all platforms affected by the fix?

Check the "Platform Scope Analysis" from the script:
- Cross-platform fix → tests should run on all platforms
- Platform-specific fix → test on that platform is sufficient
- Fix affects iOS + MacCatalyst → both should be tested (`.ios.cs` compiles for both)

### 8. Assertion Quality

**Question:** Are the assertions specific enough to catch regressions?

| Assertion Quality | Example | Verdict |
|-------------------|---------|---------|
| ✅ Specific | `Assert.That(label.Text, Is.EqualTo("Expected Value"))` | Catches regression |
| ⚠️ Vague | `Assert.That(label.Text, Is.Not.Null)` | Too permissive |
| ❌ Meaningless | `Assert.That(true)` or no assertion | Proves nothing |
| ✅ Positional | `Assert.That(rect.Y, Is.GreaterThan(safeAreaTop))` | Specific to layout fix |
| ⚠️ Brittle | `Assert.That(rect.Y, Is.EqualTo(47))` | Magic number, will break |

### 9. Fix-Test Alignment

**Question:** Do the files changed by the fix align with what the test exercises?

- Map the fix files to the controls/features they affect
- Map the test to the controls/features it exercises
- Flag if test exercises a different control than the fix changes
- Flag if test only covers one platform when fix touches multiple

**Red flags:**
- Test class is named `Issue12345` for a fix in `CollectionView` but only exercises `Label` rendering
- Fix changes `Shell.cs` but test only navigates a `ContentPage`

---

## Output Format

Produce the evaluation report in this format:

```markdown
## PR Test Evaluation Report

**PR:** #XXXXX — [Title]
**Test files evaluated:** [count]
**Fix files:** [count]

---

### Overall Verdict

[One of: ✅ Tests are adequate | ⚠️ Tests need improvement | ❌ Tests are insufficient]

[1-2 sentence summary of the most important finding]

---

### 1. Fix Coverage — [✅/⚠️/❌]

[Does the test exercise the code paths changed by the fix?]

### 2. Edge Cases & Gaps — [✅/⚠️/❌]

**Covered:**
- [edge case 1]
- [edge case 2]

**Missing:**
- [gap 1 — describe what should be tested and why]
- [gap 2]

### 3. Test Type Appropriateness — [✅/⚠️/❌]

**Current:** [UI Test / Device Test / Unit Test / XAML Test]
**Recommendation:** [Same / Could be lighter — explain why]

### 4. Convention Compliance — [✅/⚠️/❌]

[Summary from automated checks — list only issues found]

### 5. Flakiness Risk — [✅ Low / ⚠️ Medium / ❌ High]

[Specific risk factors identified]

### 6. Duplicate Coverage — [✅ No duplicates / ⚠️ Potential overlap]

[Similar existing tests found, if any]

### 7. Platform Scope — [✅/⚠️/❌]

[Does test coverage match the platforms affected by the fix?]

### 8. Assertion Quality — [✅/⚠️/❌]

[Are assertions specific enough to catch the actual bug?]

### 9. Fix-Test Alignment — [✅/⚠️/❌]

[Do the test and fix target the same code paths?]

---

### Recommendations

1. [Most important actionable recommendation]
2. [Second recommendation]
3. [...]
```

## Output Files

| File | Description |
|------|-------------|
| `CustomAgentLogsTmp/TestEvaluation/context.md` | Automated context report from script |

## Troubleshooting

| Problem | Cause | Solution |
|---------|-------|----------|
| No changed files detected | Wrong base branch | Use `-BaseBranch` explicitly |
| No fix files detected | All changes are tests | Expected for test-only PRs |
| AutomationId mismatch | HostApp and test out of sync | Update one to match the other |
| Convention check false positive | Script regex too broad | Ignore and note in report |
