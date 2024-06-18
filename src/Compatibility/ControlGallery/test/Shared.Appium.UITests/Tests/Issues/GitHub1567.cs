using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class GitHub1567 : IssuesUITest
	{
		public GitHub1567(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "NRE using TapGestureRecognizer on cell with HasUnevenRows";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void GitHub1567Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("btnFillData");
			RunningApp.Tap("btnFillData");
		}
	}
}