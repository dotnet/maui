using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21957 : _IssuesUITest
{
    public override string Issue => "Views with explicit size larger than screen do not respect margins";

    public Issue21957(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Layout)]
    public void ViewsWithLargeWidthRespectMargins()
    {
        App.WaitForElement("YellowStack");
        VerifyScreenshot();
    }
}