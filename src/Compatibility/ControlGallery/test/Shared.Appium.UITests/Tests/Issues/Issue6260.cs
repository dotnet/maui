using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue6260 : IssuesUITest
	{
		const string text = "If this number keeps increasing test has failed: ";
		string success = text + "0";

		public Issue6260(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] infinite layout loop";

		[Test]
		[Category(UITestCategories.Layout)]
		public void NonAppCompatBasicSwitchTest()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(success);
		}
	}
}