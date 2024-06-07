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

		[OneTimeTearDown]
		public void CollectionViewItemsUpdatingScrollModeOneTimeTearDown()
		{
			this.Back();
		}

		// KeepScrollOffset (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsUpdatingScrollMode.cs)
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void KeepItemsInView()
		{
			if (Device == TestDevice.Android)
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
			else
			{
				Assert.Ignore("This test is failing, requires research.");
			}
		}

		// KeepScrollOffset (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsUpdatingScrollMode.cs)
		[Test]
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
		[Test]
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