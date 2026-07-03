#if TEST_FAILS_ON_WINDOWS       //More info: https://github.com/dotnet/maui/issues/14777
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue19541 : _IssuesUITest
{
    public Issue19541(TestDevice device) : base(device)
    {
    }

    public override string Issue => "[iOS] - Swipeview with collectionview issue";

    [Test]
    [Category(UITestCategories.SwipeView)]
    public void SwipeItemShouldBeCloseWhenUpdateTheCollectionView()
    {
        App.WaitForElement("RefreshButton");
        App.Tap("OpenButton");
        App.Tap("RefreshButton");
        VerifyScreenshot();
    }
}
#endif