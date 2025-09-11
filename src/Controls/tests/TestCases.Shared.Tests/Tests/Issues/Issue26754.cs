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
			App.WaitForElement("TestLabel");
			App.Tap("More");
			VerifyScreenshot();
		}
	}
}