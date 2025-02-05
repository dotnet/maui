#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24670 : _IssuesUITest
	{
		public override string Issue => "SearchHandler.Focused event never fires";

		public Issue24670(TestDevice testDevice) : base(testDevice)
		{
		}

		[Test]
		[Category(UITestCategories.Shell)]
		[Category(UITestCategories.SearchBar)]
		public void SearchHandlerFocusAndUnfocusEventsShouldWork()
		{
			App.WaitForElement("searchHandler");
			App.Click("searchHandler");

			// Click the entry below to trigger the unfocused event
			App.Click("entry");

			var focusedLabelText = App.WaitForElement("focusedLabel").GetText();
			var unfocusedLabelText = App.WaitForElement("unfocusedLabel").GetText();

			Assert.That(focusedLabelText, Is.EqualTo("Focused: True"));
			Assert.That(unfocusedLabelText, Is.EqualTo("Unfocused: True"));
		}
	}
}
#endif