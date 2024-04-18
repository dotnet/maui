using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues;

public class Issue20631 : _IssuesUITest
{
	public Issue20631(TestDevice device) : base(device) { }

	public override string Issue => "Editor does not scroll when cursor goes behind keyboard";

	[Test]
	[Category(UITestCategories.Entry)]
	public void TestEditorKeyboardScrolling ()
	{
		this.IgnoreIfPlatforms(new[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows },
			"This is an iOS Keyboard Scrolling issue.");

		var app = App as AppiumApp;
		if (app is null)
		{
			return;
		}

		try
		{
			const string editor = "IssueEditor";

			var editorElement = app.WaitForElement(editor);
			var editorRect = editorElement.GetRect();

			// Focus the editor
			app.Click(editor);
			// Scroll to the bottom of the text
			for (int i = 0; i < 5; i++)
			{
				app.ScrollDown(editor, swipePercentage: 0.5, swipeSpeed: 50);
			}

			// We should now see "Line 99" at the bottom of the editor (which is the last line)

			// Not tap right above the keyboard, it should focus the last visible line in the editor
			var osKeyboardLocation = KeyboardScrolling.FindiOSKeyboardLocation(app.Driver)!.Value;
			const int defaultSizeAccessoryView = 44;
			var lastLineY = osKeyboardLocation.Y - defaultSizeAccessoryView - 5;
			var lastLineX = editorRect.Right - 10;
			app.Click(lastLineX, lastLineY);

			// Get the text of the editor
			var initialText = editorElement.GetText();

			// Type hello and get the new text
			app.EnterText(editor, "\nHello");
			var newText = editorElement.GetText();
			var expectedText = $"{initialText}\nHello";
			// If the last visible line is the last line for real, the text should now end with "\nHello"
			Assert.AreEqual(expectedText, newText, "Editor text did not update as expected, meaning keyboard insets are not working properly.");

			// Now tap again right above the keyboard, it should focus the last visible line in the editor which should be "Hello"
			app.Click(lastLineX, lastLineY);
			// Enter "\nWorld" and get the new text
			app.EnterText(editor, "\nWorld");
			newText = editorElement.GetText();
			expectedText = $"{expectedText}\nWorld";
			// If the last visible line is the last line for real, the text should now end with "\nWorld"
			Assert.AreEqual(expectedText, newText, "Editor text did not update as expected, meaning scrolling is not following new lines.");
		}
		finally
		{
			KeyboardScrolling.HideKeyboard(app, app.Driver, true);
		}
	}
}