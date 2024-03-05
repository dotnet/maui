using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue3089 : IssuesUITest
	{
		const string Reload = "reload";
		const string Success = "success";

		public Issue3089(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "TextCell text doesn't change when using Recycling on ListViews";

		[Test]
		[Category(UITestCategories.ListView)]
		public void ResettingItemsOnRecycledListViewKeepsOldText()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap(Reload);
			RunningApp.WaitForNoElement(Success);
		}
	}
}