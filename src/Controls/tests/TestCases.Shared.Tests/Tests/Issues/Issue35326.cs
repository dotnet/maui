using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35326 : _IssuesUITest
{
	public Issue35326(TestDevice device) : base(device) { }

	public override string Issue => "CollectionView.ScrollTo(index) doesn't work correctly when IsGrouped=\"True\" on iOS and MacCatalyst";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void GroupedCollectionViewScrollToIndexScrollsToCorrectItem()
	{
		// Initially the first item of the first group should be visible
		App.WaitForElement("Item_G1_I1");

		// Scroll to index 49 (should land at Group 5, Item 10)
		App.Tap("ScrollToEndButton");

		App.WaitForElement("Item_G5_I10");

		// Scroll back to start (index 0)
		App.Tap("ScrollToStartButton");

		App.WaitForElement("Item_G1_I1");
	}
}
