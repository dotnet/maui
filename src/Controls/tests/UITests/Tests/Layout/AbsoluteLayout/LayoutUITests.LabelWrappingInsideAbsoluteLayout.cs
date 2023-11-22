using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class LabelWrappingInsideAbsoluteLayoutUITests : UITest
	{
		const string LayoutGallery = "Layout Gallery";

		public LabelWrappingInsideAbsoluteLayoutUITests(TestDevice device)
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
		[Description("Labels inside an AbsoluteLayout is sized correctly wrapping the text.")]
		public void LabelWrappingInsideAbsoluteLayout()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS, TestDevice.Mac },
				"Currently fails on iOS and Android; see https://github.com/dotnet/maui/issues/18930");

			App.Click("StylishHeader");
			App.WaitForElement("TestAbsoluteLayout");

			// 1. Labels inside an AbsoluteLayout is sized correctly wrapping
			// the text.
			VerifyScreenshot();
		}
	}
}