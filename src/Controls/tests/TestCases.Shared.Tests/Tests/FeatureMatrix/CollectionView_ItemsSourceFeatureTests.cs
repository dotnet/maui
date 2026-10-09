using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class CollectionView_ItemsSourceFeatureTests : _GalleryUITest
{
	public const string ItemsSourceFeatureMatrix = "CollectionView Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string ModelItem = "ModelItem";
	public const string ItemsSourceObservableCollection = "ItemsSourceObservableCollection";
	public const string ItemsSourceList = "ItemsSourceList";
	public const string ItemsSourceGroupedList = "ItemsSourceGroupedList";
	public const string CollectionViewItemsSource = "CollectionViewItemsSource";
	public const string EmptyGroupedListT = "EmptyGroupedList";
	public const string EmptyObservableCollectionT = "EmptyObservableCollection";
	public const string IsGroupedTrue = "IsGroupedTrue";
	public const string ItemsLayoutVerticalList = "ItemsLayoutVerticalList";
	public const string ItemsLayoutHorizontalList = "ItemsLayoutHorizontalList";
	public const string ItemsLayoutVerticalGrid = "ItemsLayoutVerticalGrid";
	public const string ItemsLayoutHorizontalGrid = "ItemsLayoutHorizontalGrid";
	public const string AddItems = "AddItems";
	public const string RemoveItems = "RemoveItems";
	public const string ReplaceItemsSource = "ReplaceItemsSource";
	public const string IndexEntry = "IndexEntry";
	public const string CurrentSelectionTextLabel = "CurrentSelectionTextLabel";
	public const string MultipleModePreselection = "MultipleModePreselection";
	public const string SingleModePreselection = "SingleModePreselection";

	public override string GalleryPageName => ItemsSourceFeatureMatrix;
	protected override string GallerySubPageButton => "ItemsSourceButton";

	public CollectionView_ItemsSourceFeatureTests(TestDevice device)
		: base(device)
	{
	}

	private void ConfigureItemsSource(string itemsSource, string itemsLayout, bool modelItems = false, bool grouped = false)
	{
		App.WaitForElement(Options);
		App.Tap(Options);

		if (modelItems)
		{
			App.WaitForElement(ModelItem);
			App.Tap(ModelItem);
		}

		if (grouped)
		{
			App.WaitForElement(IsGroupedTrue);
			App.Tap(IsGroupedTrue);
		}

		App.WaitForElement(itemsSource);
		App.Tap(itemsSource);
		App.WaitForElement(itemsLayout);
		App.Tap(itemsLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
	}

	private void WaitForItemInItemsLayout(string item, string itemsLayout, bool scrollLeft = false)
	{
		if (itemsLayout is ItemsLayoutHorizontalList or ItemsLayoutHorizontalGrid)
		{
			if (scrollLeft)
			{
				App.ScrollLeft(CollectionViewItemsSource, ScrollStrategy.Gesture, swipePercentage: 0.9, swipeSpeed: 500);
				App.ScrollLeft(CollectionViewItemsSource, ScrollStrategy.Gesture, swipePercentage: 0.9, swipeSpeed: 500);
			}
			else
				App.ScrollRight(CollectionViewItemsSource, ScrollStrategy.Gesture, swipePercentage: 0.9, swipeSpeed: 500);
		}

		App.WaitForElement(item);
	}

	// ItemsSource replacement scenarios
	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyModelItemsGroupedListWhenItemsSourceIsReplaced()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("dotnet_bot.png");
		App.WaitForElement(ReplaceItemsSource);
		App.Tap(ReplaceItemsSource);
		App.WaitForElement("Updated Group A");
		App.WaitForElement("Updated Group B");
		App.WaitForElement("Updated calculator.png");
		App.WaitForNoElement("dotnet_bot.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyStringItemsGroupedListWhenItemsSourceIsReplaced()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Banana");
		App.WaitForElement(ReplaceItemsSource);
		App.Tap(ReplaceItemsSource);
		App.WaitForElement("Updated Fruits");
		App.WaitForElement("Updated Vegetables");
		App.WaitForElement("Updated Item 1");
		App.WaitForNoElement("Fruits");
		App.WaitForNoElement("Vegetables");
		App.WaitForNoElement("Banana");
	}

	// Source configuration scenarios
	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyChangingItemTypeAfterSelectingList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceList);
		App.Tap(ItemsSourceList);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("dotnet_bot.png");
		App.WaitForNoElement("Banana");
	}

	// Flat source mutation scenarios
	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyModelItemsObservableCollectionWhenAddIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "2");
		App.Tap(AddItems);
		App.WaitForElement("oasis.jpg");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyModelItemsObservableCollectionWhenAddItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForTextToBePresentInElement("ItemsSourceItemCount", "4");
		App.ScrollDown(CollectionViewItemsSource, ScrollStrategy.Gesture, swipePercentage: 0.9, swipeSpeed: 500);
		App.WaitForElement("green.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyModelItemsObservableCollectionWhenRemoveIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("dotnet_bot.png");
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.Tap(RemoveItems);
		App.WaitForNoElement("dotnet_bot.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyModelItemsObservableCollectionWhenRemoveItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("avatar.png");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("avatar.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyStringItemsObservableCollectionWhenAddIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "2");
		App.Tap(AddItems);
		App.WaitForElement("Chikoo");
		Assert.That(App.FindElementsByText("Chikoo").Count, Is.EqualTo(1));
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.Tap(AddItems);
		App.WaitForElement("Kiwi");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyStringItemsObservableCollectionWhenAddIndexIsAtEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "4");
		App.Tap(AddItems);
		App.WaitForElement("Papaya");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyStringItemsObservableCollectionWhenAddItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForElement("Kiwi");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyStringItemsObservableCollectionWhenIndexIsInvalid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);

		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "-1");
		App.Tap(AddItems);
		App.WaitForElement("Apple");

		App.EnterText(IndexEntry, "99");
		App.Tap(RemoveItems);
		App.WaitForElement("Broccoli");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyStringItemsObservableCollectionWhenRemoveIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Carrot");
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "2");
		App.Tap(RemoveItems);
		App.WaitForNoElement("Carrot");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyStringItemsObservableCollectionWhenRemoveItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Broccoli");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("Broccoli");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyModelItemsListWhenAddIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(ItemsSourceList);
		App.Tap(ItemsSourceList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "1");
		App.Tap(AddItems);
		App.WaitForElement("groceries.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyModelItemsListWhenAddItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(ItemsSourceList);
		App.Tap(ItemsSourceList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForElement("green.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyModelItemsListWhenRemoveIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(ItemsSourceList);
		App.Tap(ItemsSourceList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("dotnet_bot.png");
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.Tap(RemoveItems);
		App.WaitForNoElement("dotnet_bot.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyModelItemsListWhenRemoveItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(ItemsSourceList);
		App.Tap(ItemsSourceList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("calculator.png");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("calculator.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyStringItemsListWhenAddIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceList);
		App.Tap(ItemsSourceList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "2");
		App.Tap(AddItems);
		App.WaitForElement("Chikoo");
		Assert.That(App.FindElementsByText("Chikoo").Count, Is.EqualTo(1));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyStringItemsListWhenAddItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceList);
		App.Tap(ItemsSourceList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForElement("Kiwi");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyStringItemsListWhenRemoveIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceList);
		App.Tap(ItemsSourceList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Carrot");
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "2");
		App.Tap(RemoveItems);
		App.WaitForNoElement("Carrot");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyStringItemsListWhenRemoveItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceList);
		App.Tap(ItemsSourceList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Broccoli");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("Broccoli");
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyStringObservableCollectionReplacement_WithVerticalList")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyStringObservableCollectionReplacement_WithHorizontalList")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyStringObservableCollectionReplacement_WithVerticalGrid")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyStringObservableCollectionReplacement_WithHorizontalGrid")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyStringObservableCollectionReplacement(string itemsLayout)
	{
		VerifyItemsSourceReplacement(itemsLayout, ItemsSourceObservableCollection, "Banana", "Updated Item 1", expectedInitialCount: 4);
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyStringListReplacement_WithVerticalList")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyStringListReplacement_WithHorizontalList")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyStringListReplacement_WithVerticalGrid")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyStringListReplacement_WithHorizontalGrid")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyStringListReplacement(string itemsLayout)
	{
		VerifyItemsSourceReplacement(itemsLayout, ItemsSourceList, "Banana", "Updated Item 1", expectedInitialCount: 4);
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyStringGroupedListReplacement_WithVerticalList")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyStringGroupedListReplacement_WithHorizontalList")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyStringGroupedListReplacement_WithVerticalGrid")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyStringGroupedListReplacement_WithHorizontalGrid")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyStringGroupedListReplacement(string itemsLayout)
	{
		VerifyItemsSourceReplacement(itemsLayout, ItemsSourceGroupedList, "Banana", "Updated Item 1", expectedInitialCount: 6, grouped: true);
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyModelObservableCollectionReplacement_WithVerticalList")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyModelObservableCollectionReplacement_WithHorizontalList")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyModelObservableCollectionReplacement_WithVerticalGrid")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyModelObservableCollectionReplacement_WithHorizontalGrid")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyModelObservableCollectionReplacement(string itemsLayout)
	{
		VerifyItemsSourceReplacement(itemsLayout, ItemsSourceObservableCollection, "dotnet_bot.png", "Updated calculator.png", expectedInitialCount: 3, modelItems: true);
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyModelListReplacement_WithVerticalList")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyModelListReplacement_WithHorizontalList")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyModelListReplacement_WithVerticalGrid")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyModelListReplacement_WithHorizontalGrid")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyModelListReplacement(string itemsLayout)
	{
		VerifyItemsSourceReplacement(itemsLayout, ItemsSourceList, "dotnet_bot.png", "Updated calculator.png", expectedInitialCount: 2, modelItems: true);
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyModelGroupedListReplacement_WithVerticalList")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyModelGroupedListReplacement_WithHorizontalList")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyModelGroupedListReplacement_WithVerticalGrid")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyModelGroupedListReplacement_WithHorizontalGrid")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyModelGroupedListReplacement(string itemsLayout)
	{
		VerifyItemsSourceReplacement(itemsLayout, ItemsSourceGroupedList, "dotnet_bot.png", "Updated calculator.png", expectedInitialCount: 2, modelItems: true, grouped: true);
	}

	private void VerifyItemsSourceReplacement(
		string itemsLayout,
		string itemsSource,
		string oldItem,
		string newItem,
		int expectedInitialCount,
		bool modelItems = false,
		bool grouped = false)
	{
		ConfigureItemsSource(itemsSource, itemsLayout, modelItems, grouped);
		App.WaitForElement(oldItem);
		App.WaitForTextToBePresentInElement("ItemsSourceItemCount", expectedInitialCount.ToString());
		Assert.That(App.FindElement("ItemsSourceItemCount").GetText(), Is.EqualTo(expectedInitialCount.ToString()));
		App.Tap(ReplaceItemsSource);
		App.WaitForElement(newItem);
		App.WaitForNoElement(oldItem);
		App.WaitForTextToBePresentInElement("ItemsSourceItemCount", "4");
		Assert.That(App.FindElement("ItemsSourceItemCount").GetText(), Is.EqualTo("4"));
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyModelItemsListMutation_WithVerticalList")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyModelItemsListMutation_WithHorizontalList")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyModelItemsListMutation_WithVerticalGrid")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyModelItemsListMutation_WithHorizontalGrid")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyModelItemsListWhenMutatingAcrossItemsLayouts(string itemsLayout)
	{
		ConfigureItemsSource(ItemsSourceList, itemsLayout, modelItems: true);
		App.WaitForElement("calculator.png");
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		WaitForItemInItemsLayout("green.png", itemsLayout);
		App.Tap(RemoveItems);
		App.WaitForNoElement("green.png");
		WaitForItemInItemsLayout("calculator.png", itemsLayout);
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyStringItemsListMutation_WithVerticalList")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyStringItemsListMutation_WithHorizontalList")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyStringItemsListMutation_WithVerticalGrid")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyStringItemsListMutation_WithHorizontalGrid")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyStringItemsListWhenMutatingAcrossItemsLayouts(string itemsLayout)
	{
		ConfigureItemsSource(ItemsSourceList, itemsLayout);
		App.WaitForElement("Broccoli");
		App.Tap(AddItems);
		WaitForItemInItemsLayout("Kiwi", itemsLayout);
		App.Tap(RemoveItems);
		App.WaitForNoElement("Kiwi");
		WaitForItemInItemsLayout("Broccoli", itemsLayout);
	}

	// Empty and no-source scenarios
	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyModelItemsEmptyGroupedListWhenAddItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(EmptyGroupedListT);
		App.Tap(EmptyGroupedListT);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForNoElement("green.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyModelItemsEmptyGroupedListWhenRemoveItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(EmptyGroupedListT);
		App.Tap(EmptyGroupedListT);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("calculator.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyModelItemsEmptyObservableCollectionWhenAddItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(EmptyObservableCollectionT);
		App.Tap(EmptyObservableCollectionT);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForNoElement("green.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyModelItemsEmptyObservableCollectionWhenRemoveItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(EmptyObservableCollectionT);
		App.Tap(EmptyObservableCollectionT);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("calculator.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyStringItemsEmptyGroupedListWhenAddItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(EmptyGroupedListT);
		App.Tap(EmptyGroupedListT);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForNoElement("Kiwi");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyStringItemsEmptyGroupedListWhenRemoveItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(EmptyGroupedListT);
		App.Tap(EmptyGroupedListT);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("Broccoli");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyStringItemsEmptyObservableCollectionWhenAddItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(EmptyObservableCollectionT);
		App.Tap(EmptyObservableCollectionT);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForNoElement("Kiwi");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyStringItemsEmptyObservableCollectionWhenRemoveItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(EmptyObservableCollectionT);
		App.Tap(EmptyObservableCollectionT);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("Broccoli");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyModelItemsItemsSourceNoneWhenAddItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForNoElement("green.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyModelItemsItemsSourceNoneWhenRemoveItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("calculator.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyStringItemsItemsSourceNoneWhenAddItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForNoElement("Kiwi");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyStringItemsItemsSourceNoneWhenRemoveItemsDoesNotCrash()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("Broccoli");
	}

	// Grouped source mutation scenarios
	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyModelItemsGroupedListWhenAddIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Group A");
		App.WaitForElement("Group B");
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "1");
		App.Tap(AddItems);
		App.WaitForElement("groceries.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyModelItemsGroupedListWhenAddItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Group A");
		App.WaitForElement("Group B");
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForElement("green.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyModelItemsGroupedListWhenRemoveIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Group A");
		App.WaitForElement("Group B");
		App.WaitForElement("dotnet_bot.png");
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.Tap(RemoveItems);
		App.WaitForNoElement("dotnet_bot.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyModelItemsGroupedListWhenRemoveItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("dotnet_bot.png");
		App.WaitForElement("Group A");
		App.WaitForElement("Group B");
		App.WaitForElement("avatar.png");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("avatar.png");
		App.WaitForElement("dotnet_bot.png");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyStringItemsGroupedListWhenAddIndexAtFlattenedEnd()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "6");
		App.Tap(AddItems);
		App.WaitForElement("Strawberry");
		Assert.That(App.FindElementsByText("Strawberry").Count, Is.EqualTo(1));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyStringItemsGroupedListWhenAddIndexAtGroupBoundary()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "3");
		App.Tap(AddItems);
		App.WaitForElement("Raseberry");
		Assert.That(App.FindElementsByText("Raseberry").Count, Is.EqualTo(1));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyStringItemsGroupedListWhenAddIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Vegetables");
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "2");
		App.Tap(AddItems);
		App.WaitForElement("Chikoo");
		Assert.That(App.FindElementsByText("Chikoo").Count, Is.EqualTo(1));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyStringItemsGroupedListWhenIndexIsOutOfRange()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);

		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "99");
		App.Tap(AddItems);
		App.WaitForElement("Apple");
		App.WaitForNoElement("Raseberry");

		// The index is invalid, so removal must also leave the source unchanged.
		App.EnterText(IndexEntry, "99");
		App.Tap(RemoveItems);
		App.WaitForElement("Apple");
		App.WaitForElement("Spinach");
		App.WaitForElement("Vegetables");
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyStringItemsGroupedListWhenMutatingSecondGroupByIndex()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "3");
		App.Tap(AddItems);
		App.WaitForElement("Raseberry");
		Assert.That(App.FindElementsByText("Raseberry").Count, Is.EqualTo(1));
		App.EnterText(IndexEntry, "3");
		App.Tap(RemoveItems);
		App.WaitForNoElement("Raseberry");
		Assert.That(App.FindElementsByText("Raseberry").Count, Is.EqualTo(0));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyStringItemsGroupedListWhenRemoveIndexAtItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Vegetables");
		App.WaitForElement("Orange");
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "2");
		App.Tap(RemoveItems);
		App.WaitForNoElement("Orange");
	}

	// Grouped source layout scenarios
	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyStringItemsGroupedListWhenAddItemsWithVerticalListItemsLayout")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyStringItemsGroupedListWhenAddItemsWithHorizontalListItemsLayout")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyStringItemsGroupedListWhenAddItemsWithVerticalGridItemsLayout")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyStringItemsGroupedListWhenAddItemsWithHorizontalGridItemsLayout")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyStringItemsGroupedListWhenAddItemsWithItemsLayout(string itemsLayout)
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(itemsLayout);
		App.Tap(itemsLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Vegetables");
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		WaitForItemInItemsLayout("Kiwi", itemsLayout);
		VerifyScreenshot();
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyStringItemsGroupedListWhenRemoveItemsWithVerticalListItemsLayout")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyStringItemsGroupedListWhenRemoveItemsWithHorizontalListItemsLayout")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyStringItemsGroupedListWhenRemoveItemsWithVerticalGridItemsLayout")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyStringItemsGroupedListWhenRemoveItemsWithHorizontalGridItemsLayout")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyStringItemsGroupedListWhenRemoveItemsWithItemsLayout(string itemsLayout)
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(itemsLayout);
		App.Tap(itemsLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Vegetables");
		WaitForItemInItemsLayout("Orange", itemsLayout);
		App.WaitForElement("Spinach");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForTextToBePresentInElement("ItemsSourceItemCount", "5");
		App.WaitForNoElement("Spinach");
		VerifyScreenshot();
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyStringItemsObservableCollectionWhenMutatingWithVerticalListItemsLayout")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyStringItemsObservableCollectionWhenMutatingWithHorizontalListItemsLayout")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyStringItemsObservableCollectionWhenMutatingWithVerticalGridItemsLayout")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyStringItemsObservableCollectionWhenMutatingWithHorizontalGridItemsLayout")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyStringItemsObservableCollectionWhenMutatingWithItemsLayout(string itemsLayout)
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(itemsLayout);
		App.Tap(itemsLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Apple");
		App.Tap(AddItems);
		WaitForItemInItemsLayout("Kiwi", itemsLayout);
		App.Tap(RemoveItems);
		App.WaitForNoElement("Kiwi");
		VerifyScreenshot();
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyStringItemsGroupedListWhenMutatingByIndexWithVerticalListItemsLayout")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyStringItemsGroupedListWhenMutatingByIndexWithHorizontalListItemsLayout")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyStringItemsGroupedListWhenMutatingByIndexWithVerticalGridItemsLayout")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyStringItemsGroupedListWhenMutatingByIndexWithHorizontalGridItemsLayout")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyStringItemsGroupedListWhenMutatingByIndexWithItemsLayout(string itemsLayout)
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(itemsLayout);
		App.Tap(itemsLayout);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Apple");
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.Tap(AddItems);
		App.WaitForElement("Kiwi");
		App.EnterText(IndexEntry, "0");
		App.Tap(RemoveItems);
		App.WaitForNoElement("Kiwi");
		App.WaitForElement("Apple");
		VerifyScreenshot();
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyModelItemsGroupedListMutation_WithVerticalList")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyModelItemsGroupedListMutation_WithHorizontalList")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyModelItemsGroupedListMutation_WithVerticalGrid")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyModelItemsGroupedListMutation_WithHorizontalGrid")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyModelItemsGroupedListWhenMutatingAcrossItemsLayouts(string itemsLayout)
	{
		ConfigureItemsSource(ItemsSourceGroupedList, itemsLayout, modelItems: true, grouped: true);
		App.WaitForElement("dotnet_bot.png");
		App.Tap(AddItems);
		WaitForItemInItemsLayout("green.png", itemsLayout);
		App.Tap(RemoveItems);
		App.WaitForNoElement("avatar.png");
		WaitForItemInItemsLayout("green.png", itemsLayout);
	}

	[TestCase(ItemsLayoutVerticalList, TestName = "VerifyStringItemsGroupedListMutation_WithVerticalList")]
	[TestCase(ItemsLayoutHorizontalList, TestName = "VerifyStringItemsGroupedListMutation_WithHorizontalList")]
	[TestCase(ItemsLayoutVerticalGrid, TestName = "VerifyStringItemsGroupedListMutation_WithVerticalGrid")]
	[TestCase(ItemsLayoutHorizontalGrid, TestName = "VerifyStringItemsGroupedListMutation_WithHorizontalGrid")]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyStringItemsGroupedListWhenMutatingAcrossItemsLayouts(string itemsLayout)
	{
		ConfigureItemsSource(ItemsSourceGroupedList, itemsLayout, grouped: true);
		App.WaitForElement("Apple");
		App.Tap(AddItems);
		WaitForItemInItemsLayout("Kiwi", itemsLayout);
		App.Tap(RemoveItems);
		WaitForItemInItemsLayout("Kiwi", itemsLayout);
	}

	// Source configuration and selection scenarios
	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyFlatItemsSourceAllowsIndependentIsGroupedSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceList);
		App.Tap(ItemsSourceList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.WaitForNoElement("Banana");
	}

	// Selection and preselection scenarios
	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyListSelectionIsClearedWhenItemsSourceIsReplaced()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceList);
		App.Tap(ItemsSourceList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(SingleModePreselection);
		App.Tap(SingleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Apple"));
		App.WaitForElement(ReplaceItemsSource);
		App.Tap(ReplaceItemsSource);
		App.WaitForElement("Updated Item 1");
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("No current items"));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyModelItemsGroupedListWhenMultipleModePreSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Group A");
		App.WaitForElement("Group B");
		App.WaitForElement(MultipleModePreselection);
		App.Tap(MultipleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("dotnet_bot.png, avatar.png"));
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifyModelItemsGroupedListWhenSingleModePreSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Group A");
		App.WaitForElement("Group B");
		App.WaitForElement(SingleModePreselection);
		App.Tap(SingleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("dotnet_bot.png"));
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifyModelItemsObservableCollectionWhenMultipleModePreSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MultipleModePreselection);
		App.Tap(MultipleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("dotnet_bot.png, avatar.png"));
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("avatar.png"));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyModelItemsObservableCollectionWhenSingleModePreSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ModelItem);
		App.Tap(ModelItem);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(SingleModePreselection);
		App.Tap(SingleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("dotnet_bot.png"));
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("No current items"));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyMultipleSelectionIsClearedWhenItemsSourceIsReplaced()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MultipleModePreselection);
		App.Tap(MultipleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Apple, Carrot"));
		App.WaitForElement(ReplaceItemsSource);
		App.Tap(ReplaceItemsSource);
		App.WaitForElement("Updated Item 1");
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("No current items"));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 3)]
	public void VerifyMultipleSelectionWorksAfterItemsSourceReplacement()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ReplaceItemsSource);
		App.Tap(ReplaceItemsSource);
		App.WaitForElement("Updated Item 1");
		App.Tap(MultipleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Updated Item 1, Updated Item 2"));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 4)]
	public void VerifySelectionIsClearedWhenItemsSourceIsReplaced()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(SingleModePreselection);
		App.Tap(SingleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Apple"));
		App.WaitForElement(ReplaceItemsSource);
		App.Tap(ReplaceItemsSource);
		App.WaitForElement("Updated Item 1");
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("No current items"));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifySelectionIsClearedWhenItemsSourceOptionsAreReset()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(SingleModePreselection);
		App.Tap(SingleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Apple"));
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("No current items"));
		App.WaitForNoElement("Banana");
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("No current items"));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 2)]
	public void VerifySingleSelectionWorksAfterItemsSourceReplacement()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(ReplaceItemsSource);
		App.Tap(ReplaceItemsSource);
		App.WaitForElement("Updated Item 1");
		App.Tap(SingleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Updated Item 1"));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 1)]
	public void VerifyStringItemsGroupedListWhenMultipleModePreSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Vegetables");
		App.WaitForElement(MultipleModePreselection);
		App.Tap(MultipleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Apple, Carrot"));
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 5)]
	public void VerifyStringItemsGroupedListWhenSingleModePreSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Vegetables");
		App.WaitForElement(SingleModePreselection);
		App.Tap(SingleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Apple"));
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("No current items"));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 6)]
	public void VerifyStringItemsObservableCollectionWhenMultipleModePreSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(MultipleModePreselection);
		App.Tap(MultipleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Apple, Carrot"));
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Carrot"));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}

	[Test]
	[ShardedTestCategory(UITestCategories.CollectionView, shard: 7)]
	public void VerifyStringItemsObservableCollectionWhenSingleModePreSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection);
		App.Tap(ItemsSourceObservableCollection);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(SingleModePreselection);
		App.Tap(SingleModePreselection);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Apple"));
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("No current items"));
		VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));
	}
}
