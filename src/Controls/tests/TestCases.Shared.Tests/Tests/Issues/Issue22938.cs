using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22938 : _IssuesUITest
{
	public Issue22938(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Keyboard focus does not shift to a newly opened modal page";

	[Test]
	[Category(UITestCategories.Focus)]
	public void ModalPageShouldReceiveKeyboardFocus()
	{
		App.WaitForElement("OpenModalButton");

		// Open the modal page
		App.Tap("OpenModalButton");

		// Wait for modal to appear
		App.WaitForElement("ModalPageLabel");

		// Press Enter — if focus is correctly on the modal, this should NOT
		// activate the MainPage button (click count should stay at 0)
		App.PressEnter();

		// Close the modal
		App.Tap("CloseModalButton");

		// Wait for main page to reappear
		App.WaitForElement("ClickCountLabel");

		// Verify the main page button was NOT clicked by the Enter key
		var clickCount = App.WaitForElement("ClickCountLabel").GetText();
		Assert.That(clickCount, Is.EqualTo("0"),
			"Enter key should not activate buttons on the page beneath a modal");
	}
}
