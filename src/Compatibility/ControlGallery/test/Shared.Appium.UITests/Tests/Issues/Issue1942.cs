#if ANDROID
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1942 : IssuesUITest
	{
		const string ClickMeString = "CLICK ME";

		public Issue1942(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] Attached Touch Listener events do not dispatch to immediate parent Grid Renderer View on Android when Child fakes handled";

		[Test]
		[Category(UITestCategories.Button)]
		[FailsOnAndroid]
		public void ClickPropagatesToOnTouchListener()
		{
			RunningApp.Tap(ClickMeString);
		}
	}
}
#endif