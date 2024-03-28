using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Issue5951 : IssuesUITest
	{
		public Issue5951(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "App Crashes On Shadow Effect's OnDetached On Button That's Never Visible";

		[Test]
		[Category(UITestCategories.Button)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Issue5951Test()
		{
			RunningApp.Tap("Push page");
			RunningApp.WaitForElement("Push page");
			RunningApp.WaitForNoElement("Success");
		}
	}
}