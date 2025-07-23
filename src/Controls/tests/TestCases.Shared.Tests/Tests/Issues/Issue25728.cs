using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue25728 : _IssuesUITest
	{
		public Issue25728(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "App crashes when entry bound to float value with fractional format";

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryWithStringFormatShouldNotCrashOnTextInput()
		{
			// Verify the initial state
			App.WaitForElement("TestEntry");
			
			// Clear the entry field and type a new value
			App.Tap("TestEntry");
			App.ClearText();
			
			// Type a simple number - this should trigger the string formatting
			App.EnterText("TestEntry", "5");
			
			// The app should not crash at this point
			// Verify that the entry still exists and is responsive
			App.WaitForElement("TestEntry");
			
			// Verify we can tap the test button without crashes
			App.Tap("TestButton");
			
			// Verify the entry is still there and functional after the button click
			App.WaitForElement("TestEntry");
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryWithStringFormatShouldHandleDecimalNumbers()
		{
			App.WaitForElement("TestEntry");
			
			// Clear and enter a decimal number
			App.Tap("TestEntry");
			App.ClearText();
			App.EnterText("TestEntry", "3.14");
			
			// App should not crash and entry should still be functional
			App.WaitForElement("TestEntry");
			
			// Clear and try another decimal
			App.ClearText();
			App.EnterText("TestEntry", "1.5");
			
			// Verify entry is still responsive
			App.WaitForElement("TestEntry");
		}

		[Test]
		[Category(UITestCategories.Entry)]
		public void EntryWithStringFormatShouldHandleZero()
		{
			App.WaitForElement("TestEntry");
			
			// Clear and enter zero
			App.Tap("TestEntry");
			App.ClearText();
			App.EnterText("TestEntry", "0");
			
			// App should not crash
			App.WaitForElement("TestEntry");
		}
	}
}