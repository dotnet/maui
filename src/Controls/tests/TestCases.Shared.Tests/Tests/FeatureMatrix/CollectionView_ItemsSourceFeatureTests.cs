using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class CollectionView_ItemsSourceFeatureTests : UITest
{
	public const string ItemsSourceFeatureMatrix = "CollectionView Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string ModelItem = "ModelItem";
	public const string ItemsSourceObservableCollection = "ItemsSourceObservableCollection";
	public const string ItemsSourceList = "ItemsSourceList";
	public const string ItemsSourceGroupedList = "ItemsSourceGroupedList";
	public const string EmptyGroupedListT = "EmptyGroupedList";
	public const string EmptyObservableCollectionT = "EmptyObservableCollection";
	public const string IsGroupedTrue = "IsGroupedTrue";
	public const string AddItems = "AddItems";
	public const string RemoveItems = "RemoveItems";
	public const string IndexEntry = "IndexEntry";
	public const string CurrentSelectionTextLabel = "CurrentSelectionTextLabel";
	public const string MultipleModePreselection = "MultipleModePreselection";
	public const string SingleModePreselection = "SingleModePreselection";

	public CollectionView_ItemsSourceFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(ItemsSourceFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void VerifyStringItemsObservableCollectionWhenAddItems()
	{
		App.WaitForElement("ItemsSourceButton");
		App.Tap("ItemsSourceButton");
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
	[Category(UITestCategories.CollectionView)]
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
	[Category(UITestCategories.CollectionView)]
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
		App.WaitForNoElement("Kiwi");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
		App.WaitForElement("Broccoli");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyStringItemsEmptyGroupedListWhenAddItems()
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyStringItemsEmptyGroupedListWhenRemoveItems()
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyStringItemsEmptyObservableCollectionWhenAddItems()
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyStringItemsEmptyObservableCollectionWhenRemoveItems()
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyStringItemsItemsSourceNoneWhenAddItems()
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyStringItemsItemsSourceNoneWhenRemoveItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("Broccoli");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
		App.WaitForElement("green.png");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
	[Category(UITestCategories.CollectionView)]
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
		App.WaitForNoElement("green.png");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
		App.WaitForElement("calculator.png");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyModelItemsEmptyGroupedListWhenAddItems()
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyModelItemsEmptyGroupedListWhenRemoveItems()
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyModelItemsEmptyObservableCollectionWhenAddItems()
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyModelItemsEmptyObservableCollectionWhenRemoveItems()
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyModelItemsItemsSourceNoneWhenAddItems()
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyModelItemsItemsSourceNoneWhenRemoveItems()
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
	[Category(UITestCategories.CollectionView)]
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
		App.WaitForElement(IndexEntry);
		App.EnterText(IndexEntry, "0");
		App.Tap(AddItems);
		App.WaitForElement("Kiwi");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
	[Category(UITestCategories.CollectionView)]
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
	[Category(UITestCategories.CollectionView)]
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
	[Category(UITestCategories.CollectionView)]
	public void VerifyStringItemsGroupedListWhenAddItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddItems);
		App.Tap(AddItems);
		App.WaitForElement("Fruits");
		App.WaitForElement("Vegetables");
		App.WaitForElement("Kiwi");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyStringItemsGroupedListWhenRemoveItems()
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
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("Orange");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
	[Category(UITestCategories.CollectionView)]
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
		App.WaitForElement(RemoveItems);
		App.Tap(RemoveItems);
		App.WaitForNoElement("dotnet_bot.png");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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

	[Test]
	[Category(UITestCategories.CollectionView)]
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
	[Category(UITestCategories.CollectionView)]
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
	[Category(UITestCategories.CollectionView)]
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
	[Category(UITestCategories.CollectionView)]
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
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
		VerifyScreenshot();
	}
}