#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class EntryTextChangedCrash : _IssuesUITest
	{
		public EntryTextChangedCrash(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Entry crashes when setting Text in TextChanged event handler";

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryDoesNotCrashWhenSettingTextInTextChangedEvent()
		{
			// Wait for the entry to be visible
			App.WaitForElement("TestEntry");

			// Clear the entry text - this should trigger the TextChanged event
			// which will set Text to "0", previously causing a crash
			App.ClearText("TestEntry");

			// If we reach here without crashing, the fix is working
			// Verify that the status label updated
			var statusText = App.WaitForElement("StatusLabel").GetText();
			Assert.That(statusText, Does.Contain("Reset to 0"));

			// Verify the entry text was set to "0"
			var entryText = App.WaitForElement("TestEntry").GetText();
			Assert.That(entryText, Is.EqualTo("0"));
		}
	}
}
#endif
