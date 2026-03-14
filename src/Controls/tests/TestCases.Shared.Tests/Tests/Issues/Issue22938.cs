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

		// Wait for modal to appear — the Entry is the first focusable element
		App.WaitForElement("ModalEntry");

		// Press Enter — with the fix, focus is on ModalEntry (an Entry control),
		// so Enter should NOT activate MainPageButton on the page beneath
		App.PressEnter();

		// Close the modal by tapping the close button explicitly
		App.Tap("CloseModalButton");

		// Wait for main page to reappear
		App.WaitForElement("ClickCountLabel");

		// Verify the main page button was NOT clicked by the Enter key
		var clickCount = App.WaitForElement("ClickCountLabel").GetText();
		Assert.That(clickCount, Is.EqualTo("0"),
			"Enter key should not activate buttons on the page beneath a modal");
	}

	[Test]
	[Category(UITestCategories.Focus)]
	public void TabShouldNotCycleToBehindModal()
	{
		App.WaitForElement("OpenModalButton");

		// Open the modal page (uses semi-transparent background so underlying page stays in tree)
		App.Tap("OpenModalButton");

		// Wait for modal to appear
		App.WaitForElement("ModalEntry");

		// Tab through many times to attempt cycling past the modal into the underlying page.
		// The modal has 2 focusable elements (Entry + CloseModalButton), so 10 tabs should
		// cycle through them multiple times. If focus escapes to the underlying page,
		// one of the tabs could land on MainPageButton.
		for (int i = 0; i < 10; i++)
		{
			App.SendTabKey();
		}

		// Now press Enter. If focus leaked to MainPageButton, this would click it.
		App.PressEnter();

		// Close the modal
		App.Tap("CloseModalButton");

		// Wait for main page to reappear
		App.WaitForElement("ClickCountLabel");

		// Verify the main page button was NOT clicked during Tab cycling
		var clickCount = App.WaitForElement("ClickCountLabel").GetText();
		Assert.That(clickCount, Is.EqualTo("0"),
			"Tab key should not cycle focus to buttons on the page beneath a modal");
	}
}
