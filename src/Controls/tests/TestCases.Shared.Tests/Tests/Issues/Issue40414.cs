using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40414 : _IssuesUITest
{
	public override string Issue => "CollectionViewHandler2 does not display grouped CollectionView items after ItemsSource is reset";

	public Issue40414(TestDevice device)
	: base(device)
	{ }

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView)]
	public void GroupedItemsAreDisplayedAfterNavigatingBackToFlyoutPage()
	{
		App.WaitForElement("Flyout1Label");

		App.TapInShellFlyout("Flyout2Content");
		App.WaitForElement("GroupedCollectionView");

		App.TapInShellFlyout("Flyout1Content");
		App.WaitForElement("Flyout1Label");

		App.TapInShellFlyout("Flyout2Content");
		App.WaitForElement("GroupedCollectionView");

		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
