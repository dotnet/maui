using System.Linq;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class CollectionViewNullItemDragReorder : _IssuesUITest
{
	public CollectionViewNullItemDragReorder(TestDevice device) : base(device)
	{
	}

	public override string Issue => "CollectionView2 (Windows) drag-and-drop reorder with null items in the ItemsSource";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void DraggingNullItemRowDoesNotCrashAndReorders()
	{
		// This scenario exercises CollectionView2, which is currently Windows-only.
		this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

		App.WaitForElement("ReorderItemLabel0");
		App.WaitForElement("ReorderItemLabel2");
		App.WaitForElement("ReorderItemLabel3");

		var initialOrder = App.WaitForElement("ReorderStatusLabel").GetText();
		Assert.That(initialOrder, Is.EqualTo("Item A, null, Item B, Item C"));

		// The null row (index 1 in the source: Item A, null, Item B, Item C) has no
		// AutomationId of its own since ItemFactory renders it as a blank container with
		// no applied DataTemplate. Locate it via screen coordinates, directly between the
		// "Item A" and "Item B" rows, so the drag actually originates from the blank
		// null-data container. This exercises the Tag-based blank-container drag detection
		// and GetContainerIndex's ItemsRepeater.GetElementIndex fallback for null items
		// (as opposed to only testing ObservableCollection.Move() BCL semantics).
		var itemARect = App.WaitForElement("ReorderItemLabel0").GetRect();
		var itemBRect = App.WaitForElement("ReorderItemLabel2").GetRect();
		var itemCRect = App.WaitForElement("ReorderItemLabel3").GetRect();

		float nullRowX = itemARect.CenterX();
		float nullRowY = (itemARect.Bottom + itemBRect.Top) / 2f;

		// Drag the null row down past the last item ("Item C"). Success is verified by
		// the drag completing without crashing the app and the reorder round-tripping
		// through the real handler pipeline (ReorderCompleted -> status label updates).
		App.DragCoordinates(nullRowX, nullRowY, itemCRect.CenterX(), itemCRect.CenterY());

		App.WaitForElement("ReorderStatusLabel");
		var reorderedText = App.WaitForElement("ReorderStatusLabel").GetText();

		Assert.That(reorderedText, Is.Not.EqualTo(initialOrder),
			"Dragging the null-item row should trigger ReorderCompleted and change the item order.");

		var reorderedItems = reorderedText.Split(", ");
		Assert.That(reorderedItems, Has.Length.EqualTo(4), "No item should be lost or duplicated during the reorder.");
		Assert.That(reorderedItems.Count(i => i == "null"), Is.EqualTo(1), "The null item must still be present exactly once.");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void DraggingRealItemOntoNullItemRowDoesNotCrashAndReorders()
	{
		// This scenario exercises CollectionView2, which is currently Windows-only.
		this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

		App.WaitForElement("ReorderItemLabel0");
		App.WaitForElement("ReorderItemLabel2");
		App.WaitForElement("ReorderItemLabel3");

		var initialOrder = App.WaitForElement("ReorderStatusLabel").GetText();
		Assert.That(initialOrder, Is.EqualTo("Item A, null, Item B, Item C"));

		var itemARect = App.WaitForElement("ReorderItemLabel0").GetRect();
		var itemBRect = App.WaitForElement("ReorderItemLabel2").GetRect();
		var itemCRect = App.WaitForElement("ReorderItemLabel3").GetRect();

		float nullRowX = itemARect.CenterX();
		float nullRowY = (itemARect.Bottom + itemBRect.Top) / 2f;

		// Drag "Item C" (last item) and drop it onto the null row, which sits between
		// "Item A" and "Item B". This exercises PerformReorder's insertion-index handling
		// when the drop target is a blank null-data container.
		App.DragCoordinates(itemCRect.CenterX(), itemCRect.CenterY(), nullRowX, nullRowY);

		App.WaitForElement("ReorderStatusLabel");
		var reorderedText = App.WaitForElement("ReorderStatusLabel").GetText();

		Assert.That(reorderedText, Is.Not.EqualTo(initialOrder),
			"Dropping an item onto the null-item row should trigger ReorderCompleted and change the item order.");

		var reorderedItems = reorderedText.Split(", ");
		Assert.That(reorderedItems, Has.Length.EqualTo(4), "No item should be lost or duplicated during the reorder.");
		Assert.That(reorderedItems.Count(i => i == "null"), Is.EqualTo(1), "The null item must still be present exactly once.");
	}
}
