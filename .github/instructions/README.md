# .NET MAUI Development Instructions

This directory contains specialized guidance for working on the .NET MAUI repository.

## Instruction Files

| File | When It Applies | Key Topics |
|------|-----------------|------------|
| **safe-area-debugging.instructions.md** | iOS/macCatalyst safe area code, *.ios.cs, Platform/iOS/** | macOS version differences, guard debugging, semantic comparisons |
| **uitests.instructions.md** | UI test files (TestCases.HostApp, TestCases.Shared.Tests) | Test structure, categories, screenshot testing, safe area testing |
| **xaml-unittests.instructions.md** | XAML unit tests (Controls.Xaml.UnitTests) | XAML test patterns, inflators, MockCompiler |
| **android.instructions.md** | Android platform code, *.android.cs | View namespace collisions, handler lifecycle |
| **sandbox.instructions.md** | Sandbox app testing | Manual PR validation, BuildAndRunSandbox.ps1 |
| **templates.instructions.md** | Template code (src/Templates/) | Conditional compilation markers, naming |
| **integration-tests.instructions.md** | Integration tests (Microsoft.Maui.IntegrationTests) | Template testing, categories, Helix queues |
| **helix-device-tests.instructions.md** | Device tests via Helix | XHarness, test filtering, iOS category splitting |
| **collectionview-handler-detection.instructions.md** | CollectionView handlers | Items/ vs Items2/ detection by platform |

## Quick Decision Tree

**Safe area issue on iOS/macCatalyst?**
→ Read `safe-area-debugging.instructions.md`
→ Key rule: macOS 14/15 reports ~28px title bar; macOS 26 reports ~0px

**Writing UI test?**
→ Read `uitests.instructions.md`
→ Key rule: Safe area tests should use tolerances, not hardcoded values

**Callback not firing?**
→ Read `safe-area-debugging.instructions.md` Rule 2
→ Key rule: Check early-return guards FIRST

**Android build error with "View ambiguous"?**
→ Read `android.instructions.md`
→ Key rule: Add `using AView = Android.Views.View;`

**Testing PR manually?**
→ Read `sandbox.instructions.md`
→ Key rule: ALWAYS use BuildAndRunSandbox.ps1 script

## Critical Facts to Remember

1. **macOS versions matter**: macOS 14/15 ≠ macOS 26 for safe area reporting
2. **Guard granularity**: Window-level guards can block view-level changes
3. **Semantic compatibility**: Never compare raw values to adjusted values
4. **.ios.cs = iOS + macCatalyst**: These files compile for BOTH platforms
5. **Test names = clues**: TitleBar test failing → investigate TitleBar code path
6. **macCatalyst UseSafeArea = true**: Opposite of iOS default (false)

## File Auto-Application

Instruction files automatically apply based on `applyTo` patterns:

```yaml
# Example from safe-area-debugging.instructions.md
applyTo:
  - "**/*SafeArea*.cs"           # Any SafeArea-related file
  - "**/Platform/iOS/**"          # iOS platform folder
  - "**/Platform/MacCatalyst/**"  # MacCatalyst platform folder
  - "**/*.ios.cs"                 # iOS extension files
  - "**/*.maccatalyst.cs"         # MacCatalyst extension files
```

When you open a matching file, the instruction guidance automatically becomes available.

## Related Documentation

- **Main guide**: `../.github/copilot-instructions.md` - General MAUI development workflow
- **Skills**: `../.github/skills/*/SKILL.md` - Reusable automation capabilities
- **Agents**: `../.github/agents/*/README.md` - Multi-phase workflow automation

## Contributing New Instructions

When creating new instruction files:

1. **Use specific, actionable rules** - Not "be careful", but "Check X when Y happens"
2. **Include examples** - Show bad vs good patterns
3. **Add applyTo patterns** - Automatic application to relevant files
4. **Link from copilot-instructions.md** - Make it discoverable
5. **Store key facts as memories** - Max 200 chars, cite sources
6. **Prevent specific mistakes** - Each rule should target a known failure mode

**Template structure:**
```markdown
---
applyTo:
  - "pattern/**"
---

# Topic Name

## Critical Rules

### Rule 1: Specific, Actionable Guidance

**Mistake**: What went wrong
**Rule**: What to do instead
**Pattern**: When this applies

## Examples

### ❌ What Went Wrong
[Bad code example]

### ✅ Correct Approach
[Good code example]
```
