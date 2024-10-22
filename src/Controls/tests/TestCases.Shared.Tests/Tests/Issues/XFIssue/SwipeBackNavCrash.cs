using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.Navigation)]
public class SwipeBackNavCrash : _IssuesUITest
{
	public SwipeBackNavCrash(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Swipe back nav crash";

//	[Test]
//	[FailsOnIOS]
//	public void SwipeBackNavCrashTestsAllElementsPresent()
//	{
//		RunningApp.WaitForElement(q => q.Marked("Page One"));
//		RunningApp.WaitForElement(q => q.Button("Go to second page"));
//	}

//	[Test]
//	[FailsOnIOS]
//	public void SwipeBackNavCrashTestsGoToSecondPage()
//	{
//		RunningApp.WaitForElement(q => q.Marked("Page One"));
//		RunningApp.Tap(q => q.Button("Go to second page"));
//		RunningApp.Screenshot("At Second Page");
//	}

//#if IOS
//	[Test]
//	[Compatibility.UITests.FailsOnIOS]
//	public void SwipeBackNavCrashTestsSwipeBackDoesNotCrash ()
//	{
//		RunningApp.WaitForElement (q => q.Marked ("Page One"));
//		RunningApp.Tap (q => q.Button ("Go to second page"));
//		RunningApp.WaitForElement (q => q.Marked ("Swipe lightly left and right to crash this page"));
//		System.Threading.Thread.Sleep (3);

//		var mainBounds = RunningApp.RootViewRect();

//		Gestures.Pan (RunningApp, new Drag (mainBounds, 0, 125, 75, 125, Drag.Direction.LeftToRight));
//		System.Threading.Thread.Sleep (3);
//		RunningApp.Screenshot ("Crash?");
//		RunningApp.WaitForElement (q => q.Marked ("Swipe lightly left and right to crash this page"));
//	}
//#endif
}