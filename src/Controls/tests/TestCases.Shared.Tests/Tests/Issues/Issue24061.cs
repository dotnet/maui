using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24061 : _IssuesUITest
	{
		public Issue24061(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ScrollView's gradient background doesn't work with ScrollView";

		[Test]
		[Category(UITestCategories.ScrollView)]
		public void ScrollViewGradientBackgroundShouldWorkWithScrollView()
		{
			Exception? exception = null;

			App.WaitForElement("scrollButton");

			App.Click("scrollButton");
			VerifyScreenshotOrSetException(ref exception, "ScrollViewGradientBackground");
			App.Click("button");
			App.WaitForElement("scrollButton");
			VerifyScreenshotOrSetException(ref exception, "ScrollViewWithoutGradientBackground");
			App.Click("button");
			App.WaitForElement("scrollButton");
			VerifyScreenshotOrSetException(ref exception, "ScrollViewGradientBackground");
			if (exception != null)
			{
				throw exception;
			}
		}
	}
}