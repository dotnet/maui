#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class PageSafeAreaOrientationVSM : _IssuesUITest
{
    public override string Issue => "Page SafeAreaEdges with OrientationStateTrigger VSM Test";

    public PageSafeAreaOrientationVSM(TestDevice device) : base(device)
    {
    }

    [Test]
    [Category(UITestCategories.SafeAreaEdges)]
    public void PageSafeAreaEdgesUpdatesWithOrientation()
    {
        // 1. Wait for initial state to load
        var orientationLabel = App.WaitForElement("OrientationLabel");
        var safeAreaEdgesLabel = App.WaitForElement("SafeAreaEdgesLabel");

        // 2. Verify initial portrait state
        var initialOrientation = orientationLabel.GetText();
        var initialSafeAreaEdges = safeAreaEdgesLabel.GetText();

        Assert.That(initialOrientation, Does.Contain("Portrait"), 
            "Should start in portrait orientation");
        Assert.That(initialSafeAreaEdges, Does.Contain("Container"), 
            "SafeAreaEdges should be Container in portrait");

        // 3. Change to landscape
        App.SetOrientationLandscape();
        Thread.Sleep(2000); // Wait for orientation change and VSM to trigger

        // 4. Verify landscape state
        var landscapeOrientation = App.WaitForElement("OrientationLabel").GetText();
        var landscapeSafeAreaEdges = App.WaitForElement("SafeAreaEdgesLabel").GetText();

        Assert.That(landscapeOrientation, Does.Contain("Landscape"), 
            "Should now be in landscape orientation");
        Assert.That(landscapeSafeAreaEdges, Does.Contain("Container"), 
            "SafeAreaEdges should be Container in landscape");

        // 5. Verify visual markers are still visible
        var topLeftMarker = App.WaitForElement("TopLeftMarker");
        var topRightMarker = App.WaitForElement("TopRightMarker");
        var centerMarker = App.WaitForElement("CenterMarker");

        Assert.That(topLeftMarker, Is.Not.Null, "Top left marker should be visible");
        Assert.That(topRightMarker, Is.Not.Null, "Top right marker should be visible");
        Assert.That(centerMarker, Is.Not.Null, "Center marker should be visible");

        // 6. Change back to portrait
        App.SetOrientationPortrait();
        Thread.Sleep(2000); // Wait for orientation change and VSM to trigger

        // 7. Verify back to portrait state
        var finalOrientation = App.WaitForElement("OrientationLabel").GetText();
        var finalSafeAreaEdges = App.WaitForElement("SafeAreaEdgesLabel").GetText();

        Assert.That(finalOrientation, Does.Contain("Portrait"), 
            "Should be back in portrait orientation");
        Assert.That(finalSafeAreaEdges, Does.Contain("Container"), 
            "SafeAreaEdges should still be Container in portrait");
    }

    [Test]
    [Category(UITestCategories.SafeAreaEdges)]
    public void PageVSMMultipleOrientationChanges()
    {
        // Test that the VSM handles multiple orientation changes correctly
        var safeAreaEdgesLabel = App.WaitForElement("SafeAreaEdgesLabel");

        for (int i = 0; i < 3; i++)
        {
            // Change to landscape
            App.SetOrientationLandscape();
            Thread.Sleep(1500);

            var landscapeSafeArea = safeAreaEdgesLabel.GetText();
            Assert.That(landscapeSafeArea, Does.Contain("Container"), 
                $"SafeAreaEdges should be Container in landscape (iteration {i + 1})");

            // Change back to portrait
            App.SetOrientationPortrait();
            Thread.Sleep(1500);

            var portraitSafeArea = safeAreaEdgesLabel.GetText();
            Assert.That(portraitSafeArea, Does.Contain("Container"), 
                $"SafeAreaEdges should be Container in portrait (iteration {i + 1})");
        }
    }
}
#endif
