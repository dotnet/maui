#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests;

public class Material3CarouselViewFeatureTests : _GalleryUITest
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
    private const string KeepLastItemInView = "KeepLastItemInView";
    private const string AddButton = "AddButton";
    private const string PositionEntry = "PositionEntry";
    private const string CurrentPositionLabel = "CurrentPositionLabel";
    private const string CurrentItemLabel = "CurrentItemLabel";
    private const string PreviousItemLabel = "PreviousItemLabel";
    private const string ScrollToIndexEntry = "ScrollToIndexEntry";
    private const string ScrollToButton = "ScrollToButton";

    public override string GalleryPageName => CarouselViewFeatureMatrix;

    public Material3CarouselViewFeatureTests(TestDevice device) : base(device)
    {
    }

    [Test, Order(1)]
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewLoadsItems()
    {
        // Regression guard for #35637: the Material CarouselLayoutManager must lay out the
        // real item count and render the first item instead of getting stuck on splash.
        App.WaitForElement("CarouselViewButton");
        App.Tap("CarouselViewButton");
        App.WaitForElement(Options);
        App.Tap(Options);
        App.WaitForElement(Apply);
        App.Tap(Apply);
        App.WaitForElement("Item 1");
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithKeepItemInView()
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
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithKeepLastItemInView()
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
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithGridLayout()
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
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithImageView()
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
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithEmptyViewString()
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
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithEmptyViewImageView()
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
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithEmptyViewTemplate()
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

    [Test]
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithPosition()
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
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithPositionAndCurrentItem()
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
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithPositionAndCurrentPosition()
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

    [Test]
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithScrollTo()
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
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithScrollToAndCurrentItem()
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
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithCurrentItems()
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

    [Test]
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithPreviousItem()
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

    [Test]
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithVerticalLayout()
    {
        App.WaitForElement(Options);
        App.Tap(Options);
        App.WaitForElement(ItemsLayoutVertical);
        App.Tap(ItemsLayoutVertical);
        App.WaitForElement(Apply);
        App.Tap(Apply);
        App.WaitForElement("Item 1");
        App.ScrollDown(CarouselViewControl, ScrollStrategy.Gesture, 0.9, 500);
        App.WaitForElement("Item 2");
    }

    [Test]
    [Category(UITestCategories.Material3)]
    public void Material3VerifyCarouselViewWithVerticalLayoutAndKeepItemInView()
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
}
#endif
