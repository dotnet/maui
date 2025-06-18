#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue1942 : _IssuesUITest
	{
		const string ClickMeString = "CLICK ME";

		public Issue1942(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Attached Touch Listener events do not dispatch to immediate parent Grid Renderer View on Android when Child fakes handled";

		[Test]
		[Category(UITestCategories.Button)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroidWhenRunningOnXamarinUITest]
		public void ClickPropagatesToOnTouchListener()
		{
			App.WaitForElement(ClickMeString);
			App.Tap(ClickMeString);
		}
	}
}
#endif