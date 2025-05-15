#if MACCATALYST || WINDOWS
// TitleBar is only available on Mac Catalyst and Windows. https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/titlebar?view=net-maui-9.0
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29516 : _IssuesUITest
{
    public Issue29516(TestDevice device) : base(device) { }

    public override string Issue => "Issue with the TitleBar TrailingContent not being properly aligned on macOS";

    [Test]
    [Category(UITestCategories.Window)]
    public void TitleBarTrailingContentShouldRenderProperly()
    {
        App.WaitForElement("ContentLabel");
        VerifyScreenshot();
    }
}
#endif