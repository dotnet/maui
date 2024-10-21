#if !ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Bugzilla32830 : _IssuesUITest
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
		[Category(UITestCategories.Compatibility)]
		[FailsOnAllPlatforms]
		public void Bugzilla32830Test()
		{
			App.WaitForNoElement(BottomLabel);
			App.WaitForElement(Button1);
			App.Tap(Button1);
			App.WaitForElement(Button2);
			App.Tap(Button2);
			App.WaitForNoElement(BottomLabel);
		}
	}
}
#endif