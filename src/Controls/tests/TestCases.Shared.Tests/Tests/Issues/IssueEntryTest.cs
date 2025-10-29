using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class IssueEntryTest : _IssuesUITest
{
	public override string Issue => "Entry control text input and clear test";

	public IssueEntryTest(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntryTextInputAndClear()
	{
		// Wait for the Entry to be ready
		App.WaitForElement("TestEntry");
		
		// Tap on the Entry to focus it
		App.Tap("TestEntry");
		
		// Enter text into the Entry
		const string testText = "Hello MAUI";
		App.EnterText("TestEntry", testText);
		
		// Verify the text was entered by checking the Entry's text
		var entryText = App.FindElement("TestEntry").GetText();
		Assert.That(entryText, Is.EqualTo(testText), "Entry should contain the entered text");
		
		// Verify the ResultLabel shows the entered text
		var labelText = App.FindElement("ResultLabel").GetText();
		Assert.That(labelText, Is.EqualTo($"Entry text: {testText}"), "Label should display the entry text");
		
		// Tap the Clear button
		App.Tap("ClearButton");
		
		// Verify the Entry is now empty
		var clearedEntryText = App.FindElement("TestEntry").GetText();
		Assert.That(string.IsNullOrEmpty(clearedEntryText), Is.True, "Entry should be empty after clear");
		
		// Verify the ResultLabel shows empty state
		var clearedLabelText = App.FindElement("ResultLabel").GetText();
		Assert.That(clearedLabelText, Is.EqualTo("Entry is empty"), "Label should show empty state");
	}
}
