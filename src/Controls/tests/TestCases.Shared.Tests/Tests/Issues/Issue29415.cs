// On iOS this test fails with CollectionView, but passes with CollectionView2, so only run it with CollectionView2
#if TEST_FAILS_ON_WINDOWS // Carousel view tests fail on Windows
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29415 : _IssuesUITest
{
	public Issue29415(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "ItemsUpdatingScrollMode in CarouselView Not Working as expected";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void ItemsUpdatingScrollModeShouldWork()
	{
		App.WaitForElement("ItemsUpdatingKeepItemsInView");
		App.Tap("AddButton");
		App.Tap("ItemsUpdatingKeepItemsInView");
		App.WaitForElement("Item1");
		App.Tap("ItemsUpdatingKeepLastItemInView");
		App.Tap("AddButton");
		App.WaitForElement("Item4");
		App.Tap("ItemsUpdatingKeepLastItemInView");
		App.Tap("AddButton");
		App.WaitForElement("Item5");
		App.Tap("ItemsUpdatingKeepScrollOffset");
		App.Tap("AddButton");
		App.WaitForElement("Item5");
		App.Tap("ItemsUpdatingKeepItemsInView");
		App.Tap("AddButton");
		App.WaitForElement("Item1");
	}
}
#endif