using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	class CollectionViewUITests : UITest
	{
		const string CarouselViewGallery = "CollectionView Gallery";

		public CollectionViewUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(CarouselViewGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[Test]
		public void CollectionViewVerticalList()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("VerticalList");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}

		[Test]
		public void CollectionViewHorizontalList()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("HorizontalList");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}

		[Test]
		public void CollectionViewVerticalGrid()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("VerticalGrid");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}

		[Test]
		public void CollectionViewHorizontalGrid()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("HorizontalGrid");

			// 1. Refresh the data source to verify that it works.
			App.EnterText("entryUpdate", "0");
			App.Click("btnUpdate");

			// 2. With a snapshot we verify that the CollectionView is rendered with the correct size.
			VerifyScreenshot();
		}
	}
}
