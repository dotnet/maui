using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class CarouselViewFeatureTests : UITest
{
	public const string CarouselViewFeatureMatrix = "CarouselView Feature Matrix";
	private const string CarouselViewControl = "CarouselViewControl";
	private const string Options = "Options";
	private const string Apply = "Apply";
	private const string ItemTemplateGrid = "ItemTemplateGrid";
	private const string ItemTemplateCustomView = "ItemTemplateCustomView";
	private const string EmptyViewString = "EmptyViewString";
	private const string EmptyViewCustomView = "EmptyViewCustomView";
	private const string EmptyViewDataTemplate = "EmptyViewDataTemplate";
	private const string ItemsSourceNone = "ItemsSourceNone";
	private const string ItemsLayoutVertical = "ItemsLayoutVertical";
	private const string KeepItemsInView = "KeepItemsInView";
	private const string KeepScrollOffset = "KeepScrollOffset";
	private const string KeepLastItemInView = "KeepLastItemInView";
	private const string LoopLabel = "LoopLabel";
	private const string SwipeLabel = "SwipeLabel";
	private const string AddButton = "AddButton";
	private const string PositionEntry = "PositionEntry";
	private const string CurrentPositionLabel = "CurrentPositionLabel";
	private const string PreviousPositionLabel = "PreviousPositionLabel";
	private const string CurrentItemLabel = "CurrentItemLabel";
	private const string PreviousItemLabel = "PreviousItemLabel";
	private const string ScrollToIndexEntry = "ScrollToIndexEntry";
	private const string ScrollToButton = "ScrollToButton";

	public CarouselViewFeatureTests(TestDevice device) : base(device)
	{
	}

	protected override void FixtureSetup()
	{
		base.FixtureSetup();
		App.NavigateToGallery(CarouselViewFeatureMatrix);
	}

	[Test, Order(1)]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepItemInView()
	{
		App.WaitForElement("CarouselViewButton");
		App.Tap("CarouselViewButton");
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepItemsInView);
		App.Tap(KeepItemsInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
	}

#if TEST_FAILS_ON_WINDOWS //In windows related issue link: https://github.com/dotnet/maui/issues/29529
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepItemInViewAndCurrentItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepItemsInView);
		App.Tap(KeepItemsInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 1"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 6"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepItemInViewAndCurrentPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepItemsInView);
		App.Tap(KeepItemsInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepItemInViewAndPreviousItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepItemsInView);
		App.Tap(KeepItemsInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("No previous item"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 1"));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In CV2 related issue link: https://github.com/dotnet/maui/issues/29524 
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepItemInViewAndPreviousPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepItemsInView);
		App.Tap(KeepItemsInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("No previous position"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 7");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("1"));
	}
#endif
#endif

#if TEST_FAILS_ON_WINDOWS //In windows related issue link: https://github.com/dotnet/maui/issues/29462
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithGridLayout()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemTemplateGrid);
		App.Tap(ItemTemplateGrid);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1 (Grid Template)");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithImageView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemTemplateCustomView);
		App.Tap(ItemTemplateCustomView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1 (Image View)");
	}
#endif

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/28334 && https://github.com/dotnet/maui/issues/28022 && https://github.com/dotnet/maui/issues/29463 && https://github.com/dotnet/maui/issues/29472
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithEmptyViewString()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceNone);
		App.Tap(ItemsSourceNone);
		App.WaitForElement(EmptyViewString);
		App.Tap(EmptyViewString);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("No items available");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithEmptyViewImageView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceNone);
		App.Tap(ItemsSourceNone);
		App.WaitForElement(EmptyViewCustomView);
		App.Tap(EmptyViewCustomView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("No items available(Custom View)");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithEmptyViewTemplate()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsSourceNone);
		App.Tap(ItemsSourceNone);
		App.WaitForElement(EmptyViewDataTemplate);
		App.Tap(EmptyViewDataTemplate);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("No items available (DataTemplate)");
	}
#endif

#if TEST_FAILS_ON_ANDROID //In android related issue: https://github.com/dotnet/maui/issues/29415
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepScrollOffset()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepScrollOffset);
		App.Tap(KeepScrollOffset);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 1");
	}

#if TEST_FAILS_ON_WINDOWS //In CV2 related issue link: https://github.com/dotnet/maui/issues/29524 && In windows related issue link: https://github.com/dotnet/maui/issues/29529
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepScrollOffsetAndCurrentItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepScrollOffset);
		App.Tap(KeepScrollOffset);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 1"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 1"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepScrollOffsetAndCurrentPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepScrollOffset);
		App.Tap(KeepScrollOffset);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("1"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepScrollOffsetAndPreviousItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepScrollOffset);
		App.Tap(KeepScrollOffset);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("No previous item"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 6"));
	}

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In CV2 related issue link: https://github.com/dotnet/maui/issues/29524
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepScrollOffsetAndPreviousPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepScrollOffset);
		App.Tap(KeepScrollOffset);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("No previous position"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("0"));
	}
#endif
#endif
#endif

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/29420 && https://github.com/dotnet/maui/issues/29529
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepLastItemInView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepLastItemInView);
		App.Tap(KeepLastItemInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 5");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepLastItemInViewAndCurrentItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepLastItemInView);
		App.Tap(KeepLastItemInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 1"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 5");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 5"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepLastItemInViewAndCurrentPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepLastItemInView);
		App.Tap(KeepLastItemInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 5");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("5"));
	}

#if TEST_FAILS_ON_ANDROID // Issue link: https://github.com/dotnet/maui/issues/29544
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepLastItemInViewAndPreviousItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepLastItemInView);
		App.Tap(KeepLastItemInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("No previous item"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 5");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 6"));
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_ANDROID//In CV2 related issue link: https://github.com/dotnet/maui/issues/29529 Android related issue link: https://github.com/dotnet/maui/issues/29544
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithKeepLastItemInViewAndPreviousPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepLastItemInView);
		App.Tap(KeepLastItemInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("No previous position"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 5");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("0"));
	}
#endif
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In windows related issue : https://github.com/dotnet/maui/issues/29420 && In android related issue: https://github.com/dotnet/maui/issues/29415 && In CV2 related issue:https://github.com/dotnet/maui/issues/29449
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithIsLoopEnabledAndKeepItemInView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepItemsInView);
		App.Tap(KeepItemsInView);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
	}
	
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithIsLoopEnabledAndKeepScrollOffset()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepScrollOffset);
		App.Tap(KeepScrollOffset);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 1");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithIsLoopEnabledAndKeepLastItemInView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(KeepLastItemInView);
		App.Tap(KeepLastItemInView);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 5");
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue:https://github.com/dotnet/maui/issues/29449 && https://github.com/dotnet/maui/issues/29261
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithIsLoopEnabled()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		for (int i = 1; i < 7; i++)
		{
			App.ScrollRight(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		}
		App.WaitForElement("Item 2");
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS // related issue link:https://github.com/dotnet/maui/issues/29391
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithIsSwipeDisable()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SwipeLabel);
		App.Tap(SwipeLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		for (int i = 1; i < 2; i++)
		{
			App.ScrollRight(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		}
		App.WaitForElement("Item 1");
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS//In CV2 related issue link: https://github.com/dotnet/maui/issues/29312 and In windows related issue link: https://github.com/dotnet/maui/issues/15443
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(PositionEntry);
		App.Tap(PositionEntry);
		App.ClearText(PositionEntry);
		App.EnterText(PositionEntry, "2");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 3");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithPositionAndCurrentItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(PositionEntry);
		App.Tap(PositionEntry);
		App.ClearText(PositionEntry);
		App.EnterText(PositionEntry, "1");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 2"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithPositionAndCurrentPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(PositionEntry);
		App.Tap(PositionEntry);
		App.ClearText(PositionEntry);
		App.EnterText(PositionEntry, "1");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_ANDROID //In android related issue link: https://github.com/dotnet/maui/issues/29544
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithPositionAndPreviousItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(PositionEntry);
		App.Tap(PositionEntry);
		App.ClearText(PositionEntry);
		App.EnterText(PositionEntry, "1");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 1"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithPositionAndPreviousPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(PositionEntry);
		App.Tap(PositionEntry);
		App.ClearText(PositionEntry);
		App.EnterText(PositionEntry, "1");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("0"));
	}
#endif
#endif


#if TEST_FAILS_ON_WINDOWS //In windows related issue link:  https://github.com/dotnet/maui/issues/15443
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithScrollToAndCurrentItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "1");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 2"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithScrollToAndCurrentPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "1");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_ANDROID //In android related issue link: https://github.com/dotnet/maui/issues/29544  

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithScrollToAndPreviousItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "1");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 1"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithScrollToAndPreviousPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "1");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("0"));
	}
#endif
#endif

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithPeekAreaInsets()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("PeekAreaInsetsEntry");
		App.Tap("PeekAreaInsetsEntry");
		App.ClearText("PeekAreaInsetsEntry");
		App.EnterText("PeekAreaInsetsEntry", "120");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement("Item 2");
	}

#if TEST_FAILS_ON_WINDOWS //In windows related issue link: https://github.com/dotnet/maui/issues/29529
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithCurrentItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 1"));
		App.ScrollRight(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 2"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link: https://github.com/dotnet/maui/issues/29412 && CV2 related issue link:https://github.com/dotnet/maui/issues/29449 && https://github.com/dotnet/maui/issues/29261
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithEnableLoopAndCurrentItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 1"));
		App.ScrollLeft(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Item 5");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 5"));
	}
#endif

#if TEST_FAILS_ON_WINDOWS //In windows related issue link:https://github.com/dotnet/maui/issues/29529
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithCurrentPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && In CV2 related issue link:https://github.com/dotnet/maui/issues/29449
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithEnableLoopAndCurrentPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
	}
#endif

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithPreviousItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("No previous item"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 1"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue link:https://github.com/dotnet/maui/issues/29449
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithEnableLoopAndPreviousItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("No previous item"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 1"));
	}
#endif

#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST //In CV2 related issue link: https://github.com/dotnet/maui/issues/29524
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithPreviousPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("No previous position"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("1"));
	}
#endif
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue link: https://github.com/dotnet/maui/issues/29449 && https://github.com/dotnet/maui/issues/29524
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithEnableLoopAndPreviousPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("No previous position"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("1"));
	}
#endif

#if TEST_FAILS_ON_WINDOWS //In windows related issue link: https://github.com/dotnet/maui/issues/29448
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithScrollTo()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "3");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 4");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithIsSwipeDisableAndScrollTo()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(SwipeLabel);
		App.Tap(SwipeLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "3");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 4");
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In CV2 related issue link: https://github.com/dotnet/maui/issues/27711
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithRTLFlowDirection()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement("FlowDirectionRTL");
		App.Tap("FlowDirectionRTL");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.ScrollRight(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Item 1");
	}
#endif


#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS // related issue link: https://github.com/dotnet/maui/issues/29372
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayout()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		for (int i = 1; i < 5; i++)
		{
			App.ScrollDown(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		}
		App.WaitForElement("Item 5");
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS
// Android related issue link: https://github.com/dotnet/maui/issues/29544 In CV2 related issue link: https://github.com/dotnet/maui/issues/29312 and In windows related issue link: https://github.com/dotnet/maui/issues/15443
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(PositionEntry);
		App.Tap(PositionEntry);
		App.ClearText(PositionEntry);
		App.EnterText(PositionEntry, "1");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 2");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewCurrentItemWithVerticalLayoutAndPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(PositionEntry);
		App.Tap(PositionEntry);
		App.ClearText(PositionEntry);
		App.EnterText(PositionEntry, "1");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 2"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewPreviousItemWithVerticalLayoutAndPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(PositionEntry);
		App.Tap(PositionEntry);
		App.ClearText(PositionEntry);
		App.EnterText(PositionEntry, "1");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 1"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewPreviousPositionWithVerticalLayoutAndPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(PositionEntry);
		App.Tap(PositionEntry);
		App.ClearText(PositionEntry);
		App.EnterText(PositionEntry, "1");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("0"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewCurrentPositionWithVerticalLayoutAndPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(PositionEntry);
		App.Tap(PositionEntry);
		App.ClearText(PositionEntry);
		App.EnterText(PositionEntry, "1");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("1"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewCurrentItemWithVerticalLayoutAndScrollTo()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "1");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 2"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewPreviousItemWithVerticalLayoutAndScrollTo()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "1");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 1"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewCurrentPositionWithVerticalLayoutAndScrollTo()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "1");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("1"));
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewPreviousPositionWithVerticalLayoutAndScrollTo()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "1");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("0"));	
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue link: https://github.com/dotnet/maui/issues/29449 && https://github.com/dotnet/maui/issues/29524
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndEnableLoop()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		for (int i = 1; i < 7; i++)
		{
			App.ScrollDown(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		}
		App.WaitForElement("Item 2");
	}
#endif

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndKeepItemInView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(KeepItemsInView);
		App.Tap(KeepItemsInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndKeepScrollOffset()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(KeepScrollOffset);
		App.Tap(KeepScrollOffset);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 1");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndKeepLastItemInView()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(KeepLastItemInView);
		App.Tap(KeepLastItemInView);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 5");
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue link: https://github.com/dotnet/maui/issues/29449 && https://github.com/dotnet/maui/issues/29524
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndIsLoopEnabled()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		for (int i = 1; i < 7; i++)
		{
			App.ScrollDown(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		}
		App.WaitForElement("Item 2");
	}
#endif

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndIsSwipeEnabled()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(SwipeLabel);
		App.Tap(SwipeLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		for (int i = 1; i < 2; i++)
		{
			App.ScrollDown(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		}
		App.WaitForElement("Item 1");
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndPeekAreaInsets()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement("PeekAreaInsetsEntry");
		App.Tap("PeekAreaInsetsEntry");
		App.ClearText("PeekAreaInsetsEntry");
		App.EnterText("PeekAreaInsetsEntry", "120");
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement("Item 2");
	}
 
#if TEST_FAILS_ON_WINDOWS //In windows related issue link: https://github.com/dotnet/maui/issues/29529
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndCurrentItems()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 1"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 6"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue link: https://github.com/dotnet/maui/issues/29449 && https://github.com/dotnet/maui/issues/29524
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndEnableLoopAndCurrentItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 1"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(CurrentItemLabel).GetText(), Is.EqualTo("Item 6"));
	}
#endif

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndCurrentPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue link: https://github.com/dotnet/maui/issues/29449 && https://github.com/dotnet/maui/issues/29524
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndEnableLoopAndCurrentPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
	}
#endif

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndPreviousItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("No previous item"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 1"));
	}
#endif

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue link: https://github.com/dotnet/maui/issues/29449 && https://github.com/dotnet/maui/issues/29524
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndEnableLoopAndPreviousItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("No previous item"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 1"));
	}
#endif

#if TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS //In CV2 related issue link: https://github.com/dotnet/maui/issues/29524
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndPreviousPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("No previous position"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("1"));
	}


#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue link: https://github.com/dotnet/maui/issues/29449 && https://github.com/dotnet/maui/issues/29524
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndEnableLoopAndPreviousPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("No previous position"));
		App.WaitForElement(AddButton);
		App.Tap(AddButton);
		App.WaitForElement("Item 6");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("1"));
	}
#endif
#endif

#if TEST_FAILS_ON_WINDOWS //In windows related issue link: https://github.com/dotnet/maui/issues/29448
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndScrollTo()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "3");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 4");
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue link: https://github.com/dotnet/maui/issues/29449 && https://github.com/dotnet/maui/issues/29524
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndIsLoopEnableAndScrollTo()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "3");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 4");
	}
	
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithVerticalLayoutAndIsSwipeDisableAndScrollTo()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(ItemsLayoutVertical);
		App.Tap(ItemsLayoutVertical);
		App.WaitForElement(SwipeLabel);
		App.Tap(SwipeLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		App.WaitForElement(ScrollToIndexEntry);
		App.ClearText(ScrollToIndexEntry);
		App.EnterText(ScrollToIndexEntry, "3");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 4");
	}
#endif
#endif
#endif
}