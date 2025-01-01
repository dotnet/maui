using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19568 : _IssuesUITest
	{
		const string Success = "Success";

		public Issue19568(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "GraphicsView does not change the Background/BackgroundColor on Android";

		[Test]
		[Category(UITestCategories.GraphicsView)]
		public void GraphicsViewShouldHaveBackgroundColor()
		{
			App.WaitForElement("graphicsView");
			VerifyScreenshot();
		}
	}
}