#if ANDROID || IOS //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue4484 : _IssuesUITest
	{
		public Issue4484(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] ImageButton inside NavigationView.TitleView throw exception during device rotation";

		[Test]
		[Category(UITestCategories.ImageButton)]
		[Category(UITestCategories.Compatibility)]
		public void RotatingDeviceDoesntCrashTitleView()
		{
			App.WaitForElement("Instructions");
			App.SetOrientationLandscape();
			App.WaitForElement("Instructions");
			App.SetOrientationPortrait();
			App.WaitForElement("Instructions");
		}
	}
}
#endif