using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla45702 : IssuesUITest
	{
		public Bugzilla45702(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Disabling back press on modal page causes app to crash";
		public override bool ResetMainPage => false;

		protected override void FixtureTeardown()
		{
			base.FixtureTeardown();
			RunningApp.ResetApp();
		}

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public void Issue45702Test()
		{
			RunningApp.WaitForElement("ClickMe");
			RunningApp.Tap("ClickMe");
			RunningApp.WaitForNoElement("Success");
		}
	}
}