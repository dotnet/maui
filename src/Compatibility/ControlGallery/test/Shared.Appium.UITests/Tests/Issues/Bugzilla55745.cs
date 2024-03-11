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
		public void Bugzilla55745Test()
		{
			this.IgnoreIfPlatforms([TestDevice.Android, TestDevice.Mac, TestDevice.Windows]);

			RunningApp.WaitForElement(ButtonId);
			RunningApp.Tap(ButtonId);
			RunningApp.Tap(ButtonId);
		}
	}
}