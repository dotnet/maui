using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla59925 : IssuesUITest
	{
		public Bugzilla59925(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Font size does not change vertical height of Entry on iOS";

		[Test]
		[Category(UITestCategories.Label)]
		public void Issue123456Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Screenshot("I am at Issue 59925");
			RunningApp.WaitForElement("Bigger");
			RunningApp.Screenshot("0");

			RunningApp.Tap("Bigger");
			RunningApp.Screenshot("1");

			RunningApp.Tap("Bigger");
			RunningApp.Screenshot("2");

			RunningApp.Tap("Bigger");
			RunningApp.Screenshot("3");
		}
	}
}