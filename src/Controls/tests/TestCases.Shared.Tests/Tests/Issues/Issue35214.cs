#if TEST_FAILS_ON_WINDOWS // Related issue for windows: https://github.com/dotnet/maui/issues/29245 
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

    [Test, Order(1)]
    [Category(UITestCategories.IndicatorView)]
    public void VerifyIndicatorSizeBeforeReset()
    {
        App.WaitForElement("TestIndicatorView");
        VerifyScreenshot("IndicatorSizeBeforeReset", retryTimeout: TimeSpan.FromSeconds(2));
    }

    [Test, Order(2)]
    [Category(UITestCategories.IndicatorView)]
    public void VerifyIndicatorSizeAfterReset()
    {
        App.WaitForElement("SetDefaultSizeButton");
        App.Tap("SetDefaultSizeButton");
        VerifyScreenshot("IndicatorSizeAfterReset", retryTimeout: TimeSpan.FromSeconds(2));
    }
}
#endif