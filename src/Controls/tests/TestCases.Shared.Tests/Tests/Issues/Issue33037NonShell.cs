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

		try
		{
			App.WaitForElement(title);

			App.ScrollDown(scrollerId, swipePercentage: 0.8);
			App.ScrollDown(scrollerId, swipePercentage: 0.8);
			App.WaitForElement("Item 40");

			var collapsedTitle = App.WaitForElement(title, $"Timed out waiting for collapsed title '{title}'");
			var collapsedTitleRect = collapsedTitle.GetRect();

			Assert.That(collapsedTitleRect.Height, Is.GreaterThan(0),
				$"The '{title}' navigation title should remain visible in the standard navigation bar after collapsing.");
			Assert.That(collapsedTitleRect.Y, Is.LessThan(130),
				$"The '{title}' navigation title should remain in the navigation bar after collapsing.");
		}
		finally
		{
			this.Back();
		}
	}
}
#endif
