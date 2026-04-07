using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue22151 : _IssuesUITest
	{
		public override string Issue => "SearchHandler keyboard does not show on programmatic focus";

		public Issue22151(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.SearchBar)]
		public void SearchHandlerShowSoftInputShouldFocusAndFireEvent()
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
