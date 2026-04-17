using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue34402 : _IssuesUITest
	{
		public Issue34402(TestDevice device) : base(device) { }

		public override string Issue => "FlowDirection property not working on BoxView Control";

		[Test]
		[Category(UITestCategories.BoxView)]
		public void BoxViewFlowDirectionShouldMirrorOnRTL()
		{
			App.WaitForElement("Issue34402Label");
			VerifyScreenshot("BoxView_LTR_Initial");

			App.Tap("BoxViewRtlButton");
			VerifyScreenshot("BoxView_RTL_AfterButton");
		}

		[Test]
		[Category(UITestCategories.GraphicsView)]
		public void GraphicsViewFlowDirectionShouldMirrorOnRTL()
		{
			App.WaitForElement("Issue34402GraphicsViewLabel");
			VerifyScreenshot("GraphicsView_LTR_Initial");

			App.Tap("GraphicsViewRtlButton");
			VerifyScreenshot("GraphicsView_RTL_AfterButton");
		}
	}
}
