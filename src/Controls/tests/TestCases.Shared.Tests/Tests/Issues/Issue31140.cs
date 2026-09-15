using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31140 : _IssuesUITest
{
    public override string Issue => "Setting both IndicatorSize and Shadow properties on IndicatorView causes some dots to be invisible";

    public Issue31140(TestDevice device)
    : base(device)
    { }

    [Test]
    [Category(UITestCategories.IndicatorView)]
    public void VerifyAllIndicatorDotsShowShadowsWhenIndicatorSize()
    {
        App.WaitForElement("carouselview");
        VerifyScreenshot();
    }
}
