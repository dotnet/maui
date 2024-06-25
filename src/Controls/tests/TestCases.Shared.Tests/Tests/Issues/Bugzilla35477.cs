using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla35477 : _IssuesUITest
	{
		public Bugzilla35477(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Tapped event does not fire when added to Frame in Android AppCompat";

		[Test]
		[Category(UITestCategories.Frame)]
		[Category(UITestCategories.Gestures)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnIOS]
		[FailsOnMac]
		public void TapGestureFiresOnFrame()
		{
			App.WaitForNoElement("No taps yet");
			App.WaitForElement("TapHere");
			App.Tap("TapHere");
			App.WaitForNoElement("Frame was tapped");
		}
	}
}