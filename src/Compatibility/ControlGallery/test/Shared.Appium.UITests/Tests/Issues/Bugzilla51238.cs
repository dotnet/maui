using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla51238 : IssuesUITest
	{
		public Bugzilla51238(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Transparent Grid causes Java.Lang.IllegalStateException: Unable to create layer for Platform_DefaultRenderer";

		[Test]
		[Category(UITestCategories.Layout)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Issue1Test()
		{
			RunningApp.WaitForElement("TapMe");
			RunningApp.Tap("TapMe"); // Crashes the app if the issue isn't fixed
			RunningApp.WaitForElement("TapMe");
		}
	}
}