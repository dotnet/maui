using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CollectionViewItemsUpdatingScrollModeUITests : _IssuesUITest
	{
		public CollectionViewItemsUpdatingScrollModeUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "CollectionView ItemsUpdatingScrollMode";

		protected override bool ResetAfterEachTest => true;

		// KeepScrollOffset (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsUpdatingScrollMode.cs)
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void KeepItemsInView()
		{
			App.WaitForElement("ScrollToMiddle");
			App.Click("ScrollToMiddle");
			App.WaitForElement("Vegetables.jpg, 10");

			for (int n = 0; n < 25; n++)
			{
				App.WaitForElement("AddItemAbove");
				App.Tap("AddItemAbove");
			}

			App.WaitForNoElement("Vegetables.jpg, 10");
		}


#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // The test fails on iOS and macOS because Appium is unable to locate the Picker control elements resulting in a TimeoutException. For more information, see: https://github.com/dotnet/maui/issues/28024
		// KeepScrollOffset (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsUpdatingScrollMode.cs)
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void KeepScrollOffset()
		{
			App.WaitForElement("SelectScrollMode");
			App.Click("SelectScrollMode");
			App.Click("KeepScrollOffset");

			App.WaitForElement("ScrollToMiddle");
			App.Click("ScrollToMiddle");
			App.WaitForElement("Vegetables.jpg, 10");
			App.Click("AddItemAbove");
			App.WaitForElement("photo.jpg, 9");
		}

		// KeepLastItemInView(src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsUpdatingScrollMode.cs)
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void KeepLastItemInView()
		{
			App.WaitForElement("SelectScrollMode");
			App.Click("SelectScrollMode");
			App.Click("KeepLastItemInView");

			App.WaitForElement("ScrollToMiddle");
			App.Click("ScrollToMiddle");
			App.WaitForElement("Vegetables.jpg, 10");
			App.Click("AddItemToEnd");
			App.WaitForElement("Added item");
		}
#endif
	}
}