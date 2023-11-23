using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class LabelWrappingInsideAbsoluteLayoutUITests : LayoutUITests
	{
		public LabelWrappingInsideAbsoluteLayoutUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("Labels inside an AbsoluteLayout is sized correctly wrapping the text.")]
		public async Task LabelWrappingInsideAbsoluteLayout()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS, TestDevice.Mac },
				"Currently fails on iOS and Android; see https://github.com/dotnet/maui/issues/18930");

			App.Click("StylishHeader");
			App.WaitForElement("TestAbsoluteLayout");

			await Task.Delay(500);

			// 1. Labels inside an AbsoluteLayout is sized correctly wrapping
			// the text.
			VerifyScreenshot();
		}
	}
}