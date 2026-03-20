using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18679 : _IssuesUITest
{
    public Issue18679(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Canvas.GetStringSize() is not consistent with actual string size in GraphicsView";

    [Test]
    [Category(UITestCategories.GraphicsView)]
    public void DrawTextWithinBounds()
    {
        App.WaitForElement("18679DescriptionLabel");
        VerifyScreenshot();
    }
}