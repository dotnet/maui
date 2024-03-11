using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla26233 : IssuesUITest
	{
		public Bugzilla26233(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Windows phone crashing when going back to page containing listview with Frame inside ViewCell";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void DoesntCrashOnNavigatingBackToThePage()
		{
			this.IgnoreIfPlatform(TestDevice.Android, "MultiWindowService not implemented.");
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("btnPush");
			RunningApp.Tap("btnPush");
			RunningApp.WaitForElement("back");
			RunningApp.Screenshot("I see the back button");
			RunningApp.Tap("back");
			RunningApp.WaitForElement("btnPush");
		}
	}
}