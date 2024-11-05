#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue10274 : _IssuesUITest
    {
		public Issue10274(TestDevice device): base(device)
		{
		}

		public override string Issue => "MAUI Flyout does not work on Android when not using Shell";

		[Test]
		[Category(UITestCategories.FlyoutPage)]
		public void FlyoutPageNavigation()
		{
			App.WaitForElement("button");
			App.Tap("button");

			App.WaitForElement("flyoutPageButton");
			App.Tap("flyoutPageButton");

			App.WaitForElement("button");
			VerifyScreenshot();
		}
	}
}
#endif