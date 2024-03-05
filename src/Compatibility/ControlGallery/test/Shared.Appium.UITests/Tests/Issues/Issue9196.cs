using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue9196 : IssuesUITest
	{
		const string Success = "Success";

		public Issue9196(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [iOS] CollectionView EmptyView causes the application to crash";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void EmptyViewShouldNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForNoElement(Success);
		}
	}
}