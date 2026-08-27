using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28656 : _IssuesUITest
{
	public Issue28656(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "CollectionView CollectionViewHandler2 does not change ItemsLayout on DataTrigger";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void CollectionViewShouldChangeItemsLayout()
	{
		App.WaitForElement("ChangeLayoutButton");
		App.Click("ChangeLayoutButton");
		VerifyScreenshot();
	}
}