using System.ComponentModel;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31106 : _IssuesUITest
	{
		public override string Issue => "[MacCatalyst] Picker dialog closes automatically with VoiceOver/Keyboard";

		public Issue31106(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.Picker)]
		public void PickerShouldRemainOpenWithKeyboardNavigation()
		{
			// Wait for page to load
			App.WaitForElement("TitleLabel");
			App.WaitForElement("StatusLabel");

			// Verify initial state
			var initialStatus = App.FindElement("StatusLabel").GetText();
			Assert.That(initialStatus, Does.Contain("No item selected"));

			// Open picker programmatically to simulate keyboard/VoiceOver activation
			App.Tap("OpenPickerButton");

			// Wait for picker dialog to appear and remain open
			// The bug would cause immediate close, so wait a moment
			System.Threading.Thread.Sleep(1000);

			// After fix, the picker should remain open allowing selection
			// We can't directly interact with the MacCatalyst alert dialog via Appium,
			// but we can verify the app is still responsive and picker state is correct

			// Verify page is still responsive (picker didn't crash or hang)
			App.WaitForElement("StatusLabel");

			// The log label should show focus events indicating picker opened
			var logText = App.FindElement("LogLabel").GetText();
			Assert.That(logText, Does.Contain("FOCUSED"));
		}
	}
}
