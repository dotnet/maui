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
		public void ClickPropagatesToOnTouchListener()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap(ClickMeString);
		}
	}
}