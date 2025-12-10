# Sandbox Setup and Instrumentation

## CRITICAL: Determine CollectionView Handler Type First

**Before modifying the Sandbox app**, check if the PR affects CollectionView/CarouselView handlers.

See **[CollectionView Handler Detection](collectionview-handler-detection.md)** for complete algorithm and configuration.

**Quick summary**:
```bash
# Check if PR modifies handler files
git diff <base>..<pr> --name-only | grep "Handlers/Items"

# Path contains:
# "Items/" (NOT "Items2") → Enable CollectionViewHandler
# "Items2/" → Enable CollectionViewHandler2
```

**Why this matters**: iOS/MacCatalyst defaults to CollectionViewHandler2. If PR fixes a bug in CollectionViewHandler, you MUST explicitly enable it or the bug won't reproduce.

---

## Modify Sandbox App for Testing

After determining the handler type (if needed), modify the Sandbox app to test the scenario.

### Quick Start: Use Template

**For most PRs**, use the copy-paste template from [quick-ref.md](quick-ref.md#test-code-template-copy-paste):
- Complete XAML structure with visual debugging (colored backgrounds)
- Code-behind with proper timing and measurements
- Ready to customize for your scenario

### Detailed Patterns and Techniques

**For comprehensive instrumentation patterns**, see **[Instrumentation Guide](../instrumentation.md)**:
- [Key Techniques](../instrumentation.md#key-instrumentation-techniques) - Console output, timing, measurements
- [Common Patterns](../instrumentation.md#common-instrumentation-patterns) - Property testing, collections, nested content
- [Platform-Specific Positioning](../instrumentation.md#platform-specific-positioning) - Screen coordinates
- [SafeArea Testing](../safearea-testing.md) - SafeArea-specific patterns

### Standard Workflow

1. Copy template from [quick-ref.md](quick-ref.md#test-code-template-copy-paste)
2. Customize for your test scenario
3. **STOP** - Use validation checkpoint (show code to user)
4. Build, deploy, capture console output

---

## Validation Checkpoint (WHEN to Use It)

**What is the validation checkpoint?**
- A pause in the review process where you show the user your test setup BEFORE running it
- Allows the user to verify you're testing the right thing
- Prevents wasting 20+ minutes on incorrect tests

**When to use the validation checkpoint:**

✅ **REQUIRED for these situations:**
1. **SafeArea testing** - High risk of measuring the wrong element
2. **First time testing a specific control type** - Unfamiliar with how to test it
3. **Complex layout scenarios** - Nested controls, multiple layers
4. **Unclear PR description** - Not certain what behavior to test
5. **Multiple possible interpretations** - Could test in several different ways
6. **After previous test attempt failed** - Need to adjust approach

❌ **Skip validation checkpoint for:**
- Simple, straightforward property tests (color, text, visibility)
- Clear PR with obvious test scenario
- Repetitive testing of same control type you've tested before
- User explicitly said "just test it quickly"

**What to show in validation checkpoint:**

```markdown
## Validation Checkpoint

Before building and running the test (which takes time), I want to confirm my test setup:

**Test Scenario**: [Describe what you're testing]

**Sandbox Modifications**:
```xml
<!-- Show relevant XAML snippet -->
<ContentView x:Name="TestElement" 
             SafeAreaEdges="Top,Bottom"
             BackgroundColor="Yellow">
    <Label x:Name="ContentLabel" Text="Test" />
</ContentView>
```

**Instrumentation**:
```csharp
// Show key measurement code
private void OnLoaded(object sender, EventArgs e)
{
    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
    {
        Console.WriteLine($"Content Y: {ContentLabel.Y}");
        double bottomGap = screenHeight - (ContentLabel.Y + ContentLabel.Height);
        Console.WriteLine($"Bottom Gap: {bottomGap}");
    });
}
```

**What I'm measuring**: [Explain what values you'll capture and why]

**Expected result WITHOUT PR**: [What you expect to see]

**Expected result WITH PR**: [What should change]

**Edge cases to test**:
- [Edge case 1]
- [Edge case 2]

Does this test setup correctly validate the PR fix?
```

**User responses**:
- ✅ User confirms → Proceed with build/deploy/test
- ❌ User corrects you → Adjust setup and show updated checkpoint
- 💭 User asks questions → Clarify and show revised checkpoint

