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
		if (App.GetTestDevice() == TestDevice.iOS)
		{
			const string entryText = "Type text hereTest Search";
			App.ClearText("SearchEntry");
			App.EnterText("SearchEntry", entryText);
			App.RetryAssert(() => Assert.That(App.FindElement("SearchEntry").GetText(), Is.EqualTo(entryText)));
		}
		else
		{
			App.EnterText("SearchEntry", "Test Search");
		}
		App.RetryAssert(AssertSearchHandlerDoesNotOverlap);
		VerifyScreenshot();
	}

	void AssertSearchHandlerDoesNotOverlap()
	{
		var searchHandler = App.GetShellSearchHandler();
		Assert.That(searchHandler, Is.Not.Null, "The native SearchHandler was not found");
		var searchBounds = searchHandler.GetRect();
		Assert.That(searchBounds.Width, Is.GreaterThan(0));
		Assert.That(searchBounds.Height, Is.GreaterThan(0));

		if (App.GetTestDevice() is TestDevice.iOS or TestDevice.Mac)
		{
			var title = App.FindElement(AppiumQuery.ByXPath(
				"//XCUIElementTypeNavigationBar/XCUIElementTypeStaticText[@label='Home']"));
			Assert.That(title, Is.Not.Null, "The native navigation title was not found");
			var titleBounds = title.GetRect();
			Assert.That(titleBounds.Width, Is.GreaterThan(0));
			Assert.That(titleBounds.Height, Is.GreaterThan(0));
			Assert.That(
				titleBounds.Bottom <= searchBounds.Top || searchBounds.Bottom <= titleBounds.Top ||
				titleBounds.Right <= searchBounds.Left || searchBounds.Right <= titleBounds.Left,
				Is.True, $"The native title {titleBounds} overlaps the SearchHandler {searchBounds} after focus changes.");
		}

		// The root Entry fills the iOS window; its frame is not the bounds of its centered text.
		if (App.GetTestDevice() != TestDevice.iOS)
		{
			var entry = App.FindElement("SearchEntry");
			Assert.That(entry, Is.Not.Null, "The page Entry was not found");
			var entryBounds = entry.GetRect();
			Assert.That(entryBounds.Height, Is.GreaterThan(0));
			Assert.That(searchBounds.Bottom, Is.LessThanOrEqualTo(entryBounds.Top),
				$"The native SearchHandler {searchBounds} overlaps the page content {entryBounds} after focus changes.");
		}
	}
}
