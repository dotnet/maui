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
	public void VerifyCarouselViewWithIGridLayOut()
	{
		App.WaitForElement("CarouselViewButton");
		App.Tap("CarouselViewButton");
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

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/28334 && https://github.com/dotnet/maui/issues/28022
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
	public void VerifyCarouselViewWithEmptyViewCustomView()
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
	public void VerifyCarouselViewWithEmptyViewDataTemplate()
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
	public void VerifyCarouselViewWithKeepItemInView()
	{
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
#endif

#if TEST_FAILS_ON_WINDOWS //related issue link: https://github.com/dotnet/maui/issues/29420
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

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue:https://github.com/dotnet/maui/issues/29449
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
	public void VerifyCarouselViewWithIsSwipeEnabled()
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

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link: https://github.com/dotnet/maui/issues/29412 && CV2 related issue link:https://github.com/dotnet/maui/issues/29449
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithEnbleLoopAndCurrentItem()
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

#if TEST_FAILS_ON_ANDROID //In android related issue :https://github.com/dotnet/maui/issues/23023
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithCurrentPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
		App.ScrollRight(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("1"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && In CV2 related issue link:https://github.com/dotnet/maui/issues/29449
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithEnbleLoopAndCurrentPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("0"));
		App.ScrollLeft(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Item 5");
		Assert.That(App.WaitForElement(CurrentPositionLabel).GetText(), Is.EqualTo("4"));
	}
#endif
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
		App.ScrollRight(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 1"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue link:https://github.com/dotnet/maui/issues/29449
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithEnbleLoopAndPreviousItem()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("No previous item"));
		App.ScrollLeft(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollLeft(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Item 4");
		Assert.That(App.WaitForElement(PreviousItemLabel).GetText(), Is.EqualTo("Item 5"));
	}
#endif

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
		App.ScrollRight(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Item 2");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("0"));
	}

#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_IOS && TEST_FAILS_ON_WINDOWS //In android related issue link: https://github.com/dotnet/maui/issues/29411, In Windows related issue link:https://github.com/dotnet/maui/issues/29412 && CV2 related issue link: https://github.com/dotnet/maui/issues/29449
	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyCarouselViewWithEnbleLoopAndPreviousPosition()
	{
		App.WaitForElement(Options);
		App.Tap(Options);
		App.WaitForElement(LoopLabel);
		App.Tap(LoopLabel);
		App.WaitForElement(Apply);
		App.Tap(Apply);
		App.WaitForElement("Item 1");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("No previous position"));
		App.ScrollLeft(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollLeft(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
		App.WaitForElement("Item 4");
		Assert.That(App.WaitForElement(PreviousPositionLabel).GetText(), Is.EqualTo("4"));
	}
#endif

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
		App.EnterText(ScrollToIndexEntry, "3");
		App.WaitForElement(ScrollToButton);
		App.Tap(ScrollToButton);
		App.WaitForElement("Item 4");
	}
}