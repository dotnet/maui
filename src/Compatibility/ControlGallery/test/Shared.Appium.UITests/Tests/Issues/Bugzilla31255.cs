#if IOS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Bugzilla31255 : IssuesUITest
	{
		public Bugzilla31255(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "Flyout's page Icon cause memory leak after FlyoutPage is popped out by holding on page";
		public override bool ResetMainPage => false;

		[Test]
		[Ignore("The sample is crashing.")]
		[Category(UITestCategories.Navigation)]
		[IgnoredDuringMoveToAppium("The sample is crashing. More information: https://github.com/dotnet/maui/issues/21206")]
		public async Task Bugzilla31255Test()
		{
			RunningApp.Screenshot("I am at Bugzilla 31255");
			await Task.Delay(5000);
			RunningApp.WaitForNoElement("Page1. But Page2 IsAlive = False");
		}
	}
}
#endif