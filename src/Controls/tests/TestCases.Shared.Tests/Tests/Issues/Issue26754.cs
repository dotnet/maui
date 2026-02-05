using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue26754 : _IssuesUITest
	{
		public override string Issue => "[Windows] TabbedPage menu item text color";

		public Issue26754(TestDevice device)
		: base(device)
		{ }

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void VerifyTabbedPageMenuItemTextColor()
		{
			if (!OperatingSystem.IsMacOSVersionAtLeast(26)) // Issue Link: https://github.com/dotnet/maui/issues/33879
			{
				Assert.Ignore("Ignored the test on iOS if it runs on macOS version less than 26 due to known visual differences");
			}
			App.WaitForElement("TestLabel");
			App.Tap("More");
			VerifyScreenshot();
		}
	}
}