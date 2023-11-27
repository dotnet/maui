using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class StackLayoutAlignmentUITests : LayoutUITests
	{
		public StackLayoutAlignmentUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("Align child elements in the X axis works")]
		public async Task StackLayoutAlignment()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows });

			App.Click("StackLayoutAlignment");
			App.WaitForElement("TestStackLayout");

			await Task.Delay(500);

			// 1. With a snapshot we verify that align child elements in
			// the X axis works.
			VerifyScreenshot();
		}
	}
}