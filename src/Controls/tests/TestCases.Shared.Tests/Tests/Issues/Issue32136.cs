using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32136 : _IssuesUITest
{
    public Issue32136(TestDevice device) : base(device)
    {
    }

    public override string Issue => "CarouselView CurrentItem Not Updating with Vertical LinearItemsLayout";

    [Test]
    [Category(UITestCategories.CarouselView)]
    public void CurrentItemShouldUpdateWhenScrollingVerticalCarouselView()
    {
        App.WaitForElement("TestCarouselView");
        App.ScrollDown("TestCarouselView", ScrollStrategy.Gesture, 1.0);
        Task.Delay(1500).Wait();
        var currentItemText = App.WaitForElement("CurrentItemLabel").GetText();
        Assert.That(currentItemText, Is.EqualTo("CurrentItem = Capuchin Monkey"));
    }
}