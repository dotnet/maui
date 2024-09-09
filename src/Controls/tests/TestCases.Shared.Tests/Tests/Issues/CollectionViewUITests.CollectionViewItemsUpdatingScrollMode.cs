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

#if ANDROID
		// KeepScrollOffset (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsUpdatingScrollMode.cs)
		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnIOS("This test is failing, likely due to product issue")]
		[FailsOnMac("This test is failing, likely due to product issue")]
		[FailsOnWindows("This test is failing, likely due to product issue")]
		public void KeepItemsInView()
		{
			App.WaitForElement("ScrollToMiddle");
			App.Click("ScrollToMiddle");
			App.WaitForNoElement("Vegetables.jpg, 10");

			for (int n = 0; n < 25; n++)
			{
				App.Click("AddItemAbove");
			}

			App.WaitForNoElement("Vegetables.jpg, 10");
		}
#endif

		// KeepScrollOffset (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsUpdatingScrollMode.cs)
		//[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnAllPlatforms("This test is failing, likely due to product issue")]
		public void KeepScrollOffset()
		{
			App.WaitForElement("SelectScrollMode");
			App.Click("SelectScrollMode");
			App.Click("KeepScrollOffset");

			App.WaitForElement("ScrollToMiddle");
			App.Click("ScrollToMiddle");
			App.WaitForNoElement("Vegetables.jpg, 10");
			App.Click("AddItemAbove");
			App.WaitForNoElement("photo.jpg, 9");
		}

		// KeepLastItemInView(src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsUpdatingScrollMode.cs)
		//[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnAllPlatforms("This test is failing, likely due to product issue")]
		public void KeepLastItemInView()
		{
			App.WaitForElement("SelectScrollMode");
			App.Click("SelectScrollMode");
			App.Click("KeepLastItemInView");

			App.WaitForElement("ScrollToMiddle");
			App.Click("ScrollToMiddle");
			App.WaitForNoElement("Vegetables.jpg, 10");
			App.Click("AddItemToEnd");
			App.WaitForElement("Added item");
		}
	}
}