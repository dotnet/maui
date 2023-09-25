using Microsoft.Maui.Appium;
using NUnit.Framework;

namespace Microsoft.Maui.AppiumTests
{
	public class BorderUITests : UITestBase
	{
		const string BorderGallery = "* marked:'Border Gallery'";

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
			App.NavigateBack();
		}

		[Test]
		public void BordersWithVariousShapes()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "BordersWithVariousShapes");
			App.Tap("GoButton");

			App.WaitForElement("WaitForControl");

			VerifyScreenshot();
		}

		[Test]
		public void BorderBackground()
		{
			App.WaitForElement("TargetView");
			App.EnterText("TargetView", "BorderBackground");
			App.Tap("GoButton");

			App.WaitForElement("WaitForControl");

			VerifyScreenshot();
		}
	}
}
