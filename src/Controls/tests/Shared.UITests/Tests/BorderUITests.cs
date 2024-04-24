using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	public class BorderUITests : UITest
	{
		const string BorderGallery = "Border Gallery";

		public BorderUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(BorderGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		// TODO: Enable this as a test once fully working
		//[Test]
		//[Category(UITestCategories.Border)]
		public void BordersWithVariousShapes()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "BordersWithVariousShapes");
			App.Click("GoButton");

			App.WaitForElement("WaitForStubControl");

			VerifyScreenshot();
		}
	}
}
