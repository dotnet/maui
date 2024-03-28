using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue1601 : IssuesUITest
	{
		public Issue1601(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Exception thrown when `Removing Content Using LayoutCompression"; 
		
		[Test]
		[Category(UITestCategories.Layout)]
		[FailsOnIOS]
		public void Issue1601Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Screenshot("Start G1601");
			RunningApp.WaitForElement("CrashButton");
			RunningApp.Screenshot("I see the button");
			RunningApp.Tap("CrashButton");
			RunningApp.Screenshot("Didn't crash!");
		}
	}
}