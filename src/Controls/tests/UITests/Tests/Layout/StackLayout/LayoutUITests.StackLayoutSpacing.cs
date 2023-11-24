using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class StackLayoutSpacingUITests : LayoutUITests
	{
		public StackLayoutSpacingUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("Can apply space between each child")]
		public async Task StackLayoutSpacing()
		{
			App.Click("StackLayoutSpacing");
			App.WaitForElement("TestStackLayout");
			
			await Task.Delay(500);

			// 1. With a snapshot we verify that child elements has not
			// space between them.
			VerifyScreenshot("StackLayoutNoSpacing");

			// 2. Apply spacing (40) between items.
			App.Click("SpacingButton");
			await Task.Delay(500);

			// 3. With a snapshot we verify that can apply space
			// between each child.
			VerifyScreenshot("StackLayoutSpacing");

			// 4. Remove the spacing.
			App.Click("NoSpacingButton");
			await Task.Delay(500);

			VerifyScreenshot("StackLayoutNoSpacing");
		}
	}
}