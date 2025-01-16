using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue16522 : _IssuesUITest
    {
		public Issue16522(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] Tabs briefly display wrong background color when navigating between flyout items";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ShellTabBarBackgroundColor()
		{
			App.WaitForElement("Label");
			VerifyScreenshot();
		}
	}
}
