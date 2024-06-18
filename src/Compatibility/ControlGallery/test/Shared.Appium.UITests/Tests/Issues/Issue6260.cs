using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue6260 : IssuesUITest
	{
		const string Text = "If this number keeps increasing test has failed: ";
		readonly string success = Text + "0";

		public Issue6260(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] infinite layout loop";

		[Test]
		[Category(UITestCategories.Layout)]
		[FailsOnAndroid]
		public void NonAppCompatBasicSwitchTest()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(success);
		}
	}
}