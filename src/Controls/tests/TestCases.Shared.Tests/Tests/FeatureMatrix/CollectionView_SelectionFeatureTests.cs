using Xunit;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class CollectionView_SelectionFeatureTests : UITest
{
	public const string SelectionFeatureMatrix = "CollectionView Feature Matrix";
	public const string ItemsSourceGroupedList = "ItemsSourceGroupedList";
	public const string ItemsSourceObservableCollection25 = "ItemsSourceObservableCollection25";
	public const string ItemsSourceNone = "ItemsSourceNone";
	public const string IsGroupedTrue = "IsGroupedTrue";
	public const string IsGroupedFalse = "IsGroupedFalse";
	public const string SelectionModeNone = "SelectionModeNone";
	public const string SelectionModeSingle = "SelectionModeSingle";
	public const string SelectionModeMultiple = "SelectionModeMultiple";
	public const string Apply = "Apply";
	public const string Options = "Options";
	public const string SelectedSingle = "SelectedSingle";
	public const string SelectedMultiple = "SelectedMultiple";
	public const string ItemsLayoutVerticalList = "ItemsLayoutVerticalList";
	public const string ItemsLayoutHorizontalList = "ItemsLayoutHorizontalList";
	public const string ItemsLayoutVerticalGrid = "ItemsLayoutVerticalGrid";
	public const string ItemsLayoutHorizontalGrid = "ItemsLayoutHorizontalGrid";
	public const string CurrentSelectionTextLabel = "CurrentSelectionTextLabel";
	public const string PreviousSelectionTextLabel = "PreviousSelectionTextLabel";
	public const string SelectionChangedEventCountLabel = "SelectionChangedEventCountLabel";
	public CollectionView_SelectionFeatureTests(TestDevice device)
		: base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(SelectionFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWhenItemsSourceNone()
	{
		App.WaitForElement("SelectionPageButton");
		App.Tap("SelectionPageButton");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(ItemsSourceNone);
		App.Tap(ItemsSourceNone);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("0", App.WaitForElement(SelectedMultiple).GetText());
		Assert.Equal("No items selected", App.WaitForElement(SelectedSingle).GetText());
	}

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWhenItemsSourceNone()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(ItemsSourceNone);
		App.Tap(ItemsSourceNone);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("No items selected", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("0", App.WaitForElement(SelectedMultiple).GetText());
	}

#if TEST_FAILS_ON_CATALYST //related issue link:https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenItemsSourceNone()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(ItemsSourceNone);
		App.Tap(ItemsSourceNone);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("0", App.WaitForElement(SelectedMultiple).GetText());
		Assert.Equal("No items selected", App.WaitForElement(SelectedSingle).GetText());
	}
#endif

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWhenItemsSourceObservableCollection5()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("No items selected", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("0", App.WaitForElement(SelectedMultiple).GetText());
	}

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWhenItemsSourceObservableCollection5()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("Banana", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("1", App.WaitForElement(SelectedMultiple).GetText());
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenItemSourceObservableCollection5()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("Orange, Banana", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("2", App.WaitForElement(SelectedMultiple).GetText());
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In CV2 related issue link: https://github.com/dotnet/maui/issues/28509 and In windows, relates issue: https://github.com/dotnet/maui/issues/28824
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWhenItemsSourceGroupList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Vegetables");
		Assert.Equal("No items selected", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("0", App.WaitForElement(SelectedMultiple).GetText());
	}

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWhenItemsSourceGroupList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Vegetables");
		App.WaitForElement("Carrot");
		App.Tap("Carrot");
		Assert.Equal("Carrot", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("1", App.WaitForElement(SelectedMultiple).GetText());
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenItemsSourceGroupList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Fruits");
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Carrot");
		App.Tap("Carrot");
		App.WaitForElement("Apple");
		App.Tap("Apple");
		App.WaitForElement("Vegetables");
		App.WaitForElement("Spinach");
		App.Tap("Spinach");
		Assert.Equal("4", App.WaitForElement(SelectedMultiple).GetText());
		Assert.Equal("Orange, Carrot, Apple, Spinach", App.WaitForElement(SelectedSingle).GetText());
	}
#endif
#endif

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWhenItemsLayoutVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(ItemsLayoutVerticalList);
		App.Tap(ItemsLayoutVerticalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("No items selected", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("0", App.WaitForElement(SelectedMultiple).GetText());
	}

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWhenItemsLayoutVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(ItemsLayoutVerticalList);
		App.Tap(ItemsLayoutVerticalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Grapes");
		App.Tap("Grapes");
		Assert.Equal("Grapes", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("1", App.WaitForElement(SelectedMultiple).GetText());
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenItemsLayoutVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(ItemsLayoutVerticalList);
		App.Tap(ItemsLayoutVerticalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Apple");
		App.Tap("Apple");
		App.WaitForElement("Mango");
		App.Tap("Mango");
		Assert.Equal("3", App.WaitForElement(SelectedMultiple).GetText());
		Assert.Equal("Orange, Apple, Mango", App.WaitForElement(SelectedSingle).GetText());
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In CV2, related issue link: https://github.com/dotnet/maui/issues/28030 and In windows, relates issue:https://github.com/dotnet/maui/issues/27946                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWhenItemsLayoutHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("No items selected", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("0", App.WaitForElement(SelectedMultiple).GetText());
	}

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWhenItemsLayoutHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Mango");
		App.Tap("Mango");
		Assert.Equal("Mango", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("1", App.WaitForElement(SelectedMultiple).GetText());
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenItemsLayoutHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Apple");
		App.Tap("Apple");
		Assert.Equal("2", App.WaitForElement(SelectedMultiple).GetText());
		Assert.Equal("Orange, Apple", App.WaitForElement(SelectedSingle).GetText());
	}


	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWhenItemsLayoutVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("No items selected", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("0", App.WaitForElement(SelectedMultiple).GetText());
	}

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWhenItemsLayoutVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("Banana", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("1", App.WaitForElement(SelectedMultiple).GetText());
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenItemsLayoutVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Grapes");
		App.Tap("Grapes");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("3", App.WaitForElement(SelectedMultiple).GetText());
		Assert.Equal("Orange, Grapes, Banana", App.WaitForElement(SelectedSingle).GetText());
	}
#endif

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWhenItemsLayoutHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("No items selected", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("0", App.WaitForElement(SelectedMultiple).GetText());
	}

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWhenItemsLayoutHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("Banana", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("1", App.WaitForElement(SelectedMultiple).GetText());
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenItemsLayoutHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		App.WaitForElement("Apple");
		App.Tap("Apple");
		App.WaitForElement("Grapes");
		App.Tap("Grapes");
		Assert.Equal("4", App.WaitForElement(SelectedMultiple).GetText());
		Assert.Equal("Orange, Banana, Apple, Grapes", App.WaitForElement(SelectedSingle).GetText());
	}
#endif
#endif

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWhenProgrammaticSelectionWorksWithHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement("SingleModePreselection");
		App.Tap("SingleModePreselection");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("Apple", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("1", App.WaitForElement(SelectedMultiple).GetText());
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenProgrammaticSelectionWorksWithHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement("MultipleModePreselection");
		App.Tap("MultipleModePreselection");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("Apple, Orange", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("2", App.WaitForElement(SelectedMultiple).GetText());
		VerifyScreenshot();
	}
#endif
#endif

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWhenProgrammaticSelectionWorksWithVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement("SingleModePreselection");
		App.Tap("SingleModePreselection");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("Apple", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("1", App.WaitForElement(SelectedMultiple).GetText());
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenProgrammaticSelectionWorksWithVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement("MultipleModePreselection");
		App.Tap("MultipleModePreselection");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("Apple, Orange", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("2", App.WaitForElement(SelectedMultiple).GetText());
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In CV2 related issue link: https://github.com/dotnet/maui/issues/28509 and In windows, relates issue: https://github.com/dotnet/maui/issues/28824
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWhenProgrammaticSelectionWhithItemsSourceGroupList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement("SingleModePreselection");
		App.Tap("SingleModePreselection");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenProgrammaticSelectionWhithItemsSourceGroupList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList);
		App.Tap(ItemsSourceGroupedList);
		App.WaitForElement("MultipleModePreselection");
		App.Tap("MultipleModePreselection");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("Apple, Orange, Carrot, Spinach", App.WaitForElement(SelectedSingle).GetText());
		Assert.Equal("4", App.WaitForElement(SelectedMultiple).GetText());
		VerifyScreenshot();
	}
#endif
#endif

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelecctionModeSingleWhenCurrentSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("Banana", App.WaitForElement(CurrentSelectionTextLabel).GetText());
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.Equal("Orange", App.WaitForElement(CurrentSelectionTextLabel).GetText());
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenCurrentSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("Banana", App.WaitForElement(CurrentSelectionTextLabel).GetText());
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.Equal("Banana, Orange", App.WaitForElement(CurrentSelectionTextLabel).GetText());
	}
#endif

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWhenCurrentSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("No current items", App.WaitForElement(CurrentSelectionTextLabel).GetText());
	}

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWhenPreviousSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("No previous items", App.WaitForElement(PreviousSelectionTextLabel).GetText());
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.Equal("Banana", App.WaitForElement(PreviousSelectionTextLabel).GetText());
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWhenPreviousSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("No previous items", App.WaitForElement(PreviousSelectionTextLabel).GetText());
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.Equal("Banana", App.WaitForElement(PreviousSelectionTextLabel).GetText());
		App.WaitForElement("Apple");
		App.Tap("Apple");
		Assert.Equal("Banana, Orange", App.WaitForElement(PreviousSelectionTextLabel).GetText());
	}
#endif

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWhenPreviousSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("No previous items", App.WaitForElement(PreviousSelectionTextLabel).GetText());
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.Equal("No previous items", App.WaitForElement(PreviousSelectionTextLabel).GetText());
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWithToggleSelection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.Equal("1", App.WaitForElement(SelectedMultiple).GetText());
		Assert.Equal("Orange", App.WaitForElement(SelectedSingle).GetText());
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("2", App.WaitForElement(SelectedMultiple).GetText());
		Assert.Equal("Orange, Banana", App.WaitForElement(SelectedSingle).GetText());
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.Equal("1", App.WaitForElement(SelectedMultiple).GetText());
		Assert.Equal("Banana", App.WaitForElement(SelectedSingle).GetText());
	}

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleSelectionChangedEventCount()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("0 times", App.WaitForElement(SelectionChangedEventCountLabel).GetText());
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.Equal("1 times", App.WaitForElement(SelectionChangedEventCountLabel).GetText());
		Assert.Equal("Orange", App.WaitForElement(SelectedSingle).GetText());
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("2 times", App.WaitForElement(SelectionChangedEventCountLabel).GetText());
		Assert.Equal("Orange, Banana", App.WaitForElement(SelectedSingle).GetText());
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.Equal("3 times", App.WaitForElement(SelectionChangedEventCountLabel).GetText());
		Assert.Equal("Banana", App.WaitForElement(SelectedSingle).GetText());
	}
#endif

	[Fact]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleSelectionChangedEventCount()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.Equal("0 times", App.WaitForElement(SelectionChangedEventCountLabel).GetText());
		Assert.Equal("No items selected", App.WaitForElement(SelectedSingle).GetText());
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.Equal("1 times", App.WaitForElement(SelectionChangedEventCountLabel).GetText());
		Assert.Equal("Orange", App.WaitForElement(SelectedSingle).GetText());
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.Equal("2 times", App.WaitForElement(SelectionChangedEventCountLabel).GetText());
		Assert.Equal("Banana", App.WaitForElement(SelectedSingle).GetText());
	}
}