using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31535 : _IssuesUITest
{
    public Issue31535(TestDevice device) : base(device) { }

    public override string Issue => "[iOS] Crash occurred on CarouselView2 when deleting last one remaining item with loop as false";

    [Test]
    [Category(UITestCategories.CarouselView)]
    public void CarouselView2ShouldNotCrashOnRemoveLastItem()
    {
        App.WaitForElement("RemoveLastItemButton");
        App.Tap("RemoveLastItemButton");
        //remove last one remaining item
        App.Tap("RemoveLastItemButton");
        App.WaitForElement("TestCarouselView");
    }
}