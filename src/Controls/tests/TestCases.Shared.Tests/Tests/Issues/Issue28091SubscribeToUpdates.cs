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
    /// This test verifies that increasing and decreasing the width of the Rectangle triggers layout updates.
    /// It asserts that the Rectangle's width increases and then decreases appropriately after tapping the corresponding buttons.
    /// </summary>
    [Test, Order(1)]
    [Category(UITestCategories.Performance)]
    public void IncreaseAndDecreaseWidth_ShouldUpdateRectangle()
    {
        App.WaitForElement("ResizableRectangle");
        var initialBounds = App.FindElement("ResizableRectangle").GetRect();

        App.Tap("IncreaseWidthButton");
        var increasedBounds = App.FindElement("ResizableRectangle").GetRect();
        Assert.That(increasedBounds.Width, Is.GreaterThan(initialBounds.Width));

        App.Tap("DecreaseWidthButton");
        App.Tap("DecreaseWidthButton");
        var decreasedBounds = App.FindElement("ResizableRectangle").GetRect();
        Assert.That(decreasedBounds.Width, Is.LessThan(initialBounds.Width));
    }

    /// <summary>
    /// This test verifies that increasing and decreasing the height of the Rectangle triggers layout updates.
    /// It asserts that the Rectangle's height increases and then decreases appropriately after tapping the corresponding buttons.
    /// </summary>
    [Test, Order(2)]
    [Category(UITestCategories.Performance)]
    public void IncreaseAndDecreaseHeight_ShouldUpdateRectangle()
    {
        App.WaitForElement("ResizableRectangle");
        var initialBounds = App.FindElement("ResizableRectangle").GetRect();

        App.Tap("IncreaseHeightButton");
        var increasedBounds = App.FindElement("ResizableRectangle").GetRect();
        Assert.That(increasedBounds.Height, Is.GreaterThan(initialBounds.Height));

        App.Tap("DecreaseHeightButton");
        App.Tap("DecreaseHeightButton");
        var decreasedBounds = App.FindElement("ResizableRectangle").GetRect();
        Assert.That(decreasedBounds.Height, Is.LessThan(initialBounds.Height));
    }

    /// <summary>
    /// This test verifies that layout performance updates are reflected in the HistoryLabel.
    /// It performs resizing actions and asserts that the label text is updated to include the profiling information.
    /// </summary>
    [Test, Order(3)]
    [Category(UITestCategories.Performance)]
    public void HistoryLabel_ShouldUpdate_AfterInteractions()
    {
        App.WaitForElement("ResizableRectangle");
        var initialText = App.FindElement("HistoryLabel")?.GetText();

        App.Tap("IncreaseWidthButton");
        App.Tap("IncreaseHeightButton");

        var updatedText = App.FindElement("HistoryLabel")?.GetText();
        Assert.That(updatedText, Is.Not.Null.And.Contains("Rectangle").IgnoreCase);
    }
}