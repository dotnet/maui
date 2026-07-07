#if TEST_FAILS_ON_WINDOWS // https://github.com/dotnet/maui/issues/18551
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32404 : _IssuesUITest
{
    public Issue32404(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "[Android, iOS, MacOS] FlowDirection not working on EmptyView in CollectionView";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void FlowDirectionShouldWorkOnEmptyView()
    {
        App.WaitForElement("Issue32404ToggleButton");
        App.Tap("Issue32404ToggleButton");
        VerifyScreenshot("FlowDirectionShouldWorkOnEmptyView_RightToLeft");
        App.Tap("Issue32404ToggleButton");
        VerifyScreenshot("FlowDirectionShouldWorkOnEmptyView_LeftToRight");
    }
}
#endif