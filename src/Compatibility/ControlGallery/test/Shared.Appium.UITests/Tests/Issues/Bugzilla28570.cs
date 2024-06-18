#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla28570 : IssuesUITest
	{
		public Bugzilla28570(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "https://bugzilla.xamarin.com/show_bug.cgi?id=28570";
		public override bool ResetMainPage => false;

		[Test]
		[Category(UITestCategories.Navigation)]
		[FailsOnAndroid("Scroll to end not working.")]
		public void Bugzilla28570Test()
		{
			RunningApp.WaitForElement("Tap");
			RunningApp.Screenshot("At test page");
			RunningApp.Tap("Tap");

			RunningApp.WaitForElement("28570Target");
		}
	}
}
#endif