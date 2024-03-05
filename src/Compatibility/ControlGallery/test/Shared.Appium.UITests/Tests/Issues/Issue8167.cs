using NUnit.Framework;
using UITest.Appium;

namespace UITests.Tests.Issues
{
	public class Issue8167 : IssuesUITest
	{
		const string Run = "Update Text";
		const string Success = "Success";

		public Issue8167(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] XF 4.3 UWP Crash - Element not found";

		[Test]
		public void ThreadpoolBindingUpdateShouldNotCrash()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(Run);
			RunningApp.Tap(Run);
			RunningApp.WaitForNoElement(Success);
		}
	}
}
