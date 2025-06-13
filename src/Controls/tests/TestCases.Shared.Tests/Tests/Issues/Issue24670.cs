using Xunit;
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

		[Fact]
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

			Assert.Equal("Focused: True", focusedLabelText);
			Assert.Equal("Unfocused: True", unfocusedLabelText);
		}
	}
}