using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class AbsoluteLayoutPositionProportionalUITests : LayoutUITests
	{
		public AbsoluteLayoutPositionProportionalUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("The AbsoluteLayout is able of positioning its child elements with proportional positions")]
		public async Task PositionProportional()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows });

			App.Click("AbsoluteLayoutPositionProportional");
			App.WaitForElement("TestAbsoluteLayout");
			
			await Task.Delay(500);

			// 1. With a snapshot we verify that The AbsoluteLayout is able
			// of positioning its child elements with proportional positions.
			VerifyScreenshot();
		}
	}
}