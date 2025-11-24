using NUnit.Framework;
using UITest.Appium;
using UITest.Core;


namespace Microsoft.Maui.TestCases.Tests;
public class CollectionView_ScrollingFeatureTests : _GalleryUITest
{
	public const string ScrollingFeatureMatrix = "CollectionView Feature Matrix";
	public const string Options = "Options";
	public const string Apply = "Apply";
	public const string IsGroupedTrue = "IsGroupedTrue";
	public const string ItemsSourceObservableCollection3 = "ItemsSourceObservableCollection3";
	public const string ItemsSourceObservableCollection2 = "ItemsSourceObservableCollection2";
	public const string ItemsSourceGroupedList2 = "ItemsSourceGroupedList2";
	public const string ItemsSourceGroupedList3 = "ItemsSourceGroupedList3";
	public const string ItemSizingMeasureAllItems = "ItemSizingMeasureAllItems";
	public const string ItemSizingMeasureFirstItem = "ItemSizingMeasureFirstItem";
	public const string ItemsUpdatingKeepItemsInView = "ItemsUpdatingKeepItemsInView";
	public const string ItemsUpdatingKeepLastItemInView = "ItemsUpdatingKeepLastItemInView";
	public const string ItemsUpdatingKeepScrollOffset = "ItemsUpdatingKeepScrollOffset";
	public const string ItemsLayoutVerticalGrid = "ItemsLayoutVerticalGrid";
	public const string ItemsLayoutHorizontalGrid = "ItemsLayoutHorizontalGrid";
	public const string ItemsLayoutVerticalList = "ItemsLayoutVerticalList";
	public const string ItemsLayoutHorizontalList = "ItemsLayoutHorizontalList";
	public const string AddButton = "AddButton";

	public override string GalleryPageName => ScrollingFeatureMatrix;

	public CollectionView_ScrollingFeatureTests(TestDevice device)
		: base(device)
	{
	}

	[Test, Order(1)]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureAllItemsWithObservableCollection()
	{
		App.WaitForElement("ScrollingButton");
		App.Tap("ScrollingButton");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection2);
		App.Tap(ItemsSourceObservableCollection2);
		App.WaitForElement(ItemSizingMeasureAllItems);
		App.Tap(ItemSizingMeasureAllItems);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_WINDOWS // [Windows] NullReferenceException thrown When Toggling IsGrouped to True in ObservableCollection Binding Issue Link: https://github.com/dotnet/maui/issues/28824
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureAllItemsWithGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemSizingMeasureAllItems);
		App.Tap(ItemSizingMeasureAllItems);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList2);
		App.Tap(ItemsSourceGroupedList2);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif

#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
	//CollectionView2 ItemSizingStrategy="MeasureFirstItem" Fails to Apply Correct Sizing Issue Link: https://github.com/dotnet/maui/issues/29130
	//CollectionView ItemsLayout does not update while switch from LinearItemsLayout to GridItemsLayout Issue Link: https://github.com/dotnet/maui/issues/27946
	//CollectionView CollectionView2 doesnot change ItemsLayout Issue Link: https://github.com/dotnet/maui/issues/28656

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureFirstItemsWithObservableCollection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection2);
		App.Tap(ItemsSourceObservableCollection2);
		App.WaitForElement(ItemSizingMeasureFirstItem);
		App.Tap(ItemSizingMeasureFirstItem);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureFirstItemsWithObservableCollectionWhenVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection2);
		App.Tap(ItemsSourceObservableCollection2);
		App.WaitForElement(ItemSizingMeasureFirstItem);
		App.Tap(ItemSizingMeasureFirstItem);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureFirstItemsWithGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemSizingMeasureFirstItem);
		App.Tap(ItemSizingMeasureFirstItem);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList2);
		App.Tap(ItemsSourceGroupedList2);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureAllItemsWithObservableCollectionWhenHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection2);
		App.Tap(ItemsSourceObservableCollection2);
		App.WaitForElement(ItemSizingMeasureAllItems);
		App.Tap(ItemSizingMeasureAllItems);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureAllItemsWithObservableCollectionWhenHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection2);
		App.Tap(ItemsSourceObservableCollection2);
		App.WaitForElement(ItemSizingMeasureAllItems);
		App.Tap(ItemSizingMeasureAllItems);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureAllItemsWithObservableCollectionWhenVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection2);
		App.Tap(ItemsSourceObservableCollection2);
		App.WaitForElement(ItemSizingMeasureAllItems);
		App.Tap(ItemSizingMeasureAllItems);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureAllItemsWithGroupedListWhenVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemSizingMeasureAllItems);
		App.Tap(ItemSizingMeasureAllItems);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList2);
		App.Tap(ItemsSourceGroupedList2);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureAllItemsWithGroupedListWhenHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemSizingMeasureAllItems);
		App.Tap(ItemSizingMeasureAllItems);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList2);
		App.Tap(ItemsSourceGroupedList2);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureAllItemsWithGroupedListWhenHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemSizingMeasureAllItems);
		App.Tap(ItemSizingMeasureAllItems);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList2);
		App.Tap(ItemsSourceGroupedList2);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_ANDROID
//[Android] CollectionView with ItemSizingStrategy="MeasureFirstItem" Does Not Work as Expected for HorizontalList and HorizontalGrid Layouts Issue Link: https://github.com/dotnet/maui/issues/29192
//[Android] ItemSizingStrategy="MeasureFirstItem" does not work correctly with VerticalGrid and grouped ItemsSource Issue Link: https://github.com/dotnet/maui/issues/29191
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureFirstItemsWithObservableCollectionWhenHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection2);
		App.Tap(ItemsSourceObservableCollection2);
		App.WaitForElement(ItemSizingMeasureFirstItem);
		App.Tap(ItemSizingMeasureFirstItem);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureFirstItemsWithObservableCollectionWhenHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceObservableCollection2);
		App.Tap(ItemsSourceObservableCollection2);
		App.WaitForElement(ItemSizingMeasureFirstItem);
		App.Tap(ItemSizingMeasureFirstItem);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureFirstItemsWithGroupedListWhenVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemSizingMeasureFirstItem);
		App.Tap(ItemSizingMeasureFirstItem);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList2);
		App.Tap(ItemsSourceGroupedList2);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureFirstItemsWithGroupedListWhenHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemSizingMeasureFirstItem);
		App.Tap(ItemSizingMeasureFirstItem);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList2);
		App.Tap(ItemsSourceGroupedList2);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyMeasureFirstItemsWithGroupedListWhenHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemSizingMeasureFirstItem);
		App.Tap(ItemSizingMeasureFirstItem);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsSourceGroupedList2);
		App.Tap(ItemsSourceGroupedList2);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		VerifyScreenshot();
	}
#endif
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST
	//[Android] KeepItemsInView and KeepScrollOffset doesn't not works as expected when new items are added in CollectionView Issue Link: https://github.com/dotnet/maui/issues/29131
	//[iOS] KeepItemsInView Does Not Show Newly Added Items After Scrolling Down in CollectionView Issue Link: https://github.com/dotnet/maui/issues/29145
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepItemsInViewWithObservableList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepItemsInView);
		App.Tap(ItemsUpdatingKeepItemsInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
	}


	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepItemsInViewWithGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepItemsInView);
		App.Tap(ItemsUpdatingKeepItemsInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
	}

#if TEST_FAILS_ON_WINDOWS // //CollectionView ItemsLayout does not update Issue Link: https://github.com/dotnet/maui/issues/27946
    [Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepItemsInViewWithObservableListWhenVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepItemsInView);
		App.Tap(ItemsUpdatingKeepItemsInView);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepItemsInViewWithObservableListWhenHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepItemsInView);
		App.Tap(ItemsUpdatingKeepItemsInView);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepItemsInViewWithObservableListWhenHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepItemsInView);
		App.Tap(ItemsUpdatingKeepItemsInView);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepItemsInViewWithGroupedListWhenVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepItemsInView);
		App.Tap(ItemsUpdatingKeepItemsInView);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepItemsInViewWithGroupedListWhenHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepItemsInView);
		App.Tap(ItemsUpdatingKeepItemsInView);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepItemsInViewWithGroupedListWhenHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepItemsInView);
		App.Tap(ItemsUpdatingKeepItemsInView);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
	}
#endif
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
	//Scroll mode "KeepLastItemInView" does not keep the last item at the end of the displayed list when adding new items Issue Link: https://github.com/dotnet/maui/issues/28716
	//KeepLastItemInView Does Not Scroll to Last Item When Adding Items at Top, Instead Scrolls to SecondLast Item : https://github.com/dotnet/maui/issues/29207

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepLastItemInViewWithObservableList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepLastItemInView);
		App.Tap(ItemsUpdatingKeepLastItemInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Cabbage");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepLastItemInViewWithObservableListWhenVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepLastItemInView);
		App.Tap(ItemsUpdatingKeepLastItemInView);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Cabbage");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepLastItemInViewWithObservableListWhenHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepLastItemInView);
		App.Tap(ItemsUpdatingKeepLastItemInView);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Cabbage");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepLastItemInViewWithObservableListWhenHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepLastItemInView);
		App.Tap(ItemsUpdatingKeepLastItemInView);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Cabbage");
	}

#if TEST_FAILS_ON_ANDROID
//[Android] ArgumentOutOfRangeException Occurs with KeepLastItemInView for Grouped List Issue Link: https://github.com/dotnet/maui/issues/29153
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepLastItemInViewWithGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepLastItemInView);
		App.Tap(ItemsUpdatingKeepLastItemInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Pumpkin");
	}
 
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepLastItemInViewWithGroupedListWhenVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepLastItemInView);
		App.Tap(ItemsUpdatingKeepLastItemInView);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Pumpkin");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepLastItemInViewWithGroupedListWhenHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);  
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepLastItemInView);
		App.Tap(ItemsUpdatingKeepLastItemInView);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Pumpkin");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepLastItemInViewWithGroupedListWhenHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepLastItemInView);
		App.Tap(ItemsUpdatingKeepLastItemInView);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Pumpkin");
	}
#endif
#endif

#if TEST_FAILS_ON_ANDROID
	//[Android] KeepScrollOffset doesn't not works as expected when new items are added in CollectionView Issue Link:  https://github.com/dotnet/maui/issues/29131
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepScrollOffsetWithObservableList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepScrollOffset);
		App.Tap(ItemsUpdatingKeepScrollOffset);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Dragonfruit");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Passionfruit");
		App.WaitForElement("Cabbage");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepScrollOffsetWithGroupedList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepScrollOffset);
		App.Tap(ItemsUpdatingKeepScrollOffset);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("WaterMelon");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Mango");
		App.WaitForElement("Pumpkin");
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS
//CollectionView Fails to Preserve Scroll Offset with GridItemsLayout Using KeepScrollOffset Issue Link: https://github.com/dotnet/maui/issues/29202
//CollectionView ItemsLayout does not update while switch from LinearItemsLayout to GridItemsLayout Issue Link: https://github.com/dotnet/maui/issues/27946
//CollectionView CollectionView2 doesnot change ItemsLayout Issue Link: https://github.com/dotnet/maui/issues/28656

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepScrollOffsetWithObservableListWhenVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepScrollOffset);
		App.Tap(ItemsUpdatingKeepScrollOffset);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Dragonfruit");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Passionfruit");
		App.WaitForElement("Cabbage");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepScrollOffsetWithObservableListWhenHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepScrollOffset);
		App.Tap(ItemsUpdatingKeepScrollOffset);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Dragonfruit");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Passionfruit");
		App.WaitForElement("Cabbage");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepScrollOffsetWithObservableListWhenHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsUpdatingKeepScrollOffset);
		App.Tap(ItemsUpdatingKeepScrollOffset);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Dragonfruit");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Passionfruit");
		App.WaitForElement("Cabbage");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepScrollOffsetWithGroupedListWhenVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepScrollOffset);
		App.Tap(ItemsUpdatingKeepScrollOffset);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("WaterMelon");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Mango");
		App.WaitForElement("Pumpkin");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepScrollOffsetWithGroupedListWhenHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepScrollOffset);
		App.Tap(ItemsUpdatingKeepScrollOffset);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("WaterMelon");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Mango");
		App.WaitForElement("Pumpkin");
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyKeepScrollOffsetWithGroupedListWhenHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement(ItemsUpdatingKeepScrollOffset);
		App.Tap(ItemsUpdatingKeepScrollOffset);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("WaterMelon");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Mango");
		App.WaitForElement("Pumpkin");
	}
#endif
#endif

	//Scrolled event tests
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrolledEventWithVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("CollectionViewControl");
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Scrolled Event Fired"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrolledEventWithVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("CollectionViewControl");
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Scrolled Event Fired"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrolledEventWithHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("CollectionViewControl");
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Scrolled Event Fired"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrolledEventWithHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("CollectionViewControl");
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Scrolled Event Fired"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyDefaultScrollToRequested()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithMakeVisiblePositionAndVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "17");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Spinach");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithMakeVisiblePositionAndVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "27");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Pumpkin");
		App.WaitForElement("Radish");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithMakeVisiblePositionAndHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "17");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Spinach");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithMakeVisiblePositionAndHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "17");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Spinach");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithStartPositionAndVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "4");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Mango");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithStartPositionAndVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "4");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Mango");
		App.WaitForElement("Pineapple");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithStartPositionAndHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "17");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Spinach");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithStartPositionAndHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "17");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Spinach");
		App.WaitForElement("Broccoli");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithCenterPositionAndVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Kiwi");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithCenterPositionAndVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Kiwi");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithCenterPositionAndHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Kiwi");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithCenterPositionAndHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Kiwi");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithEndPositionAndVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Kiwi");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithEndPositionAndVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "26");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Pumpkin");
		App.WaitForElement("Radish");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithEndPositionAndHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Kiwi");
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToWithEndPositionAndHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo(string.Empty));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("ScrollToRequested Event Fired"));
		App.WaitForElement("Kiwi");
		App.WaitForElement("Pear");
		VerifyScreenshot();
	}

	//RemainingItemsThresholdReached Event Tests
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyRemainingItemsThresholdReachedWithVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo(string.Empty));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("RemainingItemsThresholdLabel");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("RemainingItemsThresholdReached Event Fired"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyRemainingItemsThresholdReachedWithVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo(string.Empty));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("RemainingItemsThresholdLabel");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("RemainingItemsThresholdReached Event Fired"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyRemainingItemsThresholdReachedWithHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo(string.Empty));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("RemainingItemsThresholdLabel");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("RemainingItemsThresholdReached Event Fired"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyRemainingItemsThresholdReachedWithHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo(string.Empty));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("RemainingItemsThresholdLabel");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("RemainingItemsThresholdReached Event Fired"));
	}

	//Reorder event Tests
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyReorderCompletedWithVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("CanReorderItemsTrue");
		App.Tap("CanReorderItemsTrue");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo(string.Empty));
		App.DragAndDrop("Banana", "Mango");
		App.WaitForElement("ReorderCompletedLabel");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("ReorderCompleted Event Fired"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyReorderCompletedWithVerticalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("CanReorderItemsTrue");
		App.Tap("CanReorderItemsTrue");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo(string.Empty));
		App.DragAndDrop("Banana", "Mango");
		App.WaitForElement("ReorderCompletedLabel");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("ReorderCompleted Event Fired"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyReorderCompletedWithHorizontalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("CanReorderItemsTrue");
		App.Tap("CanReorderItemsTrue");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo(string.Empty));
		App.DragAndDrop("Banana", "Mango");
		App.WaitForElement("ReorderCompletedLabel");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("ReorderCompleted Event Fired"));
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyReorderCompletedWithHorizontalGrid()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("CanReorderItemsTrue");
		App.Tap("CanReorderItemsTrue");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo(string.Empty));
		App.DragAndDrop("Banana", "Mango");
		App.WaitForElement("ReorderCompletedLabel");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("ReorderCompleted Event Fired"));
	}
}