using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue3275 : IssuesUITest
	{
		readonly string BtnLeakId = "btnLeak";
		readonly string BtnScrollToId = "btnScrollTo";

		public Issue3275(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "For ListView in Recycle mode ScrollTo causes cell leak and in some cases NRE";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void Issue3275Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(BtnLeakId);
			RunningApp.Tap(BtnLeakId);
			RunningApp.WaitForElement(BtnScrollToId);
			RunningApp.Tap(BtnScrollToId);
			RunningApp.Back();
			RunningApp.WaitForElement(BtnLeakId);
		}
	}
}
