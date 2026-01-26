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
	public const string ScrollToIndexEntry = "ScrollToIndexEntry";
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Dragonfruit");
		App.Tap(AddButton);
		App.WaitForElement("Passionfruit");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Dragonfruit");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Passionfruit");
		App.WaitForElement("Cabbage");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("WaterMelon");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Mango");
		App.WaitForElement("Pumpkin");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Dragonfruit");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Passionfruit");
		App.WaitForElement("Cabbage");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Dragonfruit");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Passionfruit");
		App.WaitForElement("Cabbage");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("WaterMelon");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Mango");
		App.WaitForElement("Pumpkin");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("WaterMelon");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForNoElement("Mango");
		App.WaitForElement("Pumpkin");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
	}
#endif
#endif

	//Scrolled Event Tests
#if TEST_FAILS_ON_ANDROID // Issue Link: https://github.com/dotnet/maui/issues/33333

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrolledEventWithVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS
	//Issue Link: https://github.com/dotnet/maui/issues/33333
	//CollectionView Fails to Preserve Scroll Offset with GridItemsLayout Using KeepScrollOffset Issue Link: https://github.com/dotnet/maui/issues/29202
	//CollectionView ItemsLayout does not update while switch from LinearItemsLayout to GridItemsLayout Issue Link: https://github.com/dotnet/maui/issues/27946
	//CollectionView CollectionView2 doesnot change ItemsLayout Issue Link: https://github.com/dotnet/maui/issues/28656

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
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
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
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		Assert.That(App.WaitForElement("ScrolledEventLabel").GetText(), Is.EqualTo("Fired"));
	}
#endif

	//ScrollToRequested Event Tests
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyDefaultScrollToRequested()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		VerifyScreenshot();
	}

	// ScrollTo By Index Tests
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithMakeVisiblePositionAndVerticalList_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "15");
		App.WaitForElement("ScrollToPositionMakeVisible");
		App.Tap("ScrollToPositionMakeVisible");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("15"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("15"));
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/33614

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithStartPositionAndVerticalList_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "15");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("15"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("15"));
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithCenterPositionAndVerticalList_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "15");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("15"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("15"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithEndPositionAndVerticalList_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "15");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("15"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("15"));
		VerifyScreenshot();
	}

	// ScrollTo By Item Tests
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithMakeVisiblePositionAndVerticalList_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Carrot");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("15"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Carrot"));
		VerifyScreenshot();
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // Issue Link: https://github.com/dotnet/maui/issues/33614

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithStartPositionAndVerticalList_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Carrot");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("15"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Carrot"));
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithCenterPositionAndVerticalList_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Carrot");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("15"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Carrot"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithEndPositionAndVerticalList_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Carrot");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("15"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Carrot"));
		VerifyScreenshot();
	}

	// Grouped ScrollTo By Index Tests
#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS // Issue - https://github.com/dotnet/maui/issues/17664

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithMakeVisiblePositionAndVerticalList_Apricot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("FruitGroup");
		App.Tap("FruitGroup");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "23");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "0");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionMakeVisible");
		App.Tap("ScrollToPositionMakeVisible");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Apricot");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("24"));
		Assert.That(App.WaitForElement("GroupIndexLabel").GetText(), Is.EqualTo("0"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("23"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithStartPositionAndVerticalList_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "0");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "1");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("27"));
		Assert.That(App.WaitForElement("GroupIndexLabel").GetText(), Is.EqualTo("1"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("0"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithCenterPositionAndVerticalList_Potato()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "3");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "1");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Potato");
		Assert.That(App.WaitForElement("GroupIndexLabel").GetText(), Is.EqualTo("1"));
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("30"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("3"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithEndPositionAndVerticalList_Papaya()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "11");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "0");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Papaya");
		Assert.That(App.WaitForElement("GroupIndexLabel").GetText(), Is.EqualTo("0"));
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("11"));
		VerifyScreenshot();
	}

	//Grouped ScrollTo By Item Tests
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByItemWithMakeVisiblePositionAndVerticalList_Apricot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Apricot");
		App.WaitForElement("ScrollToPositionMakeVisible");
		App.Tap("ScrollToPositionMakeVisible");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Apricot");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("24"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Fruits"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Apricot"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByItemWithStartPositionAndVerticalList_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("VegetableGroup");
		App.Tap("VegetableGroup");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Carrot");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("27"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Vegetables"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Carrot"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByItemWithCenterPositionAndVerticalList_Potato()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("VegetableGroup");
		App.Tap("VegetableGroup");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Potato");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Potato");
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Vegetables"));
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("30"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Potato"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByItemWithEndPositionAndVerticalList_Papaya()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Papaya");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Papaya");
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Fruits"));
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Papaya"));
		VerifyScreenshot();
	}
#endif

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyRemainingItemsThresholdReachedWithVerticalList()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("RemainingItemsThresholdLabel");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("Fired"));
	}

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
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.DragAndDrop("Banana", "Mango");
		App.WaitForElement("ReorderCompletedLabel");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("Fired"));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_IOS
	//CollectionView Fails to Preserve Scroll Offset with GridItemsLayout Using KeepScrollOffset Issue Link: https://github.com/dotnet/maui/issues/29202
	//CollectionView ItemsLayout does not update while switch from LinearItemsLayout to GridItemsLayout Issue Link: https://github.com/dotnet/maui/issues/27946
	//CollectionView CollectionView2 doesnot change ItemsLayout Issue Link: https://github.com/dotnet/maui/issues/28656

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithMakeVisiblePositionAndVerticalGrid_Radish()
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
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Radish");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("27"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("27"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithMakeVisiblePositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithMakeVisiblePositionAndHorizontalGrid_Pear()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "13");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Pear");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("13"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithStartPositionAndVerticalGrid_Mango()
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
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Mango");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("4"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("4"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithStartPositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithStartPositionAndHorizontalGrid_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithCenterPositionAndVerticalGrid_Kiwi()
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
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithCenterPositionAndHorizontalList_Kiwi()
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
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithCenterPositionAndHorizontalGrid_Kiwi()
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
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithEndPositionAndVerticalGrid_Radish()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "27");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Radish");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("27"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("27"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithEndPositionAndHorizontalList_Kiwi()
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
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByIndexWithEndPositionAndHorizontalGrid_Pear()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "13");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Pear");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("13"));
		VerifyScreenshot();
	}

	// ScrollTo By Item Tests
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithMakeVisiblePositionAndVerticalGrid_Radish()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Radish");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Radish");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("27"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Radish"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithMakeVisiblePositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithMakeVisiblePositionAndHorizontalGrid_Pear()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Pear");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Pear");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Pear"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithStartPositionAndVerticalGrid_Mango()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Mango");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Mango");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("4"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Mango"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithStartPositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithStartPositionAndHorizontalGrid_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithCenterPositionAndVerticalGrid_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithCenterPositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithCenterPositionAndHorizontalGrid_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithEndPositionAndVerticalGrid_Radish()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Radish");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Radish");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("27"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Radish"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithEndPositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("12"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyScrollToByItemWithEndPositionAndHorizontalGrid_Pear()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Pear");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Pear");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Pear"));
		VerifyScreenshot();
	}

	// Group ScrollTo test by index
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithMakeVisiblePositionAndVerticalGrid_Apricot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "23");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "0");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionMakeVisible");
		App.Tap("ScrollToPositionMakeVisible");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Apricot");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("24"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("23"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithMakeVisiblePositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "0");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionMakeVisible");
		App.Tap("ScrollToPositionMakeVisible");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithMakeVisiblePositionAndHorizontalGrid_Pear()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "13");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "0");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionMakeVisible");
		App.Tap("ScrollToPositionMakeVisible");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Pear");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("14"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("13"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithStartPositionAndVerticalGrid_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "0");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "1");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("27"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("0"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithStartPositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "0");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithStartPositionAndHorizontalGrid_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "0");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithCenterPositionAndVerticalGrid_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "0");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithCenterPositionAndHorizontalList_Tomato()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "4");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "1");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Tomato");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("31"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("4"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithCenterPositionAndHorizontalGrid_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "0");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithEndPositionAndVerticalGrid_Apricot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "23");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "0");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Apricot");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("24"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("23"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithEndPositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "12");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "0");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("12"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupIndexScrollToByIndexWithEndPositionAndHorizontalGrid_Potato()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToIndexEntry");
		App.ClearText("ScrollToIndexEntry");
		App.EnterText("ScrollToIndexEntry", "3");
		App.WaitForElement("GroupIndexEntry");
		App.ClearText("GroupIndexEntry");
		App.EnterText("GroupIndexEntry", "1");
		App.WaitForElement("ScrollToByIndex");
		App.Tap("ScrollToByIndex");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Potato");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("30"));
		Assert.That(App.WaitForElement("IndexLabel").GetText(), Is.EqualTo("3"));
		VerifyScreenshot();
	}

	// Group name ScrollTo test by item
	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithMakeVisiblePositionAndVerticalGrid_Apricot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Apricot");
		App.WaitForElement("ScrollToPositionMakeVisible");
		App.Tap("ScrollToPositionMakeVisible");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Apricot");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("24"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Fruits"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Apricot"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithMakeVisiblePositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement("ScrollToPositionMakeVisible");
		App.Tap("ScrollToPositionMakeVisible");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Fruits"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithMakeVisiblePositionAndHorizontalGrid_Pear()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Pear");
		App.WaitForElement("ScrollToPositionMakeVisible");
		App.Tap("ScrollToPositionMakeVisible");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Pear");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("14"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Fruits"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Pear"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithStartPositionAndVerticalGrid_Carrot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("VegetableGroup");
		App.Tap("VegetableGroup");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Carrot");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Carrot");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("27"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Vegetables"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Carrot"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithStartPositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("FruitGroup");
		App.Tap("FruitGroup");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Fruits"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithStartPositionAndHorizontalGrid_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("FruitGroup");
		App.Tap("FruitGroup");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement("ScrollToPositionStart");
		App.Tap("ScrollToPositionStart");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("FirstIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Fruits"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithCenterPositionAndVerticalGrid_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("FruitGroup");
		App.Tap("FruitGroup");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Fruits"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithCenterPositionAndHorizontalList_Tomato()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("VegetableGroup");
		App.Tap("VegetableGroup");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Tomato");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Tomato");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("31"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Vegetables"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Tomato"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithCenterPositionAndHorizontalGrid_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("FruitGroup");
		App.Tap("FruitGroup");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement("ScrollToPositionCenter");
		App.Tap("ScrollToPositionCenter");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("CenterIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Fruits"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithEndPositionAndVerticalGrid_Apricot()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("FruitGroup");
		App.Tap("FruitGroup");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Apricot");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement(ItemsLayoutVerticalGrid);
		App.Tap(ItemsLayoutVerticalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Apricot");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("24"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Fruits"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Apricot"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithEndPositionAndHorizontalList_Kiwi()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("FruitGroup");
		App.Tap("FruitGroup");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Kiwi");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement(ItemsLayoutHorizontalList);
		App.Tap(ItemsLayoutHorizontalList);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Kiwi");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("13"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Fruits"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Kiwi"));
		VerifyScreenshot();
	}

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void VerifyGroupItemScrollToByIndexWithEndPositionAndHorizontalGrid_Potato()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceGroupedList3);
		App.Tap(ItemsSourceGroupedList3);
		App.WaitForElement(IsGroupedTrue);
		App.Tap(IsGroupedTrue);
		App.WaitForElement("ScrollToByItem");
		App.Tap("ScrollToByItem");
		App.WaitForElement("VegetableGroup");
		App.Tap("VegetableGroup");
		App.WaitForElement("ScrollToItemEntry");
		App.ClearText("ScrollToItemEntry");
		App.EnterText("ScrollToItemEntry", "Potato");
		App.WaitForElement("ScrollToPositionEnd");
		App.Tap("ScrollToPositionEnd");
		App.WaitForElement(ItemsLayoutHorizontalGrid);
		App.Tap(ItemsLayoutHorizontalGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("CollectionViewControl");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.WaitForElement("ScrollTo");
		App.Tap("ScrollTo");
		App.WaitForElement("ScrollToRequestedLabel");
		Assert.That(App.WaitForElement("ScrollToRequestedLabel").GetText(), Is.EqualTo("Fired"));
		App.WaitForElement("Potato");
		Assert.That(App.WaitForElement("LastIndexLabel").GetText(), Is.EqualTo("30"));
		Assert.That(App.WaitForElement("GroupLabel").GetText(), Is.EqualTo("Vegetables"));
		Assert.That(App.WaitForElement("ItemLabel").GetText(), Is.EqualTo("Potato"));
		VerifyScreenshot();
	}

	// RemainingItemsThresholdReached Event Tests
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
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollDown("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("RemainingItemsThresholdLabel");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 100);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 100);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 100);
		App.WaitForElement("RemainingItemsThresholdLabel");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("Not Fired"));
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight("CollectionViewControl", ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("RemainingItemsThresholdLabel");
		Assert.That(App.WaitForElement("RemainingItemsThresholdLabel").GetText(), Is.EqualTo("Fired"));
	}

	// ReorderCompleted Event Tests
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
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.DragAndDrop("Banana", "Mango");
		App.WaitForElement("ReorderCompletedLabel");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.DragAndDrop("Banana", "Mango");
		App.WaitForElement("ReorderCompletedLabel");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("Fired"));
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
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("Not Fired"));
		App.DragAndDrop("Banana", "Mango");
		App.WaitForElement("ReorderCompletedLabel");
		Assert.That(App.WaitForElement("ReorderCompletedLabel").GetText(), Is.EqualTo("Fired"));
	}
#endif
}