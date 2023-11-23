using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class VerticalStackLayoutUITests : LayoutUITests
	{
		public VerticalStackLayoutUITests(TestDevice device)
			: base(device)
		{
		}
	
		[Test]
		[Description("Organizes child views in a vertical one-dimensional stack")]
		public async Task VerticalStackLayout()
		{
			App.Click("VerticalStackLayout");
			App.WaitForElement("TestStackLayout");

			await Task.Delay(500);

			// 1. With a snapshot we verify that The StackLayout
			// organizes child views in a vertical one-dimensional stack.
			VerifyScreenshot();
		}
	}
}