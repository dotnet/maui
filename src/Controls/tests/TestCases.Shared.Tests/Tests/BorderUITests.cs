using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class BorderUITests : CoreGalleryBasePageTest
	{
		const string BorderGallery = "Border Gallery";

		public override string GalleryPageName => BorderGallery;

		public BorderUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(BorderGallery);
		}

		// TODO: Enable this as a test once fully working
		//[Test]
		//[Category(UITestCategories.Border)]
		public void BordersWithVariousShapes()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "BordersWithVariousShapes");
			App.Tap("GoButton");

			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
