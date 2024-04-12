#if ANDROID
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue13616 : IssuesUITest
	{
		public Issue13616(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] After updating XF 5.0.0.1931 getting Java.Lang.IllegalArgumentException: Invalid target position at Java.Interop.JniEnvironment+InstanceMethods.CallVoidMethod";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void Issue13616Test()
		{
			RunningApp.WaitForElement("AddItemButtonId");
			RunningApp.Tap("AddItemButtonId");
			RunningApp.WaitForElement("CarouselViewId");
		}
	}
}
#endif