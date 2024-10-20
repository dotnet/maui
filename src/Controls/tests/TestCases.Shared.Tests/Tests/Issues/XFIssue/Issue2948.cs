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
	// 		App.Screenshot("I am at Issue 2948");
	// 		App.SetOrientationLandscape();
	// 		Thread.Sleep(5000);
	// 		if (ShouldRunTest())
	// 		{
	// 			OpenMDP();
	// 			var btns = App.Query(c => c.Marked("btnOnDetail"));
	// 			if (btns.Length > 0)
	// 			{
	// 				// on iOS the button could be out of screen
	// 				App.Tap(c => c.Marked("btnOnDetail"));
	// 				App.Screenshot("I in landscape and flyout is open");
	// 			}
	// 			App.WaitForNoElement(c => c.Marked("Clicked"), "Time out", new TimeSpan(0, 0, 1));
	// 		}
	// 	}

	// 		public bool ShouldRunTest()
	// 	{
	// 		var isMasterVisible = App.Query(q => q.Marked("Leads")).Length > 0;
	// 		return !isMasterVisible;
	// 	}
	// 	public void OpenMDP()
	// 	{
	// #if __IOS__
	// 		App.Tap(q => q.Marked("Menu"));
	// #else
	// 		App.Tap ("ShowFlyoutBtn");
	// #endif
	// 	}

	// 	[TearDown]
	// 	public void TestTearDown()
	// 	{
	// 		App.SetOrientationPortrait();
	// 	}
}