using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33356 : _IssuesUITest
{
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
		var searchHandler = App.GetShellSearchHandler();
		searchHandler.Tap();
		searchHandler.SendKeys("A");
		App.TapFirstSearchResult(this, searchHandler, "SearchResultName");
		App.WaitForElement("Issue33356CatNameLabel");
		App.TapBackArrow(Device == TestDevice.Android ? "" : "Cats");
		App.WaitForElement("Issue33356CatsCollectionView");
		App.WaitForElement("Abyssinian");
		App.Tap("Abyssinian");
		App.WaitForElement("Issue33356CatNameLabel");
		App.TapBackArrow(Device == TestDevice.Android ? "" : "Cats");
		App.WaitForElement("Issue33356CatsCollectionView");
	}
}