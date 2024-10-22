using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2948 : _IssuesUITest
{
	public Issue2948(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "FlyoutPage Detail is interactive even when Flyout is open when in Landscape";

	// [Test]
	// [Category(UITestCategories.FlyoutPage)]
	// 	public void Issue2948Test()
	// 	{
	// 		RunningApp.Screenshot("I am at Issue 2948");
	// 		RunningApp.SetOrientationLandscape();
	// 		Thread.Sleep(5000);
	// 		if (ShouldRunTest())
	// 		{
	// 			OpenMDP();
	// 			var btns = RunningApp.Query(c => c.Marked("btnOnDetail"));
	// 			if (btns.Length > 0)
	// 			{
	// 				// on iOS the button could be out of screen
	// 				RunningApp.Tap(c => c.Marked("btnOnDetail"));
	// 				RunningApp.Screenshot("I in landscape and flyout is open");
	// 			}
	// 			RunningApp.WaitForNoElement(c => c.Marked("Clicked"), "Time out", new TimeSpan(0, 0, 1));
	// 		}
	// 	}

	// 		public bool ShouldRunTest()
	// 	{
	// 		var isMasterVisible = RunningApp.Query(q => q.Marked("Leads")).Length > 0;
	// 		return !isMasterVisible;
	// 	}
	// 	public void OpenMDP()
	// 	{
	// #if __IOS__
	// 		RunningApp.Tap(q => q.Marked("Menu"));
	// #else
	// 		RunningApp.Tap ("ShowFlyoutBtn");
	// #endif
	// 	}

	// 	[TearDown]
	// 	public void TestTearDown()
	// 	{
	// 		RunningApp.SetOrientationPortrait();
	// 	}
}