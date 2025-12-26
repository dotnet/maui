using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue2708 : _IssuesUITest
	{
		public Issue2708(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[Android] Prevent tabs from being removed during modal navigation";

		[Test]
		[Category(UITestCategories.TabbedPage)]
		public void TabsShouldRemainVisibleDuringModalNavigation()
		{
			// Verify we're on the TabbedPage and can see Tab 1
			App.WaitForElement("Tab1");
			App.WaitForElement("OpenModalButton");

			// Open modal page
			App.Tap("OpenModalButton");
			App.WaitForElement("CloseModalButton");

			// Take a screenshot to verify tabs are still visible while modal is open
			// This is the key assertion - tabs should NOT be removed when showing a modal
			VerifyScreenshot();
		}
	}
}