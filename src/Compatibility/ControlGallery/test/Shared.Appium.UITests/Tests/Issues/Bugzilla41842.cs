using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla41842 : IssuesUITest
	{
		public Bugzilla41842(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Set FlyoutPage.Detail = New Page() twice will crash the application when set FlyoutLayoutBehavior = FlyoutLayoutBehavior.Split";

		[Test]
		[Category(UITestCategories.FlyoutPage)]
		public void Bugzilla41842Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("Success");
		}
	}
}