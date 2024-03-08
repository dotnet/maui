using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla39489 : IssuesUITest
	{
		public Bugzilla39489(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Memory leak when using NavigationPage with Maps";

		[Test]
		[Category(UITestCategories.Maps)]
		[Category(UITestCategories.Performance)]
		public async Task AppDoesntCrashWhenResettingPage()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			// Original bug report (https://bugzilla.xamarin.com/show_bug.cgi?id=39489) had a crash (OOM) after 25-30
			// page loads. Obviously it's going to depend heavily on the device and amount of available memory, but
			// if this starts failing before 50 we'll know we've sprung another serious leak
			int iterations = 50;

			for (int n = 0; n < iterations; n++)
			{
				RunningApp.WaitForElement("NewPage");
				RunningApp.Tap("NewPage");
				RunningApp.WaitForElement("NewPage");
				await Task.Delay(1000);
				RunningApp.Back();
			}
		}
	}
}