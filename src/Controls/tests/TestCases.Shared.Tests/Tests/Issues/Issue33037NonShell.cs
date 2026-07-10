#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33037NonShell : _IssuesUITest
{
	public Issue33037NonShell(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS Large Title display disappears when scrolling in non-Shell NavigationPage";

	[Test]
	[Category(UITestCategories.Navigation)]
	[TestCase("Issue33037ScrollViewButton", "Issue33037ScrollViewScroller", "Issue33037 Direct")]
	[TestCase("Issue33037GridScrollViewButton", "Issue33037GridScrollViewScroller", "Issue33037 Grid")]
	[TestCase("Issue33037ListViewButton", "Issue33037ListViewScroller", "Issue33037 List")]
	[TestCase("Issue33037CollectionViewButton", "Issue33037CollectionViewScroller", "Issue33037 Collection")]
	public void LargeTitleCollapsesToVisibleStandardTitle(string buttonId, string scrollerId, string title)
	{
		App.WaitForElement(buttonId).Click();

		var largeTitle = App.WaitForElement(title);
		var largeTitleRect = largeTitle.GetRect();

		App.ScrollDown(scrollerId);
		App.ScrollDown(scrollerId);

		var collapsedTitle = App.WaitForElement(title, $"Timed out waiting for collapsed title '{title}'");
		var collapsedTitleRect = collapsedTitle.GetRect();

		Assert.That(collapsedTitleRect.Y, Is.LessThan(largeTitleRect.Y),
			$"The '{title}' navigation title should move upward when it collapses.");
		Assert.That(collapsedTitleRect.Height, Is.LessThanOrEqualTo(largeTitleRect.Height),
			$"The '{title}' navigation title should remain visible in the standard navigation bar after collapsing.");

		this.Back();
	}
}
#endif
