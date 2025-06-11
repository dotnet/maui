using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue28419 : _IssuesUITest
{
	public Issue28419(TestDevice device) : base(device) { }

	public override string Issue => "SearchBar focus/unfocus do not fire on Windows";

	[Test]
	[Category(UITestCategories.SearchBar)]
	public void SearchBarShouldTriggerFocusedAndUnFocusedEvents()
	{
		App.WaitForElement("SearchBar");
		App.Tap("SearchBar");
		App.WaitForElement("SearchBar Focused");
		App.Tap("Entry");
		App.WaitForElement("SearchBar Unfocused");
	}
}