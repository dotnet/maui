using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue34930 : _IssuesUITest
	{
		public override string Issue => "SearchHandler.ShowSoftInputAsync() does not focus the SearchHandler on Windows";

		public Issue34930(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.SearchBar)]
		public void SearchHandlerShowSoftInputShouldFocusSearchHandlerOnWindows()
		{
			App.WaitForElement("ShowKeyboardButton");
			App.Tap("ShowKeyboardButton");

			var isFocused = App.WaitForElement("isFocusedLabel").GetText();
			Assert.That(isFocused, Is.EqualTo("IsFocused: True"));

			var focusedEvent = App.WaitForElement("focusedEventLabel").GetText();
			Assert.That(focusedEvent, Is.EqualTo("FocusedEvent: True"));
		}
	}
}
