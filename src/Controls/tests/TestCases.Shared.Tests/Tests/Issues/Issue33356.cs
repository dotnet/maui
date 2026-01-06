using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33356 : _IssuesUITest
{
#if ANDROID
	const string BackButtonIdentifier = "";
#else
	const string BackButtonIdentifier = "Cats";
#endif

	public Issue33356(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "Navigate should occur when an item got selected";

	[Test]
	[Category(UITestCategories.Shell)]
	public void Issue33356NavigateShouldOccur()
	{
		App.WaitForElement("Issue33356CatsCollectionView");
		var searchHander = App.GetShellSearchHandler();
		searchHander.Tap();
		searchHander.SendKeys("A");
#if ANDROID // Android does not support selecting elements in SearchHandler's results so used tap coordinates
		var y = searchHander.GetRect().Y + searchHander.GetRect().Height;
		App.TapCoordinates(searchHander.GetRect().X + 10, y + 10);
#else
		var searchResults = App.FindElements("SearchResultName");
		searchResults.First().Tap();
#endif
		App.WaitForElement("Issue33356CatNameLabel");
		App.TapBackArrow(BackButtonIdentifier);
		App.WaitForElement("Issue33356CatsCollectionView");
		App.WaitForElement("Abyssinian");
		App.Tap("Abyssinian");
		App.WaitForElement("Issue33356CatNameLabel");
		App.TapBackArrow(BackButtonIdentifier);
		App.WaitForElement("Issue33356CatsCollectionView");
	}
}