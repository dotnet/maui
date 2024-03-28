using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue8899 : IssuesUITest
	{
		const string Go = "Go";
		const string Success = "Success";

		public Issue8899(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Clearing CollectionView IsGrouped=\"True\" crashes application iOS ";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnIOS]
		public void ClearingGroupedCollectionViewShouldNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(Go);
			RunningApp.Tap(Go);
			RunningApp.WaitForElement(Success);
		}
	}
}