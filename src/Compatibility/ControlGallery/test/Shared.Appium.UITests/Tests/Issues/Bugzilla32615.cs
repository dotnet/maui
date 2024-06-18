using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla32615 : IssuesUITest
	{
		public Bugzilla32615(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "OnAppearing is not called on previous page when modal page is popped";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public async Task Bugzilla32615Test()
		{
			RunningApp.Tap("btnModal");
			RunningApp.Tap("btnPop");
			await Task.Delay(1000);
			RunningApp.WaitForNoElement("1");
		}
	}
}