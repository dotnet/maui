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
		public override bool ResetMainPage => false;

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			RunningApp.Back();
		}

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void ClickPropagatesToOnTouchListener()
		{
			RunningApp.Tap(Go);
			RunningApp.WaitForElement(Success);
		}
	}
}