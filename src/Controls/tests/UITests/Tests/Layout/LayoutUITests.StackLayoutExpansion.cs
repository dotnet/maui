using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class StackLayoutExpansionUITests : UITest
	{
		const string LayoutGallery = "Layout Gallery";

		public StackLayoutExpansionUITests(TestDevice device)
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
		[Description("Align child elements in the Y axis works")]
		public void StackLayoutExpansion()
		{
			App.Click("StackLayoutExpansion");
			App.WaitForElement("TestStackLayout");

			// 1. With a snapshot we verify that align child elements in
			// the Y axis works.
			VerifyScreenshot();
		}
	}
}