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
	//	[FailsOnIOSWhenRunningOnXamarinUITest]
	//	public void SwipeBackNavCrashTestsAllElementsPresent()
	//	{
	//		App.WaitForElement(q => q.Marked("Page One"));
	//		App.WaitForElement(q => q.Button("Go to second page"));
	//	}

	//	[Test]
	//	[FailsOnIOSWhenRunningOnXamarinUITest]
	//	public void SwipeBackNavCrashTestsGoToSecondPage()
	//	{
	//		App.WaitForElement(q => q.Marked("Page One"));
	//		App.Tap(q => q.Button("Go to second page"));
	//		App.Screenshot("At Second Page");
	//	}

	//#if IOS
	//	[Test]
	//	[Compatibility.UITests.FailsOnIOSWhenRunningOnXamarinUITest]
	//	public void SwipeBackNavCrashTestsSwipeBackDoesNotCrash ()
	//	{
	//		App.WaitForElement (q => q.Marked ("Page One"));
	//		App.Tap (q => q.Button ("Go to second page"));
	//		App.WaitForElement (q => q.Marked ("Swipe lightly left and right to crash this page"));
	//		System.Threading.Thread.Sleep (3);

	//		var mainBounds = App.RootViewRect();

	//		Gestures.Pan (RunningApp, new Drag (mainBounds, 0, 125, 75, 125, Drag.Direction.LeftToRight));
	//		System.Threading.Thread.Sleep (3);
	//		App.Screenshot ("Crash?");
	//		App.WaitForElement (q => q.Marked ("Swipe lightly left and right to crash this page"));
	//	}
	//#endif
}