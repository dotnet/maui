using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32709 : _IssuesUITest
	{
		public Issue32709(TestDevice device) : base(device)
		{
		}

		public override string Issue => "App crashes when TimePicker has a TimeSelected event defined and the Time value is null";

		[Test]
		[Category(UITestCategories.TimePicker)]
		public void TimePickerWithNullTimeAndTimeSelectedEventShouldNotCrash()
		{
			// Wait for page to load
			App.WaitForElement("StatusLabel");
			
			// Verify both TimePickers are present (this alone would crash before the fix)
			App.WaitForElement("TimePicker1");
			App.WaitForElement("TimePicker2");
			
			// Verify status label shows "Ready"
			var statusText = App.FindElement("StatusLabel").GetText();
			Assert.That(statusText, Is.EqualTo("Ready"), "Status should be 'Ready' indicating no crash occurred");
			
			// Click the test button to trigger time changes
			App.Tap("TestButton");
			
			// Wait for status to update
			App.WaitForElement("StatusLabel");
			
			// Verify the time changes were handled without crash
			statusText = App.FindElement("StatusLabel").GetText();
			Assert.That(statusText, Does.Contain("Button clicked"), "Status should update after button click");
		}
	}
}
