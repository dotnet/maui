/*
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue9794 : IssuesUITest
	{
		public Issue9794(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Tabbar Disappears with linker";
		
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Shell)]
		public void EnsureTabBarStaysVisibleAfterPoppingPage()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap("GoForward");
			RunningApp.Back();
			RunningApp.Tap("tab2");
			RunningApp.Tap("tab1");
			RunningApp.Tap("tab2");
			RunningApp.Tap("tab1");
			RunningApp.Tap("tab2");
			RunningApp.Tap("tab1");
		}
	}
}
*/