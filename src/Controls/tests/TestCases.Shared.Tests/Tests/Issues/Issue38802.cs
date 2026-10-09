#if IOS || ANDROID // The SetOrientation method is only supported on mobile platforms.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38802 : _IssuesUITest
{
    public Issue38802(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Editor should be scrolled after rotating simulator";

    [TearDown]
    public void TearDown()
    {
        App.SetOrientationPortrait();
    }

    [Test]
    [Category(UITestCategories.Editor)]
    public void Issue38802EditorShouldBeScrolledAfterRotation()
    {
        // Step 1: Wait for slider and drag it to max
        var sliderRect = App.WaitForElement("Issue38802Slider").GetRect();
        App.DragCoordinates(
            sliderRect.X + 5,
            sliderRect.Y + sliderRect.Height / 2,
            sliderRect.X + sliderRect.Width - 5,
            sliderRect.Y + sliderRect.Height / 2);
        App.WaitForElement("Issue38802TestEditor");

        // Step 2: Get editor height before rotation
        var editorRectBefore = App.WaitForElement("Issue38802TestEditor").GetRect();
        var heightBefore = editorRectBefore.Height;

        // Step 3: Rotate to landscape
        App.SetOrientationLandscape();
        App.WaitForElement("Issue38802TestEditor");

        // Step 4: Rotate back to portrait
        App.SetOrientationPortrait();
        // Allow time for layout to settle after rotation
        Task.Delay(2000).Wait();
        App.WaitForElement("Issue38802TestEditor");

        // Step 5: Get editor height after rotation — should NOT grow
        var editorRectAfter = App.WaitForElement("Issue38802TestEditor").GetRect();
        var heightAfter = editorRectAfter.Height;

        Assert.That(heightAfter, Is.EqualTo(heightBefore).Within(1),
            $"Editor height should not grow after rotation. Before: {heightBefore}, After: {heightAfter}");
    }
}
#endif
