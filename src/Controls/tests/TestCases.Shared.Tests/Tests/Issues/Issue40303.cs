#if TEST_FAILS_ON_WINDOWS // Windows shows ShellContent choices in a dropdown when Library is selected, so WaitForNoElement sees Manga and cannot assert that the top strip is hidden.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue40303 : _IssuesUITest
{
	public Issue40303(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Android][Shell] Handler-based Shell shows a Tab's top tab strip over a pushed page after switching bottom tabs, and the strip stays tappable";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void TopTabStripRemainsHiddenOverPushedPageAfterSwitchingBottomTabs()
	{
		App.WaitForElement("Issue40303OpenDetails");
		App.Tap("Issue40303OpenDetails");
		App.WaitForElement("Issue40303DetailsLabel");
		WaitForMangaTabToBeHidden();

		App.Tap("Discover");
		App.WaitForElement("Issue40303DiscoverLabel");
		App.Tap("Library");
		App.WaitForElement("Issue40303DetailsLabel");

		WaitForMangaTabToBeHidden();
	}

	void WaitForMangaTabToBeHidden()
	{
#if ANDROID
		App.WaitForNoElement(
			AppiumQuery.ByXPath("//*[@text='Manga' or @text='MANGA']"),
			"The Manga ShellContent tab should remain hidden while the details page is pushed.");
#else
		App.WaitForNoElement(
			"Issue40303MangaTab",
			"The Manga ShellContent tab should remain hidden while the details page is pushed.");
#endif
	}
}
#endif
