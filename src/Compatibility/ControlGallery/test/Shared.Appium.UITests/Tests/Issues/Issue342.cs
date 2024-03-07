using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue342NoSource : IssuesUITest
	{
		public Issue342NoSource(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NRE when Image is not assigned source";

		[Test]
		[Category(UITestCategories.Page)]
		public void AppDoesntCrashWhenResettingPage()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			RunningApp.WaitForNoElement("Uninitialized image", "Cannot see label");
			RunningApp.Screenshot("All elements present");
		}
	}


	public class Issue342DelayedSource : IssuesUITest
	{
		public Issue342DelayedSource(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NRE when Image is delayed source";

		[Test]
		[Category(UITestCategories.Page)]
		public void AppDoesntCrashWhenResettingPage()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.iOS, TestDevice.Mac]);

			RunningApp.WaitForElement("TestReady");
			RunningApp.Screenshot("Should not crash");
		}
	}
}