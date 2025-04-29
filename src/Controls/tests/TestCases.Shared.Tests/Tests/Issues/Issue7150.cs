#if TEST_FAILS_ON_WINDOWS // Related issue for windows: https://github.com/dotnet/maui/issues/29245 
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue7150 : _IssuesUITest
{
    public Issue7150(TestDevice device) : base(device) { }

    public override string Issue => "EmptyView using Template displayed at the same time as the content";

    [Test]
    [Category(UITestCategories.CarouselView)]
    public void VerifyCarouselViewEmptyView()
    {
        App.WaitForElement("FilterButton");
        App.Tap("FilterButton");
        VerifyScreenshot();
    }
}
#endif