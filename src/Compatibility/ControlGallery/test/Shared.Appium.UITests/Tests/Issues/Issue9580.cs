using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue9580 : IssuesUITest
	{
		const string Success = "Success";
		const string Test9580 = "9580";

		public Issue9580(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CollectionView - iOS - Crash when adding first item to empty item group";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void AllEmptyGroupsShouldNotCrashOnItemInsert()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(Test9580);
			RunningApp.Tap(Test9580);
			RunningApp.WaitForElement(Success);
		}
	}
}