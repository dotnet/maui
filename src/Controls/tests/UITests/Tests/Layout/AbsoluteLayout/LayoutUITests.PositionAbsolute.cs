using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class AbsoluteLayoutPositionAbsoluteUITests : UITest
	{
		const string LayoutGallery = "Layout Gallery";

		public AbsoluteLayoutPositionAbsoluteUITests(TestDevice device)
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
		[Description("The AbsoluteLayout is able of positioning its child elements with absolute positions")]
		public void PositionAbsolute()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac },
				"Currently fails on iOS; see https://github.com/dotnet/maui/issues/18956");

			App.Click("Chessboard");
			App.WaitForElement("TestAbsoluteLayout");

			// 1. With a snapshot we verify that The AbsoluteLayout is able
			// of positioning its child elements with absolute positions.
			VerifyScreenshot();
		}
	}
}