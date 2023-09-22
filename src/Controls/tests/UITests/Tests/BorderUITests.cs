using Microsoft.Maui.Appium;

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

			VerifyScreenshot();
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			App.NavigateBack();
		}
	}
}
