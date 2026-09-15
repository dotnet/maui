using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20348 : _IssuesUITest
{
	public override string Issue => "SearchBar text incorrectly copied between multiple SearchBars on Android after back navigation";

	public Issue20348(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void SearchBarTextShouldNotBleedToOtherInstances()
	{
		App.WaitForElement("FirstSearchBar");
		App.WaitForElement("SecondSearchBar");

		App.EnterText("SecondSearchBar", "TestText");
		App.Tap("NavigateButton");

		App.WaitForElement("SecondPageLabel");
		this.Back();
		App.WaitForElement("FirstSearchBarText");

		var result = App.FindElement("FirstSearchBarText").GetText();
		Assert.That(result, Is.EqualTo("Pass"),
			"First SearchBar should be empty after back navigation — Android state save/restore should not bleed text from other SearchBar instances");
	}
}
