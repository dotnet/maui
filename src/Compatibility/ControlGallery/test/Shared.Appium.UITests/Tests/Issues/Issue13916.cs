using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue13916 : IssuesUITest
	{
		public Issue13916(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] iOS Application crashes on Back press when navigated to using GoToAsync with \"//\" or \"///\" route if 2 or more things are removed from the navigation stack";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void RemovingMoreThanOneInnerPageAndThenPushingAPageCrashes()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.Tap("ClickMe1");
			RunningApp.Tap("ClickMe2");
			RunningApp.Tap("ClickMe3");
			RunningApp.WaitForElement("Success");
		}
	}
}