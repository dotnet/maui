using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17823 : _IssuesUITest
{
	public Issue17823(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView reordering last item succeeds when header is present";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ReorderingItemToEndWithHeaderDoesNotCrash()
	{
		// Verify header is present
		App.WaitForElement("HeaderLabel");

		// Verify all items are present
		App.WaitForElement("ReorderItemLabel0");
		App.WaitForElement("ReorderItemLabel3");

		// Test dragging first item to last position
		App.DragAndDrop("ReorderItemLabel0", "ReorderItemLabel3");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ReorderingLastItemWithHeaderDoesNotCrash()
	{
		// Verify header is present
		App.WaitForElement("HeaderLabel");

		// Verify items are present
		App.WaitForElement("ReorderItemLabel0");
		App.WaitForElement("ReorderItemLabel3");

		// Test dragging last item to first position
		App.DragAndDrop("ReorderItemLabel3", "ReorderItemLabel0");
	}
}