using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class AbsoluteLayoutPositionAbsoluteUITests : LayoutUITests
	{
		public AbsoluteLayoutPositionAbsoluteUITests(TestDevice device)
			: base(device)
		{
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