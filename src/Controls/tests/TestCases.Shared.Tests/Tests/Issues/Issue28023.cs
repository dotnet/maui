using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28023 : _IssuesUITest
{
    public override string Issue => "I2 Entering the vertical and horizontal lists again Monkeys still retain the spacing value changed last time";

    public Issue28023(TestDevice device) : base(device)
    {
    }

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void ShouldUpdateItemSpacingAfterPageNavigation()
    {
        App.WaitForElement("NavigatedPageButton");
        App.Tap("NavigatedPageButton");
        App.WaitForElement("MainPageButton");
        App.Tap("MainPageButton");
        App.Tap("PopButton");
        App.WaitForElement("NavigatedPageButton");
        App.Tap("NavigatedPageButton");
        VerifyScreenshot();
    }
}
