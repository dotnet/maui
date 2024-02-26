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
		public void Issue13616Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("AddItemButtonId");
			App.Click("AddItemButtonId");
			App.WaitForElement("CarouselViewId");
		}
	}
}
