/*
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue8814 : IssuesUITest
	{
		const string Success = "Success";

		public Issue8814(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] UWP Shell cannot host CollectionView/CarouselView";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Shell)]
		public void CollectionViewInShellShouldBeVisible()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			RunningApp.WaitForNoElement(Success);
		}
	}
}
*/