using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38251 : _IssuesUITest
{
    public Issue38251(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Button RTL image and text overlap on iOS";

    [Test]
    [Category(UITestCategories.Button)]
    public void ButtonContentLayoutShouldRespectRightToLeftFlowDirection()
    {
        _ = App.WaitForElement("LtrReferenceButton");
        _ = App.WaitForElement("RtlReferenceButton");

        VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
    }
}