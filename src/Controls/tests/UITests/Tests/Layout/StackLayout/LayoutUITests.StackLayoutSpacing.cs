using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class StackLayoutSpacingUITests : UITest
	{
		const string LayoutGallery = "Layout Gallery";

		public StackLayoutSpacingUITests(TestDevice device)
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
		[Description("Can apply space between each child")]
		public void StackLayoutSpacing()
		{
			App.Click("StackLayoutSpacing");
			App.WaitForElement("TestStackLayout");

			// 1. With a snapshot we verify that child elements has not
			// space between them.
			VerifyScreenshot("StackLayoutNoSpacing");

			// 2. Apply spacing (40) between items.
			App.Click("SpacingButton");

			// 3. With a snapshot we verify that can apply space
			// between each child.
			VerifyScreenshot("StackLayoutSpacing");

			// 4. Remove the spacing.
			App.Click("NoSpacingButton");
			VerifyScreenshot("StackLayoutNoSpacing");
		}
	}
}