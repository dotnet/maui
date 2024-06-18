#if !ANDROID
using NUnit.Framework;
using UITest.Appium;

namespace UITests
{
	public class Bugzilla32830 : IssuesUITest
	{
		const string Button1 = "button1";
		const string Button2 = "button2";
		const string BottomLabel = "I am visible at the bottom of the page";

		public Bugzilla32830(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Hiding navigation bar causes layouts to shift during navigation";

		[Test]
		[Category(UITestCategories.LifeCycle)]
		[FailsOnAllPlatforms]
		public void Bugzilla32830Test()
		{
			RunningApp.WaitForNoElement(BottomLabel);
			RunningApp.WaitForElement(Button1);
			RunningApp.Tap(Button1);
			RunningApp.WaitForElement(Button2);
			RunningApp.Tap(Button2);
			RunningApp.WaitForNoElement(BottomLabel);
		}
	}
}
#endif