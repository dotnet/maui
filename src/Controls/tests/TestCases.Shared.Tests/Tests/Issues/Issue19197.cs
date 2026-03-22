#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS
// Test failing on Android.
// The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms Android and iOS.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19197 : _IssuesUITest
	{
		public Issue19197(TestDevice device) : base(device)
		{
		}

		public override string Issue => "AdaptiveTrigger does not work";

		[Test]
		[Category(UITestCategories.Page)]
		public void AdaptiveTriggerWorks()
		{
			App.WaitForElement("WaitForStubControl");
			App.SetOrientationLandscape();
			var text = App.FindElement("WaitForStubControl").GetText();
			Assert.That(text, Is.EqualTo("Horizontal"));
			App.SetOrientationPortrait();
		}
	}
}
#endif