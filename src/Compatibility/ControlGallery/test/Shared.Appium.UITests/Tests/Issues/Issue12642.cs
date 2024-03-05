using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue12642 : IssuesUITest
	{
		public Issue12642(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] Rapid ShellContent Navigation Causes Blank Screens";

		[Test]
		[Category(UITestCategories.Shell)]
		public void ClickingQuicklyBetweenTopTabsBreaksContent()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap("AutomatedRun");
			RunningApp.WaitForElement("Success");
			RunningApp.Tap("AutomatedRun");
			RunningApp.WaitForElement("Success");
		}
	}
}