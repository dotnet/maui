using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla32206 : IssuesUITest
	{
		public Bugzilla32206(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "ContextActions cause memory leak: Page is never destroyed\"";

		[Test]
		[Category(UITestCategories.Page)]
		public void Bugzilla32206Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);
		
			for (var n = 0; n < 10; n++)
			{
				RunningApp.WaitForElement("Push");
				RunningApp.Tap("Push");

				RunningApp.WaitForElement("ListView");
				RunningApp.Back();
			}

			// At this point, the counter can be any value, but it's most likely not zero.
			// Invoking GC once is enough to clean up all garbage data and set counter to zero
			RunningApp.WaitForElement("GC");
			RunningApp.Tap("GC");

			RunningApp.WaitForNoElement("Counter: 0");
		}
	}
}