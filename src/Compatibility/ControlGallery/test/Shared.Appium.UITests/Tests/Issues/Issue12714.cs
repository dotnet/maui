using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue12714 : IssuesUITest
	{
		const string Success = "Success";
		const string Show = "Show";

		public Issue12714(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] iOS application suspended at UICollectionViewFlowLayout.PrepareLayout() when using IsVisible = false";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void InitiallyInvisbleCollectionViewSurvivesiOSLayoutNonsense()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(Show);
			RunningApp.Tap(Show);
			RunningApp.WaitForNoElement(Success);
		}
	}
}