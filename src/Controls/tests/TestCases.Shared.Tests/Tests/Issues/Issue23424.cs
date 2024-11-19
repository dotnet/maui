#if !MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23424 : _IssuesUITest
	{
		public Issue23424(TestDevice device) : base(device)
		{
		}

		public override string Issue => "BackButtonBehavior IsVisible=False does not hide the back button";

		[Test]
		[Category(UITestCategories.Shell)]
		public void BackButtonBehaviorIsVisibleWorksWithCustomIcon()
		{
			App.WaitForElement("button");
			App.Click("button");

			// The test passes if the back button is not visible
			VerifyScreenshot();
		}
	}
}
#endif