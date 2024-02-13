using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue20535 : _IssuesUITest
	{
		public Issue20535(TestDevice device) : base(device) { }

		public override string Issue => "Re-enable/move TrackColorInitializesCorrectly/TrackColorUpdatesCorrectly to Appium";

		[Test]
		public void SwitchTrackColorTest()
		{
			App.WaitForElement("WaitForStubControl");
			App.Click("UpdateOnColorSwitch");
			VerifyScreenshot();
		}
	}
}