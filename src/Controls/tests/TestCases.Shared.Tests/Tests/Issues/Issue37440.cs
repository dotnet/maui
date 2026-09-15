using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37440 : _IssuesUITest
{
    public Issue37440(TestDevice device) : base(device) { }

    public override string Issue => "Editor auto expands wrongly to MaximumHeightRequest";

    [Test]
    [Category(UITestCategories.Material3)]
    public void Issue37440EmptyAutoSizeEditorDoesNotSnapToMaximumHeight()
    {
        App.WaitForElement("Issue37440Editor");
        VerifyScreenshot();
    }
}
