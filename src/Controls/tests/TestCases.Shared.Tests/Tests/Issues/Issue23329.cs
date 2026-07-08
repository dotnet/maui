#if WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue23329 : _IssuesUITest
{
	public Issue23329(TestDevice device)
		: base(device)
	{ }

	public override string Issue => "Entry select all text on refocus does not work on WinUI";

	[Test]
	[Category(UITestCategories.Entry)]
	public void EntrySelectAllOnRefocusReplacesText()
	{
		App.WaitForElement("TextBox");
		App.Click("TextBox");
		App.EnterText("TextBox", "Hello");

		// Blur and refocus twice. The second refocus is where the bug manifests:
		// the Focused handler re-applies the same CursorPosition / SelectionLength
		// values, so the bindable property change tracking would skip propagation
		// to the native TextBox unless the InputView / TextBoxExtensions fix forces
		// a handler update for unchanged values.
		App.Click("OtherElement");
		App.Click("TextBox");
		App.Click("OtherElement");
		App.Click("TextBox");

		// If select-all worked on the second refocus, typing 'X' replaces the
		// entire selected text, so the Entry ends up containing just "X". If the
		// bug is still present, 'X' is inserted at the native caret position
		// (wherever the click landed in "Hello"), producing something like "HelloX".
		App.EnterText("TextBox", "X");

		var text = App.FindElement("TextBox").GetText();
		Assert.That(text, Is.EqualTo("X"));
	}
}
#endif
