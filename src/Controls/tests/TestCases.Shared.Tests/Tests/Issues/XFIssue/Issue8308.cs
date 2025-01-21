using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue8308 : _IssuesUITest
{
	public Issue8308(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "[Bug] [iOS] Cannot access a disposed object. Object name: 'GroupableItemsViewController`1";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void NavigatingBackToCollectionViewShouldNotCrash()
	{
		App.WaitForElement("Instructions");
		App.TapInShellFlyout("Page 2");

		App.WaitForElement("Instructions2");
		App.TapInShellFlyout("Page 1");
		App.WaitForElement("Instructions");
	}
}