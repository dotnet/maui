using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38618 : _IssuesUITest
{
    public Issue38618(TestDevice testDevice) : base(testDevice) { }

    public override string Issue => "[MAUI] L2 TabBar Switching Test - Crashes when opening L2";

    [Test]
    [ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
    public void L2TabBarSwitchingDoesNotCrash()
    {
        App.WaitForElement("IssueCollectionView");

        App.Tap("IssueCollectionView");

        App.WaitForElement("Tab1");
    }
}
