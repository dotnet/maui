using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla55745 : IssuesUITest
	{
		const string ButtonId = "button";

		public Bugzilla55745(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] NRE in ListView with HasUnevenRows=true after changing content and rebinding";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void Bugzilla55745Test()
		{
			RunningApp.WaitForElement(ButtonId);
			RunningApp.Tap(ButtonId);
			RunningApp.Tap(ButtonId);
		}
	}
}