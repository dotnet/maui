using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue23674 : _IssuesUITest
	{
		public override string Issue => "Page.IsBusy activity indicators gets stuck/causes multiple to be displayed";

		public Issue23674(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Page)]
		public void Issue23674Test()
		{
			App.WaitForElement("button1");
			App.Click("button1");
			App.WaitForElement("button2");
			App.Click("button2");
			App.WaitForElement("button1");

			// The test passes if activity indicator is not visible
			VerifyScreenshot();
		}
	}
}
