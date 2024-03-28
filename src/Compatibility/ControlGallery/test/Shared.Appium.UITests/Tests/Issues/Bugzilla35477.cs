using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla35477 : IssuesUITest
	{
		public Bugzilla35477(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Tapped event does not fire when added to Frame in Android AppCompat";

		[Test]
		[Category(UITestCategories.Frame)]
		[Category(UITestCategories.Gestures)]
		[FailsOnIOS]
		public void TapGestureFiresOnFrame()
		{
			RunningApp.WaitForNoElement("No taps yet");
			RunningApp.WaitForElement("TapHere");
			RunningApp.Tap("TapHere");
			RunningApp.WaitForNoElement("Frame was tapped");
		}
	}
}