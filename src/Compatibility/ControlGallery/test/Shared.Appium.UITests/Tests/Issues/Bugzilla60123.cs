using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla60123 : IssuesUITest
	{
		public Bugzilla60123(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Rui's issue";

		[Test]
		[Category(UITestCategories.ListView)]
		public void AppDoesntCrashWhenResettingPage()
		{
			RunningApp.WaitForElement("ListView");
			RunningApp.ScrollDown("ListView");
			RunningApp.WaitForElement("ListView");
		}
	}
}