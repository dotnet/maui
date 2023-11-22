using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class HorizontalStackLayoutUITests : UITest
	{
		const string LayoutGallery = "Layout Gallery";

		public HorizontalStackLayoutUITests(TestDevice device)
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
		[Description("Organizes child views in a horizontal one-dimensional stack")]
		public void HorizontalStackLayout()
		{
			App.Click("HorizontalStackLayout");
			App.WaitForElement("TestStackLayout");

			// 1. With a snapshot we verify that The StackLayout
			// organizes child views in a horizontal one-dimensional stack.
			VerifyScreenshot();
		}
	}
}