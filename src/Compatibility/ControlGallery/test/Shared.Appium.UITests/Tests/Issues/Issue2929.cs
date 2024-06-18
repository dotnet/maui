using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue2929 : IssuesUITest
	{
		const string Success = "Success";
		const string Go = "Go";

		public Issue2929(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[UWP] ListView with null ItemsSource crashes on 3.0.0.530893";
	
		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void NullItemSourceDoesNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac]);

			// If we can see the Success label, it means we didn't crash. 
			RunningApp.WaitForElement(Success);
		}

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void SettingItemsSourceToNullDoesNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac]);
			
			RunningApp.WaitForElement(Go);
			RunningApp.Tap(Go);

			// If we can see the Success label, it means we didn't crash. 
			RunningApp.WaitForElement(Success);
		}
	}
}