using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30771 : _IssuesUITest
{
	public Issue30771(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "SearchHandler overlaps title and title view";

	[Test]
	[Category(UITestCategories.Shell)]
	public void SearchHandlerShouldNotOverlap()
	{
		App.WaitForElement("SearchEntry");
		App.EnterText("Search here", "Test Search");
		App.EnterText("SearchEntry", "Test Search");
		App.WaitForElement(() =>
		{
			var entry = App.FindElement("SearchEntry");
			var searchBounds = App.GetShellSearchHandler().GetRect();
			var entryBounds = entry.GetRect();
			return searchBounds.Height > 0 && entryBounds.Height > 0 && searchBounds.Bottom <= entryBounds.Top
				? entry : null;
		}, "The native SearchHandler overlaps the page content after focus changes");
		VerifyScreenshot();
	}
}
