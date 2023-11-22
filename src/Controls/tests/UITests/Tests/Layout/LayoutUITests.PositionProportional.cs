using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class AbsoluteLayoutPositionProportionalUITests : UITest
	{
		const string LayoutGallery = "Layout Gallery";

		public AbsoluteLayoutPositionProportionalUITests(TestDevice device)
			: base(device)
		{
		}

		protected override void FixtureSetup()
		{
			base.FixtureSetup();
			App.NavigateToGallery(LayoutGallery);
		}

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			this.Back();
		}

		[TearDown]
		public void LayoutUITestTearDown()
		{
			this.Back();
		}

		[Test]
		[Description("The AbsoluteLayout is able of positioning its child elements with proportional positions")]
		public void PositionProportional()
		{
			App.Click("AbsoluteLayoutPositionProportional");
			App.WaitForElement("TestAbsoluteLayout");

			// 1. With a snapshot we verify that The AbsoluteLayout is able
			// of positioning its child elements with proportional positions.
			VerifyScreenshot();
		}
	}
}