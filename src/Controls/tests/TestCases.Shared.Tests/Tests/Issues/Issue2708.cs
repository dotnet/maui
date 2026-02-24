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

			// Close the modal
			App.Tap("CloseModalButton");

			// Verify tabs still work after modal dismiss - this is the key assertion.
			// If tabs were destroyed (RemoveTabs) during modal navigation, they won't
			// be properly restored, and tab switching will fail.
			App.WaitForElement("StatusLabel");
			App.Tap("Tab2");
			App.WaitForElement("Tab2");
			App.Tap("Tab1");
			App.WaitForElement("OpenModalButton");
		}
	}
}
