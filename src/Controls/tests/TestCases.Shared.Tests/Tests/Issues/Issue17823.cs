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
		App.WaitForElement("ReorderItem0");
		App.WaitForElement("ReorderItem3");

		// Verify initial state
		var initialText = App.FindElement("ReorderStatusLabel").GetText();
		Assert.That(initialText, Is.EqualTo("Item 1, Item 2, Item 3, Item 4"));

		// The bug: dragging first item to the end would crash with header present
		// This is because adapter indices include header (0=header, 1=item0, 2=item1, etc.)
		// but item source indices don't (0=item0, 1=item1, etc.)
		App.DragAndDrop("ReorderItem0", "ReorderItem3");

		// Verify reorder succeeded without crash by checking status label updated
		var afterDrag = App.FindElement("ReorderStatusLabel").GetText();
		Assert.That(afterDrag, Is.EqualTo("Item 2, Item 3, Item 4, Item 1"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void ReorderingLastItemWithHeaderDoesNotCrash()
	{
		// Verify header is present
		App.WaitForElement("HeaderLabel");

		// Verify items are present
		App.WaitForElement("ReorderItem0");
		App.WaitForElement("ReorderItem3");

		// Verify initial state
		var initialText = App.FindElement("ReorderStatusLabel").GetText();
		Assert.That(initialText, Is.EqualTo("Item 1, Item 2, Item 3, Item 4"));

		// Test dragging last item to first position
		// This tests the opposite direction
		App.DragAndDrop("ReorderItem3", "ReorderItem0");

		// Verify reorder succeeded without crash
		var afterDrag = App.FindElement("ReorderStatusLabel").GetText();
		Assert.That(afterDrag, Is.EqualTo("Item 4, Item 1, Item 2, Item 3"));
	}
}
