using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	internal class EditorTextChangedIOS26 : _IssuesUITest
	{
		public override string Issue => "Editor TextChanged event not firing on iOS 26.1 release build";

		public EditorTextChangedIOS26(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.Editor)]
		public void EditorTextChangedEventShouldFireOnTextInput()
		{
			// Wait for the editor to be visible
			App.WaitForElement("TestEditor");
			
			// Verify initial state
			var initialCount = App.FindElement("EventCountLabel").GetText();
			Assert.That(initialCount, Is.EqualTo("TextChanged event count: 0"));

			// Type text in the editor
			App.Tap("TestEditor");
			App.EnterText("TestEditor", "Hello");

			// Wait a bit for the event to fire
			App.WaitForElement("EventCountLabel");
			
			// Verify that TextChanged event was fired
			var eventCount = App.FindElement("EventCountLabel").GetText();
			Assert.That(eventCount, Does.Contain("TextChanged event count:"));
			
			// The count should be greater than 0, indicating the event fired
			// Extract the number from "TextChanged event count: X"
			var countText = eventCount.Replace("TextChanged event count:", "").Trim();
			int count = int.Parse(countText);
			Assert.That(count, Is.GreaterThan(0), "TextChanged event should have fired at least once");

			// Verify the text was captured
			var lastText = App.FindElement("LastTextLabel").GetText();
			Assert.That(lastText, Does.Contain("Hello"), "Last text should contain the entered text");
		}
	}
}
