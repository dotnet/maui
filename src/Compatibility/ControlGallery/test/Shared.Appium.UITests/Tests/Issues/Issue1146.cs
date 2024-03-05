using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue1146 : IssuesUITest
	{
		public Issue1146(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Disabled Switch in Button Gallery not rendering on all devices";

		[Test]
		[Category(UITestCategories.Switch)]
		public void TestSwitchDisable()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("switch");
			RunningApp.Screenshot("Is the button here?");
		}
	}
}