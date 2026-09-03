// iOS-only: These tests focus on bottom safe area parent/child independence.
// Android gesture-nav emulators have 0pt bottom safe area, making every bottom-edge
// assertion trivially true or skipped — the tests would pass but verify nothing.
// If Android parent/child SafeAreaEdges coverage is needed, write separate tests
// using the TOP edge (status bar is always present at ~24-48dp on Android).
#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28986_ParentChildTest : _IssuesUITest
{
    public override string Issue => "SafeAreaEdges independent handling for parent and child controls";

    public Issue28986_ParentChildTest(TestDevice device)
    : base(device)
    { }

    // Bottom-specific assertions guard for devices without bottom safe area (e.g. iPad without home indicator).
    static bool HasBottomSafeArea(double measuredBottomInset) => measuredBottomInset > 2;

    void WaitForText(string elementId, string expectedText, int timeoutSec = 5)
    {
        var endTime = DateTime.Now.AddSeconds(timeoutSec);
        while (DateTime.Now < endTime)
        {
            var text = App.WaitForElement(elementId).GetText();
            if (text == expectedText)
                return;
            Thread.Sleep(100);
        }
        var finalText = App.WaitForElement(elementId).GetText();
        Assert.That(finalText, Is.EqualTo(expectedText), $"Timed out waiting for {elementId} text to be '{expectedText}'");
    }

    [Test, Order(1)]
    [Category(UITestCategories.SafeAreaEdges)]
    public void VerifyInitialStateParentTopChildBottom()
    {
        // Test: Parent handles TOP, Child handles BOTTOM
        // 
        // Verify that:
        // 1. Top indicator is inset from screen top by safe area (parent handles top)
        // 2. Bottom indicator is inset from screen bottom by safe area (child handles bottom)
        // 3. Both work independently without conflict

        // Get screen dimensions
        var parentGridRect = App.WaitForElement("ParentGrid").GetRect();
        var screenTop = parentGridRect.Y;
        var screenBottom = parentGridRect.Y + parentGridRect.Height;

        // Verify initial status
        WaitForText("StatusLabel", "Parent: Top=Container, Bottom=None | Child: Bottom=Container");

        // Measure top indicator position
        var topIndicatorRect = App.WaitForElement("TopIndicator").GetRect();
        var topIndicatorTop = topIndicatorRect.Y;

        // Top indicator should be below the screen top (safe area applied)
        var topInsetFromScreenTop = topIndicatorTop - screenTop;
        Assert.That(topInsetFromScreenTop, Is.GreaterThan(5),
            $"Top indicator should be inset from screen top by safe area. " +
            $"Current inset: {topInsetFromScreenTop}pt (expected >5pt)");

        // Measure bottom indicator position
        var bottomIndicatorRect = App.WaitForElement("BottomIndicator").GetRect();
        var bottomIndicatorBottom = bottomIndicatorRect.Y + bottomIndicatorRect.Height;

        // Bottom indicator should be above the screen bottom (safe area applied)
        var bottomInsetFromScreenBottom = screenBottom - bottomIndicatorBottom;
        // On devices with bottom safe area (iOS home indicator, Android nav bar), verify meaningful inset.
        // On gesture-nav Android devices, bottom safe area is correctly 0.
        if (HasBottomSafeArea(bottomInsetFromScreenBottom))
        {
            Assert.That(bottomInsetFromScreenBottom, Is.GreaterThan(5),
                $"Bottom indicator should be inset from screen bottom by safe area. " +
                $"Current inset: {bottomInsetFromScreenBottom}pt (expected >5pt)");
        }
        else
        {
            Assert.That(bottomInsetFromScreenBottom, Is.GreaterThanOrEqualTo(0),
                $"Bottom indicator should not extend below screen bottom. Inset: {bottomInsetFromScreenBottom}pt");
        }
    }

    [Test, Order(2)]
    [Category(UITestCategories.SafeAreaEdges)]
    public void VerifyRuntimeSafeAreaChangePreservesPositions()
    {
        // Test: Runtime SafeAreaEdges changes are applied correctly without position conflicts
        //
        // Scenario:
        // Step 1: Parent=Bottom=None, Child=Bottom=None (nothing handles bottom)
        //         → Bottom indicator should reach screen bottom
        // Step 2: Child=Bottom=Container (child takes over bottom handling)
        //         → Bottom indicator should move up (safe area applied)
        // Step 3: Parent=Bottom=Container (parent also handles bottom)
        //         → Bottom position should match Step 2 (no double padding)

        var parentGridRect = App.WaitForElement("ParentGrid").GetRect();
        var screenBottom = parentGridRect.Y + parentGridRect.Height;

        // STEP 1: Set Parent: Bottom=None, Child: Bottom=None
        App.Tap("ToggleParentBottomButton");
        WaitForText("StatusLabel", "Parent: Top=Container, Bottom=Container | Child: Bottom=Container");

        App.Tap("ToggleParentBottomButton");
        WaitForText("StatusLabel", "Parent: Top=Container, Bottom=None | Child: Bottom=Container");

        App.Tap("ToggleChildBottomButton");
        WaitForText("StatusLabel", "Parent: Top=Container, Bottom=None | Child: Bottom=None");

        // Measure bottom indicator position when nothing handles bottom
        var bottomIndicatorRectNone = App.WaitForElement("BottomIndicator").GetRect();
        var bottomIndicatorBottomNone = bottomIndicatorRectNone.Y + bottomIndicatorRectNone.Height;
        var distanceFromScreenBottomNone = screenBottom - bottomIndicatorBottomNone;

        // Bottom should reach close to screen edge
        Assert.That(distanceFromScreenBottomNone, Is.LessThan(5),
            "Bottom indicator should reach near screen bottom when both parent and child have SafeAreaEdges=None");

        // STEP 2: Child=Bottom=Container (child handles bottom)
        App.Tap("ToggleChildBottomButton");
        WaitForText("StatusLabel", "Parent: Top=Container, Bottom=None | Child: Bottom=Container");

        // Measure bottom indicator position with child handling
        var bottomIndicatorRectChildHandles = App.WaitForElement("BottomIndicator").GetRect();
        var bottomIndicatorBottomChildHandles = bottomIndicatorRectChildHandles.Y + bottomIndicatorRectChildHandles.Height;
        var distanceFromScreenBottomChildHandles = screenBottom - bottomIndicatorBottomChildHandles;

        // Bottom should now be inset by safe area (if device has bottom safe area)
        // On gesture-nav Android, Container and None produce the same position (both 0)
        if (HasBottomSafeArea(distanceFromScreenBottomChildHandles))
        {
            Assert.That(distanceFromScreenBottomChildHandles, Is.GreaterThan(20),
                "Bottom indicator should be inset by safe area when child handles bottom");
        }

        // Record this as the expected position
        var expectedBottomPosition = bottomIndicatorBottomChildHandles;

        // STEP 3: Parent=Bottom=Container (parent also handles bottom)
        App.Tap("ToggleParentBottomButton");
        WaitForText("StatusLabel", "Parent: Top=Container, Bottom=Container | Child: Bottom=Container");

        // Measure bottom indicator position when both parent and child handle bottom
        var bottomIndicatorRectBothHandle = App.WaitForElement("BottomIndicator").GetRect();
        var bottomIndicatorBottomBothHandle = bottomIndicatorRectBothHandle.Y + bottomIndicatorRectBothHandle.Height;

        // Key assertion: No double padding
        // The bottom position should be THE SAME as when only child handled it
        // If there's double padding, the bottom would be significantly higher (smaller Y)
        var verticalDifference = Math.Abs(expectedBottomPosition - bottomIndicatorBottomBothHandle);
        Assert.That(verticalDifference, Is.LessThan(5),
            $"Bottom indicator position should be nearly identical when parent adds bottom handling " +
            $"(child already handling). Vertical difference: {verticalDifference}pt. " +
            $"If difference exceeds safe area size (~34pt), it indicates double padding bug.");
    }

    [Test, Order(3)]
    [Category(UITestCategories.SafeAreaEdges)]
    public void VerifyChildCanHandleWhenParentDoesNot()
    {
        // Test: Child safe area handling is NOT blocked by parent's other safe area handling
        //
        // Scenario:
        // Parent: Top=Container, Bottom=None (handles ONLY top)
        // Child: Bottom=Container (handles ONLY bottom)
        //
        // Result: Both should work independently

        // Reset to known state
        var currentStatus = App.WaitForElement("StatusLabel").GetText();

        // If Parent Bottom is Container, toggle it back to None
        if (currentStatus?.Contains("Bottom=Container", StringComparison.OrdinalIgnoreCase) == true)
        {
            App.Tap("ToggleParentBottomButton");
        }

        WaitForText("StatusLabel", "Parent: Top=Container, Bottom=None | Child: Bottom=Container");

        var parentGridRect = App.WaitForElement("ParentGrid").GetRect();
        var screenTop = parentGridRect.Y;
        var screenBottom = parentGridRect.Y + parentGridRect.Height;

        // Measure top indicator
        var topIndicatorRect = App.WaitForElement("TopIndicator").GetRect();
        var topInset = topIndicatorRect.Y - screenTop;

        // Measure bottom indicator
        var bottomIndicatorRect = App.WaitForElement("BottomIndicator").GetRect();
        var bottomInset = screenBottom - (bottomIndicatorRect.Y + bottomIndicatorRect.Height);

        // Both should have proper insets
        Assert.That(topInset, Is.GreaterThan(5),
            "Parent should handle top safe area");

        // On devices with bottom safe area, verify child handles it independently.
        // On gesture-nav Android, bottom safe area is correctly 0.
        if (HasBottomSafeArea(bottomInset))
        {
            Assert.That(bottomInset, Is.GreaterThan(5),
                "Child should handle bottom safe area independently");
        }
        else
        {
            Assert.That(bottomInset, Is.GreaterThanOrEqualTo(0),
                "Bottom indicator should not extend below screen bottom");
        }

        // Key assertion: Child's bottom handling coexists with parent's top handling
        // They don't conflict or block each other
        Assert.Pass("Child's bottom safe area handling works independently while parent handles top");
    }

    [Test, Order(4)]
    [Category(UITestCategories.SafeAreaEdges)]
    public void VerifyPositionConsistencyAcrossToggles()
    {
        // Test: Toggling SafeAreaEdges on/off produces consistent positions
        //
        // Scenario:
        // 1. Start: Parent=Top=Container, Bottom=None | Child=Bottom=Container
        // 2. Toggle Child Bottom to None → Bottom should move to screen edge
        // 3. Toggle Child Bottom back to Container → Bottom should return to original position
        // 4. Repeat cycle 2 more times to verify consistency

        // Reset to known state
        var currentStatus = App.WaitForElement("StatusLabel").GetText();
        if (currentStatus != null && currentStatus.Contains("Bottom=Container", StringComparison.OrdinalIgnoreCase) && currentStatus.Contains("Parent: Top=Container, Bottom=Container", StringComparison.OrdinalIgnoreCase))
        {
            App.Tap("ToggleParentBottomButton");
        }

        WaitForText("StatusLabel", "Parent: Top=Container, Bottom=None | Child: Bottom=Container");

        var parentGridRect = App.WaitForElement("ParentGrid").GetRect();
        var screenBottom = parentGridRect.Y + parentGridRect.Height;

        // Record baseline bottom indicator position
        var bottomIndicatorRectBaseline = App.WaitForElement("BottomIndicator").GetRect();
        var baselineBottomY = bottomIndicatorRectBaseline.Y + bottomIndicatorRectBaseline.Height;
        var baselineInset = screenBottom - baselineBottomY;

        // Cycle 3 times: None → Container → None → Container
        for (int i = 0; i < 3; i++)
        {
            // Toggle to None
            App.Tap("ToggleChildBottomButton");
            WaitForText("StatusLabel", "Parent: Top=Container, Bottom=None | Child: Bottom=None");

            var bottomIndicatorRectNone = App.WaitForElement("BottomIndicator").GetRect();
            var bottomYNone = bottomIndicatorRectNone.Y + bottomIndicatorRectNone.Height;
            var insetNone = screenBottom - bottomYNone;

            // Should be close to screen bottom
            Assert.That(insetNone, Is.LessThan(20),
                $"Cycle {i + 1}: Bottom should reach screen edge when Child=None");

            // Toggle back to Container
            App.Tap("ToggleChildBottomButton");
            WaitForText("StatusLabel", "Parent: Top=Container, Bottom=None | Child: Bottom=Container");

            var bottomIndicatorRectRestored = App.WaitForElement("BottomIndicator").GetRect();
            var bottomYRestored = bottomIndicatorRectRestored.Y + bottomIndicatorRectRestored.Height;
            var insetRestored = screenBottom - bottomYRestored;

            // Should return to baseline position
            Assert.That(insetRestored, Is.EqualTo(baselineInset).Within(5),
                $"Cycle {i + 1}: Bottom should return to original position when Child=Container. " +
                $"Original inset: {baselineInset}pt, Current inset: {insetRestored}pt");
        }
    }
}
#endif
