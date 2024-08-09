#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue20723 : _IssuesUITest
	{
		public override string Issue => "[iOS] PullToRefresh activity indicator indicator hidden behind header in CollectionView";

		public Issue20723(TestDevice device) : base(device)
		{
		}

		[Test]
		[Category(UITestCategories.ActivityIndicator)]
		public async Task IndicatorShouldNotBeCoveredByHeader()
		{
			_ = App.WaitForElement("refreshView");

			App.ScrollUp("refreshView", swipePercentage: 1);
			await Task.Delay(500);

			//The test passes if the activity indicator is above the header
			VerifyScreenshot();
		}
	}
}
#endif