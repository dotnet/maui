using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35326 : _IssuesUITest
{
	public Issue35326(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView.ScrollTo(index) doesn't work correctly when IsGrouped=\"True\" on iOS, MacCatalyst, and Windows";

	// Index 40 maps to Group 5, Item 1 (5 groups × 10 items, groups 1-4 occupy indices 0-39)
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void GroupedCollectionViewScrollToIndexScrollsToCorrectItem()
	{
		App.WaitForElement("GroupedCollectionView");

		// Initially the first item of the first group should be visible
		App.WaitForElement("Item_G1_I1");

		// Scroll to index 40 (should land at Group 5, Item 1)
		App.Tap("ScrollToEndButton");

		App.WaitForElement("Item_G5_I1");

		// Scroll back to start (index 0)
		App.Tap("ScrollToStartButton");

		App.WaitForElement("Item_G1_I1");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void GroupedCollectionViewScrollToItemAndGroupScrollsToCorrectItem()
	{
		App.WaitForElement("GroupedCollectionView");

		// Initially the first item should be visible
		App.WaitForElement("Item_G1_I1");

		// Scroll to last item using ScrollTo(item, group) overload
		App.Tap("ScrollToItemButton");

		App.WaitForElement("Item_G5_I10");

		// Scroll back to start
		App.Tap("ScrollToStartButton");

		App.WaitForElement("Item_G1_I1");
	}
}
