using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39295 : _IssuesUITest
{
	public Issue39295(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView inside RefreshView becomes blank when scrolling on Mac Catalyst";

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView)]
	public void CollectionViewItemsRemainVisibleAfterScrolling()
	{
		App.WaitForElement("GoToCollectionViewTestButton");
		App.Tap("GoToCollectionViewTestButton");
		App.WaitForElement("ScrollTo10thItemButton");
		App.Tap("ScrollTo10thItemButton");
		VerifyScreenshot();
	}
}
