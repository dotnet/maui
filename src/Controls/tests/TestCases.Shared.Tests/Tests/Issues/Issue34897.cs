using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34897 : _IssuesUITest
{
	public Issue34897(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView Header is not visible when ItemsSource is not set and EmptyView is set";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewHeaderVisibleWithEmptyViewAndNullItemsSource()
	{
		// Wait for the page to load — EmptyView should appear since ItemsSource is null
		App.WaitForElement("Issue34897EmptyView");

		// The Header must ALSO be visible even though ItemsSource is null and EmptyView is active.
		// Bug: UICollectionViewCompositionalLayout drops boundary supplementary items (header/footer)
		// when NumberOfSections returns 0 (which happens when ItemsSource is null).
		App.WaitForElement("Issue34897Header");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void CollectionViewFooterVisibleWithEmptyViewAndNullItemsSource()
	{
		// Wait for page load
		App.WaitForElement("Issue34897EmptyView");

		// The Footer must also be visible — same root cause as the header bug.
		// This test covers the footer scenario guarded by TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST.
		App.WaitForElement("Issue34897Header");
	}
}
