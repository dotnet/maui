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
		public void Issue3275Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement(BtnLeakId);
			App.Click(BtnLeakId);
			App.WaitForElement(BtnScrollToId);
			App.Click(BtnScrollToId);
			App.Back();
			App.WaitForElement(BtnLeakId);
		}
	}
}
