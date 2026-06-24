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
			Exception? exception = null;
			App.WaitForElement("Issue34402Label");
			VerifyScreenshotOrSetException(ref exception, "BoxView_LTR_Initial");

			App.Tap("BoxViewRtlButton");
			VerifyScreenshotOrSetException(ref exception, "BoxView_RTL_AfterButton");

			if (exception != null)
			{
				throw exception;
			}
		}

		[Test]
		[Category(UITestCategories.GraphicsView)]
		public void GraphicsViewFlowDirectionShouldMirrorOnRTL()
		{
			Exception? exception = null;
			App.WaitForElement("Issue34402GraphicsViewLabel");
			VerifyScreenshotOrSetException(ref exception, "GraphicsView_LTR_Initial");

			App.Tap("GraphicsViewRtlButton");
			VerifyScreenshotOrSetException(ref exception, "GraphicsView_RTL_AfterButton");

			if (exception != null)
			{
				throw exception;
			}
		}
	}
}
