using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue12374 : IssuesUITest
	{
		public Issue12374(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] [iOS] CollectionView EmptyView causes the application to crash";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void Issue12374Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("TestReady");
			RunningApp.Tap("RemoveItems");
			RunningApp.Tap("AddItems");
			RunningApp.Tap("RemoveItems");
			RunningApp.Screenshot("CollectionViewWithEmptyView");
		}
	}
}