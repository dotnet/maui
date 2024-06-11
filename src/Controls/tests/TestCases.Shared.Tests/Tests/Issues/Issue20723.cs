using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue20723 : _IssuesUITest
	{
		public override string Issue => "[iOS] PullToRefresh activity indicator indicator hidden behind header in CollectionView";

		public Issue20723(TestDevice device) : base(device)
		{
		}

		[Test]
		public async Task IndicatorShouldNotBeCoveredByHeader()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.Mac, TestDevice.Windows });
			_ = App.WaitForElement("refreshView");

			App.ScrollUp("refreshView", swipePercentage: 1);
			await Task.Delay(500);

			//The test passes if the activity indicator is above the header
			VerifyScreenshot();
		}
	}
}
