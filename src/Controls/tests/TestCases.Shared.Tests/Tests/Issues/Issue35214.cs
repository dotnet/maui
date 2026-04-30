using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35214 : _IssuesUITest
{
    public Issue35214(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Dynamically changing IndicatorView IndicatorSize to default value does not work";

    [Test]
    [Category(UITestCategories.IndicatorView)]
    public void IndicatorSizeResetsToDefault()
    {
        App.WaitForElement("TestIndicatorView");
        VerifyScreenshot("IndicatorSizeBeforeReset");
        App.Tap("SetDefaultSizeButton");
        VerifyScreenshot("IndicatorSizeAfterReset");
    }
}
