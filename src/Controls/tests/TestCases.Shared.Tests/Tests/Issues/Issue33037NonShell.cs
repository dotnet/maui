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
	[TestCase("Issue33037ScrollViewButton", "Issue33037ScrollViewScroller", "Issue33037 Direct", "DirectScrollView")]
	[TestCase("Issue33037GridScrollViewButton", "Issue33037GridScrollViewScroller", "Issue33037 Grid", "GridScrollView")]
	[TestCase("Issue33037ContentViewGridScrollViewButton", "Issue33037ContentViewGridScrollViewScroller", "Issue33037 Wrapped", "ContentViewGridScrollView")]
	[TestCase("Issue33037DynamicContentViewGridScrollViewButton", "Issue33037DynamicContentViewGridScrollViewScroller", "Issue33037 Dynamic", "DynamicContentViewGridScrollView")]
	[TestCase("Issue33037ListViewButton", "Issue33037ListViewScroller", "Issue33037 List", "ListView")]
	[TestCase("Issue33037CollectionViewButton", "Issue33037CollectionViewScroller", "Issue33037 Collection", "CollectionView")]
	public void LargeTitleCollapsesToVisibleStandardTitle(string buttonId, string scrollerId, string title, string scenarioName)
	{
		App.WaitForElement(buttonId).Click();

		try
		{
			App.WaitForElement(title);
			App.WaitForElement(scrollerId);

			App.ScrollDown(scrollerId, swipePercentage: 0.8);
			App.ScrollDown(scrollerId, swipePercentage: 0.8);
			App.WaitForElement("Item 40");

			var collapsedTitle = App.WaitForElement(title, $"Timed out waiting for collapsed title '{title}'");
			var collapsedTitleRect = collapsedTitle.GetRect();

			Assert.That(collapsedTitleRect.Height, Is.GreaterThan(0),
				$"The '{title}' navigation title should remain visible in the standard navigation bar after collapsing.");
			Assert.That(collapsedTitleRect.Y, Is.LessThan(130),
				$"The '{title}' navigation title should remain in the navigation bar after collapsing.");

			VerifyScreenshot($"Issue33037NonShell_{scenarioName}_AfterScroll", tolerance: 0.8, retryTimeout: TimeSpan.FromSeconds(2));
		}
		finally
		{
			App.Back();
		}
	}
}
#endif
