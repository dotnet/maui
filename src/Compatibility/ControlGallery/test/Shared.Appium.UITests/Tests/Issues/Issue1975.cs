using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue1975 : IssuesUITest
	{
		const string Success = "If you can see this, the test has passed";
		const string Go = "Go";

		public Issue1975(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] ListView throws NRE when grouping enabled and data changed";

		[Test]
		[Category(UITestCategories.ListView)]
		public void ClickPropagatesToOnTouchListener()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap(Go);
			RunningApp.WaitForElement(Success);
		}
	}
}