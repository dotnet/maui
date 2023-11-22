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
		public void PositionProportional()
		{
			App.Click("AbsoluteLayoutPositionProportional");
			App.WaitForElement("TestAbsoluteLayout");

			// 1. With a snapshot we verify that The AbsoluteLayout is able
			// of positioning its child elements with proportional positions.
			VerifyScreenshot();
		}
	}
}