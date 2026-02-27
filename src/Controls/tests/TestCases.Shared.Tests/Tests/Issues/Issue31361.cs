#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS //In Catalyst and Windows, `ScrollDown` isn't functioning correctly with Appium.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31361 : _IssuesUITest
{
    public override string Issue => "CarouselView content is scrolling vertically";

    public Issue31361(TestDevice device)
    : base(device)
    { }

    [Test, Order(1)]
    [Category(UITestCategories.CarouselView)]
    public void VerifyHorizontalLoopableCarouselViewPreventsVerticalScrolling()
    {
        App.WaitForElement("carouselview");
        App.ScrollDown("carouselview");
        var text = App.FindElement("contentLabel").GetText();
        Assert.That(text, Is.EqualTo("The content is not scrollable"));
    }

    [Test, Order(2)]
    [Category(UITestCategories.CarouselView)]
    public void VerifyHorizontalCarouselViewPreventsVerticalScrolling()
    {
        App.WaitForElement("carouselview");
        App.Tap("changeLoopButton");
        App.ScrollDown("carouselview");
        var text = App.FindElement("contentLabel").GetText();
        Assert.That(text, Is.EqualTo("The content is not scrollable"));
    }

    [Test, Order(3)]
    [Category(UITestCategories.CarouselView)]
    public void VerifyVerticalCarouselViewPreventsHorizontalScrolling()
    {
        App.WaitForElement("carouselview");
        App.Tap("orientationButton");
        App.ScrollLeft("carouselview");
        var text = App.FindElement("contentLabel").GetText();
        Assert.That(text, Is.EqualTo("The content is not scrollable"));
    }

    [Test, Order(4)]
    [Category(UITestCategories.CarouselView)]
    public void VerifyVerticalLoopableCarouselViewPreventsHorizontalScrolling()
    {
        App.WaitForElement("carouselview");
        App.Tap("changeLoopButton");
        App.ScrollLeft("carouselview");
        var text = App.FindElement("contentLabel").GetText();
        Assert.That(text, Is.EqualTo("The content is not scrollable"));
    }
}
#endif