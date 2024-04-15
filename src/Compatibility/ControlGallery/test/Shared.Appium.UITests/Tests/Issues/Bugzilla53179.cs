using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla53179 : IssuesUITest
	{
		public Bugzilla53179(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "PopAsync crashing after RemovePage when support packages are updated to 25.1.1";

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnAndroid]
		public void Bugzilla53179Test()
		{
			this.IgnoreIfPlatforms([TestDevice.iOS, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement("Next Page");
			RunningApp.Tap("Next Page");

			RunningApp.WaitForElement("Next Page");
			RunningApp.Tap("Next Page");

			RunningApp.WaitForElement("Next Page");
			RunningApp.Tap("Next Page");

			RunningApp.WaitForElement("Remove previous pages");
			RunningApp.Tap("Remove previous pages");

			RunningApp.WaitForElement("Back");
			RunningApp.Tap("Back");
		}
	}
}