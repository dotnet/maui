using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;

public class CollectionView_SelectionFeatureTests : _GalleryUITest
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
	public const string FlowDirectionLTR = "FlowDirectionLeftToRight";
	public const string FlowDirectionRTL = "FlowDirectionRightToLeft";
	public const string HeaderString = "HeaderString";
	public const string HeaderGrid = "HeaderGrid";
	public const string FooterString = "FooterString";
	public const string FooterGrid = "FooterGrid";
	public const string HeaderTemplateGrid = "HeaderTemplateGrid";
	public const string FooterTemplateGrid = "FooterTemplateGrid";
	public override string GalleryPageName => SelectionFeatureMatrix;
	public CollectionView_SelectionFeatureTests(TestDevice device)
		: base(device)
	{
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
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
	}

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link:https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
	}
#endif

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In CV2 related issue link: https://github.com/dotnet/maui/issues/28509 and In windows, relates issue: https://github.com/dotnet/maui/issues/28824
	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Carrot"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("4"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Carrot, Apple, Spinach"));
	}
#endif
#endif

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Grapes"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("3"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Apple, Mango"));
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In CV2, related issue link: https://github.com/dotnet/maui/issues/28030 and In windows, relates issue:https://github.com/dotnet/maui/issues/27946                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           
	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Mango"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Apple"));
	}


	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("3"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Grapes, Banana"));
	}
#endif

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("4"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana, Apple, Grapes"));
	}
#endif
#endif

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Apple"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Apple, Orange"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
		VerifyScreenshot();
	}
#endif
#endif

	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Apple"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Apple, Orange"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In CV2 related issue link: https://github.com/dotnet/maui/issues/28509 and In windows, relates issue: https://github.com/dotnet/maui/issues/28824
	[Test]
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
	[Test]
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
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Apple, Orange, Carrot, Spinach"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("4"));
		VerifyScreenshot();
	}
#endif
#endif

	[Test]
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
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Banana"));
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Orange"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Banana"));
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("Banana, Orange"));
	}
#endif

	[Test]
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
		Assert.That(App.WaitForElement(CurrentSelectionTextLabel).GetText(), Is.EqualTo("No current items"));
	}

	[Test]
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
		Assert.That(App.WaitForElement(PreviousSelectionTextLabel).GetText(), Is.EqualTo("No previous items"));
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(PreviousSelectionTextLabel).GetText(), Is.EqualTo("Banana"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(PreviousSelectionTextLabel).GetText(), Is.EqualTo("No previous items"));
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(PreviousSelectionTextLabel).GetText(), Is.EqualTo("Banana"));
		App.WaitForElement("Apple");
		App.Tap("Apple");
		Assert.That(App.WaitForElement(PreviousSelectionTextLabel).GetText(), Is.EqualTo("Banana, Orange"));
	}
#endif

	[Test]
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
		Assert.That(App.WaitForElement(PreviousSelectionTextLabel).GetText(), Is.EqualTo("No previous items"));
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(PreviousSelectionTextLabel).GetText(), Is.EqualTo("No previous items"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028
	[Test]
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
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange"));
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleSelectionChangedEventCount()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.That(App.WaitForElement(SelectionChangedEventCountLabel).GetText(), Is.EqualTo("0 times"));
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectionChangedEventCountLabel).GetText(), Is.EqualTo("1 times"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange"));
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectionChangedEventCountLabel).GetText(), Is.EqualTo("2 times"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectionChangedEventCountLabel).GetText(), Is.EqualTo("3 times"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
	}
#endif

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleSelectionChangedEventCount()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.That(App.WaitForElement(SelectionChangedEventCountLabel).GetText(), Is.EqualTo("0 times"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectionChangedEventCountLabel).GetText(), Is.EqualTo("1 times"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange"));
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectionChangedEventCountLabel).GetText(), Is.EqualTo("2 times"));
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In CV2 related issue link: https://github.com/dotnet/maui/issues/28509 and In windows, relates issue: https://github.com/dotnet/maui/issues/28824

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionLTRWithVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionLTR);
		App.Tap(FlowDirectionLTR);
		App.WaitForElement(ItemsLayoutVerticalList);
		App.Tap(ItemsLayoutVerticalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ItemsLayoutVerticalList);
		App.Tap(ItemsLayoutVerticalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionLTRWithHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionLTR);
		App.Tap(FlowDirectionLTR);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionLTRWithVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionLTR);
		App.Tap(FlowDirectionLTR);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionLTRWithHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionLTR);
		App.Tap(FlowDirectionLTR);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderStringAndFooterStringAndVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(ItemsLayoutVerticalList);
		App.Tap(ItemsLayoutVerticalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderViewAndFooterViewAndVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(ItemsLayoutVerticalList);
		App.Tap(ItemsLayoutVerticalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderStringAndFooterStringAndHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderViewAndFooterViewAndHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderStringAndFooterStringAndVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderViewAndFooterViewAndVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderStringAndFooterStringAndHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderViewAndFooterViewAndHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderTemplateAndFooterTemplateAndVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(ItemsLayoutVerticalList);
		App.Tap(ItemsLayoutVerticalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionLTRWithHeaderTemplateAndFooterTemplateAndVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionLTR);
		App.Tap(FlowDirectionLTR);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(ItemsLayoutVerticalList);
		App.Tap(ItemsLayoutVerticalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderTemplateAndFooterTemplateAndHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionLTRWithHeaderTemplateAndFooterTemplateAndHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionLTR);
		App.Tap(FlowDirectionLTR);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderTemplateAndFooterTemplateAndVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionLTRWithHeaderTemplateAndFooterTemplateAndVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionLTR);
		App.Tap(FlowDirectionLTR);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionRTLWithHeaderTemplateAndFooterTemplateAndHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionRTL);
		App.Tap(FlowDirectionRTL);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyFlowDirectionLTRWithHeaderTemplateAndFooterTemplateAndHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(FlowDirectionLTR);
		App.Tap(FlowDirectionLTR);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#endif

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWithHeaderString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWithHeaderString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWithHeaderString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWithHeaderView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
	}
#endif

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWithHeaderView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWithHeaderView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWithFooterString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWithFooterString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWithFooterString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWithFooterView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
	}
#endif

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWithFooterView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWithFooterView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWithHeaderStringAndFooterString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWithHeaderStringAndFooterString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWithHeaderStringAndFooterString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(HeaderString);
		App.Tap(HeaderString);
		App.WaitForElement(FooterString);
		App.Tap(FooterString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWithHeaderViewAndFooterView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
	}
#endif

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWithHeaderViewAndFooterView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWithHeaderViewAndFooterView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWithHeaderTemplateView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}


	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWithFooterTemplateView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}

#if TEST_FAILS_ON_ANDROID // Issue Link - https://github.com/dotnet/maui/issues/32212

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWithHeaderTemplateView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWithFooterTemplateView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_CATALYST //related issue link: https://github.com/dotnet/maui/issues/18028

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWithHeaderTemplateView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWithFooterTemplateView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeMultipleWithHeaderTemplateViewAndFooterTemplateView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeMultiple);
		App.Tap(SelectionModeMultiple);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Orange, Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("2"));
	}
#endif

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeSingleWithHeaderTemplateViewAndFooterTemplateView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeSingle);
		App.Tap(SelectionModeSingle);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		App.WaitForElement("Banana");
		App.Tap("Banana");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("Banana"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("1"));
	}
#endif

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifySelectionModeNoneWithHeaderTemplateViewAndFooterTemplateView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SelectionModeNone);
		App.Tap(SelectionModeNone);
		App.WaitForElement(HeaderGrid);
		App.Tap(HeaderGrid);
		App.WaitForElement(FooterGrid);
		App.Tap(FooterGrid);
		App.WaitForElement(HeaderTemplateGrid);
		App.Tap(HeaderTemplateGrid);
		App.WaitForElement(FooterTemplateGrid);
		App.Tap(FooterTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Orange");
		App.Tap("Orange");
		Assert.That(App.WaitForElement(SelectedSingle).GetText(), Is.EqualTo("No items selected"));
		Assert.That(App.WaitForElement(SelectedMultiple).GetText(), Is.EqualTo("0"));
	}
}