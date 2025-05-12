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
	private const string ItemsSourceObservableCollection = "ItemsSourceObservableCollection";
	private const string ItemsLayoutVertical = "ItemsLayoutVertical";
	private const string KeepItemsInView = "KeepItemsInView";
	private const string KeepScrollOffset = "KeepScrollOffset";
	private const string KeepLastItemInView = "KeepLastItemInView";
	private const string LoopLabel = "LoopLabel";
	private const string SwipeLabel = "SwipeLabel";
	private const string AddButton = "AddButton";
	private const string PositionEntry = "PositionEntry";

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
}