using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue8486 : _IssuesUITest
	{
		public Issue8486(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "GraphicsView DrawString not rendering in iOS";

		[Test]
		[Category(UITestCategories.GraphicsView)]
		public void DrawStringShouldDrawText()
		{
#if WINDOWS // On windows AutomationId is not working for GraphicsView, so take a screenshot directly
			VerifyScreenshot();
#else
			App.WaitForElement("GraphicsView");
			VerifyScreenshot();
#endif
		}
	}
}