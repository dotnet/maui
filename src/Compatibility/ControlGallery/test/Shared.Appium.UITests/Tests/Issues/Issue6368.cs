using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue6368 : IssuesUITest
	{
		public Issue6368(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[CustomRenderer]Crash when navigating back from page with custom renderer control";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Issue6368Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			App.WaitForElement("btnGo");
			App.Click("btnGo");
			App.WaitForElement("GoToNextPage");
			App.Back();
		}
	}
}