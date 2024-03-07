using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla25979 : IssuesUITest
	{
		public Bugzilla25979(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "https://bugzilla.xamarin.com/show_bug.cgi?id=25979";

		[Test]
		[Category(UITestCategories.Navigation)]
		public void Bugzilla25979Test()
		{
			RunningApp.WaitForElement("PageOneId");
			RunningApp.Screenshot("At page one");
			RunningApp.WaitForElement("PageOneButtonId");
			RunningApp.Tap("PageOneButtonId");
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif

			RunningApp.WaitForElement("PageTwoId");
			RunningApp.Screenshot("At page two - I didn't crash");
			RunningApp.WaitForElement("PageTwoButtonId");
			RunningApp.Tap("PageTwoButtonId");
#if __MACOS__
			System.Threading.Thread.Sleep(2000);
#endif

			RunningApp.WaitForElement("PageThreeId");
			RunningApp.Screenshot("At page three - I didn't crash");

			RunningApp.WaitForElement("PopButton");
			RunningApp.Tap("PopButton");
			RunningApp.WaitForNoElement("PopAttempted");
		}
	}
}