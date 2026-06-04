using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class CollectionView_DragDropFeatureTests : _GalleryUITest
{
    public const string DragDropFeatureMatrix = "CollectionView Feature Matrix";
    public const string DragDropButton = "DragDropButton";
    public const string Options = "Options";
    public const string Apply = "Apply";
    public const string ItemsSourceGroupedList = "ItemsSourceGroupedList";
    public const string IsGroupedTrue = "IsGroupedTrue";
    public const string CanReorderItemsFalse = "CanReorderItemsFalse";
    public const string CanReorderItemsTrue = "CanReorderItemsTrue";
    public const string CanMixGroupsFalse = "CanMixGroupsFalse";
    public const string CanMixGroupsTrue = "CanMixGroupsTrue";
    public const string ItemsLayoutVerticalGrid = "ItemsLayoutVerticalGrid";

    public override string GalleryPageName => DragDropFeatureMatrix;

    public CollectionView_DragDropFeatureTests(TestDevice device)
        : base(device)
    {
    }

#if TEST_FAILS_ON_WINDOWS // CollectionView grouped drag-and-drop reorder behavior on Windows
	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void VerifyDragAndDropWithCanReorderItemsAndCanMixGroupsInVerticalGrid()
	{
		App.WaitForElement(DragDropButton);
		App.Tap(DragDropButton);
		App.WaitForElement("CollectionViewControl");
		App.WaitForElement("Fruits");
		App.WaitForElement("Vegetables");
		App.WaitForElement("Banana");
		App.WaitForElement("Potato");

		var initialY = App.WaitForElement("Banana").GetRect().Y;
		App.DragAndDrop("Banana", "Potato");
		var newY = App.WaitForElement("Banana").GetRect().Y;
		Assert.That(newY, Is.GreaterThan(initialY), "The Y position of 'Banana' should increase after drag-and-drop when CanReorderItems and CanMixGroups are enabled in the DragDrop feature page.");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyDragAndDropDoesNotReorderWhenCanReorderItemsIsFalse()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(CanReorderItemsFalse);
		App.Tap(CanReorderItemsFalse);
		App.WaitForElement(CanMixGroupsTrue);
		App.Tap(CanMixGroupsTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);

		var initialY = App.WaitForElement("Banana").GetRect().Y;
		App.DragAndDrop("Banana", "Potato");
		var newY = App.WaitForElement("Banana").GetRect().Y;
		Assert.That(newY, Is.EqualTo(initialY), "The Y position of 'Banana' should not change when CanReorderItems is disabled.");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyDragAndDropWithCanReorderItemsAndCanMixGroupsExplicitlyEnabled()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(CanReorderItemsTrue);
		App.Tap(CanReorderItemsTrue);
		App.WaitForElement(CanMixGroupsTrue);
		App.Tap(CanMixGroupsTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);

		var initialY = App.WaitForElement("Banana").GetRect().Y;
		App.DragAndDrop("Banana", "Potato");
		var newY = App.WaitForElement("Banana").GetRect().Y;
		Assert.That(newY, Is.GreaterThan(initialY), "The Y position of 'Banana' should increase after drag-and-drop when CanReorderItems and CanMixGroups are explicitly enabled.");
	}
#endif
}
