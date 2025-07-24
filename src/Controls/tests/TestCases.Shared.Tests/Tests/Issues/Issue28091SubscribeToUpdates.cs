using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28091SubscribeToUpdates : _IssuesUITest
{
    public override string Issue => "Add Layout Performance Profiler (SubscribeToUpdates)";

    public Issue28091SubscribeToUpdates(TestDevice device) : base(device)
    {
    }

    /// <summary>
    /// This test verifies that layout performance updates are reflected in the HistoryLabel.
    /// It performs resizing actions and asserts that the label text is updated to include the profiling information.
    /// </summary>
    [Test, Order(3)]
    [Category(UITestCategories.Performance)]
    public async Task HistoryLabel_ShouldUpdate_AfterInteractions()
    {
        App.WaitForElement("WaitForStubControl");
        var initialText = App.FindElement("HistoryLabel")?.GetText();

        // Simulate layout changes
        App.Tap("IncreaseWidthButton");
        App.Tap("IncreaseHeightButton");
        
        // Wait briefly to allow profiler tracking events
        await Task.Delay(500);
        
        var updatedText = App.FindElement("HistoryLabel")?.GetText();
        Assert.That(updatedText, Is.Not.Null
	        .And.Contains("Rectangle")
	        .And.Contains("Duration")
	        .IgnoreCase);
        Assert.That(updatedText, Is.Not.EqualTo(initialText));
    }
}