using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20911 : _IssuesUITest
{
	public Issue20911(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Updating text in the Entry does not update CursorPosition during the TextChanged event";

	[Test]
	[Category(UITestCategories.Entry)]
	public void VerifyEntryCursorPositionOnTextChanged()
	{
		App.WaitForElement("ValidateEntryCursorPosition");

		App.EnterText("ValidateEntryCursorPosition", "Test");

		App.WaitForElement("ValidateEntryCursorPositionBtn");
		App.Tap("ValidateEntryCursorPositionBtn");

		var cursorPositionStatus = App.FindElement("CursorPositionStatusLabel").GetText();
		Assert.That(cursorPositionStatus, Is.EqualTo("4"));
	}
}