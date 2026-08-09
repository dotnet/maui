using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34422 : _IssuesUITest
{
	public Issue34422(TestDevice device) : base(device) { }

	public override string Issue => "SearchBar clear button still appears on MacCatalyst after clearing input";

	[Test, Order(1)]
	[Category(UITestCategories.SearchBar)]
	public void SearchBarClearButtonShouldBeVisibleWithText()
	{
		App.WaitForElement("TestSearchBar");
        App.Tap("TestSearchBar");
		App.Tap("AddTextButton");
		VerifyScreenshot();
	}

	[Test, Order(2)]
	[Category(UITestCategories.SearchBar)]
	public void SearchBarClearButtonShouldDisappearAfterClearingInput()
	{
		// First add text so the clear button appears
		App.WaitForElement("TestSearchBar");
        App.Tap("TestSearchBar");
		App.Tap("AddTextButton");
		App.Tap("ClearButton");
		VerifyScreenshot();
	}
}
