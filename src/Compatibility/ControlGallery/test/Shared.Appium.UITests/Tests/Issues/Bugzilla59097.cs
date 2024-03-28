using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla59097 : IssuesUITest
	{
		public Bugzilla59097(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Calling PopAsync via TapGestureRecognizer causes an application crash";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void Bugzilla59097Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("boxView");
			RunningApp.Tap("boxView");
		}
	}
}