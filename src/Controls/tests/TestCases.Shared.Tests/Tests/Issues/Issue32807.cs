using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32807 : _IssuesUITest
	{
		public override string Issue => "Alert popup not displaying when dismissing modal page on iOS/MacOS";

		public Issue32807(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.DisplayAlert)]
		public void AlertsShouldDisplayImmediatelyAfterDismissingModal()
		{
			// Wait for the page to load
			App.WaitForElement("OpenModalButton");

			// Open the modal page
			App.Tap("OpenModalButton");

			// Wait for modal to appear and then dismiss it
			App.WaitForElement("DismissButton");
			App.Tap("DismissButton");

			// After dismissing the modal, alerts should appear without delay
			// If the bug exists, these alerts won't appear (or will be delayed significantly)

			// Wait for Alert 1 to appear - this is the critical test
			// The alert should appear within a reasonable time (not 750ms+)
			App.WaitForElement("OK", timeout: System.TimeSpan.FromSeconds(2));
			App.Tap("OK");

			// Wait for Alert 2
			App.WaitForElement("OK", timeout: System.TimeSpan.FromSeconds(2));
			App.Tap("OK");

			// Wait for Alert 3
			App.WaitForElement("OK", timeout: System.TimeSpan.FromSeconds(2));
			App.Tap("OK");

			// Verify all alerts were shown successfully
			var statusText = App.FindElement("StatusLabel").GetText();
			Assert.That(statusText, Does.Contain("All alerts shown successfully"));
		}
	}
}
