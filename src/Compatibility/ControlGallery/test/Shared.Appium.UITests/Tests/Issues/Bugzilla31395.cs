#if IOS
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla31395 : IssuesUITest
	{
		public Bugzilla31395(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Crash when switching MainPage and using a Custom Render";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnIOS]
		public void Bugzilla31395Test()
		{
			RunningApp.WaitForElement("SwitchMainPage");
			Assert.DoesNotThrow(() =>
			{
				RunningApp.Tap("SwitchMainPage");
			});
			RunningApp.WaitForNoElement("Hello");
		}
	}
}
#endif