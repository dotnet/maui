using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
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
		public void KeepItemsInView()
		{
			if (Device == TestDevice.Android || Device == TestDevice.Windows)
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
				Assert.Ignore();
		}

		// KeepScrollOffset (src\Compatibility\ControlGallery\src\Issues.Shared\CollectionViewItemsUpdatingScrollMode.cs)
		[Test]
		public void KeepScrollOffset()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });

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
		public void KeepLastItemInView()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS, TestDevice.Mac, TestDevice.Windows });

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