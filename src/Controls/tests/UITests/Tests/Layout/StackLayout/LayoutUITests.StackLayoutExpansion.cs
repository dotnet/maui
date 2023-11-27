using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class StackLayoutExpansionUITests : LayoutUITests
	{
		public StackLayoutExpansionUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("Align child elements in the Y axis works")]
		public async Task StackLayoutExpansion()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows });

			App.Click("StackLayoutExpansion");
			App.WaitForElement("TestStackLayout");

			await Task.Delay(500);

			// 1. With a snapshot we verify that align child elements in
			// the Y axis works.
			VerifyScreenshot();
		}
	}
}