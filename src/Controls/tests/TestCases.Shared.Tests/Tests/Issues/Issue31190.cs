using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31190 : _IssuesUITest
	{
		public Issue31190(TestDevice device) : base(device) { }

		public override string Issue => "[NET10, iOS] SafeArea with SoftInput needs to disable the keyboard auto scroll when there is no scrollview";

		[Test]
		[Category(UITestCategories.Entry)]
		[Category(UITestCategories.Keyboard)]
		public void SafeAreaSoftInputKeyboardAutoScrollTest()
		{
			// Wait for the page to load
			App.WaitForElement("TestEntry");
			App.WaitForElement("StatusLabel");

			// Verify initial state
			var initialStatusText = App.FindElement("StatusLabel").GetText();
			Assert.That(initialStatusText, Is.EqualTo("Tap entry below to test keyboard behavior"));

			// Focus the entry to trigger keyboard
			App.Tap("TestEntry");

			// Wait for keyboard to appear and status to update
			App.WaitForTextToBePresentInElement("StatusLabel", "Entry focused - keyboard should show without double insets");

			// Verify the entry is still visible and the keyboard doesn't cause double insets
			// The test passes if the entry remains accessible and properly positioned
			App.WaitForElement("TestEntry");

			// Tap somewhere else to unfocus and hide keyboard
			App.Tap("StatusLabel");

			// Verify unfocus state
			App.WaitForTextToBePresentInElement("StatusLabel", "Entry unfocused - test completed");
		}
	}
}