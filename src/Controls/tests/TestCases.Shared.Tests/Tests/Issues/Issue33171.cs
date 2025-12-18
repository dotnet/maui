using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue33171 : _IssuesUITest
{
    public override string Issue => "When TitleBar.IsVisible = false the caption buttons become unresponsive on Windows";

    public Issue33171(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.TitleView)]
    public void TitleBarCaptionButtonsResponsiveWhenIsVisibleFalse()
    {
    }
}