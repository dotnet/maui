using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla58645 : IssuesUITest
	{
		const string ButtonId = "button";

		public Bugzilla58645(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] NRE Thrown When ListView Items Are Replaced By Items With a Different Template";

		[Test]
		[Category(UITestCategories.ListView)]
		[FailsOnIOS]
		public void Bugzilla58645Test()
		{
			RunningApp.WaitForElement(ButtonId);
			RunningApp.Tap(ButtonId);
		}
	}
}