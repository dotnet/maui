using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.Layout)]
	public class HorizontalStackLayoutUITests : LayoutUITests
	{
		public HorizontalStackLayoutUITests(TestDevice device)
			: base(device)
		{
		}
		
		[Test]
		[Description("Organizes child views in a horizontal one-dimensional stack")]
		public void HorizontalStackLayout()
		{
			App.Click("HorizontalStackLayout");
			App.WaitForElement("TestStackLayout");

			// 1. With a snapshot we verify that The StackLayout
			// organizes child views in a horizontal one-dimensional stack.
			VerifyScreenshot();
		}
	}
}