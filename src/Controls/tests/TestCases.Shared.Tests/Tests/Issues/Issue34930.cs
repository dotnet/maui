using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue34930 : _IssuesUITest
	{
		public override string Issue => "SearchHandler ShowSoftInputAsync does not focus the SearchHandler";

		public Issue34930(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.SoftInput)]
		public void SearchHandlerShowSoftInputShouldFocusSearchHandlerOnWindows()
		{
			App.WaitForElement("ShowKeyboardButton");
			App.Tap("ShowKeyboardButton");

			App.WaitForTextToBePresentInElement("IsFocusedLabel", "IsFocused: True");
			var isFocused = App.WaitForElement("IsFocusedLabel").GetText();
			Assert.That(isFocused, Is.EqualTo("IsFocused: True"));

			App.WaitForTextToBePresentInElement("FocusedEventLabel", "FocusedEvent: True");
			var focusedEvent = App.WaitForElement("FocusedEventLabel").GetText();
			Assert.That(focusedEvent, Is.EqualTo("FocusedEvent: True"));
		}

		[Test]
		[Category(UITestCategories.SoftInput)]
		public void SearchHandlerHideSoftInputShouldUnfocusSearchHandlerOnWindows()
		{
			App.WaitForElement("ShowKeyboardButton");
			App.Tap("ShowKeyboardButton");
			App.WaitForTextToBePresentInElement("IsFocusedLabel", "IsFocused: True");

			App.Tap("HideKeyboardButton");
			App.WaitForTextToBePresentInElement("IsFocusedLabel", "IsFocused: False");
			var isFocused = App.WaitForElement("IsFocusedLabel").GetText();
			Assert.That(isFocused, Is.EqualTo("IsFocused: False"));
		}
	}
}
