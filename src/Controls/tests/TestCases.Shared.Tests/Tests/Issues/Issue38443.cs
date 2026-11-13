using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

internal class Issue38443 : _IssuesUITest
{
    public Issue38443(TestDevice device) : base(device)
    {
    }

    public override string Issue => "CollectionView SelectedItem set during Shell navigation does not update the visual state";

    [Test]
    [ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
    public void SelectedItemSetDuringShellNavigationUpdatesVisualState()
    {
        App.WaitForElement("NavigateButton");
        App.Tap("NavigateButton");

        App.WaitForElement("InstructionsLabel");
        VerifyScreenshot();
    }
}